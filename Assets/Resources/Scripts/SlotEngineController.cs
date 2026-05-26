// Plain C# adapter — no MonoBehaviour needed since the engine has no Unity scene dependency.
// GameController instantiates this directly and passes it to GameContext as ISlotEngine.
public class SlotEngineController : ISlotEngine
{
    private readonly SlotEngine m_Engine = new();

    public SpinResult GenerateSpinResult(SpinRequest request)
    {
        return m_Engine.GenerateSpinResult(request);
    }
}
