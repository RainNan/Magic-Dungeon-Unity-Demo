using System.Collections.Generic;
using UnityEngine;

public class SkillManager
{
    private static Dictionary<string, SkillConfig> _skillConfigDic = new();

    private static SkillManager _instance;
    public static SkillManager Instance
    {
        get
        {
            if (_instance is null)
            {
                _instance = new();
                Init();
            }

            return _instance;
        }
    }

    private static void Init()
    {
        var skillConfigs = Resources.LoadAll<SkillConfig>("Data/Skill");
        foreach (var skillConfig in skillConfigs)
        {
            _skillConfigDic.Add(skillConfig.name, skillConfig);
        }
    }

    public SkillConfig GetSkillByName(string name)
    {
        return _skillConfigDic.GetValueOrDefault(name);
    }
}