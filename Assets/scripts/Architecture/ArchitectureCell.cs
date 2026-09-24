using DG.Tweening;
using Newtonsoft.Json;
using System;
using UnityEngine;
using UnityEngine.UI;

public enum ArchitectureType
{
    Government_Building = 0,
    Flower_Shop,
    Police_Station,
    Restaurant,
    Bank,
    Shopping_Mall,
    Hospital,
    School,
    Iron_Gym,
    Amusement_Park,
}

public class ArchitectureCellData
{
    public int gold;
    public long timeStamp;
}

public class ArchitectureCell : MonoBehaviour
{
    public ArchitectureType type;
    public Image goldicon;
    public Image goldAnimIcon;

    public ParticleSystem liziAnim;
    //public List<Sprite> dibanSps;
    public Transform unlockTrans;
    public Transform lockTrans;

    public Image jinduImg;
    public Text exText;
    public Text goldText;
    public Button btn1;
    public Button btn2;


    public Transform goldAnimTrans;
    public CanvasGroup canvasGroup;
    public Text goldAnimText;

    private ArchitectureCellData thisData;
    private ArchitectureInfo info;
    private bool isUnLock =false;

    private bool awaitUnLockAnim = false;
    private Image icon;
    private string unit;
    private void Start()
    {
        btn1.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayBtnMusic();
            OnClick();
        });
        btn2.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayBtnMusic();
            OnClick();
        });
        GameManager.Instance.UpdateAppATTToDiamond(goldicon);
        GameManager.Instance.UpdateAppATTToDiamond(goldAnimIcon);
    }

    private void OnClick()
    {
        if(!isUnLock)
        {
            string str = LanguageManager.Instance.GetText("Unlocklevel");
            UIManager.Instance.OpenUI<GeneralTipsPanel>($"{str} {info.LockLevels[0]}");
        }
        else
        {
            if(thisData.gold <= 0)
            {
                return;
            }
            AdManager.Instance.OnClickInterstitialAd("Architecture");
            //-------------------
            float curGold = thisData.gold / 100f;
            goldAnimText.text = "+" + curGold;
            goldAnimTrans.gameObject.SetActive(true);

            //liziAnim.Play();
            goldAnimTrans.DOLocalMoveY(100f, 1f).SetEase(Ease.OutCubic);
            DOTween.Sequence()
                .AppendInterval(0.6f)
                .Append(canvasGroup.DOFade(0f, 0.5f))
                .AppendCallback(() =>
                {
                    goldAnimTrans.gameObject.SetActive(false);
                    goldAnimTrans.transform.localPosition = new Vector3(0f, 50f, 0f);
                    canvasGroup.alpha = 1f;
                });
            //--------------------
            GameManager.Instance.playerInfo.Add_gold(curGold);
            thisData.gold = 0;
            DateTime curtime = GameManager.Instance.GetNowTime();
            thisData.timeStamp = GameManager.DateTimeToTimeStamp(curtime);
            SaveData();

            PlayerInfoUI playerInfoUI = UIManager.Instance.GetUI<PlayerInfoUI>();
            playerInfoUI.StartGoldAnim();
            UpdateGold();
            GameManager.Instance.SavePlayerInfo();

            OtherSdkManager.Instance.CustomEvent("home_architecture_reward_claim", "architecture_id", type.ToString());
        }
    }

    // Start is called before the first frame update
    public void Init()
    {
        if (icon == null)
        {
            icon = transform.parent.GetComponent<Image>();
            liziAnim.transform.SetParent(icon.transform.parent.transform);
        }
        goldAnimTrans.gameObject.SetActive(false);

        info = GameManager.Instance.architectureConfig.GetInfoByIndex((int)type);
        isUnLock = GameManager.Instance.architectureConfig.IsArchitectureUnlocked((int)type);
        unlockTrans.gameObject.SetActive(isUnLock);
        lockTrans.gameObject.SetActive(!isUnLock);
        icon.enabled = isUnLock;
        if (!isUnLock)
        {
            awaitUnLockAnim = true;
            return;
        }

        LobbyScenePanel lobbyScenePanel = UIManager.Instance.GetUI<LobbyScenePanel>();
        if (awaitUnLockAnim && GameManager.Instance.gameType == GameType.LobbyScene && lobbyScenePanel.lobbyHomePanel.gameObject.activeSelf)
        {
            awaitUnLockAnim = false;

            icon.transform.localScale = Vector3.zero;
            DOTween.Sequence()
                .Append(icon.transform.DOScale(1.1f, 0.2f))
                .Append(icon.transform.DOScale(0.9f, 0.2f))
                .Append(icon.transform.DOScale(1f, 0.2f))
                .SetTarget(icon.transform)
                ;
            unlockTrans.transform.localScale = Vector3.zero;
            DOTween.Sequence()
                .Append(unlockTrans.transform.DOScale(1.1f, 0.2f))
                .Append(unlockTrans.transform.DOScale(0.9f, 0.2f))
                .Append(unlockTrans.transform.DOScale(1f, 0.2f))
                .AppendCallback(() =>
                {
                    PlayIdleAnim();
                })
                .SetTarget(unlockTrans.transform)
                ;
        }
        else
        {
            PlayIdleAnim();
        }

        LoadData();
        exText.text = $"{LanguageManager.Instance.GetText(info.Name)} Lv.{GameManager.Instance.architectureConfig.GetCurrentArchLevel((int)type) + 1}";
        UpdateGold();
        SaveData();
    }

    public void PlayIdleAnim()
    {
        transform.DOKill();
        icon.transform.localScale = Vector3.one;
        float await = (int)type * 0.2f;
        DOTween.Sequence()
            .AppendInterval(2f + await)
            .Append(icon.transform.DOScale(0.9f, 0.5f))
            .Append(icon.transform.DOScale(1.1f, 0.1f))
            .AppendCallback(() =>
            {
                UpdateGold();
                liziAnim.Play();
            })
            .Append(icon.transform.DOScale(0.95f, 0.1f))
            //.Append(icon.transform.DOScale(1.05f, 0.1f))
            //.Append(icon.transform.DOScale(0.95f, 0.1f))
            .Append(icon.transform.DOScale(1f, 0.1f))
            .SetTarget(transform)
            .SetLoops(-1);
    }

    private void UpdateGold()
    {
        //Debug.Log("更新建筑：" + info.Name);
        //this.DOKill();

        DateTime lastTime = GameManager.TimeStampToDateTime(thisData.timeStamp);
        DateTime curtime = GameManager.Instance.GetNowTime();
        TimeSpan diff = curtime - lastTime;
        int maxMinutes = GameManager.Instance.architectureConfig.GetMinutesByArchIndex((int)type);
        int goldSpeed = GameManager.Instance.architectureConfig.GetGoldByArchIndex((int)type);
        int maxGold = maxMinutes * goldSpeed;
        // 整数分钟（向下取整）
        int curMinutes = (int)diff.TotalSeconds;
        curMinutes = Mathf.Min(curMinutes, maxMinutes);
        //当前金币
        if(curMinutes > 0)
        {
            thisData.gold += curMinutes * goldSpeed;
            thisData.gold = Mathf.Min(thisData.gold, maxGold);
            thisData.timeStamp = GameManager.DateTimeToTimeStamp(curtime);
        }

        float curGold = thisData.gold / 100f;
        if (string.IsNullOrEmpty(unit))
        {
            unit = LanguageManager.Instance.GetText_Encrypt("Special_Diamond__unit");
        }
        goldText.text = $"{unit}{curGold}";
        jinduImg.fillAmount = (float)thisData.gold / maxGold;
        //if (thisData.gold < maxGold)
        //{
        //    DOTween.Sequence().AppendInterval(60f).AppendCallback(() =>
        //    {
        //        UpdateGold();
        //    })
        //    .SetTarget(this);
        //}
    }

    private void LoadData()
    {
        string json = PlayerPrefs.GetString(info.Name + "_Data");
        if (!string.IsNullOrEmpty(json))
        {
            thisData = JsonConvert.DeserializeObject<ArchitectureCellData>(json);
        }
        else
        {
            // 无数据，初始化
            thisData = new ArchitectureCellData();
            thisData.gold = 0;
            DateTime curtime = GameManager.Instance.GetNowTime();
            thisData.timeStamp = GameManager.DateTimeToTimeStamp(curtime);
            SaveData();
        }
    }
    public void SaveData()
    {
        if (thisData == null)
            return;

        // 序列化对象转json字符串
        string json = JsonConvert.SerializeObject(thisData,Formatting.Indented);
        PlayerPrefs.SetString(info.Name + "_Data", json);
        //Debug.Log(info.Name + "_Data保存: " + json);
        //PlayerPrefs.Save();

    }
}
