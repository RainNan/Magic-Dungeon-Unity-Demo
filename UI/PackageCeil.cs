using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///  对应于背包每一个item
/// </summary>
public class PackageCeil : MonoBehaviour
{
    private Transform UISelect;
    private Transform UIDeleteSelect;
    private Transform UIRarity;
    private Transform UIIcon;
    private Transform UICount;

    private ItemData itemData;

    private void Awake()
    {
        Init();
        BindBtn();
        Subscribe();
    }

    private void Subscribe()
    {
        EventBus.Instance.Subscribe(EventNames.CloseSelect, (o) =>
        {
            if (UISelect)
                UISelect.gameObject.SetActive(false);
        });
    }

    private void BindBtn()
    {
        var btnOpenDetail = GetComponent<Button>();
        btnOpenDetail.onClick.AddListener(() =>
        {
            if (itemData == null)
                return;

            EventBus.Instance.Publish(EventNames.OpenDetail, itemData);
            EventBus.Instance.Publish(EventNames.CloseSelect);
            if (UISelect != null)
                UISelect.gameObject.SetActive(true);
        });
    }

    private void Init()
    {
        UISelect = transform.Find("Select");
        UIDeleteSelect = transform.Find("DeletedSelect");
        UIRarity = transform.Find("Rarity");
        UIIcon = transform.Find("Icon");
        UICount = transform.Find("Count");

        if (UISelect != null)
            UISelect.gameObject.SetActive(false);
    }

    public void InitData(ItemData itemData)
    {
        if (itemData == null || UIIcon == null || UIRarity == null || UICount == null)
            return;

        var icon = UIIcon.GetComponent<Image>();
        var countText = UICount.GetComponent<TextMeshProUGUI>();

        ItemConfig config = null;

        if (itemData.itemConfig)
        {
            config = itemData.itemConfig;
        }
        else if (itemData is EquipmentData equipmentData)
        {
            config = equipmentData.equipmentConfig;
        }

        if (config == null)
            return;

        icon.sprite = config.icon;
        icon.transform.localScale = new Vector3(0.85268f, 0.85268f, 0.85268f);

        var rarity = UIRarity.GetComponent<Image>();
        rarity.sprite = config.rarity;

        if (UISelect != null)
            UISelect.gameObject.SetActive(false);

        if (UIDeleteSelect != null)
            UIDeleteSelect.gameObject.SetActive(false);

        // 装备不显示堆叠数量
        if (itemData is not EquipmentData)
        {
            countText.gameObject.SetActive(true);
            countText.text = itemData.count.ToString();
        }
        else
        {
            countText.gameObject.SetActive(false);
        }


        this.itemData = itemData;
    }
}
