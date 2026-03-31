using UnityEngine;

public class EnemyEvents:MonoBehaviour
{
    public Skeleton Skeleton;
    public void OnAttackEnd()
    {
        Skeleton.Anim_AttackEnd();
    }
    
    public void OnGetHitEnd()
    {
        Skeleton.Anim_GetHitEnd();
    }
}
