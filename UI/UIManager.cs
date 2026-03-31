using System.Collections.Generic;
using UnityEngine;
public class UIManager
{
    private Dictionary<string, string> _pathDict;

    // 预制件缓存
    private Dictionary<string, GameObject> _prefabDict;

    // 面板缓存
    private Dictionary<string, BasePanel> _panelDict;
    public Dictionary<string, BasePanel> PanelDict => _panelDict;

    private Transform _uiroot;
    public Transform Uiroot
    {
        get
        {
            if (_uiroot == null)
            {
                _uiroot = GameObject.Find("Canvas").transform;
            }

            return _uiroot;
        }
    }

    public UIManager()
    {
        _prefabDict = new Dictionary<string, GameObject>();
        _panelDict = new Dictionary<string, BasePanel>();
        InitDic();
    }

    private static UIManager _instance;
    public static UIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new UIManager();
            }

            return _instance;
        }
    }

    private void InitDic()
    {
        _pathDict = new Dictionary<string, string>()
        {
            { UIConst.PackagePanel, "Package/PackagePanel" },
            { UIConst.EquipmentPanel, "Package/EquipmentPanel" },
            { UIConst.DetailPanel, "Package/DetailPanel" },
        };
    }

    public BasePanel OpenPanel(string name, RectTransform rectTransform)
    {
        // 检查配置
        string path;
        if (!_pathDict.TryGetValue(name, out path))
        {
            Debug.LogError("配置路径有误!");
            return null;
        }

        // 如果有缓存的，使用缓存的
        GameObject prefab;
        if (!_prefabDict.TryGetValue(name, out prefab))
        {
            string allPath = "Prefabs/" + path;
            prefab = Resources.Load<GameObject>(allPath) as GameObject;
            _prefabDict.Add(name, prefab);
        }

        BasePanel panel;
        if (_panelDict.TryGetValue(name, out panel))
        {
            panel.OpenPanel(name);
            return panel;
        }

        if (panel)
        {
        }

        GameObject panelObj = GameObject.Instantiate(prefab, rectTransform, false);
        panel = panelObj.GetComponent<BasePanel>();
        panel.OpenPanel(name);
        _panelDict.Add(name, panel);
        return panel;
    }

    public BasePanel OpenPanel(string name)
    {
        return OpenPanel(name, Uiroot.GetComponent<RectTransform>());
    }

    public bool ClosePanel(string name)
    {
        BasePanel panel;
        if (!_panelDict.TryGetValue(name, out panel))
        {
            Debug.LogError("界面未打开!");
            return false;
        }

        panel.ClosePanel();
        return true;
    }
}

public class UIConst
{
    public static string PackagePanel = "PackagePanel";
    public static string EquipmentPanel = "EquipmentPanel";
    public static string DetailPanel = "DetailPanel";
}
