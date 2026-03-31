public class Player_Idle:IState<Player>
{
    public void Enter(Player owner)
    {
    }

    public void Update(Player owner)
    {
        if (owner.MoveInput.magnitude > 0.01f)
            owner.StateMachine.ChangeTo(owner.Move);
        if (owner.isAttacking)
            owner.StateMachine.ChangeTo(owner.Attack);
    }

    public void FixedUpdate(Player owner)
    {
    }

    public void Exit(Player owner)
    {
    }
}
