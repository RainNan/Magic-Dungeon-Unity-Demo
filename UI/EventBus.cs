using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventBus
{
    public UnityAction OpenDetail;

    private Dictionary<string, UnityAction<object>> _eventDict =
        new Dictionary<string, UnityAction<object>>();

    private static EventBus _instance;
    public static EventBus Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new EventBus();
            }

            return _instance;
        }
    }

    public void Subscribe(string eventName, UnityAction<object> callback)
    {
        if (string.IsNullOrEmpty(eventName))
        {
            Debug.LogError("事件名称不能为空！");
            return;
        }

        if (callback == null)
        {
            Debug.LogError("回调函数不能为空！");
            return;
        }

        // 事件不存在则初始化
        if (!_eventDict.ContainsKey(eventName))
        {
            _eventDict[eventName] = null;
        }

        // 添加回调（UnityAction 支持多播委托，自动累加）
        _eventDict[eventName] += callback;
    }

    public void Publish(string eventName, object data = null)
    {
        if (string.IsNullOrEmpty(eventName))
        {
            Debug.LogError("事件名称不能为空！");
            return;
        }

        // 事件不存在或无回调，直接返回
        if (!_eventDict.ContainsKey(eventName) || _eventDict[eventName] == null)
        {
            return;
        }

        // 执行所有订阅的回调
        try
        {
            _eventDict[eventName].Invoke(data);
        }
        catch (Exception e)
        {
            Debug.LogError($"执行 {eventName} 事件回调失败：{e.Message}");
        }
    }
}

public static class EventNames
{
    public const string OpenDetail = "OpenDetail";
    public const string CloseSelect = "CloseSelect";
    public const string CloseSelectMenu = "CloseSelectMenu";
    public const string RefreshDetailPanel = "RefreshDetailPanel";

    public const string RefreshEquipment = "RefreshEquipment";

    public const string AttributeChanged = "AttributeChanged";

    public const string PickUp = "PickUp"; // 宝箱也是捡起，宝箱要包含其中的物品，可以随机


    public const string AddHp = "AddHp";
    public const string RefreshPackage = "RefreshPackage";

    public const string OnEquipmentChanged = "OnEquipmentChanged"; // 装备的穿脱，影响属性

    public const string OnEquipWithModel = "OnEquipWithModel"; // 穿装备的模型

    // 画图检测成功，魔法圈跟随鼠标移动选择施法区域
    public const string OnOpenMagicCircle = "OnOpenMagicCircle";

    // 释放技能，有参数，是识别出来的技能名
    public const string OnMagic = "OnMagic";
    
    // 小地图
    public const string OnMiniMapInit = "OnMiniMapInit";
    public const string OnMiniMapChanged = "OnMiniMapChanged";
}