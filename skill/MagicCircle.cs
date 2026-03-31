using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 用于右键画图抬起识别出技能后的选择区域
/// </summary>
public class MagicCircle : MonoBehaviour
{
    [SerializeField]
    private GameObject magicCircle;

    [SerializeField]
    private Camera mainCamera;

    [SerializeField]
    private LayerMask whatIsGround;

    private bool _canMoveMagicCircle = false;

    private string skillName = String.Empty;

    // 防止bug: 放完技能之后一直点左键一直重放
    private bool _canMagic = false;

    private void Awake()
    {
        EventBus.Instance.Subscribe(EventNames.OnOpenMagicCircle, (o) =>
        {
            if (o is string s)
            {
                skillName = s;
            }

            _canMoveMagicCircle = true;
        });
        HideMagicCircle();
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
        
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, whatIsGround))
        {
            ShowMagicCircle();
            magicCircle.transform.position = hit.point + Vector3.up * 0.05f;
        }
    }

    IEnumerator WaitForOneSeconds()
    {
        yield return new WaitForSeconds(1);
        HideMagicCircle();

        EventBus.Instance.Publish(EventNames.OnMagic, (skillName, magicCircle.transform));
        skillName = string.Empty;
    }

    private void HideMagicCircle()
    {
        magicCircle.GetComponent<MeshRenderer>().enabled = false;
        _canMagic = false;
    }
    private void ShowMagicCircle()
    {
        magicCircle.GetComponent<MeshRenderer>().enabled = true;
        _canMagic = true;
    }
}
