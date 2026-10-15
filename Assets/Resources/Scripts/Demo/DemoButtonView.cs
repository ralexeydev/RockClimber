using UnityEngine;
using UnityEngine.UI;

// Button that toggles the demo ticket picker panel. Mirrors SpinButtonView's lifecycle.
public class DemoButtonView : BaseView
{
    [SerializeField] private Button m_Button;
    [SerializeField] private DemoPickerView m_Picker;

    public bool IsInteractable => m_Button != null && m_Button.interactable;

    protected override void Initialize()
    {
        if (m_Button == null)
            m_Button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        if (m_Button != null)
            m_Button.onClick.AddListener(HandleClick);
    }

    private void OnDisable()
    {
        if (m_Button != null)
            m_Button.onClick.RemoveListener(HandleClick);
    }

    public override void SetInteractable(bool interactable)
    {
        if (m_Button != null)
            m_Button.interactable = interactable;

        if (!interactable)
            ClosePicker();
    }

    public void ClosePicker()
    {
        if (m_Picker != null && m_Picker.gameObject.activeSelf)
            m_Picker.Hide();
    }

    private void HandleClick()
    {
        if (m_Picker == null)
            return;

        if (m_Picker.gameObject.activeSelf)
            m_Picker.Hide();
        else
            m_Picker.Show();
    }
}
