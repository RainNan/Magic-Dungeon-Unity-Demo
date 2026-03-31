using UnityEngine;

public class Player_Attack : IState<Player>
{
    public void Enter(Player owner)
    {
        owner.AttackComboIndex = 1;
        owner.isAttacking = true;
        
        owner.anim.ResetTrigger("Attack");
        owner.anim.SetTrigger("Attack");

        owner.CanRotation = false;
    }

    public void Update(Player owner)
    {
        if (!owner.isAttacking)
        {
            if (owner.MoveInput.sqrMagnitude > 0.0001f)
                owner.StateMachine.ChangeTo(owner.Move);
            else
                owner.StateMachine.ChangeTo(owner.Idle);
        }
        
        if (Time.time < owner.attackNextTime)
            return;
        
        DoAttack(owner);
        
        // 设置下一次攻击时间
        owner.attackNextTime = Time.time + owner.attackCooldown;
    }
    
    private void DoAttack(Player owner)
    {
        var colliders = Physics.OverlapSphere(
            owner.transform.position,
            owner.attackRange,
            owner.whatIsEnemy
        );

        foreach (var collider in colliders)
        {
            var skeleton = collider.GetComponent<Skeleton>();
            skeleton.OnHit(owner);
        }
    }

    public void FixedUpdate(Player owner)
    {
        var speed = AttributeManager.Instance.GetSpeed();
        Vector3 moveDir = Camera.main.transform.TransformDirection(new Vector3(owner.MoveInput.x, 0, owner.MoveInput.y))
            .normalized;

        owner.rb.velocity = new Vector3(
            moveDir.x * speed,
            owner.rb.velocity.y,
            moveDir.z * speed
        );
    }

    public void Exit(Player owner)
    {
        owner.AttackComboIndex = 0;
        owner.isAttacking = false;
        
        owner.CanRotation = true;
    }
}
