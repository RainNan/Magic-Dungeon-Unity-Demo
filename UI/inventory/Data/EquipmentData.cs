[System.Serializable]
public class EquipmentData : ItemData
{
    public EquipmentConfig equipmentConfig;

    public bool isEquipped;

    /// <summary>
    /// 用于装备装上的是唯一那个uid的装备
    /// </summary>
    public long uid;

    public EquipmentData(EquipmentConfig equipmentConfig, bool isEquipped, long uid)
    {
        this.equipmentConfig = equipmentConfig;
        this.isEquipped = isEquipped;
        this.uid = uid;
    }
}