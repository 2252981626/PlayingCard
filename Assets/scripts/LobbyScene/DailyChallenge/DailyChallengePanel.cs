using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DailyChallengePanel : MonoBehaviour,IEventListener
{
    public RewardAdButton rewardAdButton;
    public Text yearText;
    public List<DailyChallengeCell> dayButtons;

    private bool playCompleteAnim = false;
    private DateTime currentDate;
    private List<int> signedDays;

    [HideInInspector] public DailyChallengeCell lastCell;
    public int curSelectDay = -1;

    private const string page_id = "DailyChallengePanel";
    private void Awake()
    {
        RectTransform rect = GetComponent<RectTransform>();
        float topBlockHeight = Screen.height - Screen.safeArea.yMax;
        rect.offsetMax = new Vector2(0, -topBlockHeight);
    }
    private void Start()
    {
        EventManager.Instance.RegisterListener(GameEvent.DailyChallengeComplete, this);
    }
    public  void Open()
    {
        OtherSdkManager.Instance.CustomEvent("daily_challenge_show", "show", "");

        gameObject.SetActive(true);
        currentDate = GameManager.Instance.GetNowTime();
        signedDays = LoadSignedDays();

        rewardAdButton.Init(AdsCallback, page_id, false);

        RefreshPanel();
    }

    public void hide()
    {
        OtherSdkManager.Instance.CustomEvent("daily_challenge_click", "close", "");
        gameObject.SetActive(false);
    }

    private void AdsCallback()
    {
        OtherSdkManager.Instance.CustomEvent("daily_challenge_click", "play", "", "date_id_select",$"{curSelectDay}");
        OtherSdkManager.Instance.CustomEvent("daily_challenge_start", "day", curSelectDay);

        GameManager.Instance.gameType = GameType.DailyGame;
        UIManager.Instance.GetUI<LobbyScenePanel>().Hide();
        UIManager.Instance.OpenUI<GameScenePanel>();
        AudioManager.Instance.PlayBGM("BGM2");
    }

    private void RefreshPanel()
    {
        //获取当月信息
        int year = currentDate.Year;
        int month = currentDate.Month;
        int daysInMonth = DateTime.DaysInMonth(year, month); // 当月天数
        DateTime firstDay = new DateTime(year, month, 1);    // 当月第一天
        int firstDayWeek = (int)firstDay.DayOfWeek;         // 当月第一天是星期几（0=周日，1=周一...）


        switch (month)
        {
            case 1:
                yearText.text = LanguageManager.Instance.GetText("Jan");
                break;
            case 2:
                yearText.text = LanguageManager.Instance.GetText("Feb");
                break;                                          
            case 3:                                             
                yearText.text = LanguageManager.Instance.GetText("Mar");
               break;                                          
            case 4:                                             
                yearText.text = LanguageManager.Instance.GetText("Apr");
               break;                                          
            case 5:                                             
                yearText.text = LanguageManager.Instance.GetText("May");
               break;                                          
            case 6:                                             
                yearText.text = LanguageManager.Instance.GetText("Jun");
               break;                                          
            case 7:                                             
                yearText.text = LanguageManager.Instance.GetText("Jul");
               break;                                          
            case 8:                                             
                yearText.text = LanguageManager.Instance.GetText("Aug");
               break;                                          
            case 9:                                             
                yearText.text = LanguageManager.Instance.GetText("Sep");
               break;                                          
            case 10:                                            
                yearText.text = LanguageManager.Instance.GetText("Oct");
               break;                                          
            case 11:                                            
                yearText.text = LanguageManager.Instance.GetText("Nov");
               break;                                          
            case 12:                                            
                yearText.text = LanguageManager.Instance.GetText("Dec");
                break;
        }
        yearText.text += $" {year}";
        // 生成日期（包含上月残留、当月、下月残留）
        List<int> dates = new List<int>();

        // 添加上月的最后几天（填充月初的空白）
        if (firstDayWeek != 0) // 周日不需要补
        {
            int lastMonthDays = DateTime.DaysInMonth(year, month == 1 ? 12 : month - 1);
            for (int i = firstDayWeek; i > 0; i--)
            {
                dates.Add(lastMonthDays - i + 1);
            }
        }
        //上个月残留天数
        int lastMonthDayRemaining = dates.Count;
        // 添加当月的所有天
        for (int i = 1; i <= daysInMonth; i++)
        {
            dates.Add(i);
        }
        //下个月残留天数
        int nextMonthDayRemaining = dates.Count;
        // 添加下月的前几天（填充月末的空白）
        int needFill = 42 - dates.Count; // 日历最多显示6行×7列=42天
        for (int i = 1; i <= needFill; i++)
        {
            dates.Add(i);
        }

        for (int i = 0; i < dates.Count; i++)
        {
            DailyChallengeCell cell = dayButtons[i];
            int day = dates[i];
            cell.Init(day,this);
            //是否当月
            bool isCurrentMonth = i > (lastMonthDayRemaining - 1) && i < nextMonthDayRemaining;
            cell.gameObject.SetActive(isCurrentMonth);

            if (!isCurrentMonth) continue;

            cell.Select(false);
          
            if(curSelectDay == -1)
            {
                // 今天
                if (day == currentDate.Day)
                {
                    curSelectDay = day;
                }
            }
            if(curSelectDay == day)
            {
                cell.Select(true);
            }
            if(day <= currentDate.Day)
            {
                cell.LastDay();
            }

            // 已挑战状态
            if (signedDays.Contains(day))
            {
                bool isAnim = playCompleteAnim && day == curSelectDay;
                cell.SignIn(isAnim);
                cell.btn.interactable = false;
            }
            else
            {
                cell.btn.interactable = day <= currentDate.Day;
            }
        }
        UpdatePlayBtnState();
        playCompleteAnim = false;
    }
    public void UpdatePlayBtnState()
    {
        bool isbool = !signedDays.Contains(curSelectDay);
        rewardAdButton.UpdateAdsBtnState(isbool);
    }
    // 挑战逻辑
    private void SignIn(int day)
    {
        int year = currentDate.Year;
        int month = currentDate.Month;

        if (signedDays.Contains(day))
        {
            Debug.Log($"重复挑战：{year}年{month}月{day}日");
            return;
        }
        //添加到已签到集合
        signedDays.Add(day);
        //保存到PlayerPrefs
        SaveSignedDays();
        Debug.Log($"挑战成功！{year}年{month}月{day}日");
    }
    public List<int> LoadSignedDays()
    {
        var signedDays = new List<int>();
        // 2025-11_SignIn
        string key = $"{currentDate.Year}-{currentDate.Month}_SignIn";
        string savedData = PlayerPrefs.GetString(key, "");

        // 解析数据：空字符串→无挑战；非空按逗号分割为日期
        if (!string.IsNullOrEmpty(savedData))
        {
            string[] dayStrs = savedData.Split(',');
            foreach (string str in dayStrs)
            {
                if (int.TryParse(str, out int day))
                {
                    signedDays.Add(day);
                }
            }
        }
        Debug.Log($"加载挑战记录：{currentDate.Year}年{currentDate.Month}月，已挑战[{savedData}]");
        return signedDays;
    }

    // 保存当前月份的签到
    private void SaveSignedDays()
    {
        string key = $"{currentDate.Year}-{currentDate.Month}_SignIn";
        string saveData = string.Join(",", signedDays.OrderBy(day => day));
        Debug.Log($"每日挑战：{key}，[{saveData}]");
        PlayerPrefs.SetString(key, saveData);
        //PlayerPrefs.Save(); 
    }

    private void DailyChallengeComplete()
    {
        playCompleteAnim = true;
        SignIn(curSelectDay);
    }

    public void OnEventTriggered(GameEvent eventType, object data = null)
    {
        if(eventType == GameEvent.DailyChallengeComplete)
        {
            DailyChallengeComplete();
        }
    }
}
