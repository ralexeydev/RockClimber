using System;

// Pure C# engine with no Unity dependency.
// Symbol generation reads from authored ReelStrips: per spin, each reel picks a
// random offset and reads N consecutive symbols (with wrap-around) — the standard
// physical-reel model. Distribution is controlled by the strips themselves, not
// per-cell weights.
public class SlotEngine : ISlotEngine
{
    private const int Reels = 5;
    private const int Rows = 3;

    private readonly Random m_Random = new();
    private readonly ReelStrip[] m_ReelStrips;
    private readonly PayTable m_PayTable;

    public SlotEngine(ReelStrip[] reelStrips, PayTable payTable)
    {
        m_ReelStrips = reelStrips;
        m_PayTable = payTable;
    }

    public SpinResult GenerateSpinResult(SpinRequest request)
    {
        int[,] board = new int[Reels, Rows];
        for (int c = 0; c < Reels; c++)
        {
            ReelStrip strip = m_ReelStrips[c];
            int offset = m_Random.Next(0, strip.Length);

            for (int r = 0; r < Rows; r++)
                board[c, r] = strip.Get(offset + r);
        }

        var eval = WaysEvaluator.Evaluate(board, m_PayTable, request.Bet);

        return new SpinResult
        {
            IsValid = true,
            Bet = request.Bet,
            TotalWin = eval.TotalWin,
            HasFeature = eval.FreeSpinsAwarded > 0,
            ScatterCount = eval.ScatterCount,
            FreeSpinsAwarded = eval.FreeSpinsAwarded,
            WaysWins = eval.WaysWins,
            FinalBoard = board
        };
    }
}
