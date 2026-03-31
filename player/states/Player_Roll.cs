using UnityEngine;

public class Player_Roll : IState<Player>
{
    private Quaternion _rotation;
    public void Enter(Player owner)
    {
        owner.isRolling = true;
        owner.anim.SetTrigger("Roll");

        owner.CanRotation = false;
    }

    public void Update(Player owner)
    {
    }

    public void FixedUpdate(Player owner)
    {
        Vector3 moveDir = Camera.main.transform.TransformDirection(new Vector3(owner.MoveInput.x, 0, owner.MoveInput.y))
            .normalized;
        owner.rb.velocity = new Vector3(moveDir.x * owner.rollSpeed,
            owner.rb.velocity.y,
            moveDir.z * owner.rollSpeed);

        owner.transform.rotation = _rotation;
    }

    public void Exit(Player owner)
    {
        owner.isRolling = false;
        owner.CanRotation = true;
    }
}
