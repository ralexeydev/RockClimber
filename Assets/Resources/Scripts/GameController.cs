using UnityEngine;

// Owns the state machine and serves as the coroutine runner for states
// All game flow logic lives in the individual state classes, not here
public class GameController : BaseController
{
    [Header("References")]
    [SerializeField] private SpinButtonView m_SpinButtonView;
    [SerializeField] private BetButtonView m_BetButtonView;
    [SerializeField] private DemoButtonView m_DemoButtonView;
    [SerializeField] private BetController m_BetController;
    [SerializeField] private BalanceController m_BalanceController;
    [SerializeField] private VisualSequenceController m_VisualSequenceController;
    [SerializeField] private SlotConfigSO m_SlotConfig;
    [SerializeField] private DemoController m_DemoController;
    [SerializeField] private WinPoolController m_WinPoolController;
    [SerializeField] private GameHudView m_HudView;

    private GameStateMachine m_StateMachine;

    public GameState CurrentState => m_StateMachine?.CurrentStateKey ?? GameState.Boot;
    public BalanceController BalanceController => m_BalanceController;
    public BetController BetController => m_BetController;
    public SpinButtonView SpinButtonView => m_SpinButtonView;
    public DemoButtonView DemoButtonView => m_DemoButtonView;
    public DemoController DemoController => m_DemoController;
    public WinPoolController WinPoolController => m_WinPoolController;
    public GameHudView HudView => m_HudView;

    protected override void Initialize()
    {
        m_StateMachine = new GameStateMachine();

        ISlotEngine engine = null;
        string startupError = null;

        if (!SlotConfigurationValidator.TryValidate(m_SlotConfig, out startupError))
        {
            Debug.LogError($"GameController: {startupError}");
        }
        else if (m_VisualSequenceController == null ||
                 !m_VisualSequenceController.TryValidate(out startupError))
        {
            Debug.LogError($"GameController: {startupError}");
        }
        else
        {
            try
            {
                engine = new SlotEngineController(m_SlotConfig);
            }
            catch (System.Exception exception)
            {
                startupError = $"Failed to construct the slot engine: {exception.Message}";
                Debug.LogError($"GameController: {startupError}");
            }
        }

        // Build the context once and share it with all states.
        // States never need to find themselves

        GameContext context = new GameContext(
            coroutineRunner: this,
            stateMachine: m_StateMachine,
            balance: m_BalanceController,
            bet: m_BetController,
            engine: engine,
            visuals: m_VisualSequenceController,
            spinButton: m_SpinButtonView,
            betButton: m_BetButtonView,
            demoButton: m_DemoButtonView,
            demo: m_DemoController,
            winPool: m_WinPoolController,
            hud: m_HudView,
            startupError: startupError
        );

        if (m_HudView != null)
            m_HudView.Bind(m_BalanceController, m_BetController, m_StateMachine);

        // Register every state before starting 
        // TransitionTo will fail on unknown keys

        m_StateMachine.RegisterState(GameState.Boot, new BootState(context));
        m_StateMachine.RegisterState(GameState.Idle, new IdleState(context));
        m_StateMachine.RegisterState(GameState.PreparingSpin, new PreparingSpinState(context));
        m_StateMachine.RegisterState(GameState.RequestingResult, new RequestingResultState(context));
        m_StateMachine.RegisterState(GameState.Spinning, new SpinningState(context));
        m_StateMachine.RegisterState(GameState.PresentingWins, new ShowWinningState(context));
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
    EndingSpin,
    Error
}
