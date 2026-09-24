using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public enum PlayingCard_Shuffling
{
    NO_JQK,
    NO_A23,
}

public class SinglePlayingCard
{
    public PlayingCardType type;
    public int order;
}

public class ShufflingBase
{
    public PlayingCard_FrontOrBack frontOrBack;
    public PlayingCard_Shuffling shufflingType;

    public ShufflingBase(PlayingCard_FrontOrBack face, PlayingCard_Shuffling type)
    {
        frontOrBack = face;
        shufflingType = type;
    }
}


public class PlayingCardControl : MonoBehaviour,IEventListener
{
    public GameObject playingCardPrefab;
    public ResiduePlayingCards residuePlayingCards;

    public List<PlayingCardQueue> playingCardQueues;
    public List<PlayingCardRecycle> playingCardRecycles;

    /// <summary>
    /// 全局卡片动画速率
    /// </summary>
    public static float animRate = 1f;
    //----
    [Header("黑桃")]
    public List<Sprite> spadesPlayingCard;
    [Header("红桃")]
    public List<Sprite> heartsPlayingCard;
    [Header("梅花")]
    public List<Sprite> clubsPlayingCard;
    [Header("方块")]
    public List<Sprite> diamondsPlayingCard;
    private List<PlayingCard> allCards;
    private List<PlayingCard> residueCards;
    //上一次取牌的花色
    private PlayingCardType lastPlayingCardType;
    /// <summary>
    /// 当前背面卡数量
    /// </summary>
    private int curBackPlayingCardCnt;

    private int takeRewardCnt;
    private void OnEnable()
    {
        EventManager.Instance.RegisterListener(GameEvent.TryAutoHoming,this);
        EventManager.Instance.RegisterListener(GameEvent.StopHintAnim, this);
    }

    private void OnDisable()
    {
        EventManager.Instance.UnregisterListener(GameEvent.TryAutoHoming,this);
        EventManager.Instance.UnregisterListener(GameEvent.StopHintAnim, this);
    }

    public void Init()
    {
        GameStepRecord.Instance.ResetSteps();

        takeRewardCnt = 0;
        animRate = 1f;
        CreatAllCard();
        foreach (var card in allCards)
        {
            card.ResetThis();
        }
        residueCards = allCards.ToList().OrderBy(x => Guid.NewGuid()).ToList();

        foreach (var queue in playingCardQueues)
        {
            queue.Init();
        }
        foreach (var rec in playingCardRecycles)
        {
            rec.Init();
        }

        //CreatPlayingCardQueue();
        CreatPlayingCardQueue2();

        if (GameManager.Instance.playerInfo.level <= 1)
        {
            InitLevel1ResidueCards();
        }
        foreach (var item in residueCards)
        {
            //Debug.Log(item.GetName());
        }
        residuePlayingCards.Init(residueCards);

        curBackPlayingCardCnt = 21;
        foreach (var card in allCards)
        {
            card.ScoreInit();
            card.RandomTaskRerard();
            card.lastGameSteps= new GameStep(card.gameSteps);
        }
    }

    public void InitLevel1ResidueCards()
    {
        var targetTemplate = new List<(PlayingCardType type, int order)>
                  {
                      (PlayingCardType.spades, 1),     //黑桃2
                      (PlayingCardType.clubs, 3),      //梅花4
                      (PlayingCardType.spades, 2),     //黑桃3
                      (PlayingCardType.spades, 3),     //黑桃4
                      (PlayingCardType.diamonds,4),    //方块5
                      (PlayingCardType.clubs,5),       //梅花6
                      (PlayingCardType.hearts,6),      //红桃7
                      (PlayingCardType.clubs,6),       //梅花7
                      (PlayingCardType.hearts,7),      //红桃8
                      (PlayingCardType.hearts,8),      //红桃9
                      (PlayingCardType.spades,8),      //黑桃9
                      (PlayingCardType.spades,9),      //黑桃10
                      (PlayingCardType.hearts,9),      //红桃10
                      (PlayingCardType.hearts,10),     //红桃J
                      (PlayingCardType.clubs,10),      //梅花J
                      (PlayingCardType.spades,10),     //黑桃J
                      (PlayingCardType.clubs,11),      //梅花Q
                      (PlayingCardType.hearts,11),     //红桃Q
                      (PlayingCardType.diamonds,11),   //方块Q
                      (PlayingCardType.spades,11),     //黑桃Q
                      (PlayingCardType.hearts,12),     //红桃K
                      (PlayingCardType.diamonds,12),   //方块K
                      (PlayingCardType.clubs,12),      //梅花K
                      (PlayingCardType.spades,12)      //黑桃K
                  };

        residueCards = residueCards
            .OrderByDescending(c => targetTemplate.FindIndex(t => t.type == c.type && t.order == c.order))
            .ToList();
    }

