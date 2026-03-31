using UnityEngine;

[CreateAssetMenu(fileName = "New Equipment Data", menuName = "Create Config/Equipment")]
[System.Serializable]
public class EquipmentConfig : ItemConfig
{
    public float damage;
    public float attackRange;
    public float attackAngle;

    public EquipmentType equipmentType;
}

public enum EquipmentType
{
    Weapon,
    Armor
}