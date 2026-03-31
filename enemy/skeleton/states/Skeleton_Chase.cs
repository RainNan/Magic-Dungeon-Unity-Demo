using UnityEngine;

public class Skeleton_Chase : IState<Skeleton>
{
    public void Enter(Skeleton owner)
    {
    }

    public void Update(Skeleton owner)
    {
        owner.navMeshAgent.SetDestination(owner.playerPos.position);

        if (owner.InAttackRange())
            owner.StateMachine.ChangeTo(owner.Attack);
    }

    public void FixedUpdate(Skeleton owner)
    {
    }

    public void Exit(Skeleton owner)
    {
    }
}
