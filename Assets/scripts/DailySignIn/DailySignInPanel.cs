using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DailySignInPanel : UIBase
{
    public Transform root;
    public Button hideBtn;
    public Button claimBtn;
    public CanvasGroup canvasGroup;
    public List<DailySignInCell> dailySignInCells;

    private bool isInit = false;
    private int curDayIndex;
    private string page_id = "DailySignInPanel";
    // Start is called before the first frame update
    private void Awake()
    {
        RectTransform rect = root.GetComponent<RectTransform>();
        float topBlockHeight = Screen.height - Screen.safeArea.yMax;
        rect.offsetMax = new Vector2(0, -topBlockHeight);
    }
    void Start()
    {
        hideBtn.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayBtnMusic();
            Hide();
        });
        claimBtn.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayBtnMusic();
            dailySignInCells[curDayIndex].clickBtn.onClick.Invoke();
            claimBtn.interactable = false;
            canvasGroup.alpha =  0.5f;
        });
    }

    private void Init()
    {
        if (isInit)
            return;

        List<List<ItemData>> config = new List<List<ItemData>>();
        config.Add(new List<ItemData>() { new ItemData(ItemType.Gold, 30) });
        config.Add(new List<ItemData>() { new ItemData(ItemType.Diamond, 500) });
        config.Add(new List<ItemData>() { new ItemData(ItemType.Hint, 10) });
        config.Add(new List<ItemData>() { new ItemData(ItemType.Extract, 10) });
        config.Add(new List<ItemData>() { new ItemData(ItemType.Exchange, 10) });
        config.Add(new List<ItemData>() { new ItemData(ItemType.Return, 10) });
        config.Add(new List<ItemData>() { new ItemData(ItemType.Gold, 100)});

        for (int day = 0; day < dailySignInCells.Count; day++)
        {
            dailySignInCells[day].Init(day + 1, config[day]);
        }
        isInit = true;
    }

    public override void Refresh(object data = null)
    {
        base.Refresh(data);
        Init();
        OtherSdkManager.Instance.CustomEvent("sign_in_show", "show", "");

        bool isTodaySignIn = DailySignIn.CheckSignIn();//ÅÐ¶ÏÊÇ·ñÇ©µ½

        for (int day = 1; day <= dailySignInCells.Count; day++)
        {
            dailySignInCells[day - 1].clickBtn.interactable = false;
            dailySignInCells[day - 1].IsToday(false);

            dailySignInCells[day - 1].SignInState(day <= DailySignIn.currentDay);
        }

        curDayIndex = isTodaySignIn == true ? DailySignIn.currentDay - 1 : DailySignIn.currentDay;
        //dailySignInCells[curDayIndex].clickBtn.interactable = !isTodaySignIn;
        claimBtn.interactable = !isTodaySignIn;
        canvasGroup.alpha = !isTodaySignIn == true ? 1f : 0.5f;

        dailySignInCells[curDayIndex].IsToday(true);

    }
    public override void Hide()
    {
        callback = () =>
        {
            if (TxElementMananger.Instance != null)
            {
                string firstGame = PlayerPrefs.GetString("Guide_TxBtn", "");
                if (string.IsNullOrEmpty(firstGame))
                {
                    UIManager.Instance.OpenUI<GuidePanel_TxBtn>();
                }
            }

        };
        base.Hide();
    }
  


}
