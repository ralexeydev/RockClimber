using System.Collections;
using UnityEngine;

// Drives one reel column with a real spinning-reel effect:
//   - All slots (visible + extras) scroll downward at a constant speed.
//   - When a slot reaches the bottom wrap threshold it teleports above the top of the
//     strip, so a 2D mask covering the reel window hides the seam and produces an
//     endless-loop circular motion.
//   - During the spin each slot independently picks a random icon's blur loop and
//     cycles through its frames, so different rows show different blurred icons.
//   - On stop the scroll and blur loops end, slots snap back to their rest positions,
//     and the visible rows receive their final icons from the engine result.
//
// Inspector setup:
//   m_SymbolRenderers     — every renderer in the strip, top to bottom (include the
//                           off-screen extras above and below the visible window).
//   m_VisibleSlotIndices  — indices into m_SymbolRenderers that correspond to the
//                           three visible rows. e.g. with 5 slots and one extra at
//                           each end, this is [1, 2, 3].
//   m_IconSprites         — icon_1..icon_8 (index 0 = icon_1).
//   m_BlurFrames          — flat list of every blur PNG, ordered by icon then frame:
//                           icon_1_00..icon_1_04, icon_2_00..icon_2_04, ..., icon_8_00..icon_8_04.
//                           Each slot picks a random icon group at spin start and loops it.
public class ReelView : MonoBehaviour
{
    [Header("Strip")]
    [SerializeField] private SpriteRenderer[] m_SymbolRenderers;
    [SerializeField] private int[] m_VisibleSlotIndices;

    [Header("Sprites")]
    [SerializeField] private Sprite[] m_IconSprites;
    [SerializeField] private Sprite[] m_BlurFrames;
    [SerializeField] private int m_BlurFramesPerIcon = 5;

    [Header("Motion")]
    [SerializeField] private float m_SpinSpeed = 20f;
    [SerializeField] private float m_SymbolSpacing = 1.8f;
    [SerializeField] private float m_BlurFrameRate = 24f;

    [Header("Bounce")]
    [SerializeField] private float m_BounceAmount = 0.3f;
    [SerializeField] private float m_BounceDuration = 0.2f;

    private float[] m_RestY;
    private float m_WrapBelow;
    private float m_TotalSpan;
    private Coroutine m_ScrollCoroutine;
    private Coroutine[] m_BlurCoroutines;

    private void Awake()
    {
        m_RestY = new float[m_SymbolRenderers.Length];
        float minY = float.MaxValue;
        for (int i = 0; i < m_SymbolRenderers.Length; i++)
        {
            m_RestY[i] = m_SymbolRenderers[i].transform.localPosition.y;
            if (m_RestY[i] < minY) minY = m_RestY[i];
        }

        m_TotalSpan = m_SymbolSpacing * m_SymbolRenderers.Length;
        m_WrapBelow = minY - m_SymbolSpacing;
        m_BlurCoroutines = new Coroutine[m_SymbolRenderers.Length];
    }

    public void StartSpin()
    {
        if (m_ScrollCoroutine != null)
            StopCoroutine(m_ScrollCoroutine);

        m_ScrollCoroutine = StartCoroutine(ScrollLoop());

        for (int i = 0; i < m_SymbolRenderers.Length; i++)
        {
            if (m_BlurCoroutines[i] != null)
                StopCoroutine(m_BlurCoroutines[i]);
            m_BlurCoroutines[i] = StartCoroutine(BlurLoop(i));
        }
    }

    public IEnumerator StopSpin(int[] finalSymbolIds)
    {
        if (m_ScrollCoroutine != null)
        {
            StopCoroutine(m_ScrollCoroutine);
            m_ScrollCoroutine = null;
        }

        for (int i = 0; i < m_BlurCoroutines.Length; i++)
        {
            if (m_BlurCoroutines[i] != null)
            {
                StopCoroutine(m_BlurCoroutines[i]);
                m_BlurCoroutines[i] = null;
            }
        }

        SnapToRest();
        ApplyFinalSymbols(finalSymbolIds);
        yield return PlayBounce();
    }

