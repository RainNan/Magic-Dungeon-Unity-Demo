using System;
using UnityEngine;

/// <summary>
/// 用于挂技能的碰撞体上
/// </summary>
public class SKillHit : MonoBehaviour
{
    [SerializeField]
    private LayerMask whatIsEnemy;

    [SerializeField]
    private SkillConfig skillCfg;


    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & whatIsEnemy) == 0) return;

        other.GetComponent<Skeleton>().OnHit(skillCfg.dmg);
    }
}