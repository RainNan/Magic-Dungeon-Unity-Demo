using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DetailPanel : BasePanel
{
    private Transform ItemName;
    private Transform ItemDesc;
    private Transform EquipOrUse;

    private Transform EquipOrUseImg;

    private void Awake()
    {
        Init();
        // BindBtn();
        Subcribe();
    }

    private void OnEnable()
    {
    }

    private void Subcribe()
    {
        EventBus.Instance.Subscribe(EventNames.OpenDetail, OpenDetail);
    }

    private void OpenDetail(object o)
    {
        if (o is not ItemData itemData)
        {
            Debug.LogError("o is not ItemData!");
            return;
        }

        ItemConfig config = itemData.itemConfig;
        if (itemData is EquipmentData equipmentData)
        {
            config = equipmentData.equipmentConfig;
        }

        if (config == null)
        {
            Debug.LogError("item config is null!");
            return;
        }

        ItemName.GetComponent<TextMeshProUGUI>().text = config.name;
        ItemDesc.GetComponent<TextMeshProUGUI>().text = config.desc;
        EquipOrUseImg.GetComponent<Image>().color = Color.white;

        switch (config.type)
        {
            case ItemType.Equipment:
                EquipOrUse.GetComponent<TextMeshProUGUI>().text = "Equip";
                var currentEquipmentData = (EquipmentData)itemData;
                if (currentEquipmentData.isEquipped)
                {
                    EquipOrUse.GetComponent<TextMeshProUGUI>().text = "UnEquip";
                    EquipOrUseImg.GetComponent<Image>().color = Color.red;
                }
                else
                {
                    EquipOrUse.GetComponent<TextMeshProUGUI>().text = "Equip";
                    EquipOrUseImg.GetComponent<Image>().color = Color.green;
                }

                break;
            case ItemType.Consumable:
                EquipOrUse.GetComponent<TextMeshProUGUI>().text = "Use";
                EquipOrUseImg.GetComponent<Image>().color = Color.green;

                break;
        }

        BindBtn(itemData);
    }

    private void BindBtn(ItemData itemData)
    {
        var button = EquipOrUse.GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            if (itemData is EquipmentData equipmentData)
            {
                if (equipmentData.isEquipped)
                {
                    PackageManager.Instance.UnEquip(equipmentData.uid);
                    EventBus.Instance.Publish(EventNames.OpenDetail, equipmentData);
                    EventBus.Instance.Publish(EventNames.OnEquipmentChanged, equipmentData);
                    EventBus.Instance.Publish(EventNames.OnEquipWithModel, null);
                }
                else
                {
                    PackageManager.Instance.Equip(equipmentData.uid);
                    EventBus.Instance.Publish(EventNames.OpenDetail, equipmentData);
                    EventBus.Instance.Publish(EventNames.OnEquipmentChanged, equipmentData);
                    EventBus.Instance.Publish(EventNames.OnEquipWithModel, equipmentData);
                }
            }
            else
            {
                switch (itemData.itemConfig.name)
                {
                    case "HP Potion":
                        EventBus.Instance.Publish(EventNames.AddHp, 50f);
                        PackageManager.Instance.Use(itemData);
                        EventBus.Instance.Publish(EventNames.RefreshPackage);
                        break;
                }
            }

            EventBus.Instance.Publish(EventNames.RefreshEquipment);
        });
    }

    private void Init()
    {
        ItemName = transform.Find("Top/ItemName");
        ItemDesc = transform.Find("Center/ItemDesc");
        EquipOrUse = transform.Find("Bottom/EquipOrUse");

        EquipOrUseImg = transform.Find("Bottom/Image");
    }
}