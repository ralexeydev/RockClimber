using System;

// Pure C# engine with no Unity dependency.
// Uses System.Random so it can run and be tested outside the Unity runtime.
public class SlotEngine : ISlotEngine
{
    private readonly Random m_Random = new Random();

    public SpinResult GenerateSpinResult(SpinRequest request)
    {
        // Fill the 5x3 board with random symbol indices.
        int[,] finalBoard = new int[5, 3];

        for (int reel = 0; reel < 5; reel++)
        {
            for (int row = 0; row < 3; row++)
            {
                finalBoard[reel, row] = m_Random.Next(1, 10);
            }
        }

        return new SpinResult
        {
            IsValid = true,
            Bet = request.Bet,
            // Placeholder: 20% win chance paying 5x, 10% feature trigger.
            // Replace with a real paytable and RNG when integrating the live system.
            TotalWin = m_Random.Next(0, 5) == 0 ? request.Bet * 5 : 0,
            HasFeature = m_Random.Next(0, 10) == 0,
            FinalBoard = finalBoard
        };
    }
}
