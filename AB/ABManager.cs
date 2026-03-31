using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using FileMode = System.IO.FileMode;
using Object = UnityEngine.Object;

public class ABManager : MonoBehaviour
{
    private static readonly object _lock = new object();

    private static ABManager _instance;

    public static ABManager Instance
    {
        get
        {
            if (_instance is null)
            {
                lock (_lock)
                {
                    if (_instance is null)
                    {
                        // 查找场景中是否已有ABManager对象
                        GameObject managerObj = GameObject.Find("[ABManager]");

                        if (managerObj == null)
                        {
                            // 创建持久化的单例对象
                            managerObj = new GameObject("[ABManager]");
                            DontDestroyOnLoad(managerObj);
                        }

                        // 获取或添加组件
                        _instance = managerObj.GetComponent<ABManager>();
                        if (_instance is null)
                        {
                            _instance = managerObj.AddComponent<ABManager>();
                        }
                    }
                }
            }

            return _instance;
        }
    }

    private Dictionary<string, AssetBundle> _loadedBundleDic = new(); // 防止重复加载报错
    private AssetBundle _main;
    private AssetBundleManifest _manifest;
    private Dictionary<string, string> _bundleNameLookup;

    private string PathURL
    {
        get
        {
            string runtimePath = Path.Combine(Application.persistentDataPath, "AssetBundles", "PC");
            if (Directory.Exists(runtimePath) && File.Exists(Path.Combine(runtimePath, MainABName)))
            {
                return runtimePath;
            }

            return Path.Combine(Application.dataPath, "Bundles", "PC");
        }
    }

    private string MainABName
    {
        get
        {
#if UNITY_IOS
            return "IOS";
#elif UNITY_ANDROID
            return "Android";
#else
            return "PC";
#endif
        }
    }

    private void LoadAB(string abName)
    {
        string normalizedAbName = NormalizeBundleName(abName);

        // 加载主包
        if (_main is null)
        {
            string mainBundlePath = Path.Combine(PathURL, MainABName);
            _main = AssetBundle.LoadFromFile(mainBundlePath);
            if (_main is null)
            {
                throw new FileNotFoundException($"Main AssetBundle load failed: {mainBundlePath}");
            }
        }

        if (_manifest is null)
        {
            _manifest = _main.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            if (_manifest is null)
            {
                throw new InvalidOperationException($"AssetBundleManifest load failed from main bundle: {MainABName}");
            }

            _bundleNameLookup = _manifest.GetAllAssetBundles()
                .ToDictionary(bundleName => bundleName, bundleName => bundleName, StringComparer.OrdinalIgnoreCase);
        }

        // 加载对应依赖
        foreach (var dependency in _manifest.GetAllDependencies(normalizedAbName))
        {
            if (!_loadedBundleDic.ContainsKey(dependency))
            {
                string dependencyPath = Path.Combine(PathURL, dependency);
                var loadFromFile = AssetBundle.LoadFromFile(dependencyPath);
                if (loadFromFile is null)
                {
                    throw new FileNotFoundException($"Dependency AssetBundle load failed: {dependencyPath}");
                }

                _loadedBundleDic[dependency] = loadFromFile;
            }
        }

        // 加载指定包的指定资源
        if (!_loadedBundleDic.ContainsKey(normalizedAbName))
        {
            string bundlePath = Path.Combine(PathURL, normalizedAbName);
            var ab = AssetBundle.LoadFromFile(bundlePath);
            if (ab is null)
            {
                throw new FileNotFoundException($"AssetBundle load failed: {bundlePath}");
            }

            _loadedBundleDic[normalizedAbName] = ab;
        }
    }

    #region 同步加载

    public Object LoadRes(string abName, string resName)
    {
        string normalizedAbName = NormalizeBundleName(abName);
        LoadAB(normalizedAbName);

        AssetBundle bundle = _loadedBundleDic[normalizedAbName];
        string resolvedAssetName = ResolveAssetName(bundle, resName);
        var asset = bundle.LoadAsset(resolvedAssetName);
        if (asset is null)
        {
            Debug.LogError($"Asset load failed. bundle={normalizedAbName}, asset={resName}, resolved={resolvedAssetName}");
            return null;
        }
        
        return asset;
    }

    // 因为lua不支持泛型，所以指定type加载
    public Object LoadRes(string abName, string resName, Type type)
    {
        string normalizedAbName = NormalizeBundleName(abName);
        LoadAB(normalizedAbName);

        AssetBundle bundle = _loadedBundleDic[normalizedAbName];
        string resolvedAssetName = ResolveAssetName(bundle, resName);
        var asset = bundle.LoadAsset(resolvedAssetName, type);
        if (asset is null)
        {
            Debug.LogError($"Asset load failed. bundle={normalizedAbName}, asset={resName}, resolved={resolvedAssetName}, type={type?.Name}");
            return null;
        }

        return asset;
    }

    public T LoadRes<T>(string abName, string resName) where T : Object
    {
        string normalizedAbName = NormalizeBundleName(abName);
        LoadAB(normalizedAbName);

        AssetBundle bundle = _loadedBundleDic[normalizedAbName];
        string resolvedAssetName = ResolveAssetName(bundle, resName);
        T asset = bundle.LoadAsset<T>(resolvedAssetName);
        if (asset is null)
        {
            Debug.LogError($"Asset load failed. bundle={normalizedAbName}, asset={resName}, resolved={resolvedAssetName}, type={typeof(T).Name}");
            return null;
        }

        return asset;
    }

