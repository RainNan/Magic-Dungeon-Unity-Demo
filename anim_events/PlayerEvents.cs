using System;
using UnityEngine;

public class PlayerEvents : MonoBehaviour
{
    public Player Player;

    private void Awake()
    {
        if (Player is null)
            Player = GetComponentInParent<Player>();
    }

    public void OnAttackEnd()
    {
        Player.Anim_AttackEnd();
    }

    public void OnRollEnd()
    {
        Player.Anim_RollEnd();
    }

    public void OnGetHitEnd()
    {
        Player.Anim_GetHitEnd();
    }
}