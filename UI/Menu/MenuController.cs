using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    private const string ManifestUrl = "http://101.42.184.122/uploads/ab/PC/Manifest.txt";
    private const string DownloadBaseUrl = "http://101.42.184.122/uploads/ab/PC/";
    private const string GameSceneName = "Game";

    [SerializeField]
    private Button _btnStart;

    [SerializeField]
    private Button _btnContinue;

    [SerializeField]
    private Button _btnSetting;

    [SerializeField]
    private Button _btnExit;

    [SerializeField]
    private Slider _downloadProgress;

    [SerializeField]
    private TMP_Text _progressText;


    [Header("打开进度条，隐藏其他")]
    [SerializeField]
    private Transform _title;

    [SerializeField]
    private Transform _buttons;

    [SerializeField]
    private Transform _progress;

    private bool _isStarting;

    private string BundleDirectoryPath => Path.Combine(Application.persistentDataPath, "AssetBundles", "PC");

    private void Start()
    {
        _btnStart.onClick.AddListener(OnStart);
        _btnExit.onClick.AddListener(OnExit);
        UpdateProgress(0f, "Ready");
        
        _progress.gameObject.SetActive(false);
        
        Debug.Log(Application.persistentDataPath);
    }

    private void OnStart()
    {
        if (_isStarting)
        {
            return;
        }
        
        ShowProgress();

        HideOthers();

        StartCoroutine(StartGameFlow());

    }

    private void ShowProgress()
    {
        _progress.gameObject.SetActive(true);
    }

    private void HideOthers()
    {
        _title.gameObject.SetActive(false);
        _buttons.gameObject.SetActive(false);
    }

    private void OnExit() => Application.Quit();

    private IEnumerator StartGameFlow()
    {
        _isStarting = true;
        SetButtonsInteractable(false);

        yield return StartCoroutine(UpdateAssetBundles());

        if (!_isStarting)
        {
            yield break;
        }

        yield return StartCoroutine(LoadGameScene());
    }

    private IEnumerator UpdateAssetBundles()
    {
        Directory.CreateDirectory(BundleDirectoryPath);

        UpdateProgress(0f, "Checking manifest...");
        using UnityWebRequest manifestRequest = UnityWebRequest.Get(ManifestUrl);
        yield return manifestRequest.SendWebRequest();

        if (manifestRequest.result != UnityWebRequest.Result.Success)
        {
            FailStart($"Manifest download failed: {manifestRequest.error}");
            yield break;
        }

        string manifestContent = manifestRequest.downloadHandler.text;
        List<ManifestEntry> remoteManifest = ParseManifest(manifestContent);
        if (remoteManifest.Count == 0)
        {
            FailStart("Manifest is empty.");
            yield break;
        }

        List<ManifestEntry> needDownload = remoteManifest
            .Where(entry => NeedsDownload(entry))
            .ToList();

        if (needDownload.Count == 0)
        {
            File.WriteAllText(Path.Combine(BundleDirectoryPath, "Manifest.txt"), manifestContent);
            UpdateProgress(1f, "Assets are up to date");
            yield break;
        }

        for (int i = 0; i < needDownload.Count; i++)
        {
            ManifestEntry entry = needDownload[i];
            string fileUrl = DownloadBaseUrl + entry.FileName;
            string savePath = Path.Combine(BundleDirectoryPath, entry.FileName);
            string tempPath = savePath + ".download";

            UpdateProgress((float)i / needDownload.Count, $"Downloading {entry.FileName}...");

            using UnityWebRequest fileRequest = new UnityWebRequest(fileUrl, UnityWebRequest.kHttpVerbGET)
            {
                downloadHandler = new DownloadHandlerFile(tempPath)
            };

            UnityWebRequestAsyncOperation operation = fileRequest.SendWebRequest();
            while (!operation.isDone)
            {
                float progress = (i + fileRequest.downloadProgress) / needDownload.Count;
                UpdateProgress(progress, $"Downloading {entry.FileName}...");
                yield return null;
            }

            if (fileRequest.result != UnityWebRequest.Result.Success)
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }

                FailStart($"File download failed: {entry.FileName}\n{fileRequest.error}");
                yield break;
            }

            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }

            File.Move(tempPath, savePath);
            UpdateProgress((float)(i + 1) / needDownload.Count, $"Downloaded {entry.FileName}");
        }

        File.WriteAllText(Path.Combine(BundleDirectoryPath, "Manifest.txt"), manifestContent);
    }

    private IEnumerator LoadGameScene()
    {
        UpdateProgress(1f, "Loading game...");
        AsyncOperation operation = SceneManager.LoadSceneAsync(GameSceneName);
        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            UpdateProgress(progress, "Loading game...");
            yield return null;
        }
    }

    private bool NeedsDownload(ManifestEntry entry)
    {
        string filePath = Path.Combine(BundleDirectoryPath, entry.FileName);
        if (!File.Exists(filePath))
        {
            return true;
        }

        string localMd5 = ABManager.GetMD5(filePath);
        return !string.Equals(localMd5, entry.Md5, StringComparison.OrdinalIgnoreCase);
    }

    private List<ManifestEntry> ParseManifest(string manifestContent)
    {
        return manifestContent
            .Split('\n')
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrEmpty(line))
            .Select(line => line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            .Where(parts => parts.Length >= 2)
            .Select(parts => new ManifestEntry(parts[0], parts[1]))
            .ToList();
    }

    private void UpdateProgress(float progress, string message)
    {
        if (_downloadProgress != null)
        {
            _downloadProgress.value = progress;
        }

        if (_progressText != null)
        {
            _progressText.text = $"{message} {(int)(progress * 100)}%";
        }
    }

    private void FailStart(string message)
    {
        Debug.LogError(message);
        UpdateProgress(0f, "Start failed");
        SetButtonsInteractable(true);
        _isStarting = false;
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (_btnStart != null) _btnStart.interactable = interactable;
        if (_btnContinue != null) _btnContinue.interactable = interactable;
        if (_btnSetting != null) _btnSetting.interactable = interactable;
        if (_btnExit != null) _btnExit.interactable = interactable;
    }

    private readonly struct ManifestEntry
    {
        public ManifestEntry(string fileName, string md5)
        {
            FileName = fileName;
            Md5 = md5;
        }

        public string FileName { get; }
        public string Md5 { get; }
    }
}
