using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

// Owns the loaded demo tickets and the pending-ticket handoff to the state machine.
// Picker UI calls QueueTicket; IdleState observes OnTicketQueued and starts the spin;
// RequestingResultState consumes ConsumePending() and passes the PLAY override to the engine.
public class DemoController : BaseController
{
    public event Action OnTicketQueued;

    public IReadOnlyList<DemoTicket> Tickets => m_Tickets;
    public DemoTicket PendingTicket => m_Pending;

    private List<DemoTicket> m_Tickets = new();
    private DemoTicket m_Pending;

    protected override void Initialize()
    {
        string dir = Path.Combine(Application.streamingAssetsPath, "DemoTickets");
        string demosFile = Path.Combine(dir, "demos.xml");
        m_Tickets = DemoTicketLoader.LoadAll(demosFile);
        Debug.Log($"DemoController: loaded {m_Tickets.Count} demo tickets from {demosFile}");
    }

    public void QueueTicket(DemoTicket ticket)
    {
        if (ticket == null) return;
        m_Pending = ticket;
        OnTicketQueued?.Invoke();
    }

    public DemoTicket ConsumePending()
    {
        var t = m_Pending;
        m_Pending = null;
        return t;
    }

    // Non-fatal drift check: PLAY is authoritative, but the board from RS should
    // still evaluate to the same combination win for coherent win visuals.
    public void AssertResult(DemoTicket ticket, SpinResult result)
    {
        if (ticket == null || result == null) return;

        if (!string.IsNullOrWhiteSpace(ticket.PlayXml))
        {
            PlayTicket play = PlayTicket.Parse(ticket.PlayXml);
            int evaluatedCombinationWin = result.WaysWins?.Sum(w => w.Payout) ?? 0;

            if (evaluatedCombinationWin != play.CombinationWin)
            {
                Debug.LogError(
                    $"DemoController: ticket '{ticket.Id}' sWin mismatch - " +
                    $"ticket has {play.CombinationWin}, evaluator produced {evaluatedCombinationWin}");
            }

            if (result.TotalWin != play.TotalWin)
            {
                Debug.LogError(
                    $"DemoController: ticket '{ticket.Id}' total-win mismatch - " +
                    $"ticket has {play.TotalWin}, result has {result.TotalWin}");
            }

            return;
        }

        if (result.TotalWin != ticket.ExpectedTotalWin)
        {
            Debug.LogError(
                $"DemoController: ticket '{ticket.Id}' total-win mismatch - " +
                $"expected {ticket.ExpectedTotalWin}, engine produced {result.TotalWin}");
        }

        int expectedCount = ticket.ExpectedWaysWins?.Length ?? 0;
        int actualCount = result.WaysWins?.Count ?? 0;
        if (expectedCount != actualCount)
        {
            Debug.LogError(
                $"DemoController: ticket '{ticket.Id}' ways-wins count mismatch - " +
                $"expected {expectedCount}, engine produced {actualCount}");
            return;
        }

        if (expectedCount == 0) return;

        var remaining = new List<WaysWin>(result.WaysWins);
        foreach (var exp in ticket.ExpectedWaysWins)
        {
            int idx = remaining.FindIndex(w =>
                w.SymbolId == exp.SymbolId &&
                w.ColumnsMatched == exp.ColumnsMatched &&
                w.Ways == exp.Ways &&
                w.Payout == exp.Payout);

            if (idx < 0)
            {
                Debug.LogError(
                    $"DemoController: ticket '{ticket.Id}' missing expected win - " +
                    $"symbol={exp.SymbolId} cols={exp.ColumnsMatched} ways={exp.Ways} payout={exp.Payout}");
                return;
            }

            remaining.RemoveAt(idx);
        }
    }
}
