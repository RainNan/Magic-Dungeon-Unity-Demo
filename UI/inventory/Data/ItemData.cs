
[System.Serializable]
public class ItemData
{
    public ItemConfig itemConfig;
    public int count;


    public ItemData(ItemConfig itemConfig, int count)
    {
        this.itemConfig = itemConfig;
        this.count = count;
    }

    public ItemData()
    {
    }
}
