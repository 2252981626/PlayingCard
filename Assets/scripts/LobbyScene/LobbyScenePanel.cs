using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LobbyScenePanel : UIBase
{
    public Button playBtn;
    public Transform playTrans;
    public LobbyLevelPanel lobbyLevelPanel;
    public LobbyHomePanel lobbyHomePanel;
    public DailyChallengePanel dailyChallengePanel;

    public Button homeBtn;
    public Transform homeTrans;

    public Button dailyBtn;
    public Transform dailyTrans;
    public Transform lockTrans;
    public Button lockBtn;

    public Transform shouzhi;
    public Button shouzhiBtn;

    private float timer;

    private void OnEnable()
    {
        isOpen = false;
    }
    private void OnDisable()
    {
        isOpen = false;
    }
    private void Start()
    {
        playBtn.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayBtnMusic();
            SetBtnIndex(0);
        });
        homeBtn.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayBtnMusic();
            SetBtnIndex(1);
        });
        dailyBtn.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayBtnMusic();
            SetBtnIndex(2);
        });
        lockBtn.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayBtnMusic();
            string str = LanguageManager.Instance.GetText("Unlocklevel") + " 10";
            UIManager.Instance.OpenUI<GeneralTipsPanel>(str);
        });
        shouzhiBtn.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayBtnMusic();
            timer = 0f;
            shouzhiBtn.gameObject.SetActive(false);
            SetBtnIndex(0);


            string Guide_shouzhiBtn = PlayerPrefs.GetString("Guide_shouzhiBtn", "");
            if (string.IsNullOrEmpty(Guide_shouzhiBtn))
            {
                PlayerPrefs.SetString("Guide_shouzhiBtn", "yes");
                OtherSdkManager.Instance.CustomEvent("newbie_guide_9_complete", "step", "9");
            }

        });
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            timer = 0f;
        }
        timer += Time.deltaTime;
        if(timer >10f)
        {
            timer = 0f;
            if (UIManager.Instance.CheckIstheUIopen() || !lobbyHomePanel.gameObject.activeSelf)
            {
                return;
            }
            ShowShouzhi();
        }
    }

    public void ShowShouzhi()
    {
        shouzhi.transform.position = playBtn.transform.position;
        shouzhiBtn.gameObject.SetActive(true);
    }

    public override void Refresh(object data = null)
    {
        base.Refresh(data);
        int index = (int)data;
        GameManager.Instance.gameType = GameType.LobbyScene;
        SetBtnIndex(index);
        timer = 0f;
        shouzhiBtn.gameObject.SetActive(false);

        if(GameManager.Instance.playerInfo.level < 11)
        {
            lockTrans.gameObject.SetActive(true);
        }
        else
        {
            lockTrans.gameObject.SetActive(false);
            string firstGame = PlayerPrefs.GetString("GuidePanel_dailychangle", "");
            if (string.IsNullOrEmpty(firstGame))
            {
                DOTween.Sequence().AppendInterval(0.3f).AppendCallback(() =>
                {
                    UIManager.Instance.OpenUI<GuidePanel_dailychangle>();
                });
            }
        }
    }

    public override void Hide()
    {
        base.Hide();
    }

    private void SetBtnIndex(int index)
    {
        playTrans.gameObject.SetActive(false);
        homeTrans.gameObject.SetActive(false);
        dailyTrans.gameObject.SetActive(false);
        lobbyLevelPanel.hide();
        lobbyHomePanel.hide();
        dailyChallengePanel.hide();
        switch (index)
        {
            case 0:
                UIManager.Instance.GetUI<PlayerInfoUI>().SwitchUI(false);
                playTrans.gameObject.SetActive(true);
                lobbyLevelPanel.Open();
                break;
            case 1:
                UIManager.Instance.GetUI<PlayerInfoUI>().SwitchUI(true);
                homeTrans.gameObject.SetActive(true);
                lobbyHomePanel.Open();
                break;
            case 2:
                UIManager.Instance.GetUI<PlayerInfoUI>().SwitchUI(false);
                dailyTrans.gameObject.SetActive(true);
                dailyChallengePanel.Open();
                break;
        }

    }

}
