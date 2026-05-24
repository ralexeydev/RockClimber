using UnityEngine;

// Handles an invalid spin result. Refunds the bet that was deducted in PreparingSpinState
// then transitions to EndingSpin to restore the player to Idle cleanly.
public class ErrorState : BaseGameState
{
    public ErrorState(GameContext context) : base(context) { }

    public override void Enter()
    {
        Debug.LogError("ErrorState: Invalid spin result. Refunding bet.");
        Context.Balance.AddWin(Context.CurrentBet);
        Context.StateMachine.TransitionTo(GameState.EndingSpin);
    }

    public override void Exit() { }
}
