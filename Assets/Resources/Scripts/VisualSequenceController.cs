using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Orchestrates BaseGame spin and win sequences.
public class VisualSequenceController : BaseView
{
    [SerializeField] private ReelView[] m_Reels;

    [Header("Spin")]
    [SerializeField] private float m_MinSpinDuration = 1.5f;
    [SerializeField] private float m_ReelStopDelay = 0.2f;

    [Header("Wins")]
    [SerializeField] private Sprite m_WinFrameSprite;
    [SerializeField] private Sprite[] m_WinEffectFrames;
    [SerializeField] private float m_WinEffectFrameRate = 30f;
    [SerializeField] private float m_TimeBetweenWinlines = 0.1f;
    [SerializeField, Range(0f, 1f)] private float m_NonWinningSymbolAlpha = 0.25f;

    public IEnumerator PlaySpinSequence(SpinResult spinResult)
    {
        for (int reelIndex = 0; reelIndex < m_Reels.Length; reelIndex++)
        {
            int? reelSeed = CombineSeed(spinResult.DeterministicVisualSeed, reelIndex);
            m_Reels[reelIndex].StartSpin(reelSeed);
        }

        yield return new WaitForSeconds(m_MinSpinDuration);

        Coroutine lastReelStop = null;

        for (int reelIndex = 0; reelIndex < m_Reels.Length; reelIndex++)
        {
            int[] columnSymbols = ExtractColumnSymbols(spinResult.FinalBoard, reelIndex);
            int? reelSeed = CombineSeed(spinResult.DeterministicVisualSeed, reelIndex);
            lastReelStop = StartCoroutine(m_Reels[reelIndex].StopSpin(columnSymbols, reelSeed));

            if (reelIndex < m_Reels.Length - 1)
                yield return new WaitForSeconds(m_ReelStopDelay);
        }

        if (lastReelStop != null)
            yield return lastReelStop;
    }

    private static int? CombineSeed(int? seed, int reelIndex)
    {
        return seed.HasValue
            ? unchecked(seed.Value * 397 ^ reelIndex)
            : null;
    }

    public IEnumerator PlayWinSequence(SpinResult spinResult)
    {
        Debug.Log($"Playing win visuals. Win: {spinResult.TotalWin}");
        yield return ShowWinlines(spinResult);
    }

    public IEnumerator ShowWinlines(SpinResult spinResult)
    {
        int reelCount = m_Reels?.Length ?? 0;
        if (reelCount == 0 || spinResult?.WaysWins == null)
            yield break;

        bool hasShownSymbol = false;

        // WaysEvaluator creates one WaysWin per winning symbol. All ways for
        // that symbol (including shared wild positions) animate simultaneously;
        // the next symbol group is presented afterwards. Trigger symbols never
        // enter WaysWins and therefore cannot appear in this sequence.
        foreach (WaysWin waysWin in spinResult.WaysWins)
        {
            if (waysWin.WinningPositions == null || waysWin.WinningPositions.Count == 0)
                continue;

            if (hasShownSymbol && m_TimeBetweenWinlines > 0f)
                yield return new WaitForSeconds(m_TimeBetweenWinlines);

            yield return PlayWinningSymbols(waysWin.WinningPositions, reelCount);
            hasShownSymbol = true;
        }

        SetAllVisibleSymbolsAlpha(1f);
    }

    private IEnumerator PlayWinningSymbols(IEnumerable<int> positions, int reelCount)
    {
        SetAllVisibleSymbolsAlpha(m_NonWinningSymbolAlpha);
        SetSymbolAlphas(positions, reelCount, 1f);
        SetWinAnimations(positions, reelCount, true);

        float animationDuration = (m_WinEffectFrames?.Length ?? 0) /
            Mathf.Max(1f, m_WinEffectFrameRate);
        if (animationDuration > 0f)
            yield return new WaitForSeconds(animationDuration);

        SetWinAnimations(positions, reelCount, false);
        SetAllVisibleSymbolsAlpha(1f);
    }

    private int[] ExtractColumnSymbols(int[,] board, int reelIndex)
    {
        int rowCount = board.GetLength(1);
        int[] symbols = new int[rowCount];

        for (int row = 0; row < rowCount; row++)
            symbols[row] = board[reelIndex, row];

        return symbols;
    }

    private void SetAllVisibleSymbolsAlpha(float alpha)
    {
        foreach (ReelView reel in m_Reels)
            reel.SetVisibleSymbolsAlpha(alpha);
    }

    private void SetSymbolAlphas(IEnumerable<int> positions, int reelCount, float alpha)
    {
        if (positions == null)
            return;

        foreach (int position in positions)
        {
            int reel = position % reelCount;
            int row = position / reelCount;

            if (reel < 0 || reel >= reelCount || row < 0)
                continue;
            m_Reels[reel].SetSymbolAlphaAt(row, alpha);
        }
    }

    private void SetWinAnimations(IEnumerable<int> positions, int reelCount, bool play)
    {
        if (positions == null)
            return;

        foreach (int position in positions)
        {
            int reel = position % reelCount;
            int row = position / reelCount;

            if (reel < 0 || reel >= reelCount || row < 0)
                continue;

            if (play)
                m_Reels[reel].PlayWinAt(
                    row, m_WinFrameSprite, m_WinEffectFrames, m_WinEffectFrameRate);
            else
                m_Reels[reel].StopWinAt(row);
        }
    }

    public bool TryValidate(out string error)
    {
        error = null;

        if (m_Reels == null || m_Reels.Length != 5)
        {
            error = "VisualSequenceController requires exactly five ReelView references.";
            return false;
        }

        for (int reel = 0; reel < m_Reels.Length; reel++)
        {
            if (m_Reels[reel] == null)
            {
                error = $"ReelView {reel + 1} is not assigned.";
                return false;
            }

            if (m_Reels[reel].TryValidate(out string reelError))
                continue;

            error = $"ReelView {reel + 1} is invalid: {reelError}";
            return false;
        }

        if (m_WinEffectFrames == null || m_WinEffectFrames.Length == 0)
        {
            error = "Win effect frames are not assigned.";
            return false;
        }

        if (m_WinFrameSprite == null)
        {
            error = "The win frame sprite is not assigned.";
            return false;
        }

        return true;
    }
}
