using System;

// Pure C# engine with no Unity dependency.
public class SlotEngine
{
    private const int Reels = 5;
    private const int Rows = 3;
    private const int MaxBaseWinCents = 5000;

    private readonly Random m_Random = new();
    private readonly ReelStrip[] m_ReelStrips;
    private readonly PayTable m_PayTable;
    private readonly ExactOutcomeCatalog m_OutcomeCatalog;

    public bool IsReady => m_OutcomeCatalog.IsComplete;
    public float InitializationProgress => m_OutcomeCatalog.Progress;

    public SlotEngine(ReelStrip[] reelStrips, PayTable payTable)
    {
        m_ReelStrips = reelStrips;
        m_PayTable = payTable;
        m_OutcomeCatalog = new ExactOutcomeCatalog(reelStrips, payTable, Reels, Rows, MaxBaseWinCents);
    }

    public void BeginInitialization()
    {
        m_OutcomeCatalog.Reset();
    }

    public bool StepInitialization(int candidatesPerBatch)
    {
        return m_OutcomeCatalog.BuildNextBatch(candidatesPerBatch);
    }

    public bool CanResolveBaseWin(int baseWinCents)
    {
        return m_OutcomeCatalog.ContainsBaseWin(baseWinCents);
    }

    public SpinResult BuildSpinResult(SpinRequest request)
    {
        if (!IsReady)
            throw new InvalidOperationException("The exact outcome catalog is not ready.");

        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (request.Mode != GameMode.BaseGame)
            throw new NotSupportedException($"Game mode '{request.Mode}' is not supported.");

        bool hasTicketOverride = !string.IsNullOrWhiteSpace(request.PlayTicketOverrideXml);
        PlayTicket ticket;
        int[] offsets;

        if (hasTicketOverride)
        {
            ticket = PlayTicket.Parse(request.PlayTicketOverrideXml);
            offsets = NormalizeOffsets(ticket.ReelStops);
        }
        else
        {
            if (!request.RequestedBaseWinCents.HasValue)
                throw new InvalidOperationException("A paid BaseGame spin requires an exact base-win request.");

            if (!m_OutcomeCatalog.TryResolveBaseWin(request.RequestedBaseWinCents.Value, m_Random, out offsets))
            {
                throw new InvalidOperationException(
                    $"No BaseGame board exists for {request.RequestedBaseWinCents.Value} base cents.");
            }

            ticket = null;
        }

        int[,] board = BoardBuilder.Build(m_ReelStrips, offsets, Reels, Rows);
        WaysEvaluator.Result eval = WaysEvaluator.Evaluate(board, m_PayTable, request.BetLevel);

        if (hasTicketOverride)
            ValidateTicket(ticket, eval);
        else
            ticket = PlayTicket.FromResult(offsets, eval);

        string ticketXml = hasTicketOverride ? request.PlayTicketOverrideXml : ticket.ToXml();

        return new SpinResult
        {
            IsValid = true,
            Bet = request.Bet,
            BetLevel = request.BetLevel,
            CombinationWin = eval.TotalWin,
            TotalWin = eval.TotalWin,

            HasFreeplay = eval.FreeSpinsAwarded > 0,
            FreeplayTriggerCount = eval.FreeplayTriggerCount,
            FreeplayTriggerPositions = eval.FreeplayTriggerPositions,
            FreeSpinsAwarded = eval.FreeSpinsAwarded,

            HasFeature = eval.FeatureTriggered,
            FeatureTriggerCount = eval.FeatureTriggerCount,
            FeatureTriggerPositions = eval.FeatureTriggerPositions,
            FeatureId = 0,
            FeatureElementId = 0,

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

    private static void ValidateTicket(PlayTicket ticket, WaysEvaluator.Result evaluation)
    {
        if (ticket.FeatureId != 0 || ticket.FeatureElementId != 0 || evaluation.FeatureTriggered)
            throw new NotSupportedException("Bonus tickets are not supported by the BaseGame milestone.");

        if (ticket.CombinationWin != evaluation.TotalWin || ticket.TotalWin != evaluation.TotalWin)
        {
            throw new FormatException(
                $"Ticket payout mismatch. Ticket={ticket.TotalWin}, evaluated={evaluation.TotalWin}.");
        }
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
