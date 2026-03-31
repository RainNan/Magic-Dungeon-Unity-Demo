using UnityEngine;

public class Skeleton_Idle:IState<Skeleton>
{
    public void Enter(Skeleton owner)
    {
        owner.rb.velocity = Vector3.zero;
        owner.navMeshAgent.isStopped = true;
        owner.navMeshAgent.enabled = false;
    }

    public void Update(Skeleton owner)
    {
    }

    public void FixedUpdate(Skeleton owner)
    {
    }

    public void Exit(Skeleton owner)
    {
        owner.navMeshAgent.isStopped = false;
        owner.navMeshAgent.enabled = true;
    }
}
