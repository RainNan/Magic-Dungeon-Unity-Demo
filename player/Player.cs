using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Entity
{
    public Attribute Attribute;
    
    
    [Header("属性")]
    public float mp = 100;
    public float speed = 25;

    ///朝向
    public Transform orientation;

    /// <summary>
    /// InputSystem 输入
    /// </summary>
    private PlayerInputActions _inputActions;

    /// <summary>
    /// 输入
    /// </summary>
    public Vector2 MoveInput { get; private set; }

    /// <summary>
    /// 状态
    /// </summary>
    public StateMachine<Player> StateMachine { get; private set; }

    public string stateName;

    public Player_Idle Idle;
    public Player_Move Move;
    public Player_Attack Attack;
    public Player_Roll Roll;
    public Player_GetHit GetHit;
    public Player_Dead Dead;

    /// <summary>
    /// 攻击
    /// </summary>
    public bool isAttacking;
    public int AttackComboIndex { get; set; } // 当前连到第几段，0 表示没有进入攻击
    
    public float attackRange = 1.5f;
    public float attackAngle = 180f;
    public bool canRotate = true;
    public float attackCooldown = 1f;
    public float attackNextTime;
    public LayerMask whatIsEnemy;
    

    /// <summary>
    /// 鼠标旋转朝向
    /// </summary>
    [SerializeField]
    private Camera mainCamera;
    public float rotationSpeed = 20f;
    public bool CanRotation = true;

    /// <summary>
    /// 翻滚
    /// </summary>
    public bool isRolling = false;
    public float rollSpeed = 5f;
    public bool IsDead => StateMachine.CurrentState == Dead;
    
    // /// <summary>
    // /// 画魔法
    // /// </summary>
    // private bool _stopMagicDraw;
    //
    // public List<Vector2> mousePath = new List<Vector2>();
    // public LineRenderer unistroke;
    public float v;

    protected override void Awake()
    {
        base.Awake();
        CanRotation = true;
        _inputActions = new PlayerInputActions();

        Idle = new Player_Idle();
        Move = new Player_Move();
        Attack = new Player_Attack();
        Roll = new Player_Roll();
        GetHit = new Player_GetHit();
        Dead = new Player_Dead();
        
        StateMachine = new StateMachine<Player>(this);
        StateMachine.ChangeTo(Idle);

        var packageManager = PackageManager.Instance;
    }
    
    
    private void OnEnable()
    {
        _inputActions.Player.Move.performed += OnMove;
        _inputActions.Player.Move.canceled += OnMove;

        _inputActions.Player.Attack.performed += OnAttack;

        _inputActions.Player.Roll.performed += OnRoll;
        
        // _inputActions.Player.MagicDraw.started += OnMagicDrawStart;
        // _inputActions.Player.MagicDraw.canceled += OnMagicDrawEnd;
        
        _inputActions.Enable();
    }


    // private void OnMagicDrawStart(InputAction.CallbackContext ctx)
    // {
    //     _stopMagicDraw = false;
    //     StartCoroutine(MagicDrawCoroutine());
    // }
    //
    // IEnumerator MagicDrawCoroutine()
    // {
    //     Debug.Log("开始draw magic");
    //     while (!_stopMagicDraw)
    //     {
    //         mousePath.Add(Input.mousePosition);
    //         
    //         // 等一帧，回到主线程
    //         yield return null;
    //     }
    //     Debug.Log("结束draw magic");
    // }
    //
    // private void OnMagicDrawEnd(InputAction.CallbackContext obj)
    // {
    //     _stopMagicDraw = true;
    // }


    private void FixedUpdate()
    {
        StateMachine.FixedUpdate();
    }

    private void Update()
    {
        StateMachine.Update();

        HandleAnimation();
        stateName = StateMachine.CurrentState.ToString();

        HandleMouseRotation();
        v = rb.velocity.magnitude;
    }

    private void HandleAnimation()
    {
        Vector3 worldVelocity = rb.velocity;
        worldVelocity.y = 0f;

        Vector3 localVelocity = transform.InverseTransformDirection(worldVelocity);

        anim.SetFloat("MoveX", localVelocity.x);
        anim.SetFloat("MoveY", localVelocity.z);
        anim.SetFloat("Speed", worldVelocity.magnitude);
    }

    private void OnDisable()
    {
        _inputActions.Player.Move.performed -= OnMove;
        _inputActions.Player.Move.canceled -= OnMove;

        _inputActions.Player.Attack.performed -= OnAttack;

        _inputActions.Player.Roll.performed -= OnRoll;

        _inputActions.Disable();
    }

    #region 输入回调

    private void OnMove(InputAction.CallbackContext ctx) =>
        MoveInput = ctx.ReadValue<Vector2>();

    private void OnAttack(InputAction.CallbackContext ctx)
    {
        if (IsDead)
            return;

        if (isAttacking)
        {
            anim.ResetTrigger("Attack");
            anim.SetTrigger("Attack");
        }
        else
            StateMachine.ChangeTo(Attack);
    }

    private void OnRoll(InputAction.CallbackContext ctx)
    {
        if (IsDead)
            return;

        StateMachine.ChangeTo(Roll);
    }

    #endregion


    private void HandleMouseRotation()
    {
        if (!CanRotation)
            return;

        if (mainCamera == null)
            return;

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mouseScreenPos);

        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (!groundPlane.Raycast(ray, out float enter))
            return;

        Vector3 hitPoint = ray.GetPoint(enter);

        Vector3 dir = hitPoint - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude <= 0.0001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    #region Anim

    public void Anim_AttackEnd()
    {
        isAttacking = false;
    }

    public void Anim_RollEnd()
    {
        if (IsDead)
            return;

        StateMachine.ChangeTo(Idle);
    }
    
    public void Anim_GetHitEnd()
    {
        if (IsDead)
            return;

        StateMachine.ChangeTo(Idle);
    }

    #endregion


    /// <summary>
    /// 受击
    /// </summary>
    public void OnHit(Entity hitSource)
    {
        if (IsDead)
            return;

        var def = AttributeManager.Instance.GetDef();

        var dmg = hitSource.atk - def;
        
        if (dmg <= 0) return;
        var hp = AttributeManager.Instance.GetHp();
        hp -= dmg;
        AttributeManager.Instance.SetHp(hp);

        if (hp <= 0)
        {
            StateMachine.ChangeTo(Dead);
            return;
        }

        StateMachine.ChangeTo(GetHit);
    }


    /// <summary>
    /// 冻结移动，旋转  
    /// </summary>
    public void Freezen()
    {
        rb.velocity = Vector3.zero;
        CanRotation = false;
    }

    public void UnFreezen()
    {
        CanRotation = true;
    }
}
