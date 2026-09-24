using UnityEngine;
using UnityEngine.UI;

public class BackgroundExpandFit : MonoBehaviour
{
    private const float DESIGN_WIDTH = 1080f;
    private const float DESIGN_HEIGHT = 1920f;

    private RectTransform _bgRect;

    void Awake()
    {
        _bgRect = GetComponent<RectTransform>();
        FitBackground();
    }

    void FitBackground()
    {
        // 设计比例 & 当前屏幕比例
        float designRatio = DESIGN_WIDTH / DESIGN_HEIGHT;
        float screenRatio = (float)Screen.width / Screen.height;

        // Expand 模式：高度永远填满，只算宽度缩放
        if (screenRatio > designRatio)
        {
            // 计算需要放大的倍数
            float scale = screenRatio / designRatio;

            // 等比缩放背景，铺满宽屏，不拉伸
            _bgRect.localScale = Vector3.one * scale;
        }
        else
        {
            // 正常屏不做任何缩放
            _bgRect.localScale = Vector3.one;
        }
    }
}