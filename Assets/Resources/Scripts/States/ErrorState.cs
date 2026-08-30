using UnityEngine;

// Handles an invalid spin result. Paid spins are refunded; demos never paid a bet.
public class ErrorState : BaseGameState
{
    public ErrorState(GameContext context) : base(context) { }

    public override void Enter()
    {
        Debug.LogError("ErrorState: Invalid spin result.");
        if (Context.CurrentSpinSource != SpinStartSource.DemoMenu)
            Context.Balance.AddWin(Context.CurrentBet);
        Context.StateMachine.TransitionTo(GameState.EndingSpin);
    }

    public override void Exit() { }
}
