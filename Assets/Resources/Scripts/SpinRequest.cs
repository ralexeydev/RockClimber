using System.Collections.Generic;

public enum GameMode
{
    BaseGame
}

public class SpinRequest
{
    public int Bet;
    public int BetLevel;
    public GameMode Mode = GameMode.BaseGame;
    public int? RequestedBaseWinCents;
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
