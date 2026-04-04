using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Quest Config", menuName = "Create Config/Quest")]
public class QuestConfig : ScriptableObject
{
    public string questName;
    public string questDesc;
    public QuestType questType;
    public long rewardItemId;
    public int rewardItemCount;
}

public enum QuestType
{
    Kill
}