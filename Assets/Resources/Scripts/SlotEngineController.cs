
// GameController constructs this with a SlotConfigSO and passes it to GameContext as ISlotEngine.
public class SlotEngineController : ISlotEngine
{
    private readonly SlotEngine m_Engine;

    public bool IsReady => m_Engine.IsReady;
    public float InitializationProgress => m_Engine.InitializationProgress;

    public SlotEngineController(SlotConfigSO config)
    {
        var strips = config.BaseReels.ToReelStripArray();
        var paytable = config.PayTable.ToPayTable();
        m_Engine = new SlotEngine(strips, paytable);
    }

    public void BeginInitialization()
    {
        m_Engine.BeginInitialization();
    }

    public bool StepInitialization(int candidatesPerBatch)
    {
        return m_Engine.StepInitialization(candidatesPerBatch);
    }

    public bool CanResolveBaseWin(int baseWinCents)
    {
        return m_Engine.CanResolveBaseWin(baseWinCents);
    }

    public SpinResult GenerateSpinResult(SpinRequest request)
    {
        return m_Engine.BuildSpinResult(request);
    }
}
