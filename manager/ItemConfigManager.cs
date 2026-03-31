using System.Collections.Generic;
using UnityEngine;

public class ItemConfigManager
{
    private static Dictionary<long, ItemConfig> _itemDic;

    private static ItemConfigManager _instance;
    public static ItemConfigManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ItemConfigManager();
                Init();
            }

            return _instance;
        }
    }

    private static void Init()
    {
        _itemDic = new Dictionary<long, ItemConfig>();

        var configs = Resources.LoadAll<ItemConfig>("Config");
        
        foreach (var config in configs)
            _itemDic.Add(config.id, config);
    }

    public ItemConfig GetById(long id)
    {
        return _itemDic.GetValueOrDefault(id);
    }
}