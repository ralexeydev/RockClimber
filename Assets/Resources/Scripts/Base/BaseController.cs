using UnityEngine;

// Base class for all game controllers.
// Subclasses override Initialize instead of Awake to keep Unity lifecycle centralized here.
public abstract class BaseController : MonoBehaviour
{
    protected virtual void Awake() => Initialize();
    protected virtual void Initialize() { }
}
