using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameBox : MonoBehaviour,IEventListener
{
    public static GameBox Instance;
    public static int curLv;

    public Transform effectPoint;
    public Transform xiangzi;
    public Transform xiangzi_kai;
    public Transform xiangzi_bi;

    public Animation anim;
    public Text progressStr;
    public Slider slider;
    private int progress;
    private int rewardIndex;
    private List<int> targetCnt;
    private List<List<ItemData>> rewards;
    private int firstBoxRewardIndex;
    private int lastBoxRewardIndex;
    private int boxRewardLevelId;
    private int boxRewardReachToShowMs;
    private float boxRewardShowTime;
    private string boxRewardClaimMethod;
    private int lastBoxIndex;
    private float lastBoxReachTime;
    private void Awake()
    {
        Instance = this;
    }
    private void OnEnable()
    {
        EventManager.Instance.RegisterListener(GameEvent.AddGameBox, this);
    }
    private void OnDisable()
    {
        EventManager.Instance.UnregisterListener(GameEvent.AddGameBox, this);
    }

    public void Init()
    {
        targetCnt = new List<int>() { 15, 30, 50 };
        if(GameManager.Instance.playerInfo.level == 1)
        {
            rewards = new List<List<ItemData>>();
            List<ItemData> itemDatas = new List<ItemData>();
            itemDatas.Add(new ItemData(ItemType.GoldDui, 20));
            rewards.Add(itemDatas);

            List<ItemData> itemDatas2 = new List<ItemData>();
            itemDatas2.Add(new ItemData(ItemType.GoldDui, 20));
            rewards.Add(itemDatas2);

            List<ItemData> itemDatas3 = new List<ItemData>();
            itemDatas3.Add(new ItemData(ItemType.GoldDui, 20));
            rewards.Add(itemDatas3);
        }
        else if (GameManager.Instance.playerInfo.level == 2)
        {
            rewards = new List<List<ItemData>>();
            List<ItemData> itemDatas = new List<ItemData>();
            itemDatas.Add(new ItemData(ItemType.GoldDui, 10));
            rewards.Add(itemDatas);

            List<ItemData> itemDatas2 = new List<ItemData>();
            itemDatas2.Add(new ItemData(ItemType.GoldDui, 10));
            rewards.Add(itemDatas2);

            List<ItemData> itemDatas3 = new List<ItemData>();
            itemDatas3.Add(new ItemData(ItemType.GoldDui, 10));
            rewards.Add(itemDatas3);
        }
        else if (GameManager.Instance.playerInfo.level == 3)
        {
            rewards = new List<List<ItemData>>();
            List<ItemData> itemDatas = new List<ItemData>();
            itemDatas.Add(new ItemData(ItemType.GoldDui, 30));
            rewards.Add(itemDatas);

            List<ItemData> itemDatas2 = new List<ItemData>();
            itemDatas2.Add(new ItemData(ItemType.GoldDui, 30));
            rewards.Add(itemDatas2);

            List<ItemData> itemDatas3 = new List<ItemData>();
            itemDatas3.Add(new ItemData(ItemType.GoldDui, 30));
            rewards.Add(itemDatas3);

        }
        else
        {
            rewards = new List<List<ItemData>>();
            List<ItemData> itemDatas = new List<ItemData>();
            itemDatas.Add(new ItemData(ItemType.GoldDui, 20));
            rewards.Add(itemDatas);

            List<ItemData> itemDatas2 = new List<ItemData>();
            itemDatas2.Add(new ItemData(ItemType.GoldDui, 30));
            rewards.Add(itemDatas2);

            List<ItemData> itemDatas3 = new List<ItemData>();
            itemDatas3.Add(new ItemData(ItemType.GoldDui, 40));
            rewards.Add(itemDatas3);
        }


        progress = 0;
        rewardIndex = 0;
        lastBoxIndex = 0;
        lastBoxReachTime = 0f;
        UpdateProgressUI();
        PlayIdleAnim();

        xiangzi_bi.gameObject.SetActive(false);
        xiangzi_kai.gameObject.SetActive(false);
    }

    public void PlayIdleAnim()
    {
        anim.Play("GameBoxAnim");
    }
    public void PlayGetProgressAnim()
    {
        anim.Stop();
        transform.DOKill();
        anim.Play("GameBoxAnim2");
        DOTween.Sequence().AppendInterval(2f).AppendCallback(() =>
        {
            PlayIdleAnim();
        }).SetTarget(transform);
    }

    public int LastBoxIndex => lastBoxIndex;

    public int GetSinceLastBoxMs()
    {
        return Mathf.RoundToInt((Time.realtimeSinceStartup - lastBoxReachTime) * 1000f);
    }

    public void SetBoxRewardClaimMethod(string claimMethod)
    {
        boxRewardClaimMethod = claimMethod;
    }

    private void TrackBoxRewardShow(int levelId, int firstRewardIndex, int lastRewardIndex, int reachToShowMs)
    {
        boxRewardLevelId = levelId;
        firstBoxRewardIndex = firstRewardIndex;
        lastBoxRewardIndex = lastRewardIndex;
        boxRewardReachToShowMs = reachToShowMs;
        boxRewardShowTime = Time.realtimeSinceStartup;
        boxRewardClaimMethod = "normal";
        for (int i = firstBoxRewardIndex; i < lastBoxRewardIndex; i++)
        {
            OtherSdkManager.Instance.CustomEvent("level_box_show", "level_id", boxRewardLevelId, "box_index", i + 1, "box_target", targetCnt[i], "reach_to_show_ms", boxRewardReachToShowMs);
        }
    }

    private void TrackBoxRewardClose()
    {
        int boxDwellMs = Mathf.RoundToInt((Time.realtimeSinceStartup - boxRewardShowTime) * 1000f);
        for (int i = firstBoxRewardIndex; i < lastBoxRewardIndex; i++)
        {
            OtherSdkManager.Instance.CustomEvent("level_box_close", "level_id", boxRewardLevelId, "box_index", i + 1, "box_target", targetCnt[i], "reach_to_show_ms", boxRewardReachToShowMs, "box_dwell_ms", boxDwellMs, "claim_method", boxRewardClaimMethod);
        }
    }

    public void UpdateProgressUI()
    {
        if (rewardIndex >= targetCnt.Count)
        {
            return;
        }

        int curMaxCnt = targetCnt[rewardIndex];

        slider.maxValue = curMaxCnt;
        slider.value = progress;
        progressStr.text = $"{progress}/{curMaxCnt}";
        Debug.Log("箱子进度：" + progress);
    }

    /// <summary>
    /// 检查箱子进度
    /// </summary>
    /// <param name="_callback"></param>
    /// <returns></returns>
    public bool CheckBoxProgress(int _lv,Action _callback = null)
    {
        // 全部奖励已领完，直接回调退出
        if (rewardIndex >= targetCnt.Count)
        {
            _callback?.Invoke();
            return false;
        }

        bool hasReward = false;
        int firstRewardIndex = rewardIndex;
        float firstReachTime = Time.realtimeSinceStartup;
        List<ItemData> allGetRewards = new List<ItemData>();
        // 循环领取所有当前进度已达标的档位
        while (rewardIndex < targetCnt.Count && progress >= targetCnt[rewardIndex])
        {
            hasReward = true;
            Debug.Log("领取档位：" + rewardIndex);
            OtherSdkManager.Instance.CustomEvent("level_box_reach", "level_id", _lv, "box_index", rewardIndex + 1, "box_target", targetCnt[rewardIndex], "box_progress", progress);
            lastBoxIndex = rewardIndex + 1;
            lastBoxReachTime = Time.realtimeSinceStartup;
            allGetRewards.AddRange(rewards[rewardIndex]);
            rewardIndex++;
        }

        // 没有可领取奖励，直接回调返回
        if (!hasReward)
        {
            _callback?.Invoke();
            return false;
        }
        GameScenePanel.isPause = true;
        UIManager.Instance.OpenUIMask();

        xiangzi_bi.transform.position = xiangzi.transform.position;
        xiangzi_bi.transform.localScale = Vector3.one * 0.5f;
        xiangzi_bi.gameObject.SetActive(true);

        xiangzi_bi.transform.DOLocalMoveX(-330f, 0.5f);
        xiangzi_bi.transform.DOLocalMoveY(-280f, 0.25f);
        xiangzi_bi.transform.DOScale(1f, 0.5f);

        DOTween.Sequence()
            .AppendInterval(0.5F)
            .AppendCallback(() =>
            {
                xiangzi_bi.transform.DOScale(2f, 0.5f);
                xiangzi_bi.transform.DOLocalMoveX(0f, 0.5f);
                xiangzi_bi.transform.DOLocalMoveY(0f, 0.25f);
            })
            .AppendInterval(0.5F)
            .AppendCallback(() =>
            {
                xiangzi_bi.gameObject.SetActive(false);
                xiangzi_kai.gameObject.SetActive(true);
            })
            .AppendInterval(0.1F)
            .AppendCallback(() =>
            {

                UIManager.Instance.HideUIMask();
                OtherSdkManager.Instance.CustomEvent("game_box_show", "show", "");
                if (_lv == 1 || _lv == 2)
                {
                    UIManager.Instance.OpenUI<GeneralRewardsPanel2>(allGetRewards, () =>
                    {
                        TrackBoxRewardClose();
                        xiangzi_kai.gameObject.SetActive(false);
                        GameScenePanel.isPause = false;
                        UpdateProgressUI();
                        _callback?.Invoke();
                    });
                    int reachToShowMs = Mathf.RoundToInt((Time.realtimeSinceStartup - firstReachTime) * 1000f);
                    TrackBoxRewardShow(_lv, firstRewardIndex, rewardIndex, reachToShowMs);
                }
                else
                {
                    curLv = _lv;
                    UIManager.Instance.OpenUI<GeneralRewardsPanel>(allGetRewards, () =>
                    {
                        TrackBoxRewardClose();
                        xiangzi_kai.gameObject.SetActive(false);
                        GameScenePanel.isPause = false;
                        UpdateProgressUI();
                        _callback?.Invoke();
                    });
                    int reachToShowMs = Mathf.RoundToInt((Time.realtimeSinceStartup - firstReachTime) * 1000f);
                    TrackBoxRewardShow(_lv, firstRewardIndex, rewardIndex, reachToShowMs);
                }
                  
            })
            ;
        return true;
    }



    public void OnEventTriggered(GameEvent eventType, object data = null)
    {
        if(eventType == GameEvent.AddGameBox)
        {
            int cnt = (int)data;
            progress += cnt;
            UpdateProgressUI();
        }
    }
}
