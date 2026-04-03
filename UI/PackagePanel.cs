using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PackagePanel : BasePanel
{
    private Transform UIMenu;
    private Transform UIMenuWeapon;
    private Transform UIMenuConsumable;

    private Transform UICloseBtn;
    private Transform UICloseBtnDimBg;

    private Transform UICenter;
    private Transform UIScrollView;
    private Transform UIDetailPanel;
    private Transform UILeftBtn;
    private Transform UIRightBtn;
    private Transform UIDeletePanel;
    private Transform UIDeleteInfoText;
    private Transform UIDeleteConfirmBtn;
    private Transform UIBottomMenus;
    private Transform UIDeleteBtn;
    private Transform UIDetailBtn;
    
    // 无限列表
    public ScrollRect scrollRect;
    public RectTransform content;

    public int buffer = 3;
    public int spacingX = 20;
    public int spacingY = 25;
    public int paddingLeft = 40;
    public int paddingTop = 20;

    private float _itemWidth;
    private float _itemHeight;
    private readonly Dictionary<int, GameObject> _activeItems = new();
    private Queue<GameObject> _pool = new();
    private readonly List<ItemData> _currentItems = new();
    private int _lastStartIndex = -1;
    private int _lastEndIndex = -1;
    private bool _scrollEventBound;
    private int _columnCount = 1;
    private float _rowHeight;
    
    [SerializeField]
    private GameObject _itemPrefab;

    private void Awake()
    {
        Init();
        InitClickEvent();
        BindBtn();

        Subcribe();
    }

    private void Start()
    {
        EnsureVirtualListReady();
        BindScrollEvent();
        RebuildItems(true);
    }

    private void EnsureVirtualListReady()
    {
        if (_itemHeight <= 0f || _itemWidth <= 0f)
        {
            var rect = _itemPrefab.GetComponent<RectTransform>().rect;
            _itemWidth = rect.width;
            _itemHeight = rect.height;
            _rowHeight = _itemHeight + spacingY;
        }

        EnsurePoolSize();
    }

    private void RefreshGridInfo()
    {
        float availableWidth = content.rect.width - paddingLeft;
        float cellWidth = _itemWidth + spacingX;
        _columnCount = Mathf.Max(1, Mathf.FloorToInt((availableWidth + spacingX) / cellWidth));
        _rowHeight = _itemHeight + spacingY;
    }

    private void BindScrollEvent()
    {
        if (_scrollEventBound)
            return;

        scrollRect.onValueChanged.AddListener(_ => RefreshVisibleItems());
        _scrollEventBound = true;
    }

    private void EnsurePoolSize()
    {
        RefreshGridInfo();

        int visibleRowCount = Mathf.CeilToInt(scrollRect.viewport.rect.height / _rowHeight);
        int targetPoolSize = Mathf.Max((visibleRowCount + buffer * 2 + 2) * _columnCount, _columnCount);

        while (_pool.Count < targetPoolSize)
        {
            var item = Instantiate(_itemPrefab, content);
            item.SetActive(false);
            _pool.Enqueue(item);
        }
    }

    private void RebuildItems(bool resetScrollPosition)
    {
        EnsureVirtualListReady();
        RefreshGridInfo();

        _currentItems.Clear();

        if (UIMenuWeapon.transform.Find("Selected").gameObject.activeSelf)
        {
            foreach (var data in PackageManager.Instance.GetEquipments())
            {
                if (data is EquipmentData equipmentData && equipmentData.equipmentConfig != null)
                    _currentItems.Add(equipmentData);
            }
        }
        else
        {
            foreach (var data in PackageManager.Instance.GetItems())
            {
                if (data.itemConfig == null || data.count <= 0)
                    continue;

                _currentItems.Add(data);
            }
        }

        int rowCount = Mathf.CeilToInt(_currentItems.Count / (float)_columnCount);
        float contentHeight = rowCount <= 0 ? 0f : paddingTop + rowCount * _itemHeight + Mathf.Max(0, rowCount - 1) * spacingY;
        content.sizeDelta = new Vector2(content.sizeDelta.x, contentHeight);

        if (resetScrollPosition)
            content.anchoredPosition = new Vector2(content.anchoredPosition.x, 0f);

        RecycleAllItems();
        RefreshVisibleItems(true);
    }

    private void RecycleAllItems()
    {
        foreach (var pair in _activeItems)
        {
            pair.Value.SetActive(false);
            _pool.Enqueue(pair.Value);
        }

        _activeItems.Clear();
        _lastStartIndex = -1;
        _lastEndIndex = -1;
    }

    private void RefreshVisibleItems()
    {
        RefreshVisibleItems(false);
    }

    private void RefreshVisibleItems(bool forceRefresh)
    {
        if (_currentItems.Count == 0)
        {
            RecycleAllItems();
            return;
        }

        float contentY = Mathf.Max(0f, content.anchoredPosition.y);
        int startRow = Mathf.Max(0, Mathf.FloorToInt(contentY / _rowHeight) - buffer);
        int visibleRowCount = Mathf.CeilToInt(scrollRect.viewport.rect.height / _rowHeight);
        int endRow = startRow + visibleRowCount + buffer * 2;
        int startIndex = startRow * _columnCount;
        int endIndex = Mathf.Min(_currentItems.Count - 1, ((endRow + 1) * _columnCount) - 1);

        if (!forceRefresh && startIndex == _lastStartIndex && endIndex == _lastEndIndex)
            return;

        var recycleIndices = new List<int>();
        foreach (var pair in _activeItems)
        {
            if (pair.Key < startIndex || pair.Key > endIndex)
                recycleIndices.Add(pair.Key);
        }

        foreach (var index in recycleIndices)
        {
            GameObject item = _activeItems[index];
            item.SetActive(false);
            _pool.Enqueue(item);
            _activeItems.Remove(index);
        }

        for (int i = startIndex; i <= endIndex; i++)
        {
            if (_activeItems.ContainsKey(i))
                continue;

            GameObject item = _pool.Count > 0 ? _pool.Dequeue() : Instantiate(_itemPrefab, content);
            item.name = $"Item_{i}";
            item.SetActive(true);

            RectTransform rt = item.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0.5f, 0.5f);

            int row = i / _columnCount;
            int column = i % _columnCount;
            float x = paddingLeft + column * (_itemWidth + spacingX) + _itemWidth * 0.5f;
            float y = -(paddingTop + row * (_itemHeight + spacingY) + _itemHeight * 0.5f);
            rt.anchoredPosition = new Vector2(x, y);

            var packageCeil = item.GetComponent<PackageCeil>();
            if (packageCeil != null)
                packageCeil.InitData(_currentItems[i]);

            _activeItems[i] = item;
        }
        
        _lastStartIndex = startIndex;
        _lastEndIndex = endIndex;
    }


    private void OnEnable()
    {
        if (!gameObject.activeInHierarchy)
            return;

        RebuildItems(true);
    }

    private void Subcribe()
    {
        EventBus.Instance.Subscribe(EventNames.CloseSelectMenu, (o) =>
        {
            UIMenuWeapon.transform.Find("Selected").gameObject.SetActive(false);
            UIMenuConsumable.transform.Find("Selected").gameObject.SetActive(false);
        });

        EventBus.Instance.Subscribe(EventNames.RefreshPackage, (o) =>
        {
            RebuildItems(false);
        });
    }

    private void BindBtn()
    {
        UIMenuWeapon.GetComponent<Button>().onClick.AddListener(() =>
        {
            EventBus.Instance.Publish(EventNames.CloseSelectMenu);

            UIMenuWeapon.transform.Find("Selected").gameObject.SetActive(true);
            RebuildItems(true);
            
        });
        

        UIMenuConsumable.GetComponent<Button>().onClick.AddListener(() =>
        {
            EventBus.Instance.Publish(EventNames.CloseSelectMenu);

            UIMenuConsumable.transform.Find("Selected").gameObject.SetActive(true);
            RebuildItems(true);
        });

        UICloseBtnDimBg.GetComponent<Button>().onClick.AddListener(OnClose);
    }

    private void InitClickEvent()
    {
        UICloseBtn.GetComponent<Button>().onClick.AddListener(OnClose);
    }

    private void OnOpen()
    {
        UIManager.Instance.OpenPanel(UIConst.PackagePanel);
    }

    private void OnClose()
    {
        UIManager.Instance.ClosePanel(UIConst.PackagePanel);
    }

    private void Init()
    {
        UIMenu = transform.Find("TopCenter/Menu");
        UIMenuWeapon = transform.Find("TopCenter/Menu/Weapon");
        UIMenuConsumable = transform.Find("TopCenter/Menu/Consumable");

        UICloseBtn = transform.Find("RightTop/btnClose");
        UICenter = transform.Find("Center");
        UIScrollView = transform.Find("Center/Scroll View");
        UIDetailPanel = transform.Find("Center/DetailPanel");
        UIDeleteBtn = transform.Find("Bottom/btnDelete");

        UICloseBtnDimBg = transform.Find("DimBg");
    }
}
