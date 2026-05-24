// Decouples the spin logic from Unity. Implementations can be pure C# with no scene dependency.
public interface ISlotEngine
{
    SpinResult GenerateSpinResult(SpinRequest request);
}
