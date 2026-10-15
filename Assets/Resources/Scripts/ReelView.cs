using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Orchestrates a single reel's strip of SymbolViews.
public class ReelView : MonoBehaviour
{
    private const int SymbolCount = 8;

    // Serialized art is stored in its original icon_1..icon_8 order. Logical
    // IDs are zero-based: Yeti/icon_8, icon_1..icon_6, then logo/icon_7.
    private static readonly int[] VisualIndexBySymbolId = { 7, 0, 1, 2, 3, 4, 5, 6 };

    [Header("Strip")]
    [SerializeField] private List<SymbolView> m_SymbolViews = new();
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

    private float m_WrapBelow;
    private float m_TotalSpan;
    private int m_VisibleRowCount;
    private Coroutine m_ScrollCoroutine;

    // Runs in Start so every SymbolView.Awake has captured its rest position first.
    private void Start()
    {
        float minY = float.MaxValue;
        for (int i = 0; i < m_SymbolViews.Count; i++)
            if (m_SymbolViews[i].RestY < minY)
                minY = m_SymbolViews[i].RestY;

        m_TotalSpan = m_SymbolSpacing * m_SymbolViews.Count;
        m_WrapBelow = minY - m_SymbolSpacing;
    }

    public void StartSpin(int? deterministicSeed = null)
    {
        if (m_ScrollCoroutine != null)
            StopCoroutine(m_ScrollCoroutine);

        m_ScrollCoroutine = StartCoroutine(ScrollLoop());

        int iconCount = m_BlurFramesPerIcon > 0
            ? (m_BlurFrames?.Length ?? 0) / m_BlurFramesPerIcon
            : 0;

        for (int i = 0; i < m_SymbolViews.Count; i++)
        {
            int? startIcon = deterministicSeed.HasValue && iconCount > 0
                ? DeterministicIndex(deterministicSeed.Value, i, iconCount)
                : null;
            m_SymbolViews[i].StartBlur(
                m_BlurFrames, m_BlurFramesPerIcon, m_BlurFrameRate, startIcon);
        }
    }

    public IEnumerator StopSpin(int[] finalSymbolIds, int? deterministicSeed = null)
    {
        if (m_ScrollCoroutine != null)
        {
            StopCoroutine(m_ScrollCoroutine);
            m_ScrollCoroutine = null;
        }

        for (int i = 0; i < m_SymbolViews.Count; i++)
            m_SymbolViews[i].StopBlur();

        SnapToRest();
        ApplyFinalSymbols(finalSymbolIds, deterministicSeed);
        yield return PlayBounce();
    }

    public void PlayWinAt(
        int visibleRow,
        Sprite winFrameSprite,
        Sprite[] winEffectFrames,
        float frameRate)
    {
        SymbolView symbol = SymbolForVisibleRow(visibleRow);
        if (symbol == null)
            return;

        symbol.PlayWin(winFrameSprite, winEffectFrames, frameRate);
    }

    public void StopWinAt(int visibleRow)
    {
        SymbolForVisibleRow(visibleRow)?.StopWin();
    }

    public void SetSymbolAlphaAt(int visibleRow, float alpha)
    {
        SymbolForVisibleRow(visibleRow)?.SetAlpha(alpha);
    }

    public void SetVisibleSymbolsAlpha(float alpha)
    {
        int visibleRows = m_VisibleSlotIndices != null && m_VisibleSlotIndices.Length > 0
            ? m_VisibleSlotIndices.Length
            : m_VisibleRowCount;

        for (int row = 0; row < visibleRows; row++)
            SymbolForVisibleRow(row)?.SetAlpha(alpha);
    }

    private IEnumerator ScrollLoop()
    {
        float offset = 0f;
        while (true)
        {
            offset += m_SpinSpeed * Time.deltaTime;

            for (int i = 0; i < m_SymbolViews.Count; i++)
            {
                float y = m_SymbolViews[i].RestY - offset;

                while (y < m_WrapBelow)
                    y += m_TotalSpan;

                m_SymbolViews[i].SetLocalY(y);
            }

            yield return null;
        }
    }

    private IEnumerator PlayBounce()
    {
        if (m_BounceDuration <= 0f || m_BounceAmount == 0f)
            yield break;

        float elapsed = 0f;
        while (elapsed < m_BounceDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / m_BounceDuration);
            float displacement = Mathf.Sin(normalizedTime * Mathf.PI) * m_BounceAmount;

            for (int i = 0; i < m_SymbolViews.Count; i++)
                m_SymbolViews[i].SetLocalY(m_SymbolViews[i].RestY - displacement);

            yield return null;
        }

