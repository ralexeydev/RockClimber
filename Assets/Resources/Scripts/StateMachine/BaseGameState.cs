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
    // Tick is no-op by default. Only override when per-frame logic is required.
    public virtual void Tick() { }
    public abstract void Exit();
}
