using UnityEngine;

// Waiting for player input. Subscribes to the spin button and the demo queue on entry
// and unsubscribes on exit to prevent stale listeners if the state is re-entered.


public class IdleState : BaseGameState
{
    public IdleState(GameContext context) : base(context) { }

    public override void Enter()
    {
        Context.Demo.ClearPending();
        Context.SpinButton.SetInteractable(Context.CanStartSpin());
        Context.SpinButton.OnSpinClicked += HandleSpinClicked;
        Context.BetButton.SetBetDisplay(Context.Bet.CurrentBet, Context.Bet.CurrentBetLevel);
        Context.BetButton.SetInteractable(true);
        Context.BetButton.OnBetClicked += HandleBetClicked;
        Context.DemoButton.SetInteractable(Context.Demo.Tickets.Count > 0);
        Context.Balance.BalanceChanged += HandleBalanceChanged;

        Context.Demo.OnTicketQueued += HandleDemoTicketQueued;
    }

    public override void Exit()
    {
        Context.SpinButton.OnSpinClicked -= HandleSpinClicked;
        Context.SpinButton.SetInteractable(false);
        Context.BetButton.OnBetClicked -= HandleBetClicked;
        Context.BetButton.SetInteractable(false);
        Context.DemoButton.SetInteractable(false);
        Context.Balance.BalanceChanged -= HandleBalanceChanged;

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

    private void HandleBetClicked()
    {
        Context.Bet.IncreaseBetLevel();
        Context.BetButton.SetBetDisplay(Context.Bet.CurrentBet, Context.Bet.CurrentBetLevel);
        Context.SpinButton.SetInteractable(Context.CanStartSpin());
    }

    private void HandleDemoTicketQueued()
    {
        Context.CurrentSpinSource = SpinStartSource.DemoMenu;
        Context.StateMachine.TransitionTo(GameState.PreparingSpin);
    }

    private void HandleBalanceChanged(int balance)
    {
        Context.SpinButton.SetInteractable(Context.CanStartSpin());
    }
}
