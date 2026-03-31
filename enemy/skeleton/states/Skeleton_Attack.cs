using UnityEngine;

public class Skeleton_Attack : IState<Skeleton>
{
    public void Enter(Skeleton owner)
    {
        StopChase(owner);
        owner.canRotate = false;
    }

    private void StopChase(Skeleton owner)
    {
        owner.navMeshAgent.isStopped = true;
        owner.navMeshAgent.ResetPath();
    }

    public void Update(Skeleton owner)
    {
        if (owner.playerPos == null) return;

        // 不在攻击范围了，回追击
        if (!owner.InAttackRange())
        {
            owner.StateMachine.ChangeTo(owner.Chase);
            return;
        }

        // 冷却没到，什么都不做，继续等下一帧
        if (Time.time < owner.attackNextTime)
            return;

        // 到点才攻击
        owner.anim.SetTrigger("Attack");
        DoAttack(owner);

        // 设置下一次攻击时间
        owner.attackNextTime = Time.time + owner.attackCooldown;
    }

    private void DoAttack(Skeleton owner)
    {
        var colliders = Physics.OverlapSphere(
            owner.transform.position,
            owner.attackRange,
            owner.whatIsPlayer
        );

        if (colliders.Length > 1)
            Debug.LogWarning("攻击到不止一个玩家");

        if (colliders.Length == 0)
            return;

        var player = colliders[0].GetComponent<Player>();
        player?.OnHit(owner);
    }

    public void FixedUpdate(Skeleton owner)
    {
        owner.rb.velocity = Vector3.zero;
    }

    public void Exit(Skeleton owner)
    {
        owner.navMeshAgent.isStopped = false;
        owner.canRotate = true;
    }
}