        SnapToRest();
    }

    private void SnapToRest()
    {
        for (int i = 0; i < m_SymbolViews.Count; i++)
            m_SymbolViews[i].SnapToRest();
    }

    private void ApplyFinalSymbols(int[] finalSymbolIds, int? deterministicSeed)
    {
        if (m_IconSprites == null || m_IconSprites.Length == 0)
        {
            Debug.LogWarning($"{gameObject.name}: m_IconSprites is empty - assign icon sprites in the Inspector.", this);
            return;
        }

        m_VisibleRowCount = finalSymbolIds.Length;
        bool hasVisibleMap = m_VisibleSlotIndices != null && m_VisibleSlotIndices.Length > 0;

        for (int visibleRow = 0; visibleRow < finalSymbolIds.Length; visibleRow++)
        {
            int slot = hasVisibleMap ? m_VisibleSlotIndices[visibleRow] : visibleRow;
            if (slot < 0 || slot >= m_SymbolViews.Count)
                continue;

            int symbolId = Mathf.Clamp(finalSymbolIds[visibleRow], 0, SymbolCount - 1);
            int spriteIndex = VisualIndexForSymbolId(symbolId);
            m_SymbolViews[slot].SetSymbol(symbolId, m_IconSprites[spriteIndex]);
        }

        for (int i = 0; i < m_SymbolViews.Count; i++)
        {
            if (IsVisibleSlot(i, hasVisibleMap, finalSymbolIds.Length))
                continue;
            int symbolId = deterministicSeed.HasValue
                ? DeterministicIndex(deterministicSeed.Value, i, SymbolCount)
                : Random.Range(0, SymbolCount);
            int spriteIndex = VisualIndexForSymbolId(symbolId);
            m_SymbolViews[i].SetSymbol(symbolId, m_IconSprites[spriteIndex]);
        }
    }

    private static int VisualIndexForSymbolId(int symbolId)
    {
        return symbolId >= 0 && symbolId < VisualIndexBySymbolId.Length
            ? VisualIndexBySymbolId[symbolId]
            : 0;
    }

    private static int DeterministicIndex(int seed, int salt, int length)
    {
        unchecked
        {
            uint value = (uint)seed ^ ((uint)(salt + 1) * 0x9e3779b9u);
            value ^= value >> 16;
            value *= 0x85ebca6bu;
            value ^= value >> 13;
            return (int)(value % (uint)length);
        }
    }

    private int SlotForVisibleRow(int visibleRow)
    {
        bool hasVisibleMap = m_VisibleSlotIndices != null && m_VisibleSlotIndices.Length > 0;

        if (hasVisibleMap)
        {
            return visibleRow >= 0 && visibleRow < m_VisibleSlotIndices.Length
                ? m_VisibleSlotIndices[visibleRow]
                : -1;
        }

        return visibleRow;
    }

    private SymbolView SymbolForVisibleRow(int visibleRow)
    {
        int slot = SlotForVisibleRow(visibleRow);
        return slot >= 0 && slot < m_SymbolViews.Count ? m_SymbolViews[slot] : null;
    }

    private bool IsVisibleSlot(int index, bool hasVisibleMap, int defaultVisibleCount)
    {
        if (hasVisibleMap)
        {
            for (int i = 0; i < m_VisibleSlotIndices.Length; i++)
                if (m_VisibleSlotIndices[i] == index)
                    return true;
            return false;
        }

        return index < defaultVisibleCount;
    }

    public bool TryValidate(out string error)
    {
        error = null;

        if (m_SymbolViews == null || m_SymbolViews.Count < 3)
        {
            error = "At least three SymbolView references are required.";
            return false;
        }

        foreach (SymbolView symbolView in m_SymbolViews)
        {
            if (symbolView == null)
            {
                error = "A SymbolView reference is missing.";
                return false;
            }

            if (symbolView.TryValidate(out error))
                continue;

            return false;
        }

        if (m_VisibleSlotIndices == null || m_VisibleSlotIndices.Length != 3)
        {
            error = "Exactly three visible slot indices are required.";
            return false;
        }

        foreach (int slot in m_VisibleSlotIndices)
        {
            if (slot >= 0 && slot < m_SymbolViews.Count)
                continue;

            error = $"Visible slot index {slot} is outside the symbol-view range.";
            return false;
        }

        if (m_IconSprites == null || m_IconSprites.Length < SymbolCount)
        {
            error = $"At least {SymbolCount} icon sprites are required.";
            return false;
        }

        if (m_BlurFrames == null || m_BlurFrames.Length < SymbolCount * m_BlurFramesPerIcon)
        {
            error = "The blur-frame array does not cover every symbol.";
            return false;
        }

        if (m_BlurFramesPerIcon <= 0)
        {
            error = "Blur frames per icon must be positive.";
            return false;
        }

        return true;
    }
}
