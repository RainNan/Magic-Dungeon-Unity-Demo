using UnityEngine;

public class Player_Dead : IState<Player>
{
    public void Enter(Player owner)
    {
        owner.anim.ResetTrigger("Dead");
        owner.anim.SetTrigger("Dead");
        owner.rb.velocity = Vector3.zero;
        owner.CanRotation = false;
        owner.isAttacking = false;
        owner.isRolling = false;
    }

    public void Update(Player owner)
    {
        owner.rb.velocity = Vector3.zero;
    }

    public void FixedUpdate(Player owner)
    {
        owner.rb.velocity = Vector3.zero;
    }

    public void Exit(Player owner)
    {
    }
}
