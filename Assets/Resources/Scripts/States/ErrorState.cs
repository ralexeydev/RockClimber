using System.Collections;
using UnityEngine;

// Handles an invalid spin result. Paid spins are refunded; demos never paid a bet.
public class ErrorState : BaseGameState
{
    public ErrorState(GameContext context) : base(context) { }

    public override void Enter()
    {
        Debug.LogError("ErrorState: Invalid spin result.");
        Context.RefundBetOnce();
        Context.Hud.ShowError("Spin failed. Any paid bet was refunded.");
        Context.CoroutineRunner.StartCoroutine(ReturnToIdle());
    }

    public override void Exit() { }

    private IEnumerator ReturnToIdle()
    {
        yield return new WaitForSeconds(1.5f);
        Context.StateMachine.TransitionTo(GameState.EndingSpin);
    }
}
