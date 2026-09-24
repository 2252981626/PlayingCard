using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyHomePanel : MonoBehaviour
{
    public DailySignIn dailySignIn;
    public LuckyWheel luckyWheel;

    public ScrollRect scrollRect;
    public List<ArchitectureCell> architectureCells;
    private void Awake()
    {
        RectTransform rect = GetComponent<RectTransform>();
        float topBlockHeight = Screen.height - Screen.safeArea.yMax;
        rect.offsetMax = new Vector2(0, -topBlockHeight);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Open()
    {
        gameObject.SetActive(true);
        RefreshAllArchitecture();
        scrollRect.horizontalNormalizedPosition = 0.5f;
        EventManager.Instance.TriggerEvent(GameEvent.UpdateTxProgress);
    }

    public void RefreshAllArchitecture()
    {
        foreach (var cell in architectureCells)
        {
            cell.Init();
        }
    }

    public void hide()
    {
        gameObject.SetActive(false);
    }
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // ========== 进入后台逻辑 ==========
            Debug.Log("进入后台保存建筑数据");
            foreach (var cell in architectureCells)
            {
                cell.SaveData();
            }
        }
      
    }
}
