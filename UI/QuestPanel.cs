using System;
using TMPro;
using UnityEngine;

public class QuestPanel:BasePanel
{
    public Transform left; // 挂 Quest
    public Transform right; // 挂 QuestDesc 和 QuestProgress
    
    
    private GameObject _questPrefab;
    private GameObject _questDescPrefab;
    private GameObject _questProgressPrefab;

    private void Start()
    {
        _questPrefab = Resources.Load<GameObject>("Prefabs/Package/Quest/Quest");
        _questDescPrefab = Resources.Load<GameObject>("Prefabs/Package/Quest/questDesc");
        _questProgressPrefab = Resources.Load<GameObject>("Prefabs/Package/Quest/questProgress");

        left = GameObject.Find("left").transform;
        right = GameObject.Find("right").transform;
    }


    public override void OpenPanel(string name)
    {
        base.OpenPanel(name);

        var firstCfg = QuestConfigLoader.QuestConfigs[0];
        var desc = Instantiate(_questDescPrefab, right);
        desc.GetComponent<TextMeshProUGUI>().text = firstCfg.questDesc;
        
        var progress = Instantiate(_questDescPrefab, right);
        desc.GetComponent<TextMeshProUGUI>().text = firstCfg.questDesc;
        

        foreach (var questConfig in QuestConfigLoader.QuestConfigs)
        {
            var quest = Instantiate(_questPrefab, left);
            quest.GetComponent<TextMeshProUGUI>().text = questConfig.questName;
        }
        
        
        
        
    }
}
