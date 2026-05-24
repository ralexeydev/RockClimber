using UnityEngine;

// First state on startup. Validates that all required services are wired up before gameplay begins.
public class BootState : BaseGameState
{
    public BootState(GameContext context) : base(context) { }

    public override void Enter()
    {
        // Catch missing Inspector references early so errors surface at boot, not mid-spin.
        if (Context.Balance == null || Context.Bet == null || Context.Engine == null ||
            Context.Visuals == null || Context.SpinButton == null)
        {
            Debug.LogError("BootState: GameContext has missing references.");
            return;
        }

        Context.StateMachine.TransitionTo(GameState.Idle);
    }

    public override void Exit() { }
}
