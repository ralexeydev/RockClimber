// Builds a [reels, rows] board by reading `rows` consecutive symbols from each
// reel strip starting at the given per-reel offset (with wrap-around). Shared by
// SlotEngine (normal/forced spins) and TargetWinResolver (target-win search).
public static class BoardBuilder
{
    public static int[,] Build(ReelStrip[] strips, int[] offsets, int reels, int rows)
    {
        int[,] board = new int[reels, rows];

        for (int c = 0; c < reels; c++)
        {
            ReelStrip strip = strips[c];
            int offset = offsets[c];

            for (int r = 0; r < rows; r++)
                board[c, r] = strip.GetIndex(offset + r);
        }

        return board;
    }
}
