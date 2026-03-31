using System;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentPanel : BasePanel
{
    private Transform UICloseBlankBtn;
    private Transform UIWeapon;
    private Transform UIArmor;

    private BasePanel _detailPanel;
    
    public Sprite defaultSprite;

    private void Awake()
    {
        Init();
        BindBtn();
        Subscribe();
    }

    private void Subscribe()
    {
        EventBus.Instance.Subscribe(EventNames.RefreshEquipment, arg0 => { RefreshSlots(); });
    }

    private void OnEnable()
    {
        RefreshSlots();
    }

    private void OnDisable()
    {
        _detailPanel?.ClosePanel();
    }

    /// <summary>
    /// 加载每个格子
    /// </summary>
    private void RefreshSlots()
    {
        var equipped = PackageManager.Instance.EquipmentEquipped;
        if (equipped.TryGetValue(EquipmentType.Weapon, out var data))
        {
            UIWeapon.transform.Find("Rarity").GetComponent<Image>().sprite =
                data.equipmentConfig.rarity;
            UIWeapon.transform.Find("Icon").GetComponent<Image>().sprite = data.equipmentConfig.icon;
        }
        else
        {
            UIWeapon.transform.Find("Rarity").GetComponent<Image>().sprite = defaultSprite;
            UIWeapon.transform.Find("Icon").GetComponent<Image>().sprite = defaultSprite;
        }
        
        if (equipped.TryGetValue(EquipmentType.Armor, out var data2))
        {
            UIArmor.transform.Find("Rarity").GetComponent<Image>().sprite =
                data2.equipmentConfig.rarity;
            UIArmor.transform.Find("Icon").GetComponent<Image>().sprite = data2.equipmentConfig.icon;
        }
        else
        {
            UIArmor.transform.Find("Rarity").GetComponent<Image>().sprite = defaultSprite;
            UIArmor.transform.Find("Icon").GetComponent<Image>().sprite = defaultSprite;
        }

        UIManager.Instance.ClosePanel(UIConst.DetailPanel);
    }

    private void BindBtn()
    {
        UICloseBlankBtn.GetComponent<Button>().onClick.AddListener(OnClose);

        UIWeapon.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (PackageManager.Instance.EquipmentEquipped.TryGetValue(EquipmentType.Weapon, out var data))
            {
                // _detailPanel =
                // UIManager.Instance.OpenPanel(UIConst.DetailPanel, UIWeapon.GetComponent<RectTransform>());
                EventBus.Instance.Publish(EventNames.OpenDetail, data);
                _detailPanel = UIManager.Instance.OpenPanel(UIConst.DetailPanel);
            }
        });
    }

    private void OnClose()
    {
        UIManager.Instance.ClosePanel(name);
    }

    private void Init()
    {
        UICloseBlankBtn = transform.Find("DimBg");
        UIWeapon = transform.Find("Center/Weapon");
        UIArmor = transform.Find("Center/Armor");
    }
}