    // Scrolls every slot downward; wraps any slot that crosses the bottom threshold
    // back above the top of the strip. Using a single shared offset keeps all slots
    // perfectly equidistant regardless of how many extras are in the strip.
    private IEnumerator ScrollLoop()
    {
        float offset = 0f;
        while (true)
        {
            offset += m_SpinSpeed * Time.deltaTime;

            for (int i = 0; i < m_SymbolRenderers.Length; i++)
            {
                float y = m_RestY[i] - offset;

                while (y < m_WrapBelow)
                    y += m_TotalSpan;

                Transform transform = m_SymbolRenderers[i].transform;
                Vector3 pos = transform.localPosition;
                pos.y = y;
                transform.localPosition = pos;
            }

            yield return null;
        }
    }

    private IEnumerator BlurLoop(int slotIndex)
    {
        if (m_BlurFrames == null || m_BlurFrames.Length == 0)
        {
            Debug.LogWarning($"{gameObject.name}: Blur frames are empty", this);
            yield break;
        }

        int framesPerIcon = Mathf.Max(1, m_BlurFramesPerIcon);
        int iconCount = Mathf.Max(1, m_BlurFrames.Length / framesPerIcon);
        int startIdx = Random.Range(0, iconCount) * framesPerIcon;
        int frame = 0;
        WaitForSeconds wait = new WaitForSeconds(1f / m_BlurFrameRate);

        // Set the first frame immediately so the swap is visible without waiting one tick.
        m_SymbolRenderers[slotIndex].sprite = m_BlurFrames[startIdx];

        while (true)
        {
            yield return wait;
            frame = (frame + 1) % framesPerIcon;
            int idx = startIdx + frame;
            if (idx >= m_BlurFrames.Length) idx = startIdx;
            m_SymbolRenderers[slotIndex].sprite = m_BlurFrames[idx];
        }
    }

    
    // mimicking the physics of a reel landing with bounce
    private IEnumerator PlayBounce()
    {
        if (m_BounceDuration <= 0f || m_BounceAmount == 0f)
            yield break;

        float elapsed = 0f;
        while (elapsed < m_BounceDuration)
        {
            elapsed += Time.deltaTime;
            float t01 = Mathf.Clamp01(elapsed / m_BounceDuration);
            float displacement = Mathf.Sin(t01 * Mathf.PI) * m_BounceAmount;

            for (int i = 0; i < m_SymbolRenderers.Length; i++)
            {
                Transform transform = m_SymbolRenderers[i].transform;
                Vector3 pos = transform.localPosition;
                pos.y = m_RestY[i] - displacement;
                transform.localPosition = pos;
            }

            yield return null;
        }

        SnapToRest();
    }

    // On stop, the scroll and blur coroutines end immediately, so some slots may be mid-scroll or mid-blur.
    private void SnapToRest()
    {
        for (int i = 0; i < m_SymbolRenderers.Length; i++)
        {
            Transform transform = m_SymbolRenderers[i].transform;
            Vector3 pos = transform.localPosition;
            pos.y = m_RestY[i];
            transform.localPosition = pos;
        }
    }

    private void ApplyFinalSymbols(int[] finalSymbolIds)
    {
        if (m_IconSprites == null || m_IconSprites.Length == 0)
        {
            Debug.LogWarning($"{gameObject.name}: m_IconSprites is empty — assign icon sprites in the Inspector.", this);
            return;
        }

        bool hasVisibleMap = m_VisibleSlotIndices != null && m_VisibleSlotIndices.Length > 0;

        // Visible rows get the engine's final symbols
        for (int v = 0; v < finalSymbolIds.Length; v++)
        {
            int slot = hasVisibleMap ? m_VisibleSlotIndices[v] : v;
            if (slot < 0 || slot >= m_SymbolRenderers.Length) continue;

            int idx = Mathf.Clamp(finalSymbolIds[v] - 1, 0, m_IconSprites.Length - 1);
            m_SymbolRenderers[slot].sprite = m_IconSprites[idx];
        }

        // Off-screen extras get random icons so they're populated when they scroll in next spin.
        for (int i = 0; i < m_SymbolRenderers.Length; i++)
        {
            if (IsVisibleSlot(i, hasVisibleMap, finalSymbolIds.Length)) continue;
            int rand = Random.Range(0, m_IconSprites.Length);
            m_SymbolRenderers[i].sprite = m_IconSprites[rand];
        }
    }

    private bool IsVisibleSlot(int index, bool hasVisibleMap, int defaultVisibleCount)
    {
        if (hasVisibleMap)
        {
            for (int i = 0; i < m_VisibleSlotIndices.Length; i++)
                if (m_VisibleSlotIndices[i] == index) return true;
            return false;
        }
        return index < defaultVisibleCount;
    }
}