    #endregion

    #region 异步加载

    public void LoadResAsync(string abName, string resName, UnityAction<Object> callback)
    {
        StartCoroutine(LoadResAsyncCoroutine(abName, resName, callback, null));
    }


    public void LoadResAsync(string abName, string resName, UnityAction<Object> callback, Type type) =>
        StartCoroutine(LoadResAsyncCoroutine(abName, resName, callback, type));

    public void LoadResAsync<T>(string abName, string resName, UnityAction<Object> callback) =>
        StartCoroutine(LoadResAsyncCoroutine<T>(abName, resName, callback));
    
    public void LoadAllResAsync<T>(string abName, UnityAction<Object[]> callback) =>
        StartCoroutine(LoadAllResAsyncCoroutine<T>(abName, callback));
    
    private IEnumerator LoadResAsyncCoroutine(string abName, string resName, UnityAction<Object> callback,
        Type type)
    {
        string normalizedAbName = NormalizeBundleName(abName);
        LoadAB(normalizedAbName);
        AssetBundle bundle = _loadedBundleDic[normalizedAbName];
        string resolvedAssetName = ResolveAssetName(bundle, resName);
        AssetBundleRequest abr;
        if (type is null)
            abr = bundle.LoadAssetAsync(resolvedAssetName);
        else
            abr = bundle.LoadAssetAsync(resolvedAssetName, type);

        yield return abr;

        if (abr.asset is null)
        {
            Debug.LogError($"Async asset load failed. bundle={normalizedAbName}, asset={resName}, resolved={resolvedAssetName}, type={type?.Name}");
            callback(null);
            yield break;
        }

        callback(abr.asset);
    }

    private IEnumerator LoadResAsyncCoroutine<T>(string abName, string resName, UnityAction<Object> callback)
    {
        string normalizedAbName = NormalizeBundleName(abName);
        LoadAB(normalizedAbName);
        AssetBundle bundle = _loadedBundleDic[normalizedAbName];
        string resolvedAssetName = ResolveAssetName(bundle, resName);
        AssetBundleRequest abr;
        abr = bundle.LoadAssetAsync<T>(resolvedAssetName);

        yield return abr;

        if (abr.asset is null)
        {
            Debug.LogError($"Async asset load failed. bundle={normalizedAbName}, asset={resName}, resolved={resolvedAssetName}, type={typeof(T).Name}");
            callback(null);
            yield break;
        }

        callback(abr.asset);
    }

    private IEnumerator LoadAllResAsyncCoroutine<T>(string abName, UnityAction<Object[]> callback)
    {
        string normalizedAbName = NormalizeBundleName(abName);
        LoadAB(normalizedAbName);
        AssetBundle bundle = _loadedBundleDic[normalizedAbName];
        
        AssetBundleRequest abr;
        abr = bundle.LoadAllAssetsAsync<T>();

        yield return abr;

        if (abr.allAssets is null)
        {
            callback(null);
            yield break;
        }

        callback(abr.allAssets);
    }
    
    #endregion

    #region 工具方法

    public static string GetMD5(string filePath)
    {
        using (FileStream file = new FileStream(filePath, FileMode.Open))
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            var hash = md5.ComputeHash(file);

            var sb = new StringBuilder();
            foreach (var b in hash)
                sb.Append(b.ToString("x2"));

            return sb.ToString();
        }
    }

    private string NormalizeBundleName(string abName)
    {
        if (string.IsNullOrWhiteSpace(abName))
        {
            throw new ArgumentException("AssetBundle name is empty.", nameof(abName));
        }

        if (_bundleNameLookup != null && _bundleNameLookup.TryGetValue(abName, out string bundleName))
        {
            return bundleName;
        }

        string exactPath = Path.Combine(PathURL, abName);
        if (File.Exists(exactPath))
        {
            return abName;
        }

        string[] candidateFiles = Directory.Exists(PathURL)
            ? Directory.GetFiles(PathURL)
            : Array.Empty<string>();

        string matchedFile = candidateFiles.FirstOrDefault(filePath =>
            string.Equals(Path.GetFileName(filePath), abName, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(matchedFile))
        {
            return Path.GetFileName(matchedFile);
        }

        throw new FileNotFoundException($"AssetBundle not found: {abName}. Search path: {PathURL}");
    }

    private string ResolveAssetName(AssetBundle bundle, string resName)
    {
        string[] assetNames = bundle.GetAllAssetNames();
        if (assetNames.Length == 0)
        {
            return resName;
        }

        string exactMatch = assetNames.FirstOrDefault(assetName =>
            string.Equals(assetName, resName, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrEmpty(exactMatch))
        {
            return exactMatch;
        }

        string fileNameMatch = assetNames.FirstOrDefault(assetName =>
        {
            string fileName = Path.GetFileNameWithoutExtension(assetName);
            return string.Equals(fileName, resName, StringComparison.OrdinalIgnoreCase);
        });

        if (!string.IsNullOrEmpty(fileNameMatch))
        {
            return fileNameMatch;
        }

        Debug.LogWarning($"Asset name fallback failed. Requested={resName}, candidates={string.Join(", ", assetNames)}");
        return resName;
    }

    #endregion
}
