using UnityEngine;

// Base class for all view components.
// Provides common UI operations that subclasses can override as needed.
public abstract class BaseView : MonoBehaviour
{
    protected virtual void Awake() => Initialize();
    protected virtual void Initialize() { }

    // Show and Hide toggle the GameObject itself, suitable for overlay-style views.
    public virtual void Show() => gameObject.SetActive(true);
    public virtual void Hide() => gameObject.SetActive(false);

    // Override in subclasses that have interactive UI elements like buttons.
    public virtual void SetInteractable(bool interactable) { }
}
