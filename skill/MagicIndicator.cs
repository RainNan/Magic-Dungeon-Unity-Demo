using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 用于右键画图抬起识别出技能后的选择区域
/// </summary>
public class MagicIndicator : MonoBehaviour
{
    [SerializeField]
    private Camera mainCamera;

    [SerializeField]
    private LayerMask whatIsGround;

    private bool _canMoveMagicCircle = false;

    private SkillName _skillName;

    // 防止bug: 放完技能之后一直点左键一直重放
    private bool _canMagic = false;
    
    // 缓存组件
    private MeshRenderer _meshRenderer;
    private Material _magicIndicatorArrow;
    private Material _magicIndicatorCircle;
    private Transform _playerTrans;

    private Vector3 _arrowDir;

    private void Awake()
    {
        _meshRenderer = gameObject.GetComponent<MeshRenderer>();
        _playerTrans = GameObject.FindWithTag("Player").GetComponent<Transform>();
        HideMagicCircle();
        _magicIndicatorCircle = Resources.Load<Material>("Skill/indicator/MagicIndicatorCircle");
        _magicIndicatorArrow = Resources.Load<Material>("Skill/indicator/MagicIndicatorArrow");
    }

    private void OnEnable()
    {
        BindEventOnOpenMagicIndicator();

        BindEventOnMagic();
    }
    
    private void Update()
    {
        if (_canMoveMagicCircle && _canMagic && Input.GetMouseButtonDown(0))
        {
            _canMoveMagicCircle = false;
            StartCoroutine(WaitForOneSeconds());
            return;
        }

        if (!_canMoveMagicCircle) return;

        // 持续显示[魔法指示器]
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, whatIsGround))
        {
            ShowMagicCircle();
            // transform.position = hit.point + Vector3.up * 0.05f;

            // 之前抖动，采用插值
            transform.position = Vector3.Lerp(transform.position, hit.point + Vector3.up * 0.05f, Time.deltaTime * 10f);

            if (_skillName != SkillName.Crystal)
            {
                Vector3 lookDir = transform.position - _playerTrans.position;
                lookDir.y = 0f;

                if (lookDir.sqrMagnitude > 0.0001f)
                {
                    _arrowDir = lookDir.normalized;
                    float yaw = Mathf.Atan2(lookDir.x, lookDir.z) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.Euler(90f, yaw, 270f);
                }
            }
        }
    }

    IEnumerator WaitForOneSeconds()
    {
        yield return new WaitForSeconds(1);
        HideMagicCircle();

        EventBus.Instance.Publish(EventNames.OnMagic, (_skillName, transform));
    }

    private void HideMagicCircle()
    {
        _meshRenderer.enabled = false;
        _canMagic = false;
    }

    private void ShowMagicCircle()
    {
        _meshRenderer.enabled = true;
        _canMagic = true;
    }

    private void ChangeIndicator2Arrow() =>
        _meshRenderer.material = _magicIndicatorArrow;

    private void ChangeIndicator2Circle() =>
        _meshRenderer.material = _magicIndicatorCircle;
    
    IEnumerator WaitForRemoveTrigger(GameObject skillObj, ParticleSystem.MainModule main)
    {
        yield return new WaitForSeconds(main.startLifetime.constant * 0.6f);

        skillObj.transform.Find("Crystals").GetComponent<SphereCollider>().enabled = false;
    }
    
    private void BindEventOnOpenMagicIndicator()
    {
        EventBus.Instance.Subscribe(EventNames.OnOpenMagicCircle, (o) =>
        {
            if (o is SkillName sn) _skillName = sn;

            if (_skillName == SkillName.Crystal)
                ChangeIndicator2Circle();
            else
                ChangeIndicator2Arrow();
            _canMoveMagicCircle = true;
        });
    }

    private void BindEventOnMagic()
    {
        EventBus.Instance.Subscribe(EventNames.OnMagic, (o) =>
        {
            if (o is ValueTuple<SkillName, Transform> tuple)
            {
                var skillName = tuple.Item1;
                var skillCenterPos = tuple.Item2;

                var skillCfg = SkillManager.Instance.GetSkillByName(skillName);
                if (skillCfg == null)
                {
                    Debug.Log("skill cfg is null!");
                    return;
                }

                switch (skillName)
                {
                    case SkillName.Crystal:
                        var skillObj = Instantiate(skillCfg.prefab, skillCenterPos.position, Quaternion.identity);
                        var main = skillObj.GetComponent<ParticleSystem>().main;
                        main.loop = false;
                        StartCoroutine(WaitForRemoveTrigger(skillObj, main));
                        main.stopAction = ParticleSystemStopAction.Destroy;
                        break;
                    
                    case SkillName.Thunder:
                        var pos = _playerTrans.position;
                        pos.y += 3f;

                        Vector3 castDir = skillCenterPos.position - _playerTrans.position;
                        castDir.y = 0f;

                        if (castDir.sqrMagnitude <= 0.0001f)
                            castDir = _arrowDir;
                        else
                            castDir.Normalize();

                        Quaternion castRotation = castDir.sqrMagnitude > 0.0001f
                            ? Quaternion.LookRotation(castDir, Vector3.up)
                            : Quaternion.identity;

                        skillObj = Instantiate(skillCfg.prefab, pos, castRotation);
                        var rb = skillObj.AddComponent<Rigidbody>();
                        rb.velocity = castDir * skillCfg.speed;
                        rb.useGravity = false;
                        break;
                }
            }
        });
    }

}
