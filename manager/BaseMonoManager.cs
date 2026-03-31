using UnityEngine;

public class BaseMonoManager : MonoBehaviour
{
    private static readonly object _lock = new object();

    private static BaseMonoManager _instance;

    public static BaseMonoManager Instance
    {
        get
        {
            if (_instance is null)
            {
                lock (_lock)
                {
                    if (_instance is null)
                    {
                        GameObject managerObj = GameObject.Find("[Manager]");

                        if (managerObj == null)
                        {
                            // 创建持久化的单例对象
                            managerObj = new GameObject("[Manager]");
                            DontDestroyOnLoad(managerObj);
                        }

                        // 获取或添加组件
                        _instance = managerObj.GetComponent<BaseMonoManager>();
                        if (_instance is null)
                        {
                            _instance = managerObj.AddComponent<BaseMonoManager>();
                        }
                    }
                }
            }

            return _instance;
        }
    }
}