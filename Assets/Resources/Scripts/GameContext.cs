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

    // Mutable spin data shared across states within a single spin cycle.
    // Cleared by EndingSpinState when the cycle is complete.
    public SpinResult CurrentSpinResult { get; set; }
    public int CurrentBet { get; set; }

    public GameContext(
        MonoBehaviour coroutineRunner,
        GameStateMachine stateMachine,
        BalanceController balance,
        BetController bet,
        ISlotEngine engine,
        VisualSequenceController visuals,
        SpinButtonView spinButton)
    {
        CoroutineRunner = coroutineRunner;
        StateMachine = stateMachine;
        Balance = balance;
        Bet = bet;
        Engine = engine;
        Visuals = visuals;
        SpinButton = spinButton;
    }
}
