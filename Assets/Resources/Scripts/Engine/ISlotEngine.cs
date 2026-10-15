// Decouples the spin logic from Unity
public interface ISlotEngine
{
    bool IsReady { get; }
    float InitializationProgress { get; }
    void BeginInitialization();
    bool StepInitialization(int candidatesPerBatch);
    bool CanResolveBaseWin(int baseWinCents);
    SpinResult GenerateSpinResult(SpinRequest request);
}
