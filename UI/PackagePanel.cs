using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PackagePanel : BasePanel
{
    private Transform UIMenu;
    private Transform UIMenuWeapon;
    private Transform UIMenuConsumable;

    private Transform UICloseBtn;
    private Transform UICloseBtnDimBg;

    private Transform UICenter;
    private Transform UIScrollView;
    private Transform UIDetailPanel;
    private Transform UILeftBtn;
    private Transform UIRightBtn;
    private Transform UIDeletePanel;
    private Transform UIDeleteInfoText;
    private Transform UIDeleteConfirmBtn;
    private Transform UIBottomMenus;
    private Transform UIDeleteBtn;
    private Transform UIDetailBtn;

    [SerializeField]
    private PackageCeil _ceil;

    private void Awake()
    {
        Init();
        InitClickEvent();
        BindBtn();

        Subcribe();
    }

    private void OnEnable()
    {
        InitItems();
    }

    private void Subcribe()
    {
        EventBus.Instance.Subscribe(EventNames.CloseSelectMenu, (o) =>
        {
            UIMenuWeapon.transform.Find("Selected").gameObject.SetActive(false);
            UIMenuConsumable.transform.Find("Selected").gameObject.SetActive(false);
        });

        EventBus.Instance.Subscribe(EventNames.RefreshPackage, (o) => { InitItems(); });
    }

    private void BindBtn()
    {
        UIMenuWeapon.GetComponent<Button>().onClick.AddListener(() =>
        {
            EventBus.Instance.Publish(EventNames.CloseSelectMenu);

            UIMenuWeapon.transform.Find("Selected").gameObject.SetActive(true);
            InitItems();
        });

        UIMenuConsumable.GetComponent<Button>().onClick.AddListener(() =>
        {
            EventBus.Instance.Publish(EventNames.CloseSelectMenu);

            UIMenuConsumable.transform.Find("Selected").gameObject.SetActive(true);
            InitItems();
        });

        UICloseBtnDimBg.GetComponent<Button>().onClick.AddListener(OnClose);
    }

    /// <summary>
    /// 按类型去加载物品
    /// </summary>
    private void InitItems()
    {
        List<ItemData> dataList;

        var content = UIScrollView.transform.Find("Viewport/Content");

        // 清空content
        for (int i = content.childCount - 1; i >= 0; i--)
            Destroy(content.GetChild(i).gameObject);

        ItemType itemType;

        if (UIMenuWeapon.transform.Find("Selected").gameObject.activeSelf)
        {
            itemType = ItemType.Equipment;
            dataList = PackageManager.Instance.GetEquipments();
        }
        else
        {
            itemType = ItemType.Consumable;
            dataList = PackageManager.Instance.GetItems();
        }

        foreach (var data in dataList)
        {
            if (data.itemConfig && data.itemConfig.type != itemType) continue;

            if (data is not EquipmentData && data.count == 0) return;

            if (data is EquipmentData equipmentData && equipmentData.equipmentConfig.type != itemType) continue;

            var packageCeil = Instantiate(_ceil, content);
            packageCeil.InitData(data);
        }
    }

    private void InitClickEvent()
    {
        UICloseBtn.GetComponent<Button>().onClick.AddListener(OnClose);
    }

    private void OnOpen()
    {
        UIManager.Instance.OpenPanel(UIConst.PackagePanel);
    }

    private void OnClose()
    {
        UIManager.Instance.ClosePanel(UIConst.PackagePanel);
    }

    private void Init()
    {
        UIMenu = transform.Find("TopCenter/Menu");
        UIMenuWeapon = transform.Find("TopCenter/Menu/Weapon");
        UIMenuConsumable = transform.Find("TopCenter/Menu/Consumable");

        UICloseBtn = transform.Find("RightTop/btnClose");
        UICenter = transform.Find("Center");
        UIScrollView = transform.Find("Center/Scroll View");
        UIDetailPanel = transform.Find("Center/DetailPanel");
        UIDeleteBtn = transform.Find("Bottom/btnDelete");

        UICloseBtnDimBg = transform.Find("DimBg");
    }
}