using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Skeleton : Entity
{
    /// <summary>
    /// 状态
    /// </summary>
    public StateMachine<Skeleton> StateMachine;
    public Skeleton_Chase Chase;
    public Skeleton_Attack Attack;
    public Skeleton_GetHit GetHit;
    public Skeleton_Dead Dead;
    // public Skeleton_Idle Idle;

    /// <summary>
    /// nav
    /// </summary>
    public NavMeshAgent navMeshAgent;
    public Transform playerPos;

    /// <summary>
    /// 攻击
    /// </summary>
    // public Transform attackPoint; // 这个点必须放在和敌人同一个Y上，如果不是，会有攻击失效bug
    // 或者，不需要攻击点了
    public float attackRange = 1.5f;
    public float attackAngle = 180f;
    public bool canRotate = true;
    public float attackCooldown = 1f;
    public float attackNextTime;
    public override bool IsDead => StateMachine.CurrentState == Dead;

    public string stateName;
    public LayerMask whatIsPlayer;

    protected override void Awake()
    {
        base.Awake();

        StateMachine = new StateMachine<Skeleton>(this);
        Chase = new Skeleton_Chase();
        Attack = new Skeleton_Attack();
        GetHit = new Skeleton_GetHit();
        Dead = new Skeleton_Dead();
        // Idle = new Skeleton_Idle();

        StateMachine.ChangeTo(Chase);
        // StateMachine.ChangeTo(Idle);

        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.stoppingDistance = attackRange;
    }

    private void Start()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj is not null)
            playerPos = playerObj.transform;
    }

    private void Update()
    {
        stateName = StateMachine.CurrentState.ToString();

        StateMachine.Update();

        HandleAnim();

        LookAtPlayer();
    }

    private void LookAtPlayer()
    {
        if (!playerPos || !canRotate) return;

        Vector3 dir = playerPos.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f) return;

        transform.rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(0f, 0f, 0f);
    }

    // private void HandleAnim()
    // {
    //     var v = navMeshAgent.velocity;
    //     anim.SetFloat("MoveX", v.x);
    //     anim.SetFloat("MoveY", v.z);
    // }

    private void HandleAnim()
    {
        Vector3 localVelocity = transform.InverseTransformDirection(navMeshAgent.velocity);

        anim.SetFloat("MoveX", localVelocity.x);
        anim.SetFloat("MoveY", localVelocity.z);
    }

    private void FixedUpdate()
    {
        StateMachine.FixedUpdate();
    }

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        DrawAttackArc();
    }

    private void DrawAttackArc()
    {
        Vector3 origin = transform.position;
        origin.y += 0.1f; // 稍微抬高一点，避免和地面重合看不清

        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();

        float halfAngle = attackAngle * 0.5f;

        // 左右边界方向
        Vector3 leftDir = Quaternion.Euler(0f, -halfAngle, 0f) * forward;
        Vector3 rightDir = Quaternion.Euler(0f, halfAngle, 0f) * forward;

        Gizmos.color = Color.red;

        // 前方中心线
        Gizmos.DrawLine(origin, origin + forward * attackRange);

        // 左右边界线
        Gizmos.DrawLine(origin, origin + leftDir * attackRange);
        Gizmos.DrawLine(origin, origin + rightDir * attackRange);

        // 画圆弧
        int segmentCount = 30;
        Vector3 prevPoint = origin + leftDir * attackRange;

        for (int i = 1; i <= segmentCount; i++)
        {
            float t = i / (float)segmentCount;
            float currentAngle = Mathf.Lerp(-halfAngle, halfAngle, t);

            Vector3 dir = Quaternion.Euler(0f, currentAngle, 0f) * forward;
            Vector3 nextPoint = origin + dir * attackRange;

            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }
    }
#endif
    public void Anim_AttackEnd()
    {
        if (IsDead)
            return;

        StateMachine.ChangeTo(Chase);
    }

    public bool InAttackRange()
    {
        // 1.在攻击距离内
        var sqrDis = (playerPos.position - transform.position).sqrMagnitude;
        if (sqrDis > attackRange * attackRange) return false;

        Vector3 toTarget = (playerPos.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, toTarget);

        return angle <= attackAngle * 0.5f;
    }

    public void Anim_GetHitEnd()
    {
        if (IsDead)
            return;

        StateMachine.ChangeTo(Chase);
    }

    public void OnHit(float dmg)
    {
        dmg -= def;
        if (dmg <= 0) return;
        hp -= dmg;
        if (hp <= 0)
        {
            OnDead();
            return;
        }

        StateMachine.ChangeTo(GetHit);
    }

    public void OnHit(Player player)
    {
        if (IsDead)
            return;

        var atk = AttributeManager.Instance.GetAtk();
        var dmg = atk - def;
        if (dmg <= 0) return;
        hp -= dmg;

        if (hp <= 0)
        {
            OnDead();
            return;
        }

        // // 击退效果
        // var dir = (transform.position - player.transform.position).normalized;
        // dir.y = 0;
        // rb.AddForce(dir*10, ForceMode.Impulse);

        StateMachine.ChangeTo(GetHit);
    }

    private void OnDead()
    {
        StateMachine.ChangeTo(Dead);

        Drop();
    }

    /// <summary>
    /// 冻结移动，旋转  
    /// </summary>
    public void Freezen()
    {
        rb.velocity = Vector3.zero;
        canRotate = false;
    }

    public void UnFreezen()
    {
        canRotate = true;
    }

    // public void Chase() => StateMachine.ChangeTo(Chase);

    /// <summary>
    /// 掉落物品，应该走随机机制
    /// </summary>
    public override void Drop()
    {
        base.Drop();

        if (drops.Count == 0) return;

        foreach (var itemData in drops)
        {
            var randomValue = Random.Range(0, 100);

            if (randomValue < 90f)
            {
                Vector3 spawnOffset = new Vector3(
                    Random.Range(-1f, 1f),
                    0.5f,
                    Random.Range(-1f, 1f));
                Vector3 spawnPosition = transform.position + spawnOffset;

                if (itemData is EquipmentData equipmentData)
                    Instantiate(equipmentData.equipmentConfig.prefab, spawnPosition, Quaternion.identity);
                else
                    Instantiate(itemData.itemConfig.prefab, spawnPosition, Quaternion.identity);

                Debug.Log($"Drop spawned at: {spawnPosition}");
            }
        }
    }
}
