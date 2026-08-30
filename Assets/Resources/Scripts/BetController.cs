using UnityEngine;

public class BetController : BaseController
{
    public const int MinBetLevel = 1;
    public const int MaxBetLevel = 8;
    public const int BetStepCents = 25;

    [SerializeField, Range(MinBetLevel, MaxBetLevel)]
    private int m_CurrentBetLevel = MinBetLevel;

    public int CurrentBetLevel => m_CurrentBetLevel;
    public int CurrentBet => m_CurrentBetLevel * BetStepCents;

    public void IncreaseBetLevel()
    {
        m_CurrentBetLevel = m_CurrentBetLevel >= MaxBetLevel
            ? MinBetLevel
            : m_CurrentBetLevel + 1;
    }

    public void SetBetLevel(int betLevel)
    {
        m_CurrentBetLevel = Mathf.Clamp(betLevel, MinBetLevel, MaxBetLevel);
    }

    // Compatibility entry point for callers that still express the wager in cents.
    public void SetBet(int bet)
    {
        int level = Mathf.RoundToInt(bet / (float)BetStepCents);
        SetBetLevel(level);
    }

    private void OnValidate()
    {
        m_CurrentBetLevel = Mathf.Clamp(m_CurrentBetLevel, MinBetLevel, MaxBetLevel);
    }
}
