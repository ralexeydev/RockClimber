using UnityEngine;

public class BalanceController : BaseController
{
    [SerializeField] private int m_Balance = 10000;

    public int Balance => m_Balance;

    public bool CanPayBet(int bet)
    {
        return m_Balance >= bet;
    }

    public void PayBet(int bet)
    {
        m_Balance -= bet;
        // Guard against floating-point or multi-call edge cases producing a negative balance.
        m_Balance = Mathf.Max(0, m_Balance);
    }

    public void AddWin(int amount)
    {
        // Reject negative amounts so callers cannot accidentally reduce the balance through this method.
        m_Balance += Mathf.Max(0, amount);
    }
}
