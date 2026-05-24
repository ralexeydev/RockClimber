// Waiting for player input. Subscribes to the spin button on entry and unsubscribes on exit
// to prevent stale listeners if the state is re-entered.
public class IdleState : BaseGameState
{
    public IdleState(GameContext context) : base(context) { }

    public override void Enter()
    {
        Context.SpinButton.SetInteractable(true);
        Context.SpinButton.OnSpinClicked += HandleSpinClicked;
    }

    public override void Exit()
    {
        Context.SpinButton.OnSpinClicked -= HandleSpinClicked;
        Context.SpinButton.SetInteractable(false);
    }

    private void HandleSpinClicked()
    {
        // Silently ignore the click if the player cannot afford the current bet.
        if (!Context.Balance.CanPayBet(Context.Bet.CurrentBet))
        {
            return;
        }

        Context.StateMachine.TransitionTo(GameState.PreparingSpin);
    }
}
