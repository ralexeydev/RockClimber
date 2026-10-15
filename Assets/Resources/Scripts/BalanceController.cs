using System;
using UnityEngine;

public class BalanceController : BaseController
{
    [SerializeField] private int m_Balance = 10000;

    public int Balance => m_Balance;
    public event Action<int> BalanceChanged;

    public bool CanPayBet(int bet)
    {
        return m_Balance >= bet;
    }

    public bool TryPayBet(int bet)
    {
        if (bet <= 0 || !CanPayBet(bet))
            return false;

        m_Balance -= bet;
        m_Balance = Mathf.Max(0, m_Balance);
        BalanceChanged?.Invoke(m_Balance);
        return true;
    }

    public void AddWin(int amount)
    {
        m_Balance += Mathf.Max(0, amount);
        BalanceChanged?.Invoke(m_Balance);
    }
}
