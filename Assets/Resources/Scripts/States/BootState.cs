using System.Collections;
using UnityEngine;

// First state on startup. Validates that all required services are wired up before gameplay begins.
public class BootState : BaseGameState
{
    private const int CatalogCandidatesPerFrame = 5000;

    public BootState(GameContext context) : base(context) { }

    public override void Enter()
    {
        SetControls(false);
        Context.Hud?.SetStatus("LOADING BASEGAME 0%");

        if (IsAnyDataNull())
        {
            FailBoot("GameContext has missing scene references.");
            return;
        }

        if (!string.IsNullOrWhiteSpace(Context.StartupError))
        {
            FailBoot(Context.StartupError);
            return;
        }

        Context.CoroutineRunner.StartCoroutine(BootRoutine());
    }

    public bool IsAnyDataNull()
    {
        return Context.Balance == null || Context.Bet == null || Context.Engine == null ||
            Context.Visuals == null || Context.SpinButton == null || Context.BetButton == null ||
            Context.DemoButton == null || Context.Demo == null || Context.WinPool == null || Context.Hud == null;
    }

    public override void Exit() { }

    private IEnumerator BootRoutine()
    {
        Context.Engine.BeginInitialization();

        while (!Context.Engine.StepInitialization(CatalogCandidatesPerFrame))
        {
            int percentage = Mathf.RoundToInt(Context.Engine.InitializationProgress * 100f);
            Context.Hud.SetStatus($"LOADING BASEGAME {percentage}%");
            yield return null;
        }

        Context.Hud.SetStatus("VALIDATING WIN POOL");

        if (!Context.WinPool.TryPrepare(Context.Engine, out string error))
        {
            FailBoot(error);
            yield break;
        }

        Context.StateMachine.TransitionTo(GameState.Idle);
    }

    private void SetControls(bool interactable)
    {
        Context.SpinButton?.SetInteractable(interactable);
        Context.BetButton?.SetInteractable(interactable);
        Context.DemoButton?.SetInteractable(interactable);
    }

    private void FailBoot(string error)
    {
        Debug.LogError($"BootState: {error}");
        Context.Hud?.ShowError(error);
        SetControls(false);
    }
}
