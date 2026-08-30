using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BetButtonView : BaseView
{
    [SerializeField] private Button m_Button;
    [SerializeField] private TMP_Text m_ValueLabel;

    public event Action OnBetClicked;

    protected override void Initialize()
    {
        if (m_Button == null)
            m_Button = GetComponent<Button>();

        EnsureValueLabel();
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

    public void SetBetDisplay(int betCents, int betLevel)
    {
        EnsureValueLabel();
        if (m_ValueLabel == null) return;

        string amount = (betCents / 100m).ToString("0.00", CultureInfo.InvariantCulture);
        m_ValueLabel.text = $"${amount}  LEVEL {betLevel}";
    }

    public override void SetInteractable(bool interactable)
    {
        if (m_Button != null)
            m_Button.interactable = interactable;
    }

    private void HandleClick()
    {
        OnBetClicked?.Invoke();
    }

    private void EnsureValueLabel()
    {
        if (m_ValueLabel != null) return;

        GameObject labelObject = new(
            "BetValue",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(transform, false);

        RectTransform rect = (RectTransform)labelObject.transform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, 47f);
        rect.sizeDelta = new Vector2(230f, 30f);

        m_ValueLabel = labelObject.GetComponent<TMP_Text>();
        m_ValueLabel.alignment = TextAlignmentOptions.Center;
        m_ValueLabel.color = Color.white;
        m_ValueLabel.fontSize = 22f;
        m_ValueLabel.raycastTarget = false;
    }
}
