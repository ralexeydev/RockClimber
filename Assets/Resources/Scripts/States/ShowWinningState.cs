using System.Collections;

// Resolves the spin outcome. Paid-spin credits are added before animation;
// demos present the same ticket visuals without changing the balance.
public class ShowWinningState : BaseGameState
{
    public ShowWinningState(GameContext context) : base(context) { }

    public override void Enter()
    {
        Context.CoroutineRunner.StartCoroutine(ShowWins());
    }

    public override void Exit() { }

    private IEnumerator ShowWins()
    {
        SpinResult result = Context.CurrentSpinResult;

        if (result.TotalWin > 0)
        {
            if (Context.CurrentSpinSource != SpinStartSource.DemoMenu)
                Context.CreditWinOnce(result.TotalWin);
            yield return Context.Visuals.PlayWinSequence(result);
        }

        Context.StateMachine.TransitionTo(GameState.EndingSpin);
    }
}
