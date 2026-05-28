using UnityEngine;

// Owns the state machine and serves as the coroutine runner for states
// All game flow logic lives in the individual state classes, not here
public class GameController : BaseController
{
    [Header("References")]
    [SerializeField] private SpinButtonView m_SpinButtonView;
    [SerializeField] private BetController m_BetController;
    [SerializeField] private BalanceController m_BalanceController;
    [SerializeField] private VisualSequenceController m_VisualSequenceController;
    [SerializeField] private SlotConfigSO m_SlotConfig;

    private GameStateMachine m_StateMachine;

    public GameState CurrentState => m_StateMachine?.CurrentStateKey ?? GameState.Boot;

    protected override void Initialize()
    {
        m_StateMachine = new GameStateMachine();

        //Built from the SlotConfigSO assigned in the Inspector
        ISlotEngine engine = new SlotEngineController(m_SlotConfig);

        // Build the context once and share it with all states.
        // States never need to find themselves

        GameContext context = new GameContext(
            coroutineRunner: this,
            stateMachine: m_StateMachine,
            balance: m_BalanceController,
            bet: m_BetController,
            engine: engine,
            visuals: m_VisualSequenceController,
            spinButton: m_SpinButtonView
        );

        // Register every state before starting 
        // TransitionTo will fail on unknown keys

        m_StateMachine.RegisterState(GameState.Boot, new BootState(context));
        m_StateMachine.RegisterState(GameState.Idle, new IdleState(context));
        m_StateMachine.RegisterState(GameState.PreparingSpin, new PreparingSpinState(context));
        m_StateMachine.RegisterState(GameState.RequestingResult, new RequestingResultState(context));
        m_StateMachine.RegisterState(GameState.Spinning, new SpinningState(context));
        m_StateMachine.RegisterState(GameState.PresentingWins, new PresentingWinsState(context));
        m_StateMachine.RegisterState(GameState.Feature, new FeatureState(context));
        m_StateMachine.RegisterState(GameState.EndingSpin, new EndingSpinState(context));
        m_StateMachine.RegisterState(GameState.Error, new ErrorState(context));

        m_StateMachine.TransitionTo(GameState.Boot);
    }

    private void Update()
    {
        m_StateMachine?.Tick();
    }
}

public enum GameState
{
    Boot,
    Idle,
    PreparingSpin,
    RequestingResult,
    Spinning,
    PresentingWins,
    Feature,
    EndingSpin,
    Error
}
