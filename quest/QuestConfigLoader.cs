using System.Collections.Generic;
using UnityEngine;

public class QuestConfigLoader
{
    private static QuestConfigLoader _instance;
    public static QuestConfigLoader Instance
    {
        get
        {
            if (_instance is null)
            {
                Init();
                _instance = new();
            }

            return _instance;
        }
    }
    private static QuestConfig[] _questConfigs;

    public static QuestConfig[] QuestConfigs
    {
        get
        {
            if (_questConfigs == null)
                Init();

            return _questConfigs;
        }
    }

    private static void Init() =>
        _questConfigs = Resources.LoadAll<QuestConfig>("Prefabs/Package/Quest");
}