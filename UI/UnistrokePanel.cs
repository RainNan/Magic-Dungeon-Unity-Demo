using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class UnistrokePanel : MaskableGraphic, IPointerDownHandler, IPointerUpHandler
{
    [Serializable]
    public class RecognizeResult
    {
        public SkillName gestureName;
        public float score;
        public float distance;

        public RecognizeResult(SkillName gestureName, float score, float distance)
        {
            this.gestureName = gestureName;
            this.score = score;
            this.distance = distance;
        }
    }

    [Header("Templates")]
    [SerializeField]
    private List<GestureTemplate> templates = new();

    [SerializeField]
    private SkillName currentTemplateName;

    [Header("Draw")]
    [SerializeField]
    private Camera uiCamera;

    [SerializeField]
    private float lineThickness = 8f;

    [SerializeField]
    private float minPointDistance = 6f;

    [Header("$1 Config")]
    [SerializeField]
    private int sampleCount = 64;

    [SerializeField]
    private float squareSize = 250f;

    [SerializeField]
    private float threshold = 0.80f;

    private static UnistrokePanel _activeInstance;
    private static bool _saveTemplateMode;

    private readonly List<Vector2> _pathPoints = new();
    private bool _isDrawing;

    protected override void Awake()
    {
        base.Awake();
        raycastTarget = true;
        _activeInstance = this;
        LoadTemplatesIfNeeded();
    }

    protected override void Start()
    {
        base.Start();

        LoadTemplate();
    }

    private void LoadTemplate()
    {
        ABManager.Instance.LoadAllResAsync<GestureTemplate>("magic_template", objs =>
        {
            if (objs == null || objs.Length == 0)
            {
                LoadTemplatesIfNeeded();
                Debug.LogWarning("No gesture templates were loaded from AssetBundle. Fallback to Resources.");
                return;
            }

            templates = objs
                .OfType<GestureTemplate>()
                .Where(template => template != null)
                .ToList();

            if (templates.Count == 0)
            {
                LoadTemplatesIfNeeded();
                Debug.LogWarning("AssetBundle did not contain valid GestureTemplate assets. Fallback to Resources.");
                return;
            }

            Debug.Log($"Loaded {templates.Count} gesture templates from AssetBundle.");
        });
    }

    protected override void OnDestroy()
    {
        if (_activeInstance == this)
        {
            _activeInstance = null;
        }

        base.OnDestroy();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _isDrawing = false;
            SetVerticesDirty();
        }

        if (!_isDrawing)
        {
            return;
        }

        Vector2 screenPoint = Input.mousePosition;
        if (!RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPoint, uiCamera))
        {
            return;
        }

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, uiCamera,
                out Vector2 localPoint))
        {
            return;
        }

        if (_pathPoints.Count > 0 && Vector2.Distance(_pathPoints[^1], localPoint) < minPointDistance)
        {
            return;
        }

        _pathPoints.Add(localPoint);
        SetVerticesDirty();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right)
        {
            return;
        }

        _isDrawing = true;
        _pathPoints.Clear();

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position,
                eventData.pressEventCamera, out Vector2 localPoint))
        {
            _pathPoints.Add(localPoint);
            SetVerticesDirty();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right)
        {
            return;
        }

        _isDrawing = false;
        SetVerticesDirty();

        if (_pathPoints.Count < 2)
        {
            Debug.Log("The path contains too few points to recognize.");
            if (!_saveTemplateMode)
            {
                ClearPath();
            }

            return;
        }

        if (_saveTemplateMode)
        {
            Debug.Log($"Template draw ready: {currentTemplateName}");
            return;
        }

        LoadTemplatesIfNeeded();

        RecognizeResult result = RecognizeCurrentGesture();
        if (result == null)
        {
            Debug.Log("No gesture templates are available.");
            ClearPath();
            return;
        }

        if (result.score >= threshold)
            EventBus.Instance.Publish(EventNames.OnOpenMagicCircle, result.gestureName);

        Debug.Log($"Recognition result: {result.gestureName}, score={result.score:F4}, distance={result.distance:F4}");
        ClearPath();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (_pathPoints.Count < 2)
        {
            return;
        }

        for (int i = 0; i < _pathPoints.Count - 1; i++)
        {
            DrawLineSegment(vh, _pathPoints[i], _pathPoints[i + 1], lineThickness, color);
        }
    }

    public void ClearPath()
    {
        _pathPoints.Clear();
        SetVerticesDirty();
    }

    public IReadOnlyList<Vector2> GetPath() => _pathPoints;

    public IReadOnlyList<Vector2> GetCurrentPath() => _pathPoints;

    public void SaveCurrentTemplate()
    {
#if UNITY_EDITOR
        SaveCurrentAsTemplate(currentTemplateName);
#else
        Debug.LogWarning("SaveCurrentTemplate is only available in the Unity Editor.");
#endif
    }

    public void ClearTemplates()
    {
        templates.Clear();
        Debug.Log("All gesture templates have been cleared.");
    }

    public List<GestureTemplate> GetTemplates() => templates;

    private void LoadTemplatesIfNeeded()
    {
        if (templates != null && templates.Count > 0)
        {
            return;
        }

        templates = Resources.LoadAll<GestureTemplate>("MagicTemplate")
            .Where(template => template != null)
            .ToList();
    }

    private RecognizeResult RecognizeCurrentGesture()
    {
        if (templates == null || templates.Count == 0)
        {
            return null;
        }

        List<Vector2> candidate = Normalize(_pathPoints);
        float bestDistance = float.MaxValue;
        SkillName bestName = SkillName.Crystal;

        for (int i = 0; i < templates.Count; i++)
        {
            GestureTemplate template = templates[i];
            if (template == null || template.samples == null || template.samples.Count == 0)
            {
                continue;
            }

            for (int j = 0; j < template.samples.Count; j++)
            {
                Vector2[] rawTemplatePoints = template.samples[j]?.points;
                if (rawTemplatePoints == null || rawTemplatePoints.Length < 2)
                {
                    continue;
                }

                List<Vector2> normalizedTemplate = Normalize(rawTemplatePoints.ToList());
                float distance = PathDistance(candidate, normalizedTemplate);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestName = template.templateName;
                }
            }
        }

        if (bestDistance == float.MaxValue)
        {
            return null;
        }

        float halfDiagonal = 0.5f * Mathf.Sqrt(squareSize * squareSize + squareSize * squareSize);
        float score = 1f - (bestDistance / halfDiagonal);
        score = Mathf.Clamp01(score);
        return new RecognizeResult(bestName, score, bestDistance);
    }

    private List<Vector2> Normalize(List<Vector2> points)
    {
        List<Vector2> resampled = Resample(points, sampleCount);
        List<Vector2> rotated = RotateToZero(resampled);
        List<Vector2> scaled = ScaleToSquare(rotated, squareSize);
        List<Vector2> translated = TranslateToOrigin(scaled);
        return translated;
    }

    private static List<Vector2> Resample(List<Vector2> points, int targetCount)
    {
        var result = new List<Vector2>();

        if (points == null || points.Count == 0 || targetCount <= 0)
        {
            return result;
        }

        if (points.Count == 1)
        {
            for (int i = 0; i < targetCount; i++)
            {
                result.Add(points[0]);
            }

            return result;
        }

        float totalLength = PathLength(points);
        if (totalLength <= Mathf.Epsilon)
        {
            for (int i = 0; i < targetCount; i++)
            {
                result.Add(points[0]);
            }

            return result;
        }

        float interval = totalLength / (targetCount - 1);
        result.Add(points[0]);

        float accumulated = 0f;
        Vector2 prev = points[0];

        for (int i = 1; i < points.Count; i++)
        {
            Vector2 curr = points[i];
            float dist = Vector2.Distance(prev, curr);
            if (dist <= Mathf.Epsilon)
            {
                continue;
            }

            while (accumulated + dist >= interval)
            {
                float t = (interval - accumulated) / dist;
                Vector2 sampledPoint = Vector2.Lerp(prev, curr, t);
                result.Add(sampledPoint);
                prev = sampledPoint;
                dist = Vector2.Distance(prev, curr);
                accumulated = 0f;
            }

            accumulated += dist;
            prev = curr;
        }

        if (result.Count == targetCount - 1)
        {
            result.Add(points[^1]);
        }

        while (result.Count < targetCount)
        {
            result.Add(points[^1]);
        }

        if (result.Count > targetCount)
        {
            result.RemoveRange(targetCount, result.Count - targetCount);
        }

        return result;
    }

    private List<Vector2> RotateToZero(List<Vector2> points)
    {
        float angle = IndicativeAngle(points);
        return RotateBy(points, -angle);
    }

    private static float IndicativeAngle(List<Vector2> points)
    {
        if (points == null || points.Count == 0)
        {
            return 0f;
        }

        Vector2 center = Centroid(points);
        Vector2 first = points[0];
        return Mathf.Atan2(first.y - center.y, first.x - center.x);
    }

    private static List<Vector2> RotateBy(List<Vector2> points, float radians)
    {
        var rotated = new List<Vector2>(points.Count);
        if (points == null || points.Count == 0)
        {
            return rotated;
        }

        Vector2 center = Centroid(points);
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        for (int i = 0; i < points.Count; i++)
        {
            float dx = points[i].x - center.x;
            float dy = points[i].y - center.y;
            float x = dx * cos - dy * sin + center.x;
            float y = dx * sin + dy * cos + center.y;
            rotated.Add(new Vector2(x, y));
        }

        return rotated;
    }

    private static List<Vector2> ScaleToSquare(List<Vector2> points, float size)
    {
        var scaled = new List<Vector2>(points.Count);
        if (points == null || points.Count == 0)
        {
            return scaled;
        }

        Rect box = BoundingBox(points);
        float width = box.width <= Mathf.Epsilon ? 1f : box.width;
        float height = box.height <= Mathf.Epsilon ? 1f : box.height;

        for (int i = 0; i < points.Count; i++)
        {
            float x = points[i].x * (size / width);
            float y = points[i].y * (size / height);
            scaled.Add(new Vector2(x, y));
        }

        return scaled;
    }

    private static Rect BoundingBox(List<Vector2> points)
    {
        if (points == null || points.Count == 0)
        {
            return new Rect(0f, 0f, 0f, 0f);
        }

        float minX = points[0].x;
        float minY = points[0].y;
        float maxX = points[0].x;
        float maxY = points[0].y;

        for (int i = 1; i < points.Count; i++)
        {
            Vector2 point = points[i];
            minX = Mathf.Min(minX, point.x);
            minY = Mathf.Min(minY, point.y);
            maxX = Mathf.Max(maxX, point.x);
            maxY = Mathf.Max(maxY, point.y);
        }

        return Rect.MinMaxRect(minX, minY, maxX, maxY);
    }

    private static List<Vector2> TranslateToOrigin(List<Vector2> points)
    {
        var translated = new List<Vector2>(points.Count);
        if (points == null || points.Count == 0)
        {
            return translated;
        }

        Vector2 center = Centroid(points);
        for (int i = 0; i < points.Count; i++)
        {
            translated.Add(points[i] - center);
        }

        return translated;
    }

    private static Vector2 Centroid(List<Vector2> points)
    {
        if (points == null || points.Count == 0)
        {
            return Vector2.zero;
        }

        float sumX = 0f;
        float sumY = 0f;

        for (int i = 0; i < points.Count; i++)
        {
            sumX += points[i].x;
            sumY += points[i].y;
        }

        return new Vector2(sumX / points.Count, sumY / points.Count);
    }

    private static float PathDistance(List<Vector2> a, List<Vector2> b)
    {
        if (a == null || b == null || a.Count == 0 || b.Count == 0)
        {
            return float.MaxValue;
        }

        int count = Mathf.Min(a.Count, b.Count);
        float sum = 0f;

        for (int i = 0; i < count; i++)
        {
            sum += Vector2.Distance(a[i], b[i]);
        }

        return sum / count;
    }

    private static float PathLength(List<Vector2> points)
    {
        if (points == null || points.Count < 2)
        {
            return 0f;
        }

        float length = 0f;
        for (int i = 1; i < points.Count; i++)
        {
            length += Vector2.Distance(points[i - 1], points[i]);
        }

        return length;
    }

    private void DrawLineSegment(VertexHelper vh, Vector2 start, Vector2 end, float thickness, Color32 color32)
    {
        Vector2 dir = end - start;
        float length = dir.magnitude;
        if (length <= 0.001f)
        {
            return;
        }

        dir /= length;
        Vector2 normal = new Vector2(-dir.y, dir.x) * (thickness * 0.5f);

        Vector2 v0 = start - normal;
        Vector2 v1 = start + normal;
        Vector2 v2 = end + normal;
        Vector2 v3 = end - normal;

        int index = vh.currentVertCount;
        vh.AddVert(v0, color32, Vector2.zero);
        vh.AddVert(v1, color32, Vector2.up);
        vh.AddVert(v2, color32, Vector2.one);
        vh.AddVert(v3, color32, Vector2.right);
        vh.AddTriangle(index + 0, index + 1, index + 2);
        vh.AddTriangle(index + 0, index + 2, index + 3);
    }

