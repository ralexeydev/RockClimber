using System;
using UnityEngine;
using UnityEngine.UI;

public class SpinButtonView : BaseView
{
    [SerializeField] private Button m_Button;

    public event Action OnSpinClicked;

    protected override void Initialize()
    {
        // Fall back to a sibling Button component if none is assigned in the Inspector.
        if (m_Button == null)
        {
            m_Button = GetComponent<Button>();
        }
    }

    // Register and unregister the click listener with the GameObject's active state
    // so clicks are not received while the object is inactive.
    private void OnEnable()
    {
        if (m_Button != null)
        {
            m_Button.onClick.AddListener(HandleClick);
        }
    }

    private void OnDisable()
    {
        if (m_Button != null)
        {
            m_Button.onClick.RemoveListener(HandleClick);
        }
    }

    public override void SetInteractable(bool interactable)
    {
        if (m_Button != null)
        {
            m_Button.interactable = interactable;
        }
    }

    private void HandleClick()
    {
        OnSpinClicked?.Invoke();
    }
}
