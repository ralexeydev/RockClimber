using UnityEngine;
using UnityEngine.UI;

// Panel that lists every loaded demo ticket as a button. Clicking a row queues
// the ticket on DemoController and hides the panel — IdleState reacts to the
// queue event and starts the spin.
public class DemoPickerView : BaseView
{
    private const float RowSpacing = 8f;

    [Header("References")]
    [SerializeField] private DemoController m_Demo;
    [SerializeField] private RectTransform m_Content;
    [SerializeField] private Button m_RowTemplate;

    private bool m_Initialized;
    private bool m_Populated;

    protected override void Initialize()
    {
        if (m_Initialized) return;
        m_Initialized = true;

        if (m_RowTemplate != null)
            m_RowTemplate.gameObject.SetActive(false);
    }

    public override void Show()
    {
        // Picker starts inactive in the scene, so Awake has not necessarily run
        // when DemoButtonView calls Show for the first time.
        Initialize();
        base.Show();
        PopulateIfNeeded();
    }

    private void PopulateIfNeeded()
    {
        if (m_Populated) return;
        if (m_Demo == null || m_Content == null || m_RowTemplate == null) return;

        RectTransform templateTransform = m_RowTemplate.transform as RectTransform;
        bool usesLayoutGroup = m_Content.GetComponent<LayoutGroup>() != null;
        int rowIndex = 0;

        foreach (var ticket in m_Demo.Tickets)
        {
            var row = Instantiate(m_RowTemplate, m_Content);

            if (!usesLayoutGroup && templateTransform != null &&
                row.transform is RectTransform rowTransform)
            {
                float rowHeight = templateTransform.rect.height;
                if (rowHeight <= 0f)
                    rowHeight = Mathf.Abs(templateTransform.sizeDelta.y);

                rowTransform.anchoredPosition = templateTransform.anchoredPosition +
                    Vector2.down * (rowHeight + RowSpacing) * rowIndex;
            }

            row.gameObject.SetActive(true);

            string labelText = ticket.DisplayName;

            var label = row.GetComponentInChildren<Text>(true);
            if (label == null) label = row.GetComponent<Text>();

            if (label != null)
            {
                label.text = labelText;
            }
            else
            {
                var tmp = row.GetComponentInChildren<TMPro.TMP_Text>(true);
                if (tmp == null) tmp = row.GetComponent<TMPro.TMP_Text>();
                if (tmp != null)
                    tmp.text = labelText;
            }

            var captured = ticket;
            row.onClick.AddListener(() => HandleRowClicked(captured));
            rowIndex++;
        }

        m_Populated = true;
    }

    private void HandleRowClicked(DemoTicket ticket)
    {
        Hide();
        if (m_Demo != null)
            m_Demo.QueueTicket(ticket);
    }
}
