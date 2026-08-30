using System.Collections;
using UnityEngine;

// Owns a single symbol's rendering: its sprite, position, and animations.
public class SymbolView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer m_SymbolRenderer;

    private int m_SymbolId;
    private float m_RestY;
    private SpriteRenderer m_WinEffectRenderer;
    private SpriteRenderer m_WinFrameRenderer;
    private Coroutine m_BlurCoroutine;
    private Coroutine m_WinCoroutine;

    public float RestY => m_RestY;
    public int SymbolId => m_SymbolId;

    private void Awake()
    {
        m_RestY = transform.localPosition.y;
    }

    public void SetSymbol(int symbolId, Sprite sprite)
    {
        StopWin();
        m_SymbolId = symbolId;
        m_SymbolRenderer.sprite = sprite;
        SetAlpha(1f);
    }

    public void SetAlpha(float alpha)
    {
        if (m_SymbolRenderer == null) return;

        Color color = m_SymbolRenderer.color;
        color.a = Mathf.Clamp01(alpha);
        m_SymbolRenderer.color = color;
    }

    public void SetLocalY(float y)
    {
        Vector3 position = transform.localPosition;
        position.y = y;
        transform.localPosition = position;
    }

    public void SnapToRest()
    {
        SetLocalY(m_RestY);
    }

    public void StartBlur(
        Sprite[] blurFrames,
        int framesPerIcon,
        float frameRate,
        int? deterministicIconIndex = null)
    {
        if (blurFrames == null || blurFrames.Length == 0)
        {
            Debug.LogWarning($"{gameObject.name}: Blur frames are empty", this);
            return;
        }

        StopWin();
        StopBlur();
        SetAlpha(1f);
        m_BlurCoroutine = StartCoroutine(BlurLoop(
            blurFrames, framesPerIcon, frameRate, deterministicIconIndex));
    }

    public void PlayWin(Sprite winFrameSprite, Sprite[] winEffectFrames, float frameRate)
    {
        if (winEffectFrames == null || winEffectFrames.Length == 0)
            return;

        StopWin();
        EnsureWinRenderers();
        m_WinFrameRenderer.sprite = winFrameSprite;
        m_WinFrameRenderer.enabled = winFrameSprite != null;
        m_WinEffectRenderer.enabled = true;
        m_WinCoroutine = StartCoroutine(WinLoop(winEffectFrames, frameRate));
    }

    public void StopWin()
    {
        if (m_WinCoroutine != null)
        {
            StopCoroutine(m_WinCoroutine);
            m_WinCoroutine = null;
        }

        if (m_WinEffectRenderer != null)
        {
            m_WinEffectRenderer.sprite = null;
            m_WinEffectRenderer.enabled = false;
        }

        if (m_WinFrameRenderer != null)
        {
            m_WinFrameRenderer.sprite = null;
            m_WinFrameRenderer.enabled = false;
        }
    }

    public void StopBlur()
    {
        if (m_BlurCoroutine == null) return;
        StopCoroutine(m_BlurCoroutine);
        m_BlurCoroutine = null;
    }

    private IEnumerator BlurLoop(
        Sprite[] blurFrames,
        int framesPerIcon,
        float frameRate,
        int? deterministicIconIndex)
    {
        framesPerIcon = Mathf.Max(1, framesPerIcon);
        int iconCount = Mathf.Max(1, blurFrames.Length / framesPerIcon);
        int iconIndex = deterministicIconIndex.HasValue
            ? Mathf.Abs(deterministicIconIndex.Value) % iconCount
            : Random.Range(0, iconCount);
        int startIndex = iconIndex * framesPerIcon;
        int frame = 0;
        WaitForSeconds wait = new(1f / Mathf.Max(1f, frameRate));

        m_SymbolRenderer.sprite = blurFrames[startIndex];

        while (true)
        {
            yield return wait;
            frame = (frame + 1) % framesPerIcon;
            int index = startIndex + frame;
            if (index >= blurFrames.Length) index = startIndex;
            m_SymbolRenderer.sprite = blurFrames[index];
        }
    }

    private IEnumerator WinLoop(Sprite[] winFrames, float frameRate)
    {
        WaitForSeconds wait = new(1f / Mathf.Max(1f, frameRate));

        for (int i = 0; i < winFrames.Length; i++)
        {
            m_WinEffectRenderer.sprite = winFrames[i];
            yield return wait;
        }

        m_WinEffectRenderer.sprite = null;
        m_WinEffectRenderer.enabled = false;
        m_WinCoroutine = null;
    }

    private void EnsureWinRenderers()
    {
        m_WinEffectRenderer ??= CreateOverlayRenderer("WinEffect", 1);
        m_WinFrameRenderer ??= CreateOverlayRenderer("WinFrame", 2);
    }

    private SpriteRenderer CreateOverlayRenderer(string suffix, int sortingOrderOffset)
    {
        GameObject overlayObject = new($"{gameObject.name}_{suffix}");
        overlayObject.layer = gameObject.layer;
        overlayObject.transform.SetParent(transform, false);

        SpriteRenderer renderer = overlayObject.AddComponent<SpriteRenderer>();
        renderer.sharedMaterial = m_SymbolRenderer.sharedMaterial;
        renderer.sortingLayerID = m_SymbolRenderer.sortingLayerID;
        renderer.sortingOrder = m_SymbolRenderer.sortingOrder + sortingOrderOffset;
        renderer.maskInteraction = m_SymbolRenderer.maskInteraction;
        renderer.enabled = false;
        return renderer;
    }
}