    public int QueueBackCardCnt()
    {
        int cnt = 0;
        foreach (var queue in playingCardQueues)
        {
            foreach (var cell in queue.Cells)
            {
                if(cell.frontOrBack == PlayingCard_FrontOrBack.Back)
                {
                    cnt++;
                }
            }
        }

        return cnt;
    }

    public void TryAutoHoming()
    {
        //if(residuePlayingCards.point1.childCount > 0)
        //{
        //    PlayingCard curResiduePlayingCard = residuePlayingCards.point1.GetChild(0).GetComponent<PlayingCard>();

        //    if(curResiduePlayingCard.TryRecycleHoming())
        //    {
        //        return;
        //    }
        //    if (curResiduePlayingCard.TryQueueHoming(true))
        //    {
        //        return;
        //    }
        //}

        foreach (var queue in playingCardQueues)
        {
            int lastindex = queue.Cells.Count;
            if(lastindex == 0)
            {
                continue;
            }

            PlayingCard target = queue.Cells[lastindex - 1];
            if (target.TryRecycleHoming())
            {
                return;
            }
            if (lastindex == 1)
            {
                if (target.TryQueueHoming())
                {
                    return;
                }
            }
            else
            {
                PlayingCard target2 = queue.Cells[lastindex - 2];
                if(target2.frontOrBack == PlayingCard_FrontOrBack.Back)
                {
                    if (target.TryQueueHoming())
                    {
                        return;
                    }
                }
            }
        }

        GameScenePanel gameScenePanel = UIManager.Instance.GetUI<GameScenePanel>();
        if (gameScenePanel.isEffectualSteps)
        {
            gameScenePanel.ReportLevelStep("move");
        }
        Debug.Log("自动归位检测结束");
    }

    public List<PlayingCard> GetAllPlayingCard()
    {
        return allCards;
    }

    private void CreatPlayingCardQueue2()
    {
        var config = LevelConfigData.GetLevelData(GameManager.Instance.playerInfo.level);

        for (int i = 0; i < config.queues.Count; i++)
        {
            List<PlayingCard> playingCards = new List<PlayingCard>();
            string[] oneColCards = config.queues[i].Split(',');

            for (int c = 0; c < oneColCards.Length; c++)
            {
                PlayingCard playingCard = GetConfigPlayingCard(oneColCards[c]);
                // 是最后一张
                bool isLast = c == oneColCards.Length - 1;
                if (isLast)
                {
                    playingCard.SetFront(false);
                }
                else
                {
                    playingCard.SetBack(false);
                }
                playingCards.Add(playingCard);
            }
            playingCardQueues[i].AddPlayingCards(playingCards);
            foreach (var _cell in playingCards)
            {
                _cell.RightAwayBackCorrectPos();
            }
        }
    }

    public PlayingCard GetConfigPlayingCard(string cardStr)
    {
        var temList = residueCards.ToList();
        int order = GetOrder(cardStr);
        PlayingCardType type = GetCardSuit(cardStr);
        foreach (var card in temList)
        {
            if(order== card.order && type == card.type)
            {
                residueCards.Remove(card);
                return card;
            }
        }

        residueCards.RemoveAt(0);
        return temList[0];
    }

