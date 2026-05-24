// Snapshots the current bet and deducts it from the balance before the spin result is known.
// The bet is stored on context so later states (e.g. ErrorState) can refund the exact amount.
public class PreparingSpinState : BaseGameState
{
    public PreparingSpinState(GameContext context) : base(context) { }

    public override void Enter()
    {
        Context.CurrentBet = Context.Bet.CurrentBet;
        Context.Balance.PayBet(Context.CurrentBet);
        Context.StateMachine.TransitionTo(GameState.RequestingResult);
    }

    public override void Exit() { }
}
