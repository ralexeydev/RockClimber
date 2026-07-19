using System.Collections.Generic;

public class SpinRequest
{
    public int Bet;

    // When non-null, SlotEngine searches the reels for a board paying this coin
    // amount (or closest). 0 is a valid target (losing spin).
    public int? TargetWin;

    // Optional server/demo ticket override. The engine normally generates this
    // ticket from RNG/win-pool offsets, then this XML can replace it while the
    // rest of the spin pipeline stays identical.
    public string PlayTicketOverrideXml;
}

public class SpinResult
{
    public bool IsValid;
    public int Bet;
    public int CombinationWin;
    public int TotalWin;
    public bool HasFeature;

    public bool HasFreeplay = false;

    public int ScatterCount;
    public int FreeSpinsAwarded;
    public int FeatureId;
    public int BonusId;
    public int FeatureElementId;
    public string PlayTicketXml;
    public List<WaysWin> WaysWins = new();

    // [reel, row]
    public int[,] FinalBoard;
}
