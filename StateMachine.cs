public interface IState<T>
{
    void Enter(T owner);
    void Update(T owner);
    void FixedUpdate(T owner);
    void Exit(T owner);
}

public class StateMachine<T> where T : Entity
{
    public IState<T> CurrentState { get; private set; }

    private readonly T _owner;

    public StateMachine(T owner) => _owner = owner;

    public void ChangeTo(IState<T> newState)
    {
        if (CurrentState == newState) return;

        CurrentState?.Exit(_owner);

        CurrentState = newState;

        CurrentState?.Enter(_owner);
    }

    public void Update() => CurrentState?.Update(_owner);

    public void FixedUpdate() => CurrentState?.FixedUpdate(_owner);
}