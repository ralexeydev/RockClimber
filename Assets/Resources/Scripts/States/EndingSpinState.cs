// Cleans up shared spin data and hands control back to the player.
public class EndingSpinState : BaseGameState
{
    public EndingSpinState(GameContext context) : base(context) { }

    public override void Enter()
    {
        Context.Demo.ClearPending();
        Context.ResetSpinData();
        Context.StateMachine.TransitionTo(GameState.Idle);
    }

    public override void Exit() { }
}
