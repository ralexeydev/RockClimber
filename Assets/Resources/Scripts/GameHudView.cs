using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameHudView : BaseView
{
    [SerializeField] private Canvas m_Canvas;

    private TMP_Text m_BalanceLabel;
    private TMP_Text m_BetLabel;
    private TMP_Text m_WinLabel;
    private TMP_Text m_StateLabel;
    private BalanceController m_Balance;
    private BetController m_Bet;
    private GameStateMachine m_StateMachine;

    public int LastWinCents { get; private set; }

    protected override void Initialize()
    {
        EnsureHud();
    }

    public void Bind(BalanceController balance, BetController bet, GameStateMachine stateMachine)
    {
        Unbind();
        m_Balance = balance;
        m_Bet = bet;
        m_StateMachine = stateMachine;

        if (m_Balance != null)
            m_Balance.BalanceChanged += HandleBalanceChanged;

        if (m_Bet != null)
            m_Bet.BetChanged += HandleBetChanged;

        if (m_StateMachine != null)
            m_StateMachine.StateChanged += HandleStateChanged;

        EnsureHud();
        HandleBalanceChanged(m_Balance != null ? m_Balance.Balance : 0);
        HandleBetChanged(m_Bet != null ? m_Bet.CurrentBet : 0, m_Bet != null ? m_Bet.CurrentBetLevel : 0);
        SetWin(0);
    }

    public void SetWin(int amountCents)
    {
        LastWinCents = amountCents;
        EnsureHud();

        if (m_WinLabel != null)
            m_WinLabel.text = $"WIN  {FormatMoney(amountCents)}";
    }

    public void SetStatus(string status)
    {
        EnsureHud();

        if (m_StateLabel == null)
            return;

        m_StateLabel.color = Color.white;
        m_StateLabel.text = status ?? string.Empty;
    }

    public void ShowError(string message)
    {
        EnsureHud();

        if (m_StateLabel == null)
            return;

        m_StateLabel.color = new Color(1f, 0.35f, 0.35f, 1f);
        m_StateLabel.text = $"ERROR  {message}";
    }

    private void OnDestroy()
    {
        Unbind();
    }

    private void Unbind()
    {
        if (m_Balance != null)
            m_Balance.BalanceChanged -= HandleBalanceChanged;

        if (m_Bet != null)
            m_Bet.BetChanged -= HandleBetChanged;

        if (m_StateMachine != null)
            m_StateMachine.StateChanged -= HandleStateChanged;
    }

    private void HandleBalanceChanged(int balanceCents)
    {
        EnsureHud();

        if (m_BalanceLabel != null)
            m_BalanceLabel.text = $"BALANCE  {FormatMoney(balanceCents)}";
    }

    private void HandleBetChanged(int betCents, int betLevel)
    {
        EnsureHud();

        if (m_BetLabel != null)
            m_BetLabel.text = $"BET  {FormatMoney(betCents)}  LEVEL {betLevel}";
    }

    private void HandleStateChanged(GameState state)
    {
        SetStatus(state.ToString().ToUpperInvariant());
    }

    private void EnsureHud()
    {
        if (m_BalanceLabel != null || m_Canvas == null)
            return;

        GameObject panelObject = new("BaseGameHUD", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panelObject.transform.SetParent(m_Canvas.transform, false);

        RectTransform panel = (RectTransform)panelObject.transform;
        panel.anchorMin = new Vector2(0f, 1f);
        panel.anchorMax = new Vector2(0f, 1f);
        panel.pivot = new Vector2(0f, 1f);
        panel.anchoredPosition = new Vector2(20f, -20f);
        panel.sizeDelta = new Vector2(430f, 150f);

        Image background = panelObject.GetComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.72f);
        background.raycastTarget = false;

        m_BalanceLabel = CreateLabel(panel, "Balance", 10f);
        m_BetLabel = CreateLabel(panel, "Bet", 42f);
        m_WinLabel = CreateLabel(panel, "Win", 74f);
        m_StateLabel = CreateLabel(panel, "State", 108f);
    }

    private static TMP_Text CreateLabel(RectTransform parent, string name, float top)
    {
        GameObject labelObject = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(parent, false);

        RectTransform rect = (RectTransform)labelObject.transform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(14f, -top);
        rect.sizeDelta = new Vector2(-28f, 30f);

        TMP_Text label = labelObject.GetComponent<TMP_Text>();
        label.alignment = TextAlignmentOptions.Left;
        label.color = Color.white;
        label.fontSize = 22f;
        label.raycastTarget = false;
        return label;
    }

    private static string FormatMoney(int amountCents)
    {
        return $"${(amountCents / 100m).ToString("0.00", CultureInfo.InvariantCulture)}";
    }
}
