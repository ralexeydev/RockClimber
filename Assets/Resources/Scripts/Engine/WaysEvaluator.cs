using System.Collections.Generic;

// One winning combination produced by ways-pay evaluation.
public class WaysWin
{
    public int SymbolId;
    public int ColumnsMatched; // 3, 4, or 5
    public int Ways;           // product of matching cells across the columns
    public int Payout;         // already includes ways * bet multipliers
}

// Ways-pay evaluator: a symbol wins when it (or a wild substitute) appears in the
// leftmost N consecutive reels, with N >= 3. The number of "ways" equals the
// product of matching cells per column — e.g. a board with 2 cells in reel 0,
// 1 in reel 1, and 3 in reel 2 yields 2 * 1 * 3 = 6 ways.
//
// Wild substitutes for every non-scatter symbol and also has its own line.
// Scatter is evaluated separately: pays anywhere on the board and awards free
// spins based on how many landed.

//TODO: make ways individually for animating when multiple ways wins occur on the same spin
public static class WaysEvaluator
{
    public class Result
    {
        public List<WaysWin> WaysWins = new();
        public int TotalWin;
        public int ScatterCount;
        public int FreeSpinsAwarded;
    }

    public static Result Evaluate(int[,] board, PayTable paytable, int bet)
    {
        Result result = new Result();
        int reels = board.GetLength(0);
        int rows = board.GetLength(1);

        EvaluateScatter(board, paytable, bet, reels, rows, result);
        EvaluateWays(board, paytable, bet, reels, rows, result);

        return result;
    }

    private static void EvaluateScatter(int[,] board, PayTable paytable, int bet, int reels, int rows, Result result)
    {
        for (int c = 0; c < reels; c++)
            for (int r = 0; r < rows; r++)
                if (paytable.IsScatter(board[c, r]))
                    result.ScatterCount++;

        if (result.ScatterCount < 3) return;

        PayTable.Entry scatter = null;

        foreach (var e in paytable.Entries)
        {
            if (e.IsScatter)
            {
                scatter = e;
                break;
            }
        }

        if (scatter != null && scatter.Payouts != null && scatter.Payouts.Length > 0)
        {
            int idx = result.ScatterCount - 3;
            if (idx < 0) idx = 0;
            if (idx >= scatter.Payouts.Length) idx = scatter.Payouts.Length - 1;

            int payout = scatter.Payouts[idx] * bet;
            result.TotalWin += payout;
            result.WaysWins.Add(new WaysWin
            {
                SymbolId = scatter.Id,
                ColumnsMatched = result.ScatterCount,
                Ways = 1,
                Payout = payout
            });
        }

        // Map scatter count to free spins via the paytable's FreeSpinAwards table:
        // index 0 = 3 scatters, 1 = 4, last index = 5+ (clamped).
        if (paytable.FreeSpinAwards != null && paytable.FreeSpinAwards.Count > 0)
        {
            int awardIdx = result.ScatterCount - 3;
            if (awardIdx < 0) awardIdx = 0;
            if (awardIdx >= paytable.FreeSpinAwards.Count) awardIdx = paytable.FreeSpinAwards.Count - 1;
            result.FreeSpinsAwarded = paytable.FreeSpinAwards[awardIdx];
        }
    }

    private static void EvaluateWays(int[,] board, PayTable paytable, int bet, int reels, int rows, Result result)
    {
        int[] columnMatches = new int[reels];

        foreach (var entry in paytable.Entries)
        {
            if (entry.IsScatter) continue;

            int consecutive = 0;

            for (int col = 0; col < reels; col++)
            {
                int matches = 0;
                for (int row = 0; row < rows; row++)
                {
                    int cell = board[col, row];
                    if (cell == entry.Id)
                        matches++;
                    else if (!entry.IsWild && paytable.IsWild(cell))
                        matches++;
                }

                if (matches == 0)
                    break;

                columnMatches[col] = matches;
                consecutive++;
            }

            if (consecutive < 3)
                continue;

            if (entry.Payouts == null || entry.Payouts.Length < consecutive - 2) continue;

            int ways = 1;
            for (int c = 0; c < consecutive; c++)
                ways *= columnMatches[c];

            int payout = ways * entry.Payouts[consecutive - 3] * bet;

            if (payout <= 0)
                continue;

            result.WaysWins.Add(new WaysWin
            {
                SymbolId = entry.Id,
                ColumnsMatched = consecutive,
                Ways = ways,
                Payout = payout
            });

            result.TotalWin += payout;
        }
    }
}
