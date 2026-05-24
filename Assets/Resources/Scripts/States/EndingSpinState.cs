// Cleans up shared spin data and hands control back to the player.
public class EndingSpinState : BaseGameState
{
    public EndingSpinState(GameContext context) : base(context) { }

    public override void Enter()
    {
        // Clear context data so stale results cannot bleed into the next spin cycle.
        Context.CurrentSpinResult = null;
        Context.CurrentBet = 0;
        Context.StateMachine.TransitionTo(GameState.Idle);
    }

    public override void Exit() { }
}
