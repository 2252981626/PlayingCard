using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameScenePanel : UIBase
{
    public static bool isPause = true;
    public PlayingCardControl playingCardControl;
    public Transform root;
    public Transform fapaiAnimTrans;

    public Text level;
    public Text scoreText;
    public Text moveText;
    public Text secondText;
    public Text lianxuCntText;


    public GameSceneItem_Hint gameSceneItem_Hint;
    public GameSceneItem_Extract gameSceneItem_Extract;
    public GameSceneItem_Exchange gameSceneItem_Exchange;
    public GameSceneItem_Return gameSceneItem_Return;

    public int score;
    public int move;
    public int second = 0;
    public float levelStartTime;
    private int levelId;
    private bool settlementShown;
    private float settlementShowTime;
    private bool levelExitReported;
    private bool backgroundTracked;
    private int backgroundLevelId;
    private int backgroundLastBoxIndex;
    private bool backgroundIsSettled;
    private int backgroundLevelElapsedMs;
    private int invalidCardClickCount;
    private int levelStepCount;
    public bool isEffectualSteps;
    public Action isEffectualStepAwaitCallback;
    private bool extractGuideShown;
    private Sequence extractGuideIdleSequence;
    private float timer;
    //--------
    public static float ScoreRate = 1;
    private int lianxuCnt;
    private Sequence scoreRateSequence;
    private void Awake()
    {
        RectTransform rect = root.GetComponent<RectTransform>();
        float topBlockHeight = Screen.height - Screen.safeArea.yMax;
        rect.offsetMax = new Vector2(0, -topBlockHeight);
    }
    private void Update()
    {
        if (isPause)
            return;

        timer += Time.deltaTime;
        if (timer > 1f)
        {
            timer = 0f;
            second++;
            UpdateSecondUI();
        }
    }

    public void RecordScoreRate()
    {
        lianxuCnt++;
        if (scoreRateSequence != null)
        {
            scoreRateSequence.Kill();
            if(lianxuCnt == 2 || lianxuCnt == 3)
            {
                ScoreRate = 1.2f;
            }
            else if (lianxuCnt == 4 || lianxuCnt == 5)
            {
                ScoreRate = 1.6f;
            }
            else if(lianxuCnt > 5)
            {
                ScoreRate = 2.2f;
            }
            else
            {
                ScoreRate = 1;
            }

            if (lianxuCnt > 1)
            {
                lianxuCntText.gameObject.SetActive(true);
                lianxuCntText.text = "x" + lianxuCnt.ToString();

                lianxuCntText.transform.DOKill();
                lianxuCntText.transform.localScale = Vector3.one;
                DOTween.Sequence()
                    .Append(lianxuCntText.transform.DOScale(1.1f, 0.1f))
                    .Append(lianxuCntText.transform.DOScale(0.9f, 0.1f))
                    .Append(lianxuCntText.transform.DOScale(1f, 0.1f))
                    .SetTarget(lianxuCntText.transform);
            }
        }
         scoreRateSequence = DOTween.Sequence().AppendInterval(2f).AppendCallback(() =>
         {
             lianxuCnt = 0;
             ScoreRate = 1;
             lianxuCntText.gameObject.SetActive(false);
         });
    }

   
    public void RecordCardInteraction()
    {
        RestartExtractGuideIdleTimer();
    }

    public void RecordInvalidCardClick()
    {
        RestartExtractGuideIdleTimer();
        invalidCardClickCount++;
        if (invalidCardClickCount >= 10)
        {
            ShowExtractGuide();
        }
    }

    public void RecordNormalOperation()
    {
        invalidCardClickCount = 0;
        extractGuideShown = false;
        gameSceneItem_Extract.HideGuide();
        RestartExtractGuideIdleTimer();
    }

    public void OnPopupOpened()
    {
        extractGuideIdleSequence?.Kill();
    }

    public void OnPopupClosed()
    {
        if (isOpen)
        {
            RestartExtractGuideIdleTimer();
        }
    }

    private void RestartExtractGuideIdleTimer()
    {
        extractGuideIdleSequence?.Kill();
        extractGuideIdleSequence = DOTween.Sequence().AppendInterval(10f).AppendCallback(ShowExtractGuide).SetTarget(transform);
    }

    private void ShowExtractGuide()
    {
        extractGuideShown = true;
        gameSceneItem_Extract.ShowGuide();
    }

    private void ResetExtractGuide()
    {
        invalidCardClickCount = 0;
        extractGuideShown = false;
        extractGuideIdleSequence?.Kill();
        gameSceneItem_Extract.HideGuide();
    }


    private void OnApplicationPause(bool pauseStatus)
    {
        if (!isActiveAndEnabled || levelExitReported)
            return;

        if (pauseStatus)
        {
            backgroundLevelId = levelId;
            backgroundLastBoxIndex = GameBox.Instance.LastBoxIndex;
            backgroundIsSettled = settlementShown;
            backgroundLevelElapsedMs = Mathf.RoundToInt((Time.realtimeSinceStartup - levelStartTime) * 1000f);
            OtherSdkManager.Instance.CustomEvent("level_state_change", "level_id", backgroundLevelId, "state_reason", "background", "last_box_index", backgroundLastBoxIndex, "is_settled", backgroundIsSettled, "level_elapsed_ms", backgroundLevelElapsedMs);
            backgroundTracked = true;
        }
        else if (backgroundTracked)
        {
            int levelElapsedMs = Mathf.RoundToInt((Time.realtimeSinceStartup - levelStartTime) * 1000f);
            OtherSdkManager.Instance.CustomEvent("level_state_change", "level_id", backgroundLevelId, "state_reason", "resume", "last_box_index", backgroundLastBoxIndex, "is_settled", backgroundIsSettled, "level_elapsed_ms", levelElapsedMs);
            backgroundTracked = false;
        }
    }


    public void MarkSettlementShown()
    {
        settlementShown = true;
        settlementShowTime = Time.realtimeSinceStartup;
    }

    public void TrackLevelExit(string exitReason)
    {
        if (levelExitReported)
            return;

        levelExitReported = true;
        int currentLevelId = levelId;
        int lastBoxIndex = GameBox.Instance.LastBoxIndex;
        bool hasLastBox = lastBoxIndex > 0;
        bool hasSettlement = settlementShown;
        bool isSettled = hasSettlement;
        if (hasLastBox && hasSettlement)
        {
            OtherSdkManager.Instance.CustomEvent("level_exit", "level_id", currentLevelId, "exit_reason", exitReason, "is_settled", isSettled, "last_box_index", lastBoxIndex, "since_last_box_ms", GameBox.Instance.GetSinceLastBoxMs(), "settle_to_exit_ms", Mathf.RoundToInt((Time.realtimeSinceStartup - settlementShowTime) * 1000f));
        }
        else if (hasLastBox)
        {
            OtherSdkManager.Instance.CustomEvent("level_exit", "level_id", currentLevelId, "exit_reason", exitReason, "is_settled", isSettled, "last_box_index", lastBoxIndex, "since_last_box_ms", GameBox.Instance.GetSinceLastBoxMs());
        }
        else if (hasSettlement)
        {
            OtherSdkManager.Instance.CustomEvent("level_exit", "level_id", currentLevelId, "exit_reason", exitReason, "is_settled", isSettled, "last_box_index", lastBoxIndex, "settle_to_exit_ms", Mathf.RoundToInt((Time.realtimeSinceStartup - settlementShowTime) * 1000f));
        }
        else
        {
            OtherSdkManager.Instance.CustomEvent("level_exit", "level_id", currentLevelId, "exit_reason", exitReason, "is_settled", isSettled, "last_box_index", lastBoxIndex);
        }
    }


    public void ResetGame()
    {
        TrackLevelExit("settle_next_level");
        Refresh(); 
    }

    public override void Refresh(object data = null)
    {
        base.Refresh(data);
        levelStartTime = Time.realtimeSinceStartup;
        levelId = GameManager.Instance.playerInfo.level;
        settlementShown = false;
        settlementShowTime = 0f;
        levelExitReported = false;
        backgroundTracked = false;
        levelStepCount = 0;
        isEffectualSteps = false;
        isEffectualStepAwaitCallback = null;
        ResetExtractGuide();

        playingCardControl.Init();

        gameSceneItem_Hint.Refresh();
        gameSceneItem_Extract.Refresh();
        gameSceneItem_Exchange.Refresh();
        gameSceneItem_Return.Refresh();

        level.text = $"Lv.{GameManager.Instance.playerInfo.level}";
        level.gameObject.SetActive(GameManager.Instance.gameType == GameType.MainGame);

        lianxuCntText.gameObject.SetActive(false);
        score = 0;
        move = 0;
        second = 0;
        timer = 0f;
        isPause = false;

        lianxuCnt = 0;
        ScoreRate = 1;

        UpdateSecondUI();
        UpdateScoreUI();
        UpdateMoveUI();

        GameBox.Instance.Init();
        OtherSdkManager.Instance.CustomEvent("level_start", "level_id", GameManager.Instance.playerInfo.level);
        StartCoroutine(FaPaiAnimIE());
    }

    public IEnumerator FaPaiAnimIE()
    {
        playingCardControl.residuePlayingCards.cntText.text = "0";
        isPause = true;
        UIManager.Instance.OpenUIMask();
        fapaiAnimTrans.gameObject.SetActive(true);
        foreach (var card in playingCardControl.GetAllPlayingCard())
        {
            card.transform.position = fapaiAnimTrans.transform.position;
        }

        List<PlayingCard> queueShengxia = new List<PlayingCard>();
        for (int row = 0; row < 7; row++)
        {
            for (int col = 0; col < playingCardControl.playingCardQueues.Count; col++)
            {
                var queue = playingCardControl.playingCardQueues[col];
                if(row < queue.Cells.Count)
                {
                    if(queue.Cells[row].frontOrBack == PlayingCard_FrontOrBack.Back)
                    {
                        AudioManager.Instance.SetAudioSource(queue.Cells[row].audioSource, "fapai");
                        queue.Cells[row].BackCorrectPos();
                        yield return new WaitForSeconds(0.1f);
                    }
                    else
                    {
                        queueShengxia.Add(queue.Cells[row]);
                    }

                }
            }
        }
        foreach (var cell in queueShengxia)
        {
            AudioManager.Instance.SetAudioSource(cell.audioSource, "fapai");
            cell.BackCorrectPos();
            yield return new WaitForSeconds(0.1f);
        }

        fapaiAnimTrans.gameObject.SetActive(false);
        AudioManager.Instance.SetAudioSource(playingCardControl.residuePlayingCards.rightCards[0].audioSource, "fapai");
        foreach (var cell in playingCardControl.residuePlayingCards.rightCards)
        {
            cell.BackCorrectPos();
        }
        yield return new WaitForSeconds(0.2f);
        playingCardControl.residuePlayingCards.UpdateUI();
        UIManager.Instance.HideUIMask();
        isPause = false;
        RestartExtractGuideIdleTimer();

        string str = PlayerPrefs.GetString("GameTipsPanel", "");
        if(string.IsNullOrEmpty(str))
        {
            PlayerPrefs.SetString("GameTipsPanel", "yes");
            isPause = true;
            UIManager.Instance.OpenUI<GameTipsPanel>(null, () =>
            {
                OtherSdkManager.Instance.CustomEvent("newbie_guide_5_complete", "step", "5");

                isPause = false;
                if (TxElementMananger.Instance != null && GameManager.Instance.playerInfo.level <= 50)
                {
                    isPause = true;
                    UIManager.Instance.OpenUI<TxElementGameStartPanel>(null, () =>
                    {
                        isPause = false;
                        GameManager.Instance.EvaluationGameCallback?.Invoke();
                        GameManager.Instance.EvaluationGameCallback = null;
                    });
                }
                else
                {
                    GameManager.Instance.EvaluationGameCallback?.Invoke();
                    GameManager.Instance.EvaluationGameCallback = null;
                }
            });
        }
        else
        {
            if (TxElementMananger.Instance != null && GameManager.Instance.playerInfo.level <= 50)
            {
                isPause = true;
                UIManager.Instance.OpenUI<TxElementGameStartPanel>(null, () =>
                {
                    isPause = false;
                    GameManager.Instance.EvaluationGameCallback?.Invoke();
                    GameManager.Instance.EvaluationGameCallback = null;

                    GuideTxBtn();
                });
            }
            else
            {
                GameManager.Instance.EvaluationGameCallback?.Invoke();
                GameManager.Instance.EvaluationGameCallback = null;

                GuideTxBtn();
            }
        }
    }

    private void GuideTxBtn()
    {
        if (GameManager.Instance.playerInfo.level == 5)
        {
            if (TxElementMananger.Instance != null)
            {
                string firstGame = PlayerPrefs.GetString("Guide_TxBtn", "");
                if (string.IsNullOrEmpty(firstGame))
                {
                    UIManager.Instance.OpenUI<GuidePanel_TxBtn>();
                }
            }
        }
    }

    private void UpdateSecondUI()
    {
        secondText.text = GameManager.Instance.GetTimeString(second);
    }

    public override void Hide()
    {
        base.Hide();
        isPause = true;
    }

    private void UpdateScoreUI()
    {
        scoreText.text = score.ToString();
    }
    public void MinusScore(int _m)
    {
        score -= _m;
        score = Mathf.Max(score, 0);
        UpdateScoreUI();
    }
    public void AddScore(int _add)
    {
        score += _add;
        UpdateScoreUI();
    }

    private void UpdateMoveUI()
    {
        moveText.text = move.ToString();
    }

    public void AddMove(int _add)
    {
        move += _add;
        UpdateMoveUI();
    }

    public void ReportLevelStep(string stepType)
    {
        levelStepCount++;
        isEffectualSteps = false;
        int recycledCards = 0;
        foreach (var recycle in playingCardControl.playingCardRecycles)
        {
            recycledCards += recycle.recycleRoot.childCount;
        }

        int hiddenCardsLeft = playingCardControl.GetAllPlayingCard().Count - recycledCards;
        int levelElapsedMs = Mathf.RoundToInt((Time.realtimeSinceStartup - levelStartTime) * 1000f);
        if(GameManager.Instance.gameType == GameType.DailyGame)
        {
            int curday = UIManager.Instance.GetUI<LobbyScenePanel>().dailyChallengePanel.curSelectDay;
            Debug.Log($"[LevelStep] level_daily :{curday}, step_count:{levelStepCount}, step_type:{stepType}, level_elapsed_ms:{levelElapsedMs}, hidden_cards_left:{hiddenCardsLeft}");
            OtherSdkManager.Instance.CustomEvent("level_step", "level_daily", curday, "step_count", levelStepCount, "step_type", stepType, "level_elapsed_ms", levelElapsedMs, "hidden_cards_left", hiddenCardsLeft);
        }
        else
        {
            Debug.Log($"[LevelStep] level_id:{levelId}, step_count:{levelStepCount}, step_type:{stepType}, level_elapsed_ms:{levelElapsedMs}, hidden_cards_left:{hiddenCardsLeft}");
            OtherSdkManager.Instance.CustomEvent("level_step", "level_id", levelId, "step_count", levelStepCount, "step_type", stepType, "level_elapsed_ms", levelElapsedMs, "hidden_cards_left", hiddenCardsLeft);
        }
    }

    public void AddisEffectualStepAwaitCallback_Item2()
    {
        isEffectualStepAwaitCallback = () =>
        {
            ReportLevelStep("item_2");
        };
    }
}
