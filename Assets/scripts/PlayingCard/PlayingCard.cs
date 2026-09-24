using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;



public enum PlayingCard_FrontOrBack
{
    Front,
    Back,
}
public enum PlayingCardType
{
    /// <summary>
    /// 黑桃
    /// </summary>
    spades,
    /// <summary>
    /// 红桃
    /// </summary>
    hearts,
    /// <summary>
    /// 梅花
    /// </summary>
    clubs,
    /// <summary>
    /// 方块
    /// </summary>
    diamonds,
}

public enum TakeRewardType
{
    NULL,
    Wheel,
}

public class PlayingCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public AudioSource audioSource;
    public Image cardImg;
    public Image backImg;
    public Image hintAnimImg;
    public Transform hintAnimMask;
    //分数
    public Text ScoreText;
    public CanvasGroup ScoreTextcanvasGroup;
    //获得金币动画
    public Transform goldAnimTrans;
    public Image g1;
    public Image g2;
    public Image g3;
    [Header("回收动画")]
    public Transform RecycleAnimTrans;
    public Transform spadesAnim;
    public Transform heartsAnim;
    public Transform clubsAnim;
    public Transform diamondsAnim;
    public Transform tuoweieffect;
    public Animation correctAnim;

    public Transform pointX;
    public Transform pointY;
    //---------------------
    public GameStep lastGameSteps;
    public GameStep gameSteps;
    //--------------------------
    [HideInInspector]
    public PlayingCardQueue playingCardQueue; //所在队列
    public PlayingCardRecycle playingCardRecycle;//所在回收区域
    public ResiduePlayingCards residuePlayingCards;//剩余区域
    public Vector3 correctPos;
    [HideInInspector]
    public int order = -1;
    [HideInInspector]
    public PlayingCardType type;
    [HideInInspector]
    public PlayingCard_FrontOrBack frontOrBack;
    [HideInInspector]
    public bool isExchange = false; //交换中
    //携带奖励
    public Image takeRewardImg;
    public TakeRewardType takeRewardType = TakeRewardType.NULL;
    public List<Sprite> takeRewardImgs;
    //---------------------
    public int queueScore;//首次连接分
    public int residueScore;//剩余区连接分
    public int recycleScore;//首次回收分
    public int backScore;//首次翻开分
    //---------------------
    private float diffX;
    private float diffY;
    private bool isDrag = true;
    private bool isClick = true;
    private Vector3 _dragStartOffset; // 触摸点与芯片的初始偏移
    private float zOffset;
    private List<PlayingCard> connectedPlayingCards;
    private Action EndDragCallback;
    private Sequence hintAnimSequence;
    private void Start()
    {
        zOffset = Mathf.Abs(Camera.main.transform.position.z - transform.position.z);
        diffX = Vector3.Distance(transform.position, pointX.transform.position);
        diffY = Vector3.Distance(transform.position, pointY.transform.position);
        hintAnimMask.gameObject.SetActive(false);
        takeRewardImg.gameObject.SetActive(false);
        ScoreText.gameObject.SetActive(false);
        goldAnimTrans.gameObject.SetActive(false);
        GameManager.Instance.UpdateAppATTToDiamond(g1);
        GameManager.Instance.UpdateAppATTToDiamond(g2);
        GameManager.Instance.UpdateAppATTToDiamond(g3);
        RecycleAnimTrans.gameObject.SetActive(false);
        tuoweieffect.gameObject.SetActive(false);
    }
    public void Init(Sprite _sp, int _order, PlayingCardType _type)
    {
        cardImg.sprite = _sp;
        order = _order;
        type = _type;

        gameSteps.playingCard = this;
    }

    public void RandomTaskRerard()
    {
        takeRewardType = TakeRewardType.NULL;
        UpdateTakeRewardUI();

        if (GameManager.Instance.playerInfo.level <= 1)
        {
            return;
        }
        if (playingCardQueue != null && frontOrBack == PlayingCard_FrontOrBack.Back)
        {
            takeRewardType = TakeRewardType.Wheel;//安排标记

            //if (UnityEngine.Random.value <= 0.15f)
            //{
            //    takeRewardType = TakeRewardType.Wheel;
            //    Debug.Log(GetName() + "-携带奖励:" + takeRewardType);
            //}
        }
    }

    public void UpdateTakeRewardUI()
    {
        takeRewardImg.gameObject.SetActive(takeRewardType != TakeRewardType.NULL);
        takeRewardImg.sprite = takeRewardImgs[(int)takeRewardType];
        takeRewardImg.SetNativeSize();
    }

    public void ResetThis()
    {
        isDrag = true;
        isClick = true;
        playingCardQueue = null;
        playingCardRecycle = null;
        residuePlayingCards = null;
        StopHintAnim();

        queueScore = 0;
        residueScore = 0;
        recycleScore = 0;
        backScore = 0;
        ScoreText.transform.DOKill();
        ScoreText.gameObject.SetActive(false);
        goldAnimTrans.transform.DOKill();
        goldAnimTrans.gameObject.SetActive(false);
        RecycleAnimTrans.transform.DOKill();
        RecycleAnimTrans.gameObject.SetActive(false);
        tuoweieffect.DOKill();
        tuoweieffect.gameObject.SetActive(false);
    }

    public void ScoreInit()
    {
        recycleScore = 10;
        if (playingCardQueue != null)
        {
            queueScore = 5;
            if(frontOrBack == PlayingCard_FrontOrBack.Back)
            {
                backScore = 5;
            }
        }
        if(residuePlayingCards != null)
        {
            residueScore = 5;
        }
    }

    public void SetFront(bool isAnim = false, bool _drag = true, bool _click = true)
    {
        int score = (int)(backScore * GameScenePanel.ScoreRate);
        if (score > 0)
        {
            backScore = 0;
            UIManager.Instance.GetUI<GameScenePanel>().AddScore(score);
        }
        lastGameSteps.frontOrBack = gameSteps.frontOrBack;
        frontOrBack =  PlayingCard_FrontOrBack.Front;
        gameSteps.frontOrBack = frontOrBack;
        if (isAnim)
        {
            isDrag = false;
            isClick = false;

            backImg.transform.localScale = Vector3.one;
            backImg.gameObject.SetActive(true);

            cardImg.transform.localScale = new Vector3(0,1,1);
            cardImg.gameObject.SetActive(true);

            DOTween.Sequence()
                .Append(backImg.transform.DOScaleX(0f, 0.1f))
                .Append(cardImg.transform.DOScaleX(1f, 0.1f))
                .AppendCallback(() =>
                {
                    isDrag = _drag;
                    isClick = _click;
                    backImg.gameObject.SetActive(false);
                })
                ;
        }
        else
        {
            backImg.transform.localScale = Vector3.one;
            backImg.gameObject.SetActive(false);

            cardImg.transform.localScale = Vector3.one;
            cardImg.gameObject.SetActive(true);

            isDrag = _drag;
            isClick = _click;
        }
    }
    public void SetBack(bool isAnim = false)
    {
        lastGameSteps.frontOrBack = gameSteps.frontOrBack;
        frontOrBack = PlayingCard_FrontOrBack.Back;
        gameSteps.frontOrBack = frontOrBack;
        isDrag = false;
        isClick = false;
        if (isAnim)
        {
            backImg.transform.localScale = new Vector3(0, 1, 1);
            backImg.gameObject.SetActive(true);

            cardImg.transform.localScale =Vector3.one;
            cardImg.gameObject.SetActive(true);

            DOTween.Sequence()
                .Append(cardImg.transform.DOScaleX(0f, 0.1f))
                .Append(backImg.transform.DOScaleX(1f, 0.1f))
                .AppendCallback(() =>
                {
                    cardImg.gameObject.SetActive(false);
                })
                ;
        }
        else
        {
            backImg.transform.localScale = Vector3.one;
            backImg.gameObject.SetActive(true);

            cardImg.transform.localScale = Vector3.one;
            cardImg.gameObject.SetActive(false);
        }
    }
   
    public void SetCanvasTop()
    {
        if(playingCardQueue != null)
        {
            playingCardQueue.SetCanvasTop();
        }
        if (playingCardRecycle != null)
        {
            playingCardRecycle.SetCanvasTop();
        }
        if(residuePlayingCards != null)
        {
            residuePlayingCards.SetCanvasTop();
        }
    }
    public void SetCanvasRecover()
    {
        if (playingCardQueue != null)
        {
            playingCardQueue.SetCanvasRecover();
        }
        if (playingCardRecycle != null)
        {
            playingCardRecycle.SetCanvasRecover();
        }
        if (residuePlayingCards != null)
        {
            residuePlayingCards.SetCanvasRecover();
        }
    }

    public void BackCorrectPos(bool isThisControl = true, Action _action = null)
    {
        if(isThisControl)
        {
            SetCanvasTop();
        }

        transform.DOLocalMove(correctPos, 0.2f)
            .OnComplete(() =>
            {
                _action?.Invoke();
                if (isThisControl)
                {
                    SetCanvasRecover();
                }
            })
            .SetTarget(this);
    }

    /// <summary>
    /// 立刻回到正确位置
    /// </summary>
    public void RightAwayBackCorrectPos()
    {
        this.DOKill();
        transform.localPosition = correctPos;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isDrag)
            return;

        if (isClick)
        {
            isClick = false;
            EndDragCallback = () =>
            {
                isDrag = false;
                DOTween.Sequence().AppendInterval(0.1f).AppendCallback(() =>
                {
                    isClick = true;
                    isDrag = true;
                    EndDragCallback = null;
                });
            };
        }

        EventManager.Instance.TriggerEvent(GameEvent.StopHintAnim);
        SetCanvasTop();
        if(playingCardQueue != null)
        {
            connectedPlayingCards = playingCardQueue.GetConnectedPlayingCards(this);
            foreach (var card in connectedPlayingCards)
            {
                card.RightAwayBackCorrectPos();
            }
        }
        else
        {
            connectedPlayingCards = new List<PlayingCard>();
            connectedPlayingCards.Add(this);
            RightAwayBackCorrectPos();
        }

        Vector3 inputPos = eventData.position;
        inputPos.z = zOffset;
        Vector3 touchWorldPos = Camera.main.ScreenToWorldPoint(inputPos);
        _dragStartOffset = touchWorldPos - transform.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDrag)
            return;

        Vector3 inputPos = eventData.position;
        inputPos.z = zOffset;
        Vector3 targetPos = Camera.main.ScreenToWorldPoint(inputPos) - _dragStartOffset;
        //float lerpSpeed = Input.touchCount > 0 ? 30f : 15f; // 触控/鼠标不同的插值速度
        //Vector3 newPos = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * lerpSpeed);
        Vector3 newPos = targetPos;
        if (newPos.sqrMagnitude > 0.01f)
        {
            Vector3 _lastMainPos = transform.position;
            transform.position = newPos;
            Vector3 positionOffset = newPos - _lastMainPos;
            foreach (var card in connectedPlayingCards)
            {
                if (card != this)
                {
                    card.transform.position += positionOffset;
                }
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDrag)
            return;

        EndDragCallback?.Invoke();
        SetCanvasRecover();

        if (connectedPlayingCards == null)
        {
            connectedPlayingCards = playingCardQueue.GetConnectedPlayingCards(this);
            Debug.LogError("不该报错的地方报错了");
        }
        bool isRecycleDrag = playingCardRecycle != null;
        bool isHaveCorrect = false; //是否有完成的

        PlayingCardControl playingCardControl = UIManager.Instance.GetUI<GameScenePanel>().playingCardControl;
        //列区域检测
        foreach (var temPlayingCardQueue in playingCardControl.playingCardQueues)
        {
            //排除自身所在的queue检测
            if (playingCardQueue != null && temPlayingCardQueue == playingCardQueue)
            {
                continue;
            }
            if (temPlayingCardQueue.CheckPlayingCardPosIsCorrect(this))
            {
                if (playingCardQueue != null)
                {
                    var curQueue = playingCardQueue;
                    playingCardQueue.RemovePlayingCard(connectedPlayingCards);
                    curQueue.ShowLastBackPlayingCard();
                }
                if (residuePlayingCards != null)
                {
                    residuePlayingCards.RemovePlayingCard(this);
                }

                temPlayingCardQueue.AddPlayingCards(connectedPlayingCards);
                isHaveCorrect = true;
                break;
            }
        }
        //回收区域检测
        if (connectedPlayingCards.Count == 1)
        {
            var playingCardRecycles = playingCardControl.playingCardRecycles;
            for (int i = 0; i < playingCardRecycles.Count; i++)
            {
                if (playingCardRecycles[i].CheckPlayingCardPosIsCorrect(this))
                {
                    if (playingCardQueue != null)
                    {
                        var curQueue = playingCardQueue;
                        playingCardQueue.RemovePlayingCard(connectedPlayingCards);
                        curQueue.ShowLastBackPlayingCard();
                    }
                    if (residuePlayingCards != null)
                    {
                        residuePlayingCards.RemovePlayingCard(this);
                    }
                    playingCardRecycles[i].AddPlayingCards(this);
                    isHaveCorrect = true;
                    break;
                }
            }
        }


        //复位动画  当是回收区时卡片或者没有完成的，跳过自动检测
        if (isHaveCorrect)
        {
            UIManager.Instance.GetUI<GameScenePanel>().RecordNormalOperation();
        }
        else
        {
            UIManager.Instance.GetUI<GameScenePanel>().RecordCardInteraction();
        }

        BackCorrectPosCoroutine(isRecycleDrag || !isHaveCorrect, isHaveCorrect); 
    }

    public bool TryRecycleHoming(bool isClick = false)
    {
        bool isRecycleDrag = playingCardRecycle != null;

        PlayingCardControl playingCardControl = UIManager.Instance.GetUI<GameScenePanel>().playingCardControl;
        foreach (var pR in playingCardControl.playingCardRecycles)
        {
            if (pR == playingCardRecycle)
            {
                continue;
            }

            if (pR.CheckRecycleRootsCondition(this))
            {
                if (playingCardQueue != null)
                {
                    List<PlayingCard> thiscards = playingCardQueue.GetConnectedPlayingCards(this);
                    if (thiscards.Count > 1)
                    {
                        return false;
                    }
                    var curQueue = playingCardQueue;
                    playingCardQueue.RemovePlayingCard(thiscards);
                    curQueue.ShowLastBackPlayingCard();
                }

                if (residuePlayingCards != null)
                {
                    residuePlayingCards.RemovePlayingCard(this);
                }
                if (isClick)
                {
                    PlayCorrectAnim();
                }
                pR.AddPlayingCards(this);
                BackCorrectPosCoroutine(isRecycleDrag, true);
                return true;
            }
        }
        return false;
    }

    public bool TryQueueHoming(bool isClick = false)
    {
        bool isRecycleDrag = playingCardRecycle != null;

        PlayingCardControl playingCardControl = UIManager.Instance.GetUI<GameScenePanel>().playingCardControl;
        foreach (var queue in playingCardControl.playingCardQueues)
        {
            if (queue == playingCardQueue)
            {
                continue;
            }

            //避免空位置来回检测
            if(!isClick && queue.Cells.Count == 0)
            {
                continue;
            }

            if (queue.CheckRecycleCellsCondition(this))
            {
                List<PlayingCard> thiscards = new List<PlayingCard>();
                if (playingCardQueue != null)
                {
                    thiscards = playingCardQueue.GetConnectedPlayingCards(this);
                    var curQueue = playingCardQueue;
                    playingCardQueue.RemovePlayingCard(thiscards);
                    curQueue.ShowLastBackPlayingCard();
                }
                else
                {
                    thiscards.Add(this);
                }

                if(residuePlayingCards != null)
                {
                    residuePlayingCards.RemovePlayingCard(this);
                }
                if (isClick)
                {
                    PlayCorrectAnim();
                }
                queue.AddPlayingCards(thiscards);
                BackCorrectPosCoroutine(isRecycleDrag, true);
                return true;
            }
        }

        return false;
    }

    public void PlayCorrectAnim()
    {
        correctAnim.transform.DOKill();
        correctAnim.Stop();
        correctAnim.Play();
        correctAnim.transform.SetParent(transform.parent.transform);
        correctAnim.transform.position = transform.position;
        DOTween.Sequence().AppendInterval(0.6F).AppendCallback(() =>
        {
            correctAnim.transform.SetParent(transform);
            correctAnim.transform.localPosition = Vector3.zero;
        })
        .SetTarget(correctAnim.transform)
        ;
    }

    /// <summary>
    /// 携程自动归位
    /// </summary>
    /// <param name="isRecycleDrag">是否是回收区域拖拽</param>
    public void BackCorrectPosCoroutine(bool isRecycleDrag = false, bool isHaveCorrect = false)
    {
        StartCoroutine(BackCorrectPosIE(isRecycleDrag, isHaveCorrect));
    }

    private IEnumerator BackCorrectPosIE(bool isRecycleDrag, bool isHaveCorrect = false)
    {
        UIManager.Instance.OpenUIMask();
        var temList = new List<PlayingCard>();
        if (playingCardQueue != null)
        {
            temList = playingCardQueue.GetConnectedPlayingCards(this);
        }
        else
        {
            temList.Add(this);
        }
        SetCanvasTop();
        for (int i = 0; i < temList.Count; i++)
        {
            temList[i].BackCorrectPos(false);
            yield return new WaitForSeconds(0.02f);
        }
        yield return new WaitForSeconds(0.2f);
        //积分
        CollectScore(isHaveCorrect);
        //--------
        SetCanvasRecover();
        UIManager.Instance.HideUIMask();
        GameStepRecord.Instance.RecordTheSteps();

        //其他奖励
        PlayingCardControl playingCardControl = UIManager.Instance.GetUI<GameScenePanel>().playingCardControl;
        playingCardControl.CheckQueueTaskReward(() =>
        {
            bool temisRecycleDrag = isRecycleDrag;
            bool isBoxReward = GameBox.Instance.CheckBoxProgress(GameManager.Instance.playerInfo.level);
            if (!isBoxReward)
            {
                if (!temisRecycleDrag)
                {
                    EventManager.Instance.TriggerEvent(GameEvent.TryAutoHoming);
                }
            }
        });
     
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isClick)
            return;

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        EventManager.Instance.TriggerEvent(GameEvent.StopHintAnim);
        UIManager.Instance.GetUI<GameScenePanel>().RecordCardInteraction();
        if (TryRecycleHoming(true))
        {
            UIManager.Instance.GetUI<GameScenePanel>().RecordNormalOperation();
            return;
        }
        if(TryQueueHoming(true))
        {
            UIManager.Instance.GetUI<GameScenePanel>().RecordNormalOperation();
            return;
        }
        UIManager.Instance.GetUI<GameScenePanel>().RecordInvalidCardClick();
        ShakeThis();
    }
    /// <summary>
    /// 收集积分
    /// </summary>
    /// <param name="isHaveCorrect"></param>
    public void CollectScore(bool isHaveCorrect)
    {
        if (isHaveCorrect)
        {
            AudioManager.Instance.SetAudioSource(audioSource, "PlayingCardCorrect");
            GameScenePanel gameScenePanel = UIManager.Instance.GetUI<GameScenePanel>();
            if (playingCardQueue != null)
            {
                if (queueScore > 0 || residueScore > 0)
                {
                    gameScenePanel.RecordScoreRate();
                    //Debug.Log(GetName() + "-倍率-" + GameScenePanel.ScoreRate);
                    int score = (int)(queueScore * GameScenePanel.ScoreRate) + (int)(residueScore * GameScenePanel.ScoreRate);
                    queueScore = 0;
                    residueScore = 0;
                    gameScenePanel.AddScore(score);
                    PlayScore(score);
                    if (UnityEngine.Random.value < 0.25f)
                    {
                        int rv = UnityEngine.Random.Range(1, 100);
                        float tV = rv / 100f;
                        Debug.Log("获得金币奖励：" + tV);
                        PlayGoldAnim(tV);
                    }
                    //Debug.Log(GetName() + "-队列-" + score);
                }
            }
            if (playingCardRecycle != null && recycleScore > 0)
            {
                gameScenePanel.RecordScoreRate();
                //Debug.Log(GetName() + "-倍率-" + GameScenePanel.ScoreRate);
                int score = (int)(recycleScore * GameScenePanel.ScoreRate);
                recycleScore = 0;
                gameScenePanel.AddScore(score);
                PlayScore(score);
                if (UnityEngine.Random.value < 0.25f)
                {
                    int rv = UnityEngine.Random.Range(1, 100);
                    float tV = rv / 100f;
                    Debug.Log("获得金币奖励：" + tV);
                    PlayGoldAnim(tV);
                }
                PlayScoreAnim();
                PlayRecycleAnim();
                //Debug.Log(GetName() + "-回收-" + score);
                EventManager.Instance.TriggerEvent(GameEvent.AddGameBox, 1);
                gameScenePanel.isEffectualSteps = true;
                if(gameScenePanel.isEffectualStepAwaitCallback != null)
                {
                    gameScenePanel.isEffectualStepAwaitCallback.Invoke();
                    gameScenePanel.isEffectualStepAwaitCallback = null;
                }
            }
        }
    }

    public void PlayScore(int _score)
    {
        ScoreText.transform.DOKill();
        ScoreText.text = $"+{_score}";
        ScoreText.gameObject.SetActive(true);

        ScoreText.transform.DOLocalMoveY(120f, 1f).SetEase(Ease.OutCubic);
        DOTween.Sequence()
            .AppendInterval(0.6f)
            .Append(ScoreTextcanvasGroup.DOFade(0f, 0.5f))
            .AppendCallback(() =>
            {
                ScoreText.gameObject.SetActive(false);
                ScoreText.transform.localPosition = new Vector3(0f, 50f, 0f);
                ScoreTextcanvasGroup.alpha = 1f;
            })
            .SetTarget(ScoreText.transform)
            ;

    }

    public void PlayGoldAnim(float _goldCnt)
    {
        goldAnimTrans.transform.DOKill();
        g1.transform.localPosition = Vector3.zero;
        g2.transform.localPosition = Vector3.zero;
        g3.transform.localPosition = Vector3.zero;
        g1.gameObject.SetActive(true);
        g2.gameObject.SetActive(true);
        g3.gameObject.SetActive(true);
        goldAnimTrans.gameObject.SetActive(true);

        PlayerInfoUI playerInfoUI = UIManager.Instance.GetUI<PlayerInfoUI>();

        GameManager.Instance.playerInfo.Add_gold(_goldCnt);
        DOTween.Sequence().AppendInterval(0.8f).AppendCallback(() =>
        {
            playerInfoUI.StartGoldAnim();
        })
        .SetTarget(goldAnimTrans.transform);
        DOTween.Sequence()
            .Append(g1.transform.DOLocalMove(new Vector3(-18f, 186f, 0f), 0.2f))
            .Append(g1.transform.DOMove(playerInfoUI.goldIcon.transform.position, 0.6f))
            .AppendCallback(() =>
            {
                g1.transform.gameObject.SetActive(false);
            })
            .SetTarget(goldAnimTrans.transform);
        DOTween.Sequence()
           .Append(g2.transform.DOLocalMove(new Vector3(-89f, -159f, 0f), 0.2f))
           .AppendInterval(0.1f)
           .Append(g2.transform.DOMove(playerInfoUI.goldIcon.transform.position, 0.6f))
           .AppendCallback(() =>
           {
               g2.transform.gameObject.SetActive(false);
           })
          .SetTarget(goldAnimTrans.transform);
        DOTween.Sequence()
           .Append(g3.transform.DOLocalMove(new Vector3(133f, -8f, 0f), 0.2f))
           .AppendInterval(0.2f)
           .Append(g3.transform.DOMove(playerInfoUI.goldIcon.transform.position, 0.6f))
           .AppendCallback(() =>
           {
               g3.transform.gameObject.SetActive(false);
               goldAnimTrans.gameObject.SetActive(false);
           })
          .SetTarget(goldAnimTrans.transform);
    }

    //首次回收动画
    public void PlayRecycleAnim()
    {
        RecycleAnimTrans.gameObject.SetActive(true);
        spadesAnim.gameObject.SetActive(type == PlayingCardType.spades);
        heartsAnim.gameObject.SetActive(type == PlayingCardType.hearts);
        clubsAnim.gameObject.SetActive(type == PlayingCardType.clubs);
        diamondsAnim.gameObject.SetActive(type == PlayingCardType.diamonds);

        DOTween.Sequence().AppendInterval(1f).AppendCallback(() =>
        {
            RecycleAnimTrans.gameObject.SetActive(false);
        })
        .SetTarget(RecycleAnimTrans.transform)
        ;
    }

    public void PlayScoreAnim()
    {
        tuoweieffect.DOKill();
        tuoweieffect.gameObject.SetActive(true);
        DOTween.Sequence().Append(tuoweieffect.transform.DOMove(GameBox.Instance.effectPoint.transform.position, 0.4f))
                          .AppendCallback(() =>
                          {
                              GameBox.Instance.PlayGetProgressAnim();
                          })
                          .AppendInterval(0.15f)
                          .AppendCallback(() =>
                          {
                              tuoweieffect.gameObject.SetActive(false);
                              tuoweieffect.transform.localPosition = Vector3.zero;
                          })
                          .SetTarget(tuoweieffect.transform)
                          ;
       
    }

    public bool CheckPosIsCorrect(PlayingCard _cell)
    {
        // 分别计算X、Y轴坐标差值绝对值
        float temX = Mathf.Abs(transform.position.x - _cell.transform.position.x);
        float temY = Mathf.Abs(transform.position.y - _cell.transform.position.y);
       
        //Debug.Log(transform.name);
        //Debug.Log(_cell.name);

        //Debug.Log($"{temX}/{diffX}");
        //Debug.Log($"{temY}/{diffY}");

        bool isUp = temX <= diffX && temY <= diffY;
        return isUp;
    }

    public void ShakeThis()
    {
        AudioManager.Instance.PlaySceneSingleMusic("PlayingCardShake");
        UIManager.Instance.OpenUIMask();
        if (playingCardQueue != null)
        {
            Transform thisParent = transform.parent;
            GameObject shakeParent = new GameObject("ShakeParent");
            shakeParent.transform.SetParent(transform.parent); // 设置父级
            shakeParent.transform.localPosition = transform.localPosition;
            shakeParent.transform.localScale = Vector3.one;
            Vector3 temPos = shakeParent.transform.localPosition;

            var temList = playingCardQueue.GetConnectedPlayingCards(this);
            for (int i = 0; i < temList.Count; i++)
            {
                temList[i].transform.SetParent(shakeParent.transform);
            }
     
            //让图片在X轴方向轻微抖动
            shakeParent.transform.DOShakePosition(
                duration: 0.3f,                    // 持续时间
                strength: new Vector3(8f, 0, 0),   // X轴强度8像素
                vibrato: 20,                       // 振动次数（频率控制核心）
                randomness: 0,                     // 随机度设为0，保证频率固定
                snapping: false,                   // 是否对齐像素
                fadeOut: false                      // 是否渐出（设为false可保持强度）
            )
            .OnComplete(() =>
            {
                shakeParent.transform.localPosition = temPos;
                for (int i = 0; i < temList.Count; i++)
                {
                    temList[i].transform.SetParent(thisParent);
                }

                Destroy(shakeParent);
                UIManager.Instance.HideUIMask();
            })
            ;
        }
        else
        {
            Vector3 temPos = transform.localPosition;
            //让图片在X轴方向轻微抖动
            transform.DOShakePosition(
                duration: 0.3f,                    // 持续时间
                strength: new Vector3(8f, 0, 0),   // X轴强度8像素
                vibrato: 20,                       // 振动次数（频率控制核心）
                randomness: 0,                     // 随机度设为0，保证频率固定
                snapping: false,                   // 是否对齐像素
                fadeOut: false                      // 是否渐出（设为false可保持强度）
            )
            .OnComplete(() =>
            {
                transform.localPosition = temPos;
                UIManager.Instance.HideUIMask();
            })
            ;
        }
    }

    public string GetName()
    {
        string typeName = "";

        switch (type)
        {
            case PlayingCardType.spades:
                typeName = "黑桃";
                break;
            case PlayingCardType.hearts:
                typeName = "红桃";
                break;
            case PlayingCardType.clubs:
                typeName = "梅花";
                break;
            case PlayingCardType.diamonds:
                typeName = "方块";
                break;
            default:
                typeName = "未知类型";
                break;
        }

        return typeName + PlayingCardControl.GetName(order);
    }

    public void ShowHintAnim()
    {
        StopHintAnim();
        hintAnimMask.gameObject.SetActive(true);
        hintAnimSequence = DOTween.Sequence()
            .Append(hintAnimImg.DOFade(0.7f, 0.5f))
            .Append(hintAnimImg.DOFade(0.1f, 0.5f))
            .SetLoops(-1);
    }

    public void StopHintAnim()
    {
        if (hintAnimSequence == null) return;
        hintAnimSequence.Kill();
        hintAnimSequence = null;

        hintAnimMask.gameObject.SetActive(false);
        Color color = Color.red;
        color.a = 0.1f;
        hintAnimImg.color = color;
    }
}
