using System;
using UnityEngine;
using UnityEngine.UI;

public class MainPanel : BasePanel
{
    private Transform UIOpenPackageBtn;
    private Transform UIOpenEquipmentBtn;

    private void Awake()
    {
        Init();
        InitClickEvent();
    }

    private void InitClickEvent()
    {
        UIOpenPackageBtn.GetComponent<Button>().onClick.AddListener(OnOpenPackage);
        UIOpenEquipmentBtn.GetComponent<Button>().onClick.AddListener(OnOpenEquipment);
    }

    private void OnOpenEquipment()
    {
        UIManager.Instance.OpenPanel(UIConst.EquipmentPanel);
    }

    private void OnOpenPackage()
    {
        UIManager.Instance.OpenPanel(UIConst.PackagePanel);
    }

    private void Init()
    {
        UIOpenPackageBtn = GameObject.Find("Canvas/MainPanel/BottomRight/btnOpenPackage").transform;
        UIOpenEquipmentBtn = GameObject.Find("Canvas/MainPanel/BottomRight/btnOpenEquipment").transform;
    }
}