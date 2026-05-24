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

    // [reel, row]
    public int[,] FinalBoard;
}