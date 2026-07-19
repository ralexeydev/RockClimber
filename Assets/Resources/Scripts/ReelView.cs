using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Orchestrates a single reel's strip of SymbolViews.
public class ReelView : MonoBehaviour
{
    [Header("Strip")]
    [SerializeField] private List<SymbolView> m_SymbolViews = new();
    [SerializeField] private int[] m_VisibleSlotIndices;

    [Header("Sprites")]
    [SerializeField] private Sprite[] m_IconSprites;
    [SerializeField] private Sprite[] m_BlurFrames;
    [SerializeField] private int m_BlurFramesPerIcon = 5;

    [Header("Win")]
    // One entry per symbol id, parallel to m_IconSprites (index = symbolId - 1).
    [SerializeField] private SymbolWinAnimation[] m_WinAnimations;
    [SerializeField] private float m_WinFrameRate = 18f;

    [Header("Motion")]
    [SerializeField] private float m_SpinSpeed = 20f;
    [SerializeField] private float m_SymbolSpacing = 1.8f;
    [SerializeField] private float m_BlurFrameRate = 24f;

    [Header("Bounce")]
    [SerializeField] private float m_BounceAmount = 0.3f;
    [SerializeField] private float m_BounceDuration = 0.2f;

    private float m_WrapBelow;
    private float m_TotalSpan;
    private Coroutine m_ScrollCoroutine;

    // Runs in Start so every SymbolView.Awake has captured its rest position first.
    private void Start()
    {
        float minY = float.MaxValue;
        for (int i = 0; i < m_SymbolViews.Count; i++)
            if (m_SymbolViews[i].RestY < minY) minY = m_SymbolViews[i].RestY;

        m_TotalSpan = m_SymbolSpacing * m_SymbolViews.Count;
        m_WrapBelow = minY - m_SymbolSpacing;
    }

    public void StartSpin()
    {
        if (m_ScrollCoroutine != null)
            StopCoroutine(m_ScrollCoroutine);

        m_ScrollCoroutine = StartCoroutine(ScrollLoop());

        for (int i = 0; i < m_SymbolViews.Count; i++)
            m_SymbolViews[i].StartBlur(m_BlurFrames, m_BlurFramesPerIcon, m_BlurFrameRate);
    }

    public IEnumerator StopSpin(int[] finalSymbolIds)
    {
        if (m_ScrollCoroutine != null)
        {
            StopCoroutine(m_ScrollCoroutine);
            m_ScrollCoroutine = null;
        }

        for (int i = 0; i < m_SymbolViews.Count; i++)
            m_SymbolViews[i].StopBlur();

        SnapToRest();
        ApplyFinalSymbols(finalSymbolIds);
        yield return PlayBounce();
    }

    public void PlayWinAt(int visibleRow)
    {
        SymbolView symbol = SymbolForVisibleRow(visibleRow);
        if (symbol == null) return;

        int animationIndex = symbol.SymbolId - 1;
        if (m_WinAnimations == null || animationIndex < 0 || animationIndex >= m_WinAnimations.Length)
            return;

        symbol.PlayWin(m_WinAnimations[animationIndex]?.Frames, m_WinFrameRate);
    }

    public void StopWinAt(int visibleRow)
    {
        SymbolForVisibleRow(visibleRow)?.StopWin();
    }

    public bool TryGetVisibleSymbolPosition(int visibleRow, out Vector3 position)
    {
        SymbolView symbol = SymbolForVisibleRow(visibleRow);
        if (symbol == null)
        {
            position = default;
            return false;
        }

        position = symbol.WorldPosition;
        return true;
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

    private void ApplyFinalSymbols(int[] finalSymbolIds)
    {
        if (m_IconSprites == null || m_IconSprites.Length == 0)
        {
            Debug.LogWarning($"{gameObject.name}: m_IconSprites is empty - assign icon sprites in the Inspector.", this);
            return;
        }

        bool hasVisibleMap = m_VisibleSlotIndices != null && m_VisibleSlotIndices.Length > 0;

        for (int visibleRow = 0; visibleRow < finalSymbolIds.Length; visibleRow++)
        {
            int slot = hasVisibleMap ? m_VisibleSlotIndices[visibleRow] : visibleRow;
            if (slot < 0 || slot >= m_SymbolViews.Count) continue;

            int spriteIndex = Mathf.Clamp(finalSymbolIds[visibleRow] - 1, 0, m_IconSprites.Length - 1);
            m_SymbolViews[slot].SetSymbol(finalSymbolIds[visibleRow], m_IconSprites[spriteIndex]);
        }

        for (int i = 0; i < m_SymbolViews.Count; i++)
        {
            if (IsVisibleSlot(i, hasVisibleMap, finalSymbolIds.Length)) continue;
            int randomIndex = Random.Range(0, m_IconSprites.Length);
            m_SymbolViews[i].SetSymbol(randomIndex + 1, m_IconSprites[randomIndex]);
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
                if (m_VisibleSlotIndices[i] == index) return true;
            return false;
        }

        return index < defaultVisibleCount;
    }
}

[System.Serializable]
public class SymbolWinAnimation
{
    public Sprite[] Frames;
}
