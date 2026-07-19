using UnityEngine;
using UnityEngine.UI;

// Panel that lists every loaded demo ticket as a button. Clicking a row queues
// the ticket on DemoController and hides the panel — IdleState reacts to the
// queue event and starts the spin.
public class DemoPickerView : BaseView
{
    [Header("References")]
    [SerializeField] private DemoController m_Demo;
    [SerializeField] private RectTransform m_Content;
    [SerializeField] private Button m_RowTemplate;

    private bool m_Populated;

    protected override void Initialize()
    {
        if (m_RowTemplate != null)
            m_RowTemplate.gameObject.SetActive(false);

        Hide();
    }

    public override void Show()
    {
        PopulateIfNeeded();
        m_Content.gameObject.SetActive(true);
        base.Show();
    }

    private void PopulateIfNeeded()
    {
        if (m_Populated) return;
        if (m_Demo == null || m_Content == null || m_RowTemplate == null) return;

        foreach (var ticket in m_Demo.Tickets)
        {
            var row = Instantiate(m_RowTemplate, m_Content);
            row.gameObject.SetActive(true);

            string labelText = $"{ticket.Id}  {ticket.Description}";

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
