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

    private T owner;

    public StateMachine(T owner)
    {
        this.owner = owner;
    }

    public void ChangeTo(IState<T> newState)
    {
        if (CurrentState == newState)
            return;

        CurrentState?.Exit(owner);

        CurrentState = newState;

        CurrentState?.Enter(owner);
    }

    public void Update()
    {
        CurrentState?.Update(owner);
    }

    public void FixedUpdate()
    {
        CurrentState?.FixedUpdate(owner);
    }
}