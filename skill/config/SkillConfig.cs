using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Skill Config", menuName = "Create Config/Skill")]
public class SkillConfig : ScriptableObject
{
    public string name;
    public float dmg;
    public float range;
    public float costMp;

    public GameObject prefab;
    public Sprite icon;
}