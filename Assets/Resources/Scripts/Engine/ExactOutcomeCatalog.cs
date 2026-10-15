using System;
using System.Collections.Generic;

public sealed class ExactOutcomeCatalog
{
    private readonly ReelStrip[] m_Strips;
    private readonly PayTable m_PayTable;
    private readonly int m_Reels;
    private readonly int m_Rows;
    private readonly int m_MaxBaseWinCents;
    private readonly Dictionary<int, List<int[]>> m_OffsetsByBaseWin = new();
    private int m_NextCombination;
    private int m_TotalCombinations;

    public bool IsComplete => m_NextCombination >= m_TotalCombinations;
    public float Progress => m_TotalCombinations == 0 ? 0f : (float)m_NextCombination / m_TotalCombinations;
    public IReadOnlyCollection<int> ReachableBaseWins => m_OffsetsByBaseWin.Keys;

    public ExactOutcomeCatalog(ReelStrip[] strips, PayTable payTable, int reels, int rows, int maxBaseWinCents)
    {
        m_Strips = strips ?? throw new ArgumentNullException(nameof(strips));
        m_PayTable = payTable ?? throw new ArgumentNullException(nameof(payTable));
        m_Reels = reels;
        m_Rows = rows;
        m_MaxBaseWinCents = maxBaseWinCents;
        Reset();
    }

    public void Reset()
    {
        m_OffsetsByBaseWin.Clear();
        m_NextCombination = 0;
        m_TotalCombinations = 1;

        for (int reel = 0; reel < m_Reels; reel++)
            m_TotalCombinations = checked(m_TotalCombinations * m_Strips[reel].Length);
    }

    public bool BuildNextBatch(int candidatesPerBatch)
    {
        if (candidatesPerBatch <= 0)
            throw new ArgumentOutOfRangeException(nameof(candidatesPerBatch));

        int batchEnd = Math.Min(m_TotalCombinations, m_NextCombination + candidatesPerBatch);
        int[] offsets = new int[m_Reels];

        while (m_NextCombination < batchEnd)
        {
            DecodeOffsets(m_NextCombination, offsets);
            AddCandidate(offsets);
            m_NextCombination++;
        }

        return IsComplete;
    }

    public void BuildAll()
    {
        while (!BuildNextBatch(5000))
        {
        }
    }

    public bool ContainsBaseWin(int baseWinCents)
    {
        return m_OffsetsByBaseWin.TryGetValue(baseWinCents, out List<int[]> offsets) && offsets.Count > 0;
    }

    public bool TryResolveBaseWin(int baseWinCents, Random random, out int[] offsets)
    {
        offsets = null;

        if (!IsComplete || random == null)
            return false;

        if (!m_OffsetsByBaseWin.TryGetValue(baseWinCents, out List<int[]> candidates) || candidates.Count == 0)
            return false;

        offsets = (int[])candidates[random.Next(0, candidates.Count)].Clone();
        return true;
    }

    private void DecodeOffsets(int combination, int[] offsets)
    {
        int remaining = combination;

        for (int reel = 0; reel < m_Reels; reel++)
        {
            offsets[reel] = remaining % m_Strips[reel].Length;
            remaining /= m_Strips[reel].Length;
        }
    }

    private void AddCandidate(int[] offsets)
    {
        int[,] board = BoardBuilder.Build(m_Strips, offsets, m_Reels, m_Rows);
        WaysEvaluator.Result evaluation = WaysEvaluator.EvaluateSummary(board, m_PayTable, 1);

        if (evaluation.TotalWin < 0 || evaluation.TotalWin > m_MaxBaseWinCents)
            return;

        if (evaluation.FreeSpinsAwarded > 0 || evaluation.FeatureTriggered)
            return;

        if (!m_OffsetsByBaseWin.TryGetValue(evaluation.TotalWin, out List<int[]> candidates))
        {
            candidates = new List<int[]>();
            m_OffsetsByBaseWin.Add(evaluation.TotalWin, candidates);
        }

        candidates.Add((int[])offsets.Clone());
    }
}
