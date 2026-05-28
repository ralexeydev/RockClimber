// Decouples the spin logic from Unity
public interface ISlotEngine
{
    SpinResult GenerateSpinResult(SpinRequest request);
}
