using System.Collections;

// Resolves the spin outcome. Credits are added before the win animation plays
// so the balance is already correct if the player skips the animation.
public class PresentingWinsState : BaseGameState
{
    public PresentingWinsState(GameContext context) : base(context) { }

    public override void Enter()
    {
        Context.CoroutineRunner.StartCoroutine(PresentRoutine());
    }

    public override void Exit() { }

    private IEnumerator PresentRoutine()
    {
        SpinResult result = Context.CurrentSpinResult;

        if (result.TotalWin > 0)
        {
            Context.Balance.AddWin(result.TotalWin);
            yield return Context.Visuals.PlayWinSequence(result);
        }

        // Skip directly to EndingSpin if no feature was triggered.
        GameState next = result.HasFeature ? GameState.Feature : GameState.EndingSpin;
        Context.StateMachine.TransitionTo(next);
    }
}
