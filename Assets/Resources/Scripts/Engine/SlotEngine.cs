using System;

// Pure C# engine with no Unity dependency.
public class SlotEngine
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

    public SpinResult BuildSpinResult(SpinRequest request)
    {
        bool hasTicketOverride = !string.IsNullOrWhiteSpace(request.PlayTicketOverrideXml);
        string ticketXml;

        if (hasTicketOverride)
        {
            // A demo/server ticket is authoritative: do not consume RNG or run
            // target-win resolution before applying it.
            ticketXml = request.PlayTicketOverrideXml;
        }
        else
        {
            int[] offsets = ResolveOffsets(request);
            int[,] generatedBoard = BoardBuilder.Build(m_ReelStrips, offsets, Reels, Rows);
            WaysEvaluator.Result generatedEval = WaysEvaluator.Evaluate(
                generatedBoard, m_PayTable, request.BetLevel);
            ticketXml = PlayTicket.FromResult(offsets, generatedEval).ToXml();
        }

        PlayTicket ticket = PlayTicket.Parse(ticketXml);
        int[] ticketOffsets = NormalizeOffsets(ticket.ReelStops);
        int[,] board = BoardBuilder.Build(m_ReelStrips, ticketOffsets, Reels, Rows);
        WaysEvaluator.Result eval = WaysEvaluator.Evaluate(board, m_PayTable, request.BetLevel);

        int featureId = ticket.FeatureId != 0
            ? ticket.FeatureId
            : eval.FeatureTriggered ? 1 : 0;

        return new SpinResult
        {
            IsValid = true,
            Bet = request.Bet,
            BetLevel = request.BetLevel,
            CombinationWin = ticket.CombinationWin,
            TotalWin = ticket.TotalWin,

            HasFreeplay = eval.FreeSpinsAwarded > 0,
            FreeplayTriggerCount = eval.FreeplayTriggerCount,
            FreeplayTriggerPositions = eval.FreeplayTriggerPositions,
            FreeSpinsAwarded = eval.FreeSpinsAwarded,

            HasFeature = featureId != 0,
            FeatureTriggerCount = eval.FeatureTriggerCount,
            FeatureTriggerPositions = eval.FeatureTriggerPositions,
            FeatureId = featureId,
            FeatureElementId = ticket.FeatureElementId,

            PlayTicketXml = ticketXml,
            DeterministicVisualSeed = hasTicketOverride
                ? CalculateStableSeed(ticket.ToXml())
                : null,
            WaysWins = eval.WaysWins,
            FinalBoard = board
        };
    }

    private static int CalculateStableSeed(string value)
    {
        unchecked
        {
            uint hash = 2166136261;
            foreach (char character in value)
            {
                hash ^= character;
                hash *= 16777619;
            }

            return (int)(hash & 0x7fffffff);
        }
    }

    private int[] ResolveOffsets(SpinRequest request)
    {
        if (request.TargetWin.HasValue)
        {
            return TargetWinResolver.Resolve(
                m_ReelStrips, m_PayTable, request.BetLevel, request.TargetWin.Value,
                Reels, Rows, m_Random);
        }

        int[] offsets = new int[Reels];
        for (int col = 0; col < Reels; col++)
            offsets[col] = m_Random.Next(0, m_ReelStrips[col].Length);
        return offsets;
    }

    private int[] NormalizeOffsets(int[] reelStops)
    {
        if (reelStops == null || reelStops.Length != Reels)
            throw new ArgumentException($"Play ticket must contain {Reels} reel stops.");

        int[] normalized = new int[Reels];
        for (int col = 0; col < Reels; col++)
        {
            int length = m_ReelStrips[col].Length;
            normalized[col] = ((reelStops[col] % length) + length) % length;
        }
        return normalized;
    }
}
