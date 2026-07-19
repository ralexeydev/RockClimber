using UnityEngine;

// Waiting for player input. Subscribes to the spin button and the demo queue on entry
// and unsubscribes on exit to prevent stale listeners if the state is re-entered.


public class IdleState : BaseGameState
{
    public IdleState(GameContext context) : base(context) { }

    public override void Enter()
    {
        Context.SpinButton.SetInteractable(true);
        Context.SpinButton.OnSpinClicked += HandleSpinClicked;

        if (Context.Demo != null)
            Context.Demo.OnTicketQueued += HandleDemoTicketQueued;
    }

    public override void Exit()
    {
        Context.SpinButton.OnSpinClicked -= HandleSpinClicked;
        Context.SpinButton.SetInteractable(false);

        if (Context.Demo != null)
            Context.Demo.OnTicketQueued -= HandleDemoTicketQueued;
    }

    private void HandleSpinClicked()
    {
        // Silently ignore the click if the player cannot afford the current bet
        if (!Context.CanStartSpin())
        {
            Debug.LogWarning("Spin button clicked - Insufficient funds.");
            return;
        }

        Context.CurrentSpinSource = SpinStartSource.SpinButton;
        Context.StateMachine.TransitionTo(GameState.PreparingSpin);
    }

    private void HandleDemoTicketQueued()
    {
        if (!Context.CanStartSpin())
        {
            Debug.LogWarning("Demo ticket clicked - Insufficient funds.");
            Context.Demo?.ConsumePending();
            return;
        }

        Context.CurrentSpinSource = SpinStartSource.DemoMenu;
        Context.StateMachine.TransitionTo(GameState.PreparingSpin);
    }
}
