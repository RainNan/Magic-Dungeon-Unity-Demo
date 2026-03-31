using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Button btn;

    private RectTransform _descPanelRect;
    private Canvas _canvas;

    private void Awake()
    {
        if (btn == null)
            btn = GetComponent<Button>();

        if (btn == null)
            btn = GetComponentInChildren<Button>();
    }

    private void OnEnable()
    {
        if (btn != null)
            btn.onClick.AddListener(OnClick);
    }

    private void OnDisable()
    {
        if (btn != null)
            btn.onClick.RemoveListener(OnClick);
    }

    /// <summary>
    /// 由 Inventory 在实例化后注入依赖
    /// </summary>
    public void Init(RectTransform descPanelRect, Canvas canvas)
    {
        _descPanelRect = descPanelRect;
        _canvas = canvas;
    }

    /// <summary>
    /// 在当前鼠标位置显示 desc panel
    /// </summary>
    private void OnClick()
    {
        if (_descPanelRect == null)
        {
            Debug.LogError($"{name}: descPanelRect 未初始化");
            return;
        }

        if (_canvas == null)
        {
            Debug.LogError($"{name}: canvas 未初始化");
            return;
        }

        RectTransform parentRect = _descPanelRect.parent as RectTransform;
        if (parentRect == null)
        {
            Debug.LogError("panelDesc 的父节点不是 RectTransform");
            return;
        }

        Vector2 localPoint;
        Camera uiCamera = null;

        if (_canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCamera = _canvas.worldCamera;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            Input.mousePosition,
            uiCamera,
            out localPoint
        );

        _descPanelRect.gameObject.SetActive(true);
        _descPanelRect.SetAsLastSibling();
        _descPanelRect.anchoredPosition = localPoint + new Vector2(20f, -20f);
    }
}