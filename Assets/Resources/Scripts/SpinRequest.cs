using System.Collections.Generic;

public class SpinRequest
{
    public int Bet;
}

public class SpinResult
{
    public bool IsValid;
    public int Bet;
    public int TotalWin;
    public bool HasFeature;

    public bool HasFreeplay = false; 

    public int ScatterCount;
    public int FreeSpinsAwarded;
    public List<WaysWin> WaysWins = new();

    // [reel, row]
    public int[,] FinalBoard;
}
