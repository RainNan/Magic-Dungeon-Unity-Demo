using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Skill Config", menuName = "Create Config/Skill")]
public class SkillConfig : ScriptableObject
{
    public SkillName name;
    public float dmg;
    public float speed;
    public float range;
    public float costMp;

    public GameObject prefab;
    public Sprite icon;
}

public enum SkillName
{
    Crystal,
    Thunder,
    Circle,
    Flame,
    Rune
}