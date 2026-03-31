using UnityEngine;

public class BasePanel:MonoBehaviour
{
    protected bool isRemove = false;
    protected new string name;

    public virtual void OpenPanel(string name)
    {
        this.name = name;
        gameObject.SetActive(true);
    }

    public virtual void ClosePanel()
    {
        isRemove = true;
        gameObject.SetActive(false);
        // Destroy(gameObject);
        
        // 把已经打开的界面，移除缓存
        // UIManager.Instance.PanelDict.Remove(name);
    }
}
