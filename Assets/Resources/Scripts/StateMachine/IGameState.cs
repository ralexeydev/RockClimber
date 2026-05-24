// Contract for every game state. Enter and Exit handle setup and teardown.
// Tick is called each frame for states that need per-frame polling.
public interface IGameState
{
    void Enter();
    void Tick();
    void Exit();
}
