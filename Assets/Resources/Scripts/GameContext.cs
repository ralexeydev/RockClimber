using UnityEngine;

// Shared service locator passed to every state.
// Avoids states having to search the scene for components.
public class GameContext
{
    // States are plain C# objects and cannot start coroutines themselves.
    // CoroutineRunner (GameController) is the bridge to Unity's coroutine system.
    public MonoBehaviour CoroutineRunner { get; }
    public GameStateMachine StateMachine { get; }

    // Game services - read-only references assigned once at startup.
    public BalanceController Balance { get; }
    public BetController Bet { get; }
    public ISlotEngine Engine { get; }
    public VisualSequenceController Visuals { get; }
    public SpinButtonView SpinButton { get; }
    public BetButtonView BetButton { get; }
    public DemoButtonView DemoButton { get; }
    public DemoController Demo { get; }
    public WinPoolController WinPool { get; }
    public GameHudView Hud { get; }
    public string StartupError { get; }

    // Mutable spin data shared across states within a single spin cycle.
    // Cleared by EndingSpinState when the cycle is complete.
    public SpinResult CurrentSpinResult { get; set; }
    public int CurrentBet { get; set; }
    public int CurrentBetLevel { get; set; }
    public DemoTicket CurrentDemoTicket { get; set; }
    public SpinStartSource CurrentSpinSource { get; set; }
    public bool BetWasDebited { get; private set; }
    public bool WinWasCredited { get; private set; }
    public bool RefundWasIssued { get; private set; }

    public GameContext(
        MonoBehaviour coroutineRunner,
        GameStateMachine stateMachine,
        BalanceController balance,
        BetController bet,
        ISlotEngine engine,
        VisualSequenceController visuals,
        SpinButtonView spinButton,
        BetButtonView betButton,
        DemoButtonView demoButton,
        DemoController demo,
        WinPoolController winPool,
        GameHudView hud,
        string startupError)
    {
        CoroutineRunner = coroutineRunner;
        StateMachine = stateMachine;
        Balance = balance;
        Bet = bet;
        Engine = engine;
        Visuals = visuals;
        SpinButton = spinButton;
        BetButton = betButton;
        DemoButton = demoButton;
        Demo = demo;
        WinPool = winPool;
        Hud = hud;
        StartupError = startupError;
    }

    public bool CanStartSpin()
    {
        return Balance.CanPayBet(Bet.CurrentBet);
    }

    public bool TryDebitBet()
    {
        if (BetWasDebited)
            return false;

        if (!Balance.TryPayBet(CurrentBet))
            return false;

        BetWasDebited = true;
        return true;
    }

    public void CreditWinOnce(int amount)
    {
        if (!BetWasDebited || WinWasCredited || RefundWasIssued)
            return;

        Balance.AddWin(amount);
        WinWasCredited = true;
    }

    public void RefundBetOnce()
    {
        if (!BetWasDebited || WinWasCredited || RefundWasIssued)
            return;

        Balance.AddWin(CurrentBet);
        RefundWasIssued = true;
    }

    public void ResetSpinData()
    {
        CurrentSpinResult = null;
        CurrentDemoTicket = null;
        CurrentBet = 0;
        CurrentBetLevel = 0;
        CurrentSpinSource = SpinStartSource.None;
        BetWasDebited = false;
        WinWasCredited = false;
        RefundWasIssued = false;
    }
}

public enum SpinStartSource
{
    None,
    SpinButton,
    DemoMenu
}