    public int GetOrder(string cardStr)
    {
        string s = cardStr.TrimEnd('S', 'C', 'D', 'H');
        List<string> arr = new List<string>(){ "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };
        return arr.IndexOf(s);
    }
    public static PlayingCardType GetCardSuit(string cardStr)
    {
        char suit = cardStr[cardStr.Length - 1];
        switch (suit)
        {
            case 'S':
                return PlayingCardType.spades;
            case 'H':
                return PlayingCardType.hearts;
            case 'C':
                return PlayingCardType.clubs;
            case 'D':
                return PlayingCardType.diamonds;
            default:
                return PlayingCardType.spades;
        }
    }

    private void CreatPlayingCardQueue()
    {
        var config = GetPlayingCardQueueConfig();
        int changeCount = 2;
        // 直接创建并初始化
        List<int> hava_A = Enumerable.Range(0, 7)
            .Select((x, index) => index < changeCount ? 1 : 0)
            .OrderBy(x => Guid.NewGuid())
            .ToList();

        Debug.Log($"hava_A = [{string.Join(", ", hava_A)}]");
        //先取正面跟hava_A要求的A
        int queueIndex = 0;
        foreach (var queueList in config)
        {
            List<PlayingCard> _cells = new List<PlayingCard>();
            foreach (var shufflingBase in queueList)
            {
                if (shufflingBase.frontOrBack == PlayingCard_FrontOrBack.Front)
                {
                    if(hava_A[queueIndex] == 1)
                    {
                        PlayingCard _card = GetPlayingCard_A(shufflingBase);
                        _cells.Add(_card);
                    }
                    else
                    {
                        PlayingCard _card = GetListTargetPlayingCard(shufflingBase);
                        _cells.Add(_card);
                    }
                }
            }
            playingCardQueues[queueIndex].AddPlayingCards(_cells);
            foreach (var _cell in _cells)
            {
                _cell.RightAwayBackCorrectPos();
            }
            queueIndex++;
        }
        //背面
        queueIndex = 0;
        foreach (var queueList in config)
        {
            List<PlayingCard> _cells = new List<PlayingCard>();
            foreach (var shufflingBase in queueList)
            {
                if (shufflingBase.frontOrBack == PlayingCard_FrontOrBack.Back)
                {
                    PlayingCard _card = GetListTargetPlayingCard(shufflingBase);
                    _cells.Add(_card);
                }
            }
            //把正面放在最后面
            _cells.Add(playingCardQueues[queueIndex].Cells[0]);
            playingCardQueues[queueIndex].RemovePlayingCard(_cells);
            playingCardQueues[queueIndex].AddPlayingCards(_cells);
            foreach (var _cell in _cells)
            {
                _cell.RightAwayBackCorrectPos();
            }
            queueIndex++;
        }


    }

    /// <summary>
    /// 队列配置
    /// </summary>
    /// <returns></returns>
    private List<List<ShufflingBase>> GetPlayingCardQueueConfig()
    {
        List<List<ShufflingBase>> config = new List<List<ShufflingBase>>();
        List<ShufflingBase> queue1 = new List<ShufflingBase>()
        {
           new ShufflingBase(PlayingCard_FrontOrBack.Front,PlayingCard_Shuffling.NO_JQK)
        };
        List<ShufflingBase> queue2 = new List<ShufflingBase>()
        {
           new ShufflingBase(PlayingCard_FrontOrBack.Back,PlayingCard_Shuffling.NO_JQK),
           new ShufflingBase(PlayingCard_FrontOrBack.Front,PlayingCard_Shuffling.NO_JQK)
        };
        List<ShufflingBase> queue3 = new List<ShufflingBase>()
        {
           new ShufflingBase(PlayingCard_FrontOrBack.Back,PlayingCard_Shuffling.NO_A23),
           new ShufflingBase(PlayingCard_FrontOrBack.Back,PlayingCard_Shuffling.NO_JQK),
           new ShufflingBase(PlayingCard_FrontOrBack.Front,PlayingCard_Shuffling.NO_JQK)
        };
        List<ShufflingBase> queue4 = new List<ShufflingBase>()
        {
           new ShufflingBase(PlayingCard_FrontOrBack.Back,PlayingCard_Shuffling.NO_A23),
           new ShufflingBase(PlayingCard_FrontOrBack.Back,PlayingCard_Shuffling.NO_JQK),
           new ShufflingBase(PlayingCard_FrontOrBack.Back,PlayingCard_Shuffling.NO_JQK),
           new ShufflingBase(PlayingCard_FrontOrBack.Front,PlayingCard_Shuffling.NO_JQK)
        };
        List<ShufflingBase> queue5 = new List<ShufflingBase>()
        {
           new ShufflingBase(PlayingCard_FrontOrBack.Back,PlayingCard_Shuffling.NO_A23),
           new ShufflingBase(PlayingCard_FrontOrBack.Back,PlayingCard_Shuffling.NO_JQK),
           new ShufflingBase(PlayingCard_FrontOrBack.Back,PlayingCard_Shuffling.NO_JQK),
           new ShufflingBase(PlayingCard_FrontOrBack.Back,PlayingCard_Shuffling.NO_JQK),
           new ShufflingBase(PlayingCard_FrontOrBack.Front,PlayingCard_Shuffling.NO_JQK)
        };
        List<ShufflingBase> queue6 = new List<ShufflingBase>()
        {
           new ShufflingBase(PlayingCard_FrontOrBack.Back,PlayingCard_Shuffling.NO_A23),
           new ShufflingBase(PlayingCard_FrontOrBack.Back,PlayingCard_Shuffling.NO_A23),
           new ShufflingBase(PlayingCard_FrontOrBack.Back,PlayingCard_Shuffling.NO_JQK),
           new ShufflingBase(PlayingCard_FrontOrBack.Back,PlayingCard_Shuffling.NO_JQK),
           new ShufflingBase(PlayingCard_FrontOrBack.Back,PlayingCard_Shuffling.NO_JQK),
           new ShufflingBase(PlayingCard_FrontOrBack.Front,PlayingCard_Shuffling.NO_JQK)
        };
        List<ShufflingBase> queue7 = new List<ShufflingBase>()
        {
           new ShufflingBase(PlayingCard_FrontOrBack.Back,PlayingCard_Shuffling.NO_A23),
           new ShufflingBase(PlayingCard_FrontOrBack.Back,PlayingCard_Shuffling.NO_A23),
           new ShufflingBase(PlayingCard_FrontOrBack.Back,PlayingCard_Shuffling.NO_A23),
           new ShufflingBase(PlayingCard_FrontOrBack.Back,PlayingCard_Shuffling.NO_JQK),
           new ShufflingBase(PlayingCard_FrontOrBack.Back,PlayingCard_Shuffling.NO_JQK),
           new ShufflingBase(PlayingCard_FrontOrBack.Back,PlayingCard_Shuffling.NO_JQK),
           new ShufflingBase(PlayingCard_FrontOrBack.Front,PlayingCard_Shuffling.NO_JQK)
        };
        config.Add(queue1);
        config.Add(queue2);
        config.Add(queue3);
        config.Add(queue4);
        config.Add(queue5);
        config.Add(queue6);
        config.Add(queue7);
        return config;
    }

   /// <summary>
   /// 取配置对应的卡
   /// </summary>
   /// <param name="shufflingBase"></param>
   /// <returns></returns>
    private PlayingCard GetListTargetPlayingCard(ShufflingBase shufflingBase)
    {
        var temList = residueCards.ToList().OrderBy(x => Guid.NewGuid()).ToList();
        PlayingCard target = temList[0];
        foreach (var _card in temList)
        {
            //取背面牌不包括A
            if(shufflingBase.frontOrBack == PlayingCard_FrontOrBack.Back && GetName(_card.order) == "A")
            {
                continue;
            }
            //红黑红黑交替取牌
            if(lastPlayingCardType == PlayingCardType.clubs || lastPlayingCardType == PlayingCardType.spades)
            {
                if (_card.type == PlayingCardType.clubs || _card.type == PlayingCardType.spades)
                {
                    continue;
                }
            }
            else if (lastPlayingCardType == PlayingCardType.diamonds || lastPlayingCardType == PlayingCardType.hearts)
            {
                if (_card.type == PlayingCardType.diamonds || _card.type == PlayingCardType.hearts)
                {
                    continue;
                }
            }

            if (CheckPlayingCard_Shuffling(_card, shufflingBase.shufflingType))
            {
                target = _card;
                lastPlayingCardType = _card.type;
                break;
            }
        }
        switch (shufflingBase.frontOrBack)
        {
            case PlayingCard_FrontOrBack.Front:
                target.SetFront(false);
                break;
            case PlayingCard_FrontOrBack.Back:
                target.SetBack(false);
                break;
        }
        residueCards.Remove(target);
        return target;
    }
    /// <summary>
    /// 取A
    /// </summary>
    /// <param name="shufflingBase"></param>
    /// <returns></returns>
    private PlayingCard GetPlayingCard_A(ShufflingBase shufflingBase)
    {
        var temList = residueCards.ToList().OrderBy(x => Guid.NewGuid()).ToList();
        PlayingCard target = temList[0];
        foreach (var _card in temList)
        {
            if(GetName(_card.order) == "A")
            {
                target = _card;
                break;
            }
        }
        switch (shufflingBase.frontOrBack)
        {
            case PlayingCard_FrontOrBack.Front:
                target.SetFront(false);
                break;
            case PlayingCard_FrontOrBack.Back:
                target.SetBack(false);
                break;
        }
        residueCards.Remove(target);
        return target;
    }
    /// <summary>
    /// 生成所有卡
    /// </summary>
    private void CreatAllCard()
    {
        if(allCards == null)
        {
            allCards = new List<PlayingCard>();
            for (int i = 0; i < spadesPlayingCard.Count; i++)
            {
                var obj = Instantiate(playingCardPrefab, transform);
                PlayingCard playingCard = obj.GetComponent<PlayingCard>();
                string name = GetName(i);
                obj.name = name;
                playingCard.Init(spadesPlayingCard[i], i, PlayingCardType.spades);
                allCards.Add(playingCard);
            }
            for (int i = 0; i < heartsPlayingCard.Count; i++)
            {
                var obj = Instantiate(playingCardPrefab, transform);
                PlayingCard playingCard = obj.GetComponent<PlayingCard>();
                string name = GetName(i);
                obj.name = name;
                playingCard.Init(heartsPlayingCard[i], i, PlayingCardType.hearts);
                allCards.Add(playingCard);
            }
            for (int i = 0; i < clubsPlayingCard.Count; i++)
            {
                var obj = Instantiate(playingCardPrefab, transform);
                PlayingCard playingCard = obj.GetComponent<PlayingCard>();
                string name = GetName(i);
                obj.name = name;
                playingCard.Init(clubsPlayingCard[i], i, PlayingCardType.clubs);
                allCards.Add(playingCard);
            }
            for (int i = 0; i < diamondsPlayingCard.Count; i++)
            {
                var obj = Instantiate(playingCardPrefab, transform);
                PlayingCard playingCard = obj.GetComponent<PlayingCard>();
                string name = GetName(i);
                obj.name = name;
                playingCard.Init(diamondsPlayingCard[i], i, PlayingCardType.diamonds);
                allCards.Add(playingCard);
            }
        }
    }

    public static string GetName(int index)
    {
        string[] nameArr = { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };
        return nameArr[index];
    }

    public bool CheckPlayingCard_Shuffling(PlayingCard card, PlayingCard_Shuffling type)
    {
        switch (type)
        {
            case PlayingCard_Shuffling.NO_JQK:
                if(GetName(card.order) != "J" && GetName(card.order) != "Q" && GetName(card.order) != "K")
                {
                    return true;
                }
                break;
            case PlayingCard_Shuffling.NO_A23:
                if (GetName(card.order) != "A" && GetName(card.order) != "2" && GetName(card.order) != "3")
                {
                    return true;
                }
                break;
        }
        return false;
    }

    public void OnEventTriggered(GameEvent eventType, object data = null)
    {
        switch (eventType)
        {
            case GameEvent.TryAutoHoming:
                if (QueueBackCardCnt() > 0)
                {
                    TryAutoHoming();
                }
                else
                {
                    StartCoroutine(AutoRecycleAllCard());
                }
                break;
            case GameEvent.StopHintAnim:

                foreach (var card in allCards)
                {
                    card.StopHintAnim();
                }

                break;


        }
    }


    private IEnumerator AutoRecycleAllCard()
    {
        Debug.Log("游戏结束，自动回收");

        UIManager.Instance.OpenUIMask();
        //先找到4个A
        foreach (var card in allCards)
        {
            if(GetName(card.order) == "A")
            {
                //不在回收区
                if(card.playingCardRecycle == null)
                {
                    foreach (var Recycle in playingCardRecycles)
                    {
                        if(Recycle.CheckRecycleRootsCondition(card))
                        {
                            if(card.playingCardQueue != null)
                            {
                                List<PlayingCard> temlist = new List<PlayingCard>();
                                temlist.Add(card);
                                var pQ = card.playingCardQueue;
                                pQ.RemovePlayingCard(temlist);
                                pQ.ResetCellPos();
                            }
                            residuePlayingCards.RemovePlayingCard(card);
                            Recycle.AddPlayingCards(card);
                            card.SetFront(false, false, false);
                            card.BackCorrectPos(true, () =>
                            {
                                card.CollectScore(true);
                            });
                            
                            yield return new WaitForSeconds(0.1f);
                            break;
                        }
                    }
                }
            }
        }

        int index = 0;
        while (index < 12)
        {
            foreach (var Recycle in playingCardRecycles)
            {
                PlayingCard target = Recycle.recycleRoot.GetChild(Recycle.recycleRoot.childCount - 1).GetComponent<PlayingCard>();
                foreach (var card in allCards)
                {
                    if (card.type == target.type && card.order == target.order + 1)
                    {
                        //不在回收区
                        if (card.playingCardRecycle == null)
                        {
                            if (card.playingCardQueue != null)
                            {
                                List<PlayingCard> temlist = new List<PlayingCard>();
                                temlist.Add(card);
                                var pQ = card.playingCardQueue;
                                pQ.RemovePlayingCard(temlist);
                                pQ.ResetCellPos();
                            }
                            residuePlayingCards.RemovePlayingCard(card);
                            Recycle.AddPlayingCards(card);
                            card.SetFront(false, false, false);
                            card.BackCorrectPos(true, () =>
                            {
                                card.CollectScore(true);
                            });
                            yield return new WaitForSeconds(0.1f);
                            break;
                        }
                    }
                }
            }
            index++;
        }

        yield return new WaitForSeconds(0.5f);
        UIManager.Instance.GetUI<GameScenePanel>().ReportLevelStep("move");

        if (SettingPanel.IsVibrateEnabled)
        {
            Handheld.Vibrate();
        }
        GameScenePanel.isPause = true;
        UIManager.Instance.HideUIMask();
        int curLv = GameManager.Instance.playerInfo.level;
        if (GameManager.Instance.gameType == GameType.MainGame)
        {
            GameManager.Instance.playerInfo.level++;
            //箱子
            GameBox.Instance.CheckBoxProgress(curLv ,() =>
            {
                GameScenePanel.isPause = true;
                UIManager.Instance.OpenUI<GameWinPanel>(null, () =>
                {
                    ArchitectureQueryResult architectureQueryResult = GameManager.Instance.architectureConfig.QueryFirstByLevel(curLv);
                    if (architectureQueryResult != null)
                    {
                        UIManager.Instance.OpenUI<ArchitecturePanel>(curLv, () =>
                        {
                            //刷新建筑
                            UIManager.Instance.GetUI<LobbyScenePanel>().lobbyHomePanel.RefreshAllArchitecture();

                            NextLevel(curLv);
                        });
                    }
                    else
                    {
                        NextLevel(curLv);
                    }
                });
            });
        }
        else if (GameManager.Instance.gameType == GameType.DailyGame)
        {
            //箱子
            GameBox.Instance.CheckBoxProgress(curLv ,() =>
            {
                GameScenePanel.isPause = true;
                UIManager.Instance.OpenUI<GameWinPanel>(null, () =>
                {
                    NextLevel(curLv);
                });
            });
        }
     
    }

    private void NextLevel(int curLv)
    {
        if (GameManager.Instance.gameType == GameType.MainGame)
        {
            if (curLv % 3 == 0)
            {
                GameManager.Instance.EvaluationGameCallback = () =>
                {
                    string str = PlayerPrefs.GetString("EvaluationGame", "");
                    if (string.IsNullOrEmpty(str))
                    {
                        PlayerPrefs.SetString("EvaluationGame", "yes");
                        AdManager.Instance.applovinMaxInterstitialOperator.insertClickCoolingTime = 0f;
                        AdManager.Instance.applovinMaxInterstitialOperator.insertTimer = ApplovinMaxInterstitialOperator.insertTime;
                        OtherSdkManager.Instance.CustomEvent("game_60s_start", "time", SecondsToHMS(Time.realtimeSinceStartup));
                    }
                    //GameManager.Instance.TryEvaluationGame();
                };
            }
            UIManager.Instance.GetUI<GameScenePanel>().ResetGame();
        }
        else if (GameManager.Instance.gameType == GameType.DailyGame)
        {
            EventManager.Instance.TriggerEvent(GameEvent.DailyChallengeComplete);
            UIManager.Instance.GetUI<GameScenePanel>().TrackLevelExit("settle_back_lobby");
            UIManager.Instance.GetUI<GameScenePanel>().Hide();
            UIManager.Instance.OpenUI<LobbyScenePanel>(2);
            AudioManager.Instance.PlayBGM("BGM1");
        }


        GameManager.Instance.SavePlayerInfo();
    }

    /// <summary>
    /// 检查卡牌携带奖励
    /// </summary>
    /// <param name="_callback"></param>
    /// <returns></returns>
    public bool CheckQueueTaskReward(Action _callback = null)
    {
        PlayingCard target = null;
        foreach (var queue in playingCardRecycles)
        {
            int cellCnt = queue.recycleRoot.childCount;
            if (cellCnt == 0)
            {
                continue;
            }

            PlayingCard temP = queue.recycleRoot.GetChild(cellCnt - 1).transform.GetComponent<PlayingCard>();
            if (temP.takeRewardType != TakeRewardType.NULL)
            {
                target = temP;
                break;
            }
        }

        if (target == null)
        {
            foreach (var queue in playingCardQueues)
            {
                int cellCnt = queue.Cells.Count;
                if (cellCnt == 0)
                {
                    continue;
                }

                if (queue.Cells[cellCnt - 1].takeRewardType != TakeRewardType.NULL)
                {
                    target = queue.Cells[cellCnt - 1];
                    break;
                }
            }
        }


        if(target != null)
        {
            takeRewardCnt++;
            int baseV = 10;
            int signV = 2;
            bool isOK = false;
            if (takeRewardCnt <= 4)
            {
                isOK = false;//一定不触发
            }
            else if(takeRewardCnt >= 8)
            {
                isOK = true;//一定触发
            }
            else
            {
                int rG = UnityEngine.Random.Range(1, 101);
                int cG = baseV + signV * takeRewardCnt;
                isOK = rG <= cG;
            }

            if (isOK)
            {
                Debug.Log($"第{takeRewardCnt}次暗牌{target.GetName()}--触发奖励{target.takeRewardType}");
                takeRewardCnt = 0;
                switch (target.takeRewardType)
                {
                    case TakeRewardType.Wheel:
                        GameScenePanel.isPause = true;
                        UIManager.Instance.OpenUI<TakeReward_WheelPanel>(target, () =>
                        {
                            GameScenePanel.isPause = false;
                            _callback?.Invoke();
                        });
                        break;
                }
                return true;
            }
            target.takeRewardType = TakeRewardType.NULL;
            target.UpdateTakeRewardUI();
        }
       
        _callback?.Invoke();
        return false;
    }

    public string SecondsToHMS(float totalSeconds)
    {
        int hours = Mathf.FloorToInt(totalSeconds / 3600);
        int minutes = Mathf.FloorToInt((totalSeconds % 3600) / 60);
        int seconds = Mathf.FloorToInt(totalSeconds % 60);
        return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
    }
}
