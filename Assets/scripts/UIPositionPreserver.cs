using UnityEngine;

/// <summary>
/// 保持 UI 元素在设计分辨率下的相对位置（百分比定位）
/// 用法：在 1080x1920 下摆好位置，挂载脚本，运行时自动适配
/// </summary>
public class UIPositionPreserver : MonoBehaviour
{
    [Header("设计分辨率（自动获取）")]
    [SerializeField] private Vector2 _designResolution = new Vector2(1080f, 1920f);

    [Header("设计分辨率下的百分比位置（自动计算）")]
    [SerializeField] private Vector2 _savedPercent = new Vector2(0f, 0f);

    [Header("设计分辨率下的缩放（自动计算）")]
    [SerializeField] private Vector2 _savedScale = Vector2.one;

    [Header("设置")]
    [Tooltip("锁定水平位置，不随分辨率变化")]
    public bool lockHorizontal = false;

    [Tooltip("锁定垂直位置，不随分辨率变化")]
    public bool lockVertical = false;

    [Tooltip("对齐到整数像素，避免模糊")]
    public bool snapToPixel = true;

    [Header("缩放模式")]
    public ScaleMode scaleMode = ScaleMode.Uniform;

    public enum ScaleMode
    {
        Uniform,        // 等比缩放（宽高取较小值）
        Stretch,        // 拉伸铺满（宽高分别缩放）
        WidthBased,     // 以宽度为基准
        HeightBased,    // 以高度为基准
    }

    private RectTransform _rectTransform;
    private RectTransform _parentRect;

    void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        _parentRect = transform.parent as RectTransform;

        if (_parentRect == null)
        {
            Debug.LogError("父节点不是 RectTransform！");
            enabled = false;
            return;
        }

        Apply();
    }

    /// <summary>
    /// 保存当前百分比位置和缩放（在编辑器中摆好位置后调用）
    /// </summary>
    public void SaveCurrent()
    {
        if (_parentRect == null) return;
        
        // 保存位置百分比
        Vector2 parentSize = _parentRect.rect.size;
        Vector2 localPos = _rectTransform.anchoredPosition;

        _savedPercent.x = (localPos.x + parentSize.x / 2f) / parentSize.x;
        _savedPercent.y = (localPos.y + parentSize.y / 2f) / parentSize.y;

        _savedPercent.x = Mathf.Clamp01(_savedPercent.x);
        _savedPercent.y = Mathf.Clamp01(_savedPercent.y);

        // 保存缩放
        _savedScale = _rectTransform.localScale;

        Debug.Log($"保存百分比位置: ({_savedPercent.x:F3}, {_savedPercent.y:F3})，缩放: ({_savedScale.x:F3}, {_savedScale.y:F3})");
    }

    /// <summary>
    /// 应用百分比位置和缩放到当前分辨率
    /// </summary>
    public void Apply()
    {
        if (_parentRect == null || _rectTransform == null) return;

        Vector2 parentSize = _parentRect.rect.size;

        // 应用位置
        float x = (_savedPercent.x - 0.5f) * parentSize.x;
        float y = (_savedPercent.y - 0.5f) * parentSize.y;

        if (lockHorizontal)
            x = _rectTransform.anchoredPosition.x;
        if (lockVertical)
            y = _rectTransform.anchoredPosition.y;

        if (snapToPixel)
        {
            x = Mathf.Round(x);
            y = Mathf.Round(y);
        }

        _rectTransform.anchoredPosition = new Vector2(x, y);

        // 应用缩放
        float scaleX = parentSize.x / _designResolution.x;
        float scaleY = parentSize.y / _designResolution.y;

        float finalScaleX, finalScaleY;

        switch (scaleMode)
        {
            case ScaleMode.Uniform:
                float uniformScale = Mathf.Min(scaleX, scaleY);
                finalScaleX = uniformScale * _savedScale.x;
                finalScaleY = uniformScale * _savedScale.y;
                break;

            case ScaleMode.Stretch:
                finalScaleX = scaleX * _savedScale.x;
                finalScaleY = scaleY * _savedScale.y;
                break;

            case ScaleMode.WidthBased:
                finalScaleX = scaleX * _savedScale.x;
                finalScaleY = scaleX * _savedScale.y;
                break;

            case ScaleMode.HeightBased:
                finalScaleX = scaleY * _savedScale.x;
                finalScaleY = scaleY * _savedScale.y;
                break;

            default:
                finalScaleX = _savedScale.x;
                finalScaleY = _savedScale.y;
                break;
        }

        _rectTransform.localScale = new Vector3(finalScaleX, finalScaleY, 1f);
    }

    /// <summary>
    /// 父节点尺寸变化时自动更新
    /// </summary>
    void OnRectTransformDimensionsChange()
    {
        if (_parentRect != null && _rectTransform != null)
        {
            Apply();
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (_savedPercent.x == 0f && _savedPercent.y == 0f)
        {
            if (Application.isPlaying) return;
            _rectTransform = GetComponent<RectTransform>();
            _parentRect = transform.parent as RectTransform;
            if (_parentRect != null && _rectTransform.anchoredPosition != Vector2.zero)
            {
                SaveCurrent();
            }
        }
    }

    [ContextMenu("保存当前位置 + 缩放")]
    public void SaveInEditor()
    {
        _rectTransform = GetComponent<RectTransform>();
        _parentRect = transform.parent as RectTransform;
        if (_rectTransform != null && _parentRect != null)
        {
            SaveCurrent();
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }

    [ContextMenu("应用位置 + 缩放")]
    public void ApplyInEditor()
    {
        _rectTransform = GetComponent<RectTransform>();
        _parentRect = transform.parent as RectTransform;
        if (_rectTransform != null && _parentRect != null)
        {
            Apply();
            UnityEditor.EditorUtility.SetDirty(_rectTransform);
        }
    }
#endif
}