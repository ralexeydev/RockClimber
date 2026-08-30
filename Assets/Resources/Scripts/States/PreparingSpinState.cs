// Snapshots the current bet. Paid spins deduct it before requesting a result;
// demos leave the balance untouched so they can always be replayed.
public class PreparingSpinState : BaseGameState
{
    public PreparingSpinState(GameContext context) : base(context) { }

    public override void Enter()
    {
        bool isDemo = Context.CurrentSpinSource == SpinStartSource.DemoMenu;

        // Demo tickets are authored and replayed at the base $0.25 level. They
        // never alter the player's selected level or balance.
        Context.CurrentBetLevel = isDemo
            ? BetController.MinBetLevel
            : Context.Bet.CurrentBetLevel;
        Context.CurrentBet = Context.CurrentBetLevel * BetController.BetStepCents;

        if (!isDemo)
            Context.Balance.PayBet(Context.CurrentBet);

        Context.StateMachine.TransitionTo(GameState.RequestingResult);
    }

    public override void Exit() { }
}
