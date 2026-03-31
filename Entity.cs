using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public Animator anim;
    public Rigidbody rb;

    [Header("属性")]
    public float hp = 100f;
    public float atk = 25f;
    public float def = 0f;

    /// <summary>
    /// 受伤闪红
    /// </summary>
    [Header("闪红材质")]
    [SerializeField]
    private Material flashMaterial;

    [Header("持续时间")]
    [SerializeField]
    private float flashDuration = 0.1f;


    [Header("掉落")]
    [SerializeField]
    protected List<ItemData> drops = new();

    private Renderer[] _renderers;

    // 保存每个 Renderer 原本的材质数组
    private Material[][] _originalMaterials;

    private Coroutine _flashCoroutine;

    public virtual bool IsDead { get; set; }
    
    protected virtual void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();

        _renderers = GetComponentsInChildren<Renderer>(true);
        _originalMaterials = new Material[_renderers.Length][];

        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i] == null) continue;
            _originalMaterials[i] = _renderers[i].sharedMaterials;
        }
    }

    public void PlayFlash()
    {
        if (flashMaterial == null)
        {
            Debug.LogWarning("没有指定 flashMaterial", this);
            return;
        }

        if (_flashCoroutine != null)
            StopCoroutine(_flashCoroutine);

        _flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        // 切成红色材质
        for (int i = 0; i < _renderers.Length; i++)
        {
            var r = _renderers[i];
            if (r == null) continue;

            var originalMats = _originalMaterials[i];
            if (originalMats == null || originalMats.Length == 0) continue;

            Material[] flashMats = new Material[originalMats.Length];
            for (int j = 0; j < flashMats.Length; j++)
            {
                flashMats[j] = flashMaterial;
            }

            r.materials = flashMats;
        }

        yield return new WaitForSeconds(flashDuration);

        // 恢复原材质
        for (int i = 0; i < _renderers.Length; i++)
        {
            var r = _renderers[i];
            if (r == null) continue;

            var originalMats = _originalMaterials[i];
            if (originalMats == null || originalMats.Length == 0) continue;

            r.materials = originalMats;
        }

        _flashCoroutine = null;
    }

    /// <summary>
    /// 死亡后的掉落物品
    /// </summary>
    public virtual void Drop()
    {
        
    }
}