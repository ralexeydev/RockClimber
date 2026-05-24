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

        try
        {
            result = Context.Engine.GenerateSpinResult(new SpinRequest { Bet = Context.CurrentBet });
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

        Context.CurrentSpinResult = result;
        Context.StateMachine.TransitionTo(GameState.Spinning);
    }

    public override void Exit() { }
}
