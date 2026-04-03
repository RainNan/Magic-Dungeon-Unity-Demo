using UnityEditor;
using UnityEngine;


public class GMCMD
{
#if UNITY_EDITOR
    [MenuItem("GMCMD/导入背包物品")]
    public static void SaveItems()
    {
        var common = Resources.Load<ItemConfig>("Data/CommonCutlass");
        var epic = Resources.Load<ItemConfig>("Data/EpicCutlass");
        var hpPotion = Resources.Load<ItemConfig>("Data/HpPotion");


        for (int i = 0; i < 300; i++)
        {
            PackageManager.Instance.Obtain(common);
            PackageManager.Instance.Obtain(epic);
            PackageManager.Instance.Obtain(hpPotion);
        }


        PackageManager.Instance.Save();
    }

    [MenuItem("GMCMD/读取背包物品")]
    public static void LoadItems()
    {
        // var itemDatas = PackageManager.Instance.Load();
        // foreach (var itemData in itemDatas)
        // {
        //     Debug.Log(itemData);
        // }
    }

    [MenuItem("GMCMD/打开 PackagePanel")]
    public static void OpenPackagePanel()
    {
        UIManager.Instance.OpenPanel(UIConst.PackagePanel);
    }
# endif
}