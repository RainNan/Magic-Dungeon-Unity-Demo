// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.EventSystems;
// using UnityEngine.UI;
//
// public class Inventory : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
// {
//     [Header("Reference")]
//     [SerializeField]
//     private GameObject itemPrefab;
//
//     [SerializeField]
//     private GameObject pagePrefab;
//
//     [SerializeField]
//     public Transform content; // ScrollView 的 Content 对象
//
//     [Tooltip("不同页之间的偏移")]
//     [SerializeField]
//     private float pageXOffset = 200f; // 旧方案遗留字段，当前自动吸附方案不再使用
//
//     private GameObject _lastPage; // 上一页
//     private readonly List<float> _pageX = new(); // 旧方案遗留字段，当前逻辑中未使用
//
//     [Tooltip("打开背包按钮")]
//     [SerializeField]
//     private Button btnOpenInventory;
//
//     [Tooltip("关闭背包按钮")]
//     [SerializeField]
//     private Button btnCloseInventory;
//
//     [Tooltip("背包面板")]
//     [SerializeField]
//     private GameObject panelInventory;
//
//     [Tooltip("滚动视图")]
//     [SerializeField]
//     private ScrollRect scrollRect;
//
//     [Header("Desc Panel")]
//     [SerializeField]
//     private RectTransform panelDesc;
//
//     [SerializeField]
//     private Canvas canvas;
//
//     private static List<ItemData> _ownedItems;
//
//     private readonly List<GameObject> _spawnedPages = new();
//     private readonly List<GameObject> _needDestroy = new();
//
//     private Coroutine _snapCoroutine;
//     private bool isDragging;
//     private bool _pendingSnap;
//     private bool _isOpen;
//
//     private Sprite _common;
//     private Sprite _epic;
//     private Sprite _legendary;
//
//     private const int Capicity = 20;
//     private const float SnapVelocityThreshold = 80f;
//     private const float SnapPositionThreshold = 0.001f;
//
//     private void Awake()
//     {
//         var commonCutlass = Resources.Load<WeaponData>("Data/CommonCutlass");
//         var epicCutlass = Resources.Load<WeaponData>("Data/EpicCutlass");
//
//         _ownedItems = new List<ItemData>();
//         _needDestroy.Clear();
//
//         for (int i = 0; i < 30; i++)
//         {
//             Add(commonCutlass);
//             Add(epicCutlass);
//         }
//
//         _isOpen = false;
//
//         _common = Resources.Load<Texture2D>("Prefabs/sc/Rarity_Common");
//         _epic = Resources.Load<Texture2D>("Prefabs/sc/Rarity_Epic");
//         _legendary = Resources.Load<Texture2D>("Prefabs/sc/Rarity_Legendary");
//
//         if (canvas == null)
//             canvas = GetComponentInParent<Canvas>();
//
//         if (panelDesc != null)
//             panelDesc.gameObject.SetActive(false);
//     }
//
//     private void Start()
//     {
//         if (btnOpenInventory != null)
//             btnOpenInventory.onClick.AddListener(OnOpen);
//
//         if (btnCloseInventory != null)
//             btnCloseInventory.onClick.AddListener(OnClose);
//
//         if (panelInventory != null)
//             panelInventory.SetActive(false);
//     }
//
//     private void OnDestroy()
//     {
//         if (btnOpenInventory != null)
//             btnOpenInventory.onClick.RemoveListener(OnOpen);
//
//         if (btnCloseInventory != null)
//             btnCloseInventory.onClick.RemoveListener(OnClose);
//     }
//
//     private void Update()
//     {
//         if (panelInventory == null || !panelInventory.activeInHierarchy)
//             return;
//
//         if (_spawnedPages.Count <= 1 || _snapCoroutine != null)
//             return;
//
//         if (!_pendingSnap && NeedsSnap())
//             _pendingSnap = true;
//
//         if (!_pendingSnap || isDragging || IsPointerHeld())
//             return;
//
//         if (scrollRect != null && Mathf.Abs(scrollRect.velocity.x) > SnapVelocityThreshold)
//             return;
//
//         SnapToNearestPage();
//     }
//
//     public void Add(ItemData item)
//     {
//         if (item == null) return;
//         _ownedItems.Add(item);
//     }
//
//     /// <summary>
//     /// 打开背包
//     /// </summary>
//     public void OnOpen()
//     {
//         // 重复点击就是关闭
//         if (_isOpen)
//         {
//             OnClose();
//             return;
//         }
//
//         _isOpen = true;
//
//         if (panelInventory == null || content == null || scrollRect == null)
//         {
//             Debug.LogError("Inventory 缺少必要引用：panelInventory / content / scrollRect");
//             return;
//         }
//
//         panelInventory.SetActive(true);
//
//         if (panelDesc != null)
//             panelDesc.gameObject.SetActive(false);
//
//         // 每次打开前，先清空旧页面和旧状态
//         ClearPages();
//
//         int pageCount = Mathf.CeilToInt(_ownedItems.Count / (float)Capicity);
//
//         RectTransform contentRect = content as RectTransform;
//         RectTransform viewportRect = scrollRect.viewport != null
//             ? scrollRect.viewport
//             : scrollRect.GetComponent<RectTransform>();
//
//         for (int pageIndex = 0; pageIndex < pageCount; pageIndex++)
//         {
//             int startIndex = pageIndex * Capicity;
//             int endIndex = Mathf.Min(startIndex + Capicity, _ownedItems.Count);
//
//             GameObject curPage = Instantiate(pagePrefab, content);
//             RectTransform pageRect = curPage.GetComponent<RectTransform>();
//
//             _spawnedPages.Add(curPage);
//
//             // 让每一页的尺寸严格等于 Viewport 尺寸，保证“一页一屏”
//             if (pageRect != null && viewportRect != null)
//             {
//                 pageRect.anchorMin = new Vector2(0f, 0f);
//                 pageRect.anchorMax = new Vector2(0f, 1f);
//                 pageRect.pivot = new Vector2(0f, 0.5f);
//                 pageRect.anchoredPosition = Vector2.zero;
//
//                 pageRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, viewportRect.rect.width);
//                 pageRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, viewportRect.rect.height);
//             }
//
//             for (int itemIndex = startIndex; itemIndex < endIndex; itemIndex++)
//             {
//                 ItemData ownedItem = _ownedItems[itemIndex];
//                 GameObject itemGo = InstantiateItem(itemPrefab, curPage.transform, ownedItem);
//                 if (itemGo != null)
//                     _needDestroy.Add(itemGo);
//             }
//
//             _lastPage = curPage;
//         }
//
//         // 强制刷新一次布局，确保 Content 尺寸和分页位置是正确的
//         Canvas.ForceUpdateCanvases();
//
//         if (contentRect != null)
//             LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
//
//         // 重置到第一页
//         scrollRect.velocity = Vector2.zero;
//         scrollRect.horizontalNormalizedPosition = 0f;
//         _pendingSnap = false;
//     }
//
//     private GameObject InstantiateItem(GameObject prefab, Transform curPageTransform, ItemData ownedItem)
//     {
//         if (prefab == null)
//         {
//             Debug.LogError("itemPrefab 为空");
//             return null;
//         }
//
//         GameObject go = Instantiate(prefab, curPageTransform);
//
//         // 1. 给 Item 注入 descPanel 和 canvas
//         Item item = go.GetComponent<Item>();
//         if (item == null)
//             item = go.GetComponentInChildren<Item>();
//
//         if (item != null)
//             item.Init(panelDesc, canvas);
//
//         // 2. 设置稀有度底图 / 图标
//         Transform rarityTransform = go.transform.Find("Rarity");
//         if (rarityTransform != null)
//         {
//             RawImage rawImage = rarityTransform.GetComponent<RawImage>();
//             if (rawImage != null)
//             {
//                 rawImage.texture = GetRarityTexture(ownedItem);
//             }
//         }
//
//         Transform actualTransform = go.transform.Find("Actual");
//         if (actualTransform != null)
//         {
//             Image image = actualTransform.GetComponent<Image>();
//             if (image != null)
//             {
//                 image.sprite = ownedItem.icon;
//             }
//         }
//
//         return go;
//     }
//
//     private Sprite GetRarityTexture(ItemData ownedItem)
//     {
//         if (ownedItem == null)
//             return _common;
//
//         // 如果你的 ItemData 已经直接带 texture，就优先用它
//         if (ownedItem.rarity != null)
//             return ownedItem.rarity;
//
//         // 如果你后面有 enum，可以在这里改成 switch(enum)
//         return _common;
//     }
//
//     public void OnClose()
//     {
//         _isOpen = false;
//
//         if (panelInventory != null)
//             panelInventory.SetActive(false);
//
//         if (panelDesc != null)
//             panelDesc.gameObject.SetActive(false);
//
//         if (_snapCoroutine != null)
//         {
//             StopCoroutine(_snapCoroutine);
//             _snapCoroutine = null;
//         }
//
//         if (scrollRect != null)
//             scrollRect.velocity = Vector2.zero;
//
//         ClearPages();
//     }
//
//     private void ClearPages()
//     {
//         for (int i = content.childCount - 1; i >= 0; i--)
//         {
//             Destroy(content.GetChild(i).gameObject);
//         }
//
//         _spawnedPages.Clear();
//         _needDestroy.Clear();
//         _pageX.Clear();
//
//         _lastPage = null;
//         isDragging = false;
//         _pendingSnap = false;
//     }
//
//     public void OnBeginDrag(PointerEventData eventData)
//     {
//         isDragging = true;
//         _pendingSnap = true;
//
//         if (_snapCoroutine != null)
//         {
//             StopCoroutine(_snapCoroutine);
//             _snapCoroutine = null;
//         }
//     }
//
//     public void OnEndDrag(PointerEventData eventData)
//     {
//         isDragging = false;
//
//         if (_spawnedPages.Count <= 1)
//             return;
//
//         if (_snapCoroutine != null)
//         {
//             StopCoroutine(_snapCoroutine);
//             _snapCoroutine = null;
//         }
//
//         _snapCoroutine = StartCoroutine(SnapAfterRelease());
//     }
//
//     private IEnumerator SnapAfterRelease()
//     {
//         // 等一帧，让 ScrollRect 先结束本次拖拽内部状态更新
//         yield return null;
//         SnapToNearestPage();
//     }
//
//     private void SnapToNearestPage()
//     {
//         int pageCount = _spawnedPages.Count;
//         if (pageCount <= 1 || scrollRect == null)
//             return;
//
//         float current = scrollRect.horizontalNormalizedPosition;
//
//         int nearestPageIndex = Mathf.RoundToInt(current * (pageCount - 1));
//         nearestPageIndex = Mathf.Clamp(nearestPageIndex, 0, pageCount - 1);
//
//         float target = pageCount == 1 ? 0f : nearestPageIndex / (float)(pageCount - 1);
//         if (Mathf.Abs(current - target) <= SnapPositionThreshold)
//         {
//             _pendingSnap = false;
//             return;
//         }
//
//         if (_snapCoroutine != null)
//             StopCoroutine(_snapCoroutine);
//
//         _snapCoroutine = StartCoroutine(SmoothSnap(target));
//     }
//
//     private IEnumerator SmoothSnap(float target)
//     {
//         // 吸附时清掉惯性，避免 ScrollRect 自己继续滑
//         scrollRect.velocity = Vector2.zero;
//
//         const float duration = 0.2f;
//         float time = 0f;
//         float start = scrollRect.horizontalNormalizedPosition;
//
//         while (time < duration)
//         {
//             if (isDragging)
//             {
//                 _snapCoroutine = null;
//                 yield break;
//             }
//
//             time += Time.unscaledDeltaTime;
//             float t = Mathf.Clamp01(time / duration);
//             t = Mathf.SmoothStep(0f, 1f, t);
//
//             scrollRect.horizontalNormalizedPosition = Mathf.Lerp(start, target, t);
//             scrollRect.velocity = Vector2.zero;
//
//             yield return null;
//         }
//
//         scrollRect.horizontalNormalizedPosition = target;
//         scrollRect.velocity = Vector2.zero;
//         _pendingSnap = false;
//         _snapCoroutine = null;
//     }
//
//     private bool NeedsSnap()
//     {
//         int pageCount = _spawnedPages.Count;
//         if (pageCount <= 1 || scrollRect == null)
//             return false;
//
//         float current = scrollRect.horizontalNormalizedPosition;
//         float target = Mathf.Round(current * (pageCount - 1)) / (pageCount - 1);
//         return Mathf.Abs(current - target) > SnapPositionThreshold;
//     }
//
//     private static bool IsPointerHeld()
//     {
//         return Input.GetMouseButton(0) || Input.touchCount > 0;
//     }
//
//     public void OnDrag(PointerEventData eventData)
//     {
//     }
// }