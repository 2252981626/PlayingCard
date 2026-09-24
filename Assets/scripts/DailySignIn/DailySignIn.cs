using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DailySignIn : MonoBehaviour
{
    public Button clickBtn;
    public static int currentDay;


    void Start()
    {
        clickBtn.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayBtnMusic();
            UIManager.Instance.OpenUI<DailySignInPanel>();
        });
    }

    public void RefreshUI()
    {
       
    }

    public static void SignIn(int _day)
    {
        DateTime currentDate = GameManager.Instance.GetNowTime();
        PlayerPrefs.SetString("SignIn_LastDate", GameManager.DateTimeToTimeStamp(currentDate).ToString());
        PlayerPrefs.SetInt("SignIn_CurrentDay", _day);
        //PlayerPrefs.Save();
    }

    public static bool CheckSignIn()
    {
        currentDay = PlayerPrefs.GetInt("SignIn_CurrentDay", 0);

        string lastDateStr = PlayerPrefs.GetString("SignIn_LastDate", "");
        if (string.IsNullOrEmpty(lastDateStr))
        {
            return false;
        }

        DateTime currentDate = GameManager.Instance.GetNowTime();
        DateTime lastSignDate = GameManager.TimeStampToDateTime(long.Parse(lastDateStr));
        //Áè³¿ÅÐ¶Ï
        DateTime todayMidnight = new DateTime(
            currentDate.Year,
            currentDate.Month,
            currentDate.Day,
            0, 0, 0
        );

        DateTime lastSignMidnight = new DateTime(
            lastSignDate.Year,
            lastSignDate.Month,
            lastSignDate.Day,
            0, 0, 0
        );

        TimeSpan timeDiff = todayMidnight - lastSignMidnight;
        if (timeDiff.TotalDays >= 1)
        {
            currentDay = currentDay >= 7 ? 0 : currentDay;
            return false;
        }

        return true;
    }


}
