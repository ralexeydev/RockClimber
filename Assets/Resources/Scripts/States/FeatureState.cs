using System.Collections;

// Plays the bonus feature sequence then returns to the normal end-of-spin flow.
public class FeatureState : BaseGameState
{
    public FeatureState(GameContext context) : base(context) { }

    public override void Enter()
    {
        Context.CoroutineRunner.StartCoroutine(FeatureRoutine());
    }

    public override void Exit() { }

    private IEnumerator FeatureRoutine()
    {
        yield return Context.Visuals.PlayFeatureSequence(Context.CurrentSpinResult);
        Context.StateMachine.TransitionTo(GameState.EndingSpin);
    }
}
