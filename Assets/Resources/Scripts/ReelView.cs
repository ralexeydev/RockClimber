using System;
using System.Collections;
using UnityEngine;

// Drives one reel column with a real spinning-reel effect:
//   - All slots (visible + extras) scroll downward at a constant speed.
//   - When a slot reaches the bottom wrap threshold it teleports above the top of the
//     strip, so a 2D mask covering the reel window hides the seam and produces an
//     endless-loop circular motion.
//   - During the spin each slot independently cycles a randomly-chosen blur sequence,
//     so different reels / rows show different blurred icons for visual variety.
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
//   m_BlurAnimations      — one BlurSequence per icon. A random sequence is picked
//                           for each slot on each spin.
public class ReelView : MonoBehaviour
{
    [Serializable]
    public class BlurSequence
    {
        public Sprite[] Frames;
    }

    [Header("Strip")]
    [SerializeField] private SpriteRenderer[] m_SymbolRenderers;
    [SerializeField] private int[]            m_VisibleSlotIndices;

    [Header("Sprites")]
    [SerializeField] private Sprite[]         m_IconSprites;
    [SerializeField] private BlurSequence[]   m_BlurAnimations;

    [Header("Motion")]
    [SerializeField] private float m_SpinSpeed      = 20f;
    [SerializeField] private float m_SymbolSpacing  = 1.8f;
    [SerializeField] private float m_BlurFrameRate  = 24f;

    private float[]     m_RestY;
    private float       m_WrapBelow;
    private float       m_TotalSpan;
    private Coroutine   m_ScrollCoroutine;
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

        m_TotalSpan      = m_SymbolSpacing * m_SymbolRenderers.Length;
        m_WrapBelow      = minY - m_SymbolSpacing;
        m_BlurCoroutines = new Coroutine[m_SymbolRenderers.Length];
    }

    public void StartSpin()
    {
        if (m_ScrollCoroutine != null)
            StopCoroutine(m_ScrollCoroutine);
        m_ScrollCoroutine = StartCoroutine(ScrollLoop());

        // Each slot picks its own random blur sequence so the column doesn't look uniform.
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
        yield break;
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

                var t   = m_SymbolRenderers[i].transform;
                var pos = t.localPosition;
                pos.y   = y;
                t.localPosition = pos;
            }

            yield return null;
        }
    }

    private IEnumerator BlurLoop(int slotIndex)
    {
        if (m_BlurAnimations == null || m_BlurAnimations.Length == 0)
            yield break;

        var sequence = m_BlurAnimations[UnityEngine.Random.Range(0, m_BlurAnimations.Length)];
        if (sequence == null || sequence.Frames == null || sequence.Frames.Length == 0)
            yield break;

        int  frame = UnityEngine.Random.Range(0, sequence.Frames.Length);
        var  wait  = new WaitForSeconds(1f / m_BlurFrameRate);

        while (true)
        {
            m_SymbolRenderers[slotIndex].sprite = sequence.Frames[frame];
            frame = (frame + 1) % sequence.Frames.Length;
            yield return wait;
        }
    }

    private void SnapToRest()
    {
        for (int i = 0; i < m_SymbolRenderers.Length; i++)
        {
            var t   = m_SymbolRenderers[i].transform;
            var pos = t.localPosition;
            pos.y   = m_RestY[i];
            t.localPosition = pos;
        }
    }

    private void ApplyFinalSymbols(int[] finalSymbolIds)
    {
        if (m_IconSprites == null || m_IconSprites.Length == 0)
        {
            Debug.LogWarning($"{gameObject.name}: m_IconSprites is empty — assign icon sprites in the Inspector.");
            return;
        }

        bool hasVisibleMap = m_VisibleSlotIndices != null && m_VisibleSlotIndices.Length > 0;

        // Visible rows get the engine's final symbols.
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
            int rand = UnityEngine.Random.Range(0, m_IconSprites.Length);
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
