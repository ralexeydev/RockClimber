using System.Collections.Generic;
using UnityEngine;

public class GameStateMachine
{
    private readonly Dictionary<GameState, IGameState> m_States = new Dictionary<GameState, IGameState>();
    private IGameState m_CurrentState;

    public GameState CurrentStateKey { get; private set; }

    // All states must be registered before the machine starts.
    public void RegisterState(GameState key, IGameState state)
    {
        m_States[key] = state;
    }

    // Exits the current state before entering the next.
    // Order matters: Exit cleans up listeners, Enter sets new ones.
    public void TransitionTo(GameState newState)
    {
        if (!m_States.TryGetValue(newState, out IGameState next))
        {
            Debug.LogError($"GameStateMachine: No state registered for {newState}");
            return;
        }

        m_CurrentState?.Exit();

        GameState previousState = CurrentStateKey;
        CurrentStateKey = newState;
        m_CurrentState = next;

        Debug.Log($"State: {previousState} -> {CurrentStateKey}");

        m_CurrentState.Enter();
    }

    // Called every frame from GameController.Update to drive per-frame state logic.
    public void Tick()
    {
        m_CurrentState?.Tick();
    }
}
