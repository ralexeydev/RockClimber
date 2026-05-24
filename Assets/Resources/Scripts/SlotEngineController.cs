// Unity adapter for SlotEngine.
// Keeps the engine as a plain C# object while exposing it to the scene via ISlotEngine.
public class SlotEngineController : BaseController, ISlotEngine
{
    private SlotEngine m_Engine;

    protected override void Initialize()
    {
        m_Engine = new SlotEngine();
    }

    // Delegates directly to the pure C# engine instance.
    public SpinResult GenerateSpinResult(SpinRequest request)
    {
        return m_Engine.GenerateSpinResult(request);
    }
}
