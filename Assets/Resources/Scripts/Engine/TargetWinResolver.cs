using System;

// Given a target coin amount, searches the reel strips for a set of per-reel
// offsets whose resulting board pays as close as possible to that target.
// Trigger symbols do not contribute to the compared win amount.
public static class TargetWinResolver
{
    public const int DefaultMaxAttempts = 4000;

    public static int[] Resolve(
        ReelStrip[] strips,
        PayTable paytable,
        int betLevel,
        int target,
        int reels,
        int rows,
        Random rng,
        int maxAttempts = DefaultMaxAttempts)
    {
        int[] best = null;
        int bestDiff = int.MaxValue;
        int[] candidate = new int[reels];

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            for (int col = 0; col < reels; col++)
                candidate[col] = rng.Next(0, strips[col].Length);

            int[,] board = BoardBuilder.Build(strips, candidate, reels, rows);
            int win = WaysEvaluator.Evaluate(board, paytable, betLevel).TotalWin;

            int difference = Math.Abs(win - target);
            if (difference >= bestDiff) continue;

            bestDiff = difference;
            best = (int[])candidate.Clone();

            if (difference == 0)
                return best;
        }

        return best;
    }
}
