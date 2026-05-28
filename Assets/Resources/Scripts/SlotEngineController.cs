
// GameController constructs this with a SlotConfigSO and passes it to GameContext as ISlotEngine.
public class SlotEngineController : ISlotEngine
{
    private readonly SlotEngine m_Engine;

    public SlotEngineController(SlotConfigSO config)
    {
        var strips = config.BaseReels.ToReelStripArray();
        var paytable = config.PayTable.ToPayTable();
        m_Engine = new SlotEngine(strips, paytable);
    }

    public SpinResult GenerateSpinResult(SpinRequest request)
    {
        return m_Engine.GenerateSpinResult(request);
    }
}
