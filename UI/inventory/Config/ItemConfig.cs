using UnityEngine;

public enum ItemType
{
    Consumable,
    Equipment
}

public enum ItemRarity
{
    Common,
    Epic,
    Legendary
}
[System.Serializable]
[CreateAssetMenu(fileName = "New Item Config", menuName = "Create Config/Item")]
public class ItemConfig : ScriptableObject
{
    public long id;
    public Sprite icon;
    public string name;
    public string desc;
    public ItemType type;
    public Sprite rarity;
    public GameObject prefab;
}
