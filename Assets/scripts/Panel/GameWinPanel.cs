using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameWinPanel : UIBase
{
    public Text moveText;
    public Text addmoveText;

    public Text timeText;
    public Text addtimeText;

    public Text scoreText;

    public Button claimBtn;
    public RewardAdButton rewardAdButton;
    public Button noAdButton;

    public Transform itemRoot;
    private List<ItemBase> itemBase;
    private int addMovescore;
    private int addTimescore;
    private int allscore;
    private string page_id = "GameWinPanel";
    private void OnEnable()
    {
        isOpen = true;
    }
    private void OnDisable()
    {
        isOpen = false;
        foreach (Transform item in itemRoot)
        {
            Destroy(item.gameObject);
        }
    }

    private void Start()
    {
        claimBtn.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayBtnMusic();
            CollectClick();
        });

        noAdButton.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayBtnMusic();
            AdsCallback2();
        });
        
    }

    public void ScoreAnimStop()
    {
        transform.DOKill();
        addmoveText.transform.DOKill();
        addtimeText.transform.DOKill();
        scoreText.transform.DOKill();

        addmoveText.text = addMovescore.ToString();
        addtimeText.text = addTimescore.ToString();
        scoreText.text = allscore.ToString();
    }

    public override void Refresh(object data = null)
    {
        base.Refresh(data);

        AudioManager.Instance.PlaySceneSingleMusic("gamewin");
        GameScenePanel gameScenePanel = UIManager.Instance.GetUI<GameScenePanel>();
        gameScenePanel.MarkSettlementShown();
        int levelElapsedMs = Mathf.RoundToInt((Time.realtimeSinceStartup - gameScenePanel.levelStartTime) * 1000f);
        OtherSdkManager.Instance.CustomEvent("level_settle_show", "level_id", GameManager.Instance.playerInfo.level - 1, "level_elapsed_ms", levelElapsedMs);
        moveText.text = gameScenePanel.move.ToString();
        timeText.text = GameManager.Instance.GetTimeString(gameScenePanel.second);
        scoreText.text = gameScenePanel.score.ToString();

        //积分
        addmoveText.text = "0";
        addtimeText.text = "0";
        if (gameScenePanel.move > 180)
        {
            addMovescore = 500;
        }
        else if(gameScenePanel.move >= 101 && gameScenePanel.move <= 180)
        {
            addMovescore = 1000;
        }
        else
        {
            addMovescore = 2000;
        }

        if (gameScenePanel.second > 120)
        {
            addTimescore = 500;
        }
        else if (gameScenePanel.second >= 60 && gameScenePanel.second <= 120)
        {
            addTimescore = 1000;
        }
        else
        {
            addTimescore = 3000;
        }
        allscore = gameScenePanel.score + addMovescore + addTimescore;

        OtherSdkManager.Instance.CustomEvent("level_complete", "level_id", GameManager.Instance.playerInfo.level - 1, "score", $"{allscore}");

        DOTween.Sequence()
            .AppendInterval(0.2f)
            .AppendCallback(() =>
            {
                StartScoredAnim(addmoveText.transform, 0, addMovescore, addmoveText);
                StartScoredAnim(scoreText.transform, gameScenePanel.score, gameScenePanel.score + addMovescore, scoreText);
            })
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                StartScoredAnim(addtimeText.transform, 0, addTimescore, addtimeText);
                StartScoredAnim(scoreText.transform, allscore - addTimescore, allscore, scoreText);

            })
            .SetTarget(transform)
            ;


        List<ItemData> itemDatas = new List<ItemData>();
        if (GameManager.Instance.gameType == GameType.MainGame)
        {
            int rV = Random.Range(100, 201);
            itemDatas.Add(new ItemData(ItemType.GoldDui, rV / 100f));
            itemDatas.Add(new ItemData(ItemType.DiamondDui, Random.Range(10, 21)));
        }
        else if (GameManager.Instance.gameType == GameType.DailyGame)
        {
            int rV = Random.Range(100, 201);
            itemDatas.Add(new ItemData(ItemType.GoldDui, rV / 100f));
            itemDatas.Add(new ItemData(ItemType.DiamondDui, Random.Range(20, 41)));
        }
        int curGameWinLv = GameManager.Instance.playerInfo.level - 1;
        if (curGameWinLv > 50)
        {
            if (TxElementMananger.Instance != null)
            {
                //掉落字母
                List<int> zimuW = new List<int>() { 5, 4, 3, 0, 1 };
                List<ItemData> zimuData = new List<ItemData>();
                zimuData.Add(new ItemData(ItemType.Z1, 1));
                zimuData.Add(new ItemData(ItemType.Z2, 1));
                zimuData.Add(new ItemData(ItemType.Z3, 1));
                zimuData.Add(new ItemData(ItemType.Z4, 1));
                zimuData.Add(new ItemData(ItemType.Z5, 1));

                int zimuT = GameManager.GetWeightIndex(zimuData, zimuW);
                itemDatas.Add(zimuData[zimuT]);
            }
            
        }
        //提前X 10
        if(curGameWinLv <= 2)
        {
            foreach (var dataconfig in itemDatas)
            {
                dataconfig.count = dataconfig.count * 10f;
                dataconfig.count = System.MathF.Round(dataconfig.count, 2);
            }
        }

        itemBase = GameManager.Instance.CreatItems(itemDatas, itemRoot);

        rewardAdButton.Init(AdsCallback, page_id, true);

        string lv_1 = PlayerPrefs.GetString("Guide_LV1", "");
        if (string.IsNullOrEmpty(lv_1))
        {
            PlayerPrefs.SetString("Guide_LV1", "yes");
            OtherSdkManager.Instance.CustomEvent("newbie_guide_6_complete", "step", "6");
        }

        noAdButton.gameObject.SetActive(curGameWinLv <= 2);
        rewardAdButton.gameObject.SetActive(curGameWinLv > 2);
        claimBtn.transform.parent.gameObject.SetActive(curGameWinLv > 2);
    }

    private void AdsCallback()
    {
        OtherSdkManager.Instance.CustomEvent("settle_popup_click", "level_id", GameManager.Instance.playerInfo.level - 1, "click", "claim_more");
        OtherSdkManager.Instance.CustomEvent("settlement_ad_reward_claim", "level_id", GameManager.Instance.playerInfo.level - 1);

        ScoreAnimStop();

        PlayerInfoUI playerInfoUI = UIManager.Instance.GetUI<PlayerInfoUI>();
        UIManager.Instance.OpenUIMask();
        float awaitTime = 2f;

    
        foreach (var item in itemBase)
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

            item.count = item.count * 10;
            item.GetItemReward();
            item.PlayItemAnim();
        }
        //GameManager.Instance.SavePlayerInfo();
        //动画
        DOTween.Sequence().AppendInterval(awaitTime).AppendCallback(() =>
        {
            playerInfoUI.GoldCanvasRecover();
            playerInfoUI.DiamondCanvasRecover();
            Hide();
        });
    }

    private void AdsCallback2()
    {
        OtherSdkManager.Instance.CustomEvent("settle_popup_click", "level_id", GameManager.Instance.playerInfo.level - 1, "click", "claim_more");
        OtherSdkManager.Instance.CustomEvent("settlement_ad_reward_claim", "level_id", GameManager.Instance.playerInfo.level - 1);

        ScoreAnimStop();

        PlayerInfoUI playerInfoUI = UIManager.Instance.GetUI<PlayerInfoUI>();
        UIManager.Instance.OpenUIMask();
        float awaitTime = 2f;


        foreach (var item in itemBase)
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
        //GameManager.Instance.SavePlayerInfo();
        //动画
        DOTween.Sequence().AppendInterval(awaitTime).AppendCallback(() =>
        {
            playerInfoUI.GoldCanvasRecover();
            playerInfoUI.DiamondCanvasRecover();
            Hide();
        });
    }

    private void CollectClick()
    {
        OtherSdkManager.Instance.CustomEvent("settle_popup_click", "level_id", GameManager.Instance.playerInfo.level - 1, "click", "claim");
        OtherSdkManager.Instance.CustomEvent("settlement_single_reward_claim", "level_id", GameManager.Instance.playerInfo.level - 1);

        ScoreAnimStop();

        PlayerInfoUI playerInfoUI = UIManager.Instance.GetUI<PlayerInfoUI>();
        UIManager.Instance.OpenUIMask();
        float awaitTime = 0.1f;
        foreach (var item in itemBase)
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
        //GameManager.Instance.SavePlayerInfo();
        //动画
        DOTween.Sequence().AppendInterval(awaitTime).AppendCallback(() =>
        {
            playerInfoUI.GoldCanvasRecover();
            playerInfoUI.DiamondCanvasRecover();
            AdManager.Instance.OnClickInterstitialAd(page_id);
            Hide();
        });
    }

    public override void Hide()
    {
        if (TxElementMananger.Instance != null)
        {
            //进度
            EventManager.Instance.TriggerEvent(GameEvent.UpdateTxProgress);
        }
        base.Hide();
    }

    public void StartScoredAnim(Transform targetTrans,int startCnt, int targetCnt, Text tartgetText)
    {
        targetTrans.DOKill();
        int _currentValue = startCnt;
        int targetGold = targetCnt;
        DOTween.To(
          () => _currentValue,
          x =>
          {
              tartgetText.text = x.ToString();
          },
          targetGold, // 目标值
          0.5f // 时长
        ).SetTarget(targetTrans)
        .OnComplete(() =>
        {
            tartgetText.text = targetGold.ToString();
        });
    }
}
