using System.Collections;

// Drives the reel spin animation. States are plain C# objects, so coroutines
// are started through the CoroutineRunner (GameController) instead of this class.
public class SpinningState : BaseGameState
{
    public SpinningState(GameContext context) : base(context) { }

    public override void Enter()
    {
        Context.CoroutineRunner.StartCoroutine(SpinRoutine());
    }

    public override void Exit() { }

    private IEnumerator SpinRoutine()
    {
        yield return Context.Visuals.PlaySpinSequence(Context.CurrentSpinResult);
        Context.StateMachine.TransitionTo(GameState.PresentingWins);
    }
}
