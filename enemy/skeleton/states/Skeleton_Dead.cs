using UnityEngine;

public class Skeleton_Dead : IState<Skeleton>
{
    public void Enter(Skeleton owner)
    {
        owner.anim.ResetTrigger("Dead");
        owner.anim.SetTrigger("Dead");
        owner.rb.velocity = Vector3.zero;
        owner.canRotate = false;

        owner.rb.freezeRotation = true;
            
        owner.navMeshAgent.isStopped = true;
        owner.navMeshAgent.ResetPath();
        owner.navMeshAgent.enabled = false;
    }

    public void Update(Skeleton owner)
    {
        owner.rb.velocity = Vector3.zero;
    }

    public void FixedUpdate(Skeleton owner)
    {
        owner.rb.velocity = Vector3.zero;
    }

    public void Exit(Skeleton owner)
    {
    }
}
