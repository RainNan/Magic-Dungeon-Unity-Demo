using UnityEngine;

public class Player_Move : IState<Player>
{
    public void Enter(Player owner)
    {
    }

    public void Update(Player owner)
    {
        if (owner.MoveInput.magnitude <= 0.01f)
            owner.StateMachine.ChangeTo(owner.Idle);

        if (owner.isAttacking)
            owner.StateMachine.ChangeTo(owner.Attack);
    }

    public void FixedUpdate(Player owner)
    {
        var speed = AttributeManager.Instance.GetSpeed();
        
        // 世界坐标转换
        Vector3 moveDir = Camera.main.transform.TransformDirection(new Vector3(owner.MoveInput.x, 0, owner.MoveInput.y))
            .normalized;
        
        // var moveDir = new Vector3(owner.MoveInput.x, 0, owner.MoveInput.y).normalized;
        owner.rb.velocity = new Vector3(moveDir.x * speed, owner.rb.velocity.y, moveDir.z * speed);
        // owner.rb.velocity = new Vector3(
        //     owner.MoveInput.x * owner.speed,
        //     owner.rb.velocity.y,
        //     owner.MoveInput.y * owner.speed
        // );
    }

    public void Exit(Player owner)
    {
    }
}