#if UNITY_EDITOR
    [MenuItem("Game/Save Magic Template/Enable Save Mode")]
    private static void EnableSaveTemplateMode()
    {
        _saveTemplateMode = true;
        Debug.Log("Save template mode enabled.");
    }

    [MenuItem("Game/Save Magic Template/Disable Save Mode")]
    private static void DisableSaveTemplateMode()
    {
        _saveTemplateMode = false;
        Debug.Log("Save template mode disabled.");
    }

    public static void SaveCurrentAsTemplate(SkillName templateName)
    {
        if (_activeInstance == null)
        {
            Debug.LogWarning("No active UnistrokePanel found.");
            return;
        }

        _activeInstance.SaveCurrentTemplateInternal(templateName);
    }

    private void SaveCurrentTemplateInternal(SkillName templateName)
    {
        if (_pathPoints.Count < 2)
        {
            Debug.LogWarning("The current path contains too few points to save as a template.");
            return;
        }

        List<Vector2> normalized = Normalize(_pathPoints);
        string assetPath = $"Assets/Resources/MagicTemplate/{templateName}.asset";
        GestureTemplate existingTemplate = AssetDatabase.LoadAssetAtPath<GestureTemplate>(assetPath);

        if (existingTemplate != null)
        {
            if (existingTemplate.samples == null)
            {
                existingTemplate.samples = new List<GestureTemplate.GestureSample>();
            }

            existingTemplate.templateName = templateName;
            existingTemplate.samples.Add(new GestureTemplate.GestureSample
            {
                points = normalized.ToArray()
            });
            EditorUtility.SetDirty(existingTemplate);
        }
        else
        {
            GestureTemplate newTemplate = ScriptableObject.CreateInstance<GestureTemplate>();
            newTemplate.templateName = templateName;
            newTemplate.samples = new List<GestureTemplate.GestureSample>
            {
                new GestureTemplate.GestureSample
                {
                    points = normalized.ToArray()
                }
            };
            AssetDatabase.CreateAsset(newTemplate, assetPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        LoadTemplatesIfNeeded();
        Debug.Log($"Template saved: {templateName}, point count = {normalized.Count}");
    }

    [MenuItem("Game/Save Magic Template/Crystal")]
    private static void SaveCrystal() => SaveCurrentAsTemplate(SkillName.Crystal);

    [MenuItem("Game/Save Magic Template/Circle")]
    private static void SaveCircle() => SaveCurrentAsTemplate(SkillName.Circle);

    [MenuItem("Game/Save Magic Template/Flame")]
    private static void SaveFlame() => SaveCurrentAsTemplate(SkillName.Flame);

    [MenuItem("Game/Save Magic Template/Thunder")]
    private static void SaveThunder() => SaveCurrentAsTemplate(SkillName.Thunder);

    [MenuItem("Game/Save Magic Template/Rune")]
    private static void SaveRune() => SaveCurrentAsTemplate(SkillName.Rune); // 符文
#endif
}