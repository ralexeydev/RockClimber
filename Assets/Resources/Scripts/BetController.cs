using UnityEngine;

public class BetController : BaseController
{
    [SerializeField] private int m_CurrentBet = 100;

    public int CurrentBet => m_CurrentBet;

    public void SetBet(int bet)
    {
        // Minimum bet of 1 to prevent a zero-cost spin.
        m_CurrentBet = Mathf.Max(1, bet);
    }
}
