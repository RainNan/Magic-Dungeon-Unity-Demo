using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 背包单例
/// </summary>
public class PackageManager
{
    /// <summary>
    /// 持有物品，id是配置id，允许堆叠
    /// </summary>
    private Dictionary<long, ItemData> _itemDict = new Dictionary<long, ItemData>();

    /// <summary>
    /// 持有装备，id是随机生成的uid，不允许堆叠
    /// </summary>
    private Dictionary<long, EquipmentData> _equipmentDict = new Dictionary<long, EquipmentData>();

    /// <summary>
    /// 穿戴的装备
    /// </summary>
    private Dictionary<EquipmentType, EquipmentData>
        _equipmentEquipped = new Dictionary<EquipmentType, EquipmentData>();
    public Dictionary<EquipmentType, EquipmentData> EquipmentEquipped => _equipmentEquipped;

    private static PackageManager _instance;
    public static PackageManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new PackageManager();
                Subcribe();
            }

            return _instance;
        }
    }

    private static void Subcribe()
    {
        EventBus.Instance.Subscribe(EventNames.PickUp, (o) =>
        {
            if (o is ItemConfig itemConfig)
            {
                Instance.Obtain(itemConfig);
            }
        });
    }


    /// <summary>
    ///  获得物品
    /// </summary>
    /// <param name="item"></param>
    public void Obtain(ItemConfig itemConfig)
    {
        if (itemConfig is EquipmentConfig equipmentConfig)
        {
            var uid = (long)Random.Range(1, 999999999L);
            _equipmentDict.Add(uid, new EquipmentData(equipmentConfig, false, uid));
            return;
        }

        if (_itemDict.TryGetValue(itemConfig.id, out var data))
        {
            data.count++;
            return;
        }

        _itemDict.Add(itemConfig.id, new ItemData(itemConfig, 1));
    }

    public void Use(ItemData data)
    {
        if (_itemDict.TryGetValue(data.itemConfig.id, out var value))
        {
            if (value.count <= 0) return;
            value.count--;
        }
    }

    /// <summary>
    /// 装备
    /// </summary>
    /// <param name="uid">装备uid</param>
    /// <returns>是否成功</returns>
    public bool Equip(long uid)
    {
        if (!_equipmentDict.TryGetValue(uid, out var data))
        {
            Debug.LogError("equip not found!");
            return false;
        }

        if (_equipmentEquipped.TryGetValue(data.equipmentConfig.equipmentType, out var value))
        {
            Debug.LogError("already equipped same type!");
            return false;
        }

        data.isEquipped = true;
        _equipmentEquipped.Add(data.equipmentConfig.equipmentType, data);

        EventBus.Instance.Publish(EventNames.OnEquipmentChanged);
        EventBus.Instance.Publish(EventNames.OnEquipWithModel, data);

        return true;
    }

    public bool UnEquip(long uid)
    {
        if (!_equipmentDict.TryGetValue(uid, out var data))
        {
            Debug.LogError("equip not found!");
            return false;
        }

        data.isEquipped = false;
        _equipmentEquipped.Remove(data.equipmentConfig.equipmentType);

        EventBus.Instance.Publish(EventNames.OnEquipmentChanged);
        EventBus.Instance.Publish(EventNames.OnEquipWithModel, null);


        return true;
    }

    public void Save()
    {
        var json = JsonUtility.ToJson(this);
        PlayerPrefs.SetString("PackageData", json);
        PlayerPrefs.Save();
    }

    public List<ItemData> GetEquipments()
    {
        var res = new List<ItemData>();
        foreach (var keyValuePair in _equipmentDict)
            res.Add(keyValuePair.Value);

        return res;
    }

    public List<ItemData> GetItems()
    {
        var res = new List<ItemData>();
        foreach (var keyValuePair in _itemDict)
            res.Add(keyValuePair.Value);

        return res;
    }

    public void OnEquipmentChanged()
    {
    }
}