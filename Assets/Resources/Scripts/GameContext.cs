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
    public DemoController Demo { get; }
    public WinPoolController WinPool { get; }

    // Mutable spin data shared across states within a single spin cycle.
    // Cleared by EndingSpinState when the cycle is complete.
    public SpinResult CurrentSpinResult { get; set; }
    public int CurrentBet { get; set; }
    public int CurrentBetLevel { get; set; }
    public DemoTicket CurrentDemoTicket { get; set; }
    public SpinStartSource CurrentSpinSource { get; set; }

    public GameContext(
        MonoBehaviour coroutineRunner,
        GameStateMachine stateMachine,
        BalanceController balance,
        BetController bet,
        ISlotEngine engine,
        VisualSequenceController visuals,
        SpinButtonView spinButton,
        BetButtonView betButton,
        DemoController demo,
        WinPoolController winPool)
    {
        CoroutineRunner = coroutineRunner;
        StateMachine = stateMachine;
        Balance = balance;
        Bet = bet;
        Engine = engine;
        Visuals = visuals;
        SpinButton = spinButton;
        BetButton = betButton;
        Demo = demo;
        WinPool = winPool;
    }

    public bool CanStartSpin()
    {
        return Balance.CanPayBet(Bet.CurrentBet);
    }
}

public enum SpinStartSource
{
    None,
    SpinButton,
    DemoMenu
}
