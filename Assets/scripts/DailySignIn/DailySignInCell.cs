using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DailySignInCell : MonoBehaviour
{
    public Button clickBtn;
    public GameObject baseBgObj;
    public Text baseTitle;
    public GameObject todayBgObj;
    public Text todayTitle;
    public GameObject signInObj;
    public Transform cells;


    private List<ItemBase> items;
    private List<ItemData> itemDatas;
    private int day;

    // Start is called before the first frame update
    void Start()
    {
        clickBtn.onClick.AddListener(() =>
        {
            OtherSdkManager.Instance.CustomEvent("sign_in_reward", "day", $"{day}");
            AudioManager.Instance.PlayBtnMusic();
            SignIn();
        });
    }

    public void Init(int index, List<ItemData> _itemDatas)
    {
        day = index;
        itemDatas = _itemDatas;
        if (index == 0)
            day = 7;

        string unit = LanguageManager.Instance.GetText("Day");
        baseTitle.text = unit + $" {day}";
        todayTitle.text = unit + $" {day}";
        items = GameManager.Instance.CreatItems(itemDatas, cells);
        foreach (var item in items)
        {
            item.cntText.transform.GetComponent<Shadow>().enabled = false;
            item.cntText.transform.GetComponent<Outline>().enabled = false;
        }
    }

    public void IsToday(bool istoday)
    {
        baseBgObj.SetActive(!istoday);
        todayBgObj.SetActive(istoday);
        foreach (var item in items)
        {
            item.effect.gameObject.SetActive(istoday);
            item.effect.transform.localScale = Vector3.one * 1.5f;
            item.cntText.color = istoday == true ? new Color(61f / 255f, 172f / 255f, 22f / 255f, 1f) : new Color(135f / 255f, 33f / 255f, 19f / 255f, 1f);
        }
    }
    public void SignInState(bool isSignIn)
    {
        signInObj.gameObject.SetActive(isSignIn);
    }
    private void SignIn()
    {
        clickBtn.interactable = false;
        SignInState(true);
        DailySignIn.SignIn(day);

        ItemsGetRewardAndAnim();

    }

    /// <summary>
    /// 获得道具奖励并且走动画
    /// </summary>
    private void ItemsGetRewardAndAnim()
    {
        PlayerInfoUI playerInfoUI = UIManager.Instance.GetUI<PlayerInfoUI>();
        UIManager.Instance.OpenUIMask();
        float awaitTime = 0.1f;
        foreach (var item in items)
        {
            if (item.itemType == ItemType.Gold || item.itemType == ItemType.GoldDui)
            {
                awaitTime = 2f;
                playerInfoUI.GoldCanvasTop();
            }
            else if (item.itemType == ItemType.Diamond || item.itemType == ItemType.DiamondDui)
            {
                awaitTime = 2f;
                playerInfoUI.DiamondCanvasTop();
            }
            item.GetItemReward();
            item.PlayItemAnim();
        }
        //动画
        DOTween.Sequence().AppendInterval(awaitTime).AppendCallback(() =>
        {
            GameManager.Instance.SavePlayerInfo();

            playerInfoUI.GoldCanvasRecover();
            playerInfoUI.DiamondCanvasRecover();
            UIManager.Instance.HideUIMask();

            string firstGame = PlayerPrefs.GetString("Guide_firstGame", "");
            if (string.IsNullOrEmpty(firstGame))
            {
                PlayerPrefs.SetString("Guide_firstGame", "yes");
                UIManager.Instance.GetUI<DailySignInPanel>().Hide();
            }
        });

    }
}
