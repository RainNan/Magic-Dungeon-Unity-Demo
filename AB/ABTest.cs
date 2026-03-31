using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ABTest : MonoBehaviour
{
    private string mainBundleName = "StandaloneWindows";

    private void Start()
    {
        var ab = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + "Weapon");
        var cutlass1 = ab.LoadAsset<GameObject>("Cutlass1");
        Instantiate(cutlass1, Vector3.zero, Quaternion.identity);

        AssetBundle.UnloadAllAssetBundles(false);

        ab.Unload(false);

        var bundleName = Application.streamingAssetsPath + "/" + "Weapon";
        var resourceName = "Cutlass1";

        StartCoroutine(LoadResourceAsyc(bundleName, resourceName));
    }

    IEnumerator LoadResourceAsyc(string bundleName, string resourceName)
    {
        AssetBundleCreateRequest abcr = AssetBundle.LoadFromFileAsync(bundleName);
        yield return abcr;

        AssetBundleRequest abr = abcr.assetBundle.LoadAssetAsync<GameObject>(resourceName);
        yield return abr;

        GameObject loadedObj = abr.asset as GameObject;
        if (loadedObj) Instantiate(loadedObj);
    }

    private void LoadDependencies(string bundleName)
    {
        var abMain = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + mainBundleName);
        var abManifest = abMain.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        var dependencies = abManifest.GetAllDependencies(bundleName);

        foreach (var dependency in dependencies)
            AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + dependency);
    }
    
    
}