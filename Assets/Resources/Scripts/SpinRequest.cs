using System.Collections.Generic;

public class SpinRequest
{
    // Paid amount in cents. Balance and win-pool operations use this value.
    public int Bet;
    // Paytable multiplier: 1 at $0.25 through 8 at $2.00.
    public int BetLevel;

    // When non-null, SlotEngine searches the reels for a board paying this coin
    // amount (or closest). 0 is a valid target (losing spin).
    public int? TargetWin;

    // Optional server/demo ticket override.
    public string PlayTicketOverrideXml;
}

public class SpinResult
{
    public bool IsValid;
    public int Bet;
    public int BetLevel;
    public int CombinationWin;
    public int TotalWin;

    public bool HasFreeplay;
    public int FreeplayTriggerCount;
    public List<int> FreeplayTriggerPositions = new();
    public int FreeSpinsAwarded;

    public bool HasFeature;
    public int FeatureTriggerCount;
    public List<int> FeatureTriggerPositions = new();
    public int FeatureId;
    public int FeatureElementId;

    public string PlayTicketXml;
    public int? DeterministicVisualSeed;
    public List<WaysWin> WaysWins = new();

    // [reel, row]
    public int[,] FinalBoard;
}
