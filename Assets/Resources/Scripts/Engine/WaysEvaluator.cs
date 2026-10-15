using System.Collections.Generic;

// One winning symbol group produced by ways-pay evaluation.
public class WaysWin
{
    public int SymbolId;
    public int ColumnsMatched;
    public int Ways;
    public int Payout;

    // Union of every symbol and wild involved in this symbol's winning ways.
    public List<int> WinningPositions = new();

    // Individual paths remain available as result data, although presentation
    // animates their union simultaneously for each winning symbol.
    public List<List<int>> WinningPaths = new();
}

// Ways are evaluated only for paying symbols. Freeplay and feature triggers are
// counted independently and never contribute payout or a WaysWin.
public static class WaysEvaluator
{
    public class Result
    {
        public List<WaysWin> WaysWins = new();
        public int TotalWin;

        public int FreeplayTriggerCount;
        public List<int> FreeplayTriggerPositions = new();
        public int FreeSpinsAwarded;

        public int FeatureTriggerCount;
        public List<int> FeatureTriggerPositions = new();
        public bool FeatureTriggered;
    }

    public static Result Evaluate(int[,] board, PayTable paytable, int betLevel)
    {
        return Evaluate(board, paytable, betLevel, true);
    }

    public static Result EvaluateSummary(int[,] board, PayTable paytable, int betLevel)
    {
        return Evaluate(board, paytable, betLevel, false);
    }

    private static Result Evaluate(int[,] board, PayTable paytable, int betLevel, bool includeWinDetails)
    {
        Result result = new();
        int reels = board.GetLength(0);
        int rows = board.GetLength(1);

        EvaluateTriggers(board, paytable, reels, rows, result);
        EvaluateWays(board, paytable, betLevel, reels, rows, result, includeWinDetails);

        return result;
    }

    private static void EvaluateTriggers(
        int[,] board,
        PayTable paytable,
        int reels,
        int rows,
        Result result)
    {
        for (int col = 0; col < reels; col++)
        {
            for (int row = 0; row < rows; row++)
            {
                int symbolId = board[col, row];
                int position = FindPosition(col, row, reels);

                if (paytable.IsFreeplayTrigger(symbolId))
                {
                    result.FreeplayTriggerCount++;
                    result.FreeplayTriggerPositions.Add(position);
                }

                if (paytable.IsFeatureTrigger(symbolId))
                {
                    result.FeatureTriggerCount++;
                    result.FeatureTriggerPositions.Add(position);
                }
            }
        }

        if (result.FreeplayTriggerCount >= paytable.FreeplayTriggerMinimumCount &&
            paytable.FreeSpinAwards != null &&
            paytable.FreeSpinAwards.Count > 0)
        {
            int awardIndex = result.FreeplayTriggerCount - paytable.FreeplayTriggerMinimumCount;
            if (awardIndex >= paytable.FreeSpinAwards.Count)
                awardIndex = paytable.FreeSpinAwards.Count - 1;

            result.FreeSpinsAwarded = paytable.FreeSpinAwards[awardIndex];
        }

        result.FeatureTriggered =
            result.FeatureTriggerCount >= paytable.FeatureTriggerMinimumCount;
    }

    private static void EvaluateWays(
        int[,] board,
        PayTable paytable,
        int betLevel,
        int reelsCount,
        int rows,
        Result result,
        bool includeWinDetails)
    {
        int[] columnMatches = new int[reelsCount];
        List<int>[] columnPositions = new List<int>[reelsCount];

        foreach (PayTable.Entry entry in paytable.Entries)
        {
            if (entry.IsTrigger)
                continue;

            int consecutive = 0;
            bool hasNaturalSymbol = false;

            for (int col = 0; col < reelsCount; col++)
            {
                int matches = 0;
                List<int> positions = null;

                for (int row = 0; row < rows; row++)
                {
                    int cell = board[col, row];
                    bool isMatch = cell == entry.Id || (!entry.IsWild && paytable.IsWild(cell));

                    if (!isMatch)
                        continue;

                    if (cell == entry.Id)
                        hasNaturalSymbol = true;

                    matches++;
                    if (includeWinDetails)
                    {
                        positions ??= new List<int>();
                        positions.Add(FindPosition(col, row, reelsCount));
                    }
                }

                if (matches == 0)
                    break;

                columnMatches[col] = matches;
                columnPositions[col] = positions;
                consecutive++;
            }

            if (consecutive < 3)
                continue;

            // A board made exclusively from wilds is paid once by the wild's
            // own entry, rather than once again for every regular symbol.
            if (!entry.IsWild && !hasNaturalSymbol)
                continue;

            if (entry.Payouts == null || entry.Payouts.Length < consecutive - 2)
                continue;

            int ways = 1;
            for (int col = 0; col < consecutive; col++)
                ways *= columnMatches[col];

            int payout = ways * entry.Payouts[consecutive - 3] * betLevel;
            if (payout <= 0)
                continue;

            result.TotalWin += payout;

            if (!includeWinDetails)
                continue;

            List<int> winningPositions = new();
            for (int col = 0; col < consecutive; col++)
                winningPositions.AddRange(columnPositions[col]);

            result.WaysWins.Add(new WaysWin
            {
                SymbolId = entry.Id,
                ColumnsMatched = consecutive,
                Ways = ways,
                Payout = payout,
                WinningPositions = winningPositions,
                WinningPaths = BuildWinningPaths(columnPositions, consecutive)
            });

        }
    }

    public static int FindPosition(int col, int row, int reelsCount)
    {
        return row * reelsCount + col;
    }

    private static List<List<int>> BuildWinningPaths(List<int>[] columnPositions, int columnsMatched)
    {
        List<List<int>> paths = new() { new List<int>() };

        for (int col = 0; col < columnsMatched; col++)
        {
            List<List<int>> expanded = new();

            foreach (List<int> path in paths)
            {
                foreach (int position in columnPositions[col])
                {
                    List<int> nextPath = new(path) { position };
                    expanded.Add(nextPath);
                }
            }

            paths = expanded;
        }

        return paths;
    }
}
