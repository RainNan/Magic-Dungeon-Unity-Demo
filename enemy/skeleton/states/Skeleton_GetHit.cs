using UnityEngine;

public class Skeleton_GetHit :IState<Skeleton>
{
    public void Enter(Skeleton owner)
    {
        owner.anim.SetTrigger("GetHit");
        // owner.Freezen();

        Debug.Log("准备闪红");
        owner.PlayFlash();
    }

    public void Update(Skeleton owner)
    {
    }

    public void FixedUpdate(Skeleton owner)
    {
    }

    public void Exit(Skeleton owner)
    {
        owner.UnFreezen();
    }
}
