using System;
using UnityEngine;

// Asks the engine for a spin result. On any failure the machine transitions to Error,
// which will refund the bet that was already deducted in PreparingSpinState.
public class RequestingResultState : BaseGameState
{
    public RequestingResultState(GameContext context) : base(context) { }

    public override void Enter()
    {
        SpinResult result = null;

        int currentBet = Context.CurrentBet;
        var request = new SpinRequest
        {
            Bet = currentBet,
            BetLevel = Context.CurrentBetLevel,
            Mode = GameMode.BaseGame
        };

        bool isDemoSpin = Context.CurrentSpinSource == SpinStartSource.DemoMenu;
        DemoTicket demoTicket = isDemoSpin && Context.Demo != null
            ? Context.Demo.ConsumePending()
            : null;

        if (demoTicket != null)
        {
            request.PlayTicketOverrideXml = demoTicket.PlayXml;
            Context.CurrentDemoTicket = demoTicket;
        }
        else
        {
            Context.CurrentDemoTicket = null;
        }

        // Paid spins draw from the win pool when it is active. If a demo ticket
        // is present, it replaces the generated PLAY ticket inside the engine.
        int? target = demoTicket == null
            ? Context.WinPool.DrawBaseWinCents()
            : null;
        if (target.HasValue)
        {
            request.RequestedBaseWinCents = target.Value;
            if (Context.WinPool.ForcedWin)
            {
                Debug.Log($"RequestingResultState: forced base win {target.Value} cents");
            }
            else
            {
                Debug.Log($"RequestingResultState: base-win target {target.Value}, level {Context.CurrentBetLevel}");
            }
        }
        else if (demoTicket == null)
        {
            Debug.LogError("RequestingResultState: Win pool did not provide a BaseGame target.");
            Context.StateMachine.TransitionTo(GameState.Error);
            return;
        }

        try
        {
            result = Context.Engine.GenerateSpinResult(request);
        }
        catch (Exception e)
        {
            Debug.LogError($"RequestingResultState: Engine error — {e}");
        }

        // Treat an engine exception the same as an invalid result.
        if (result == null || !result.IsValid)
        {
            Context.StateMachine.TransitionTo(GameState.Error);
            return;
        }

        if (demoTicket != null)
        {
            Context.Demo.AssertResult(demoTicket, result);
        }

        Context.CurrentSpinResult = result;
        Context.Hud.SetWin(result.TotalWin);
        Context.StateMachine.TransitionTo(GameState.Spinning);
    }

    public override void Exit() { }
}
