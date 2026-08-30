// Shared base for all concrete states.
// Stores the GameContext so every state has access to services without finding components.
public abstract class BaseGameState : IGameState
{
    protected GameContext Context;

    protected BaseGameState(GameContext context)
    {
        Context = context;
    }

    public abstract void Enter();

    // Tick can be overridden by states that need per-frame polling, but is a no-op by default.
    public virtual void Tick() { }

    public abstract void Exit();
}
