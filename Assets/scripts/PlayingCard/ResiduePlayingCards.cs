using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ResiduePlayingCards : MonoBehaviour
{
    public Canvas canvas;
    public Transform point1;
    public Transform point2;
    public Transform point3;
    public Transform cells;
    public Button btn;
    public Text cntText;

    public List<PlayingCard> leftCards;
    public List<PlayingCard> rightCards;

    private Coroutine coroutine;
    private void Start()
    {
        btn.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlaySceneSingleMusic("ResiduePlayingCardClick");
            UIManager.Instance.GetUI<GameScenePanel>().RecordNormalOperation();
            EventManager.Instance.TriggerEvent(GameEvent.StopHintAnim);
            SwitchCard();
        });
    }
    public void Init(List<PlayingCard> _ResiduePlayingCards)
    {
        foreach (var card in _ResiduePlayingCards)
        {
            card.residuePlayingCards = this;
        }
        rightCards = _ResiduePlayingCards.ToList();
        leftCards = new List<PlayingCard>();

        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        ResetRightCardsPos();
        UpdateUI();
    }

    /// <summary>
    /// 左边所有的移动到右边
    /// </summary>
    public void AllL_to_R()
    {
        foreach (var card in leftCards)
        {
           rightCards.Add(card);
        }
        leftCards.Clear();

        ResetRightCardsPos();
        //重置last  只能是false
        foreach (var card in rightCards)
        {
            card.lastGameSteps = new GameStep(card.gameSteps);
        }
    }

    public void RemovePlayingCard(PlayingCard _cell)
    {
        _cell.residuePlayingCards = null;
        if (leftCards.Contains(_cell))
        {
            leftCards.Remove(_cell);
            SwitchAnim();
            _cell.SetFront(_cell.frontOrBack == PlayingCard_FrontOrBack.Back, true, true);
        }
        else if (rightCards.Contains(_cell))
        {
            rightCards.Remove(_cell);
        }
    }

    IEnumerator ResetRightCardsPosAnimIE()
    {
        btn.interactable = false;
        for (int i = rightCards.Count - 1; i >= 0; i--)
        {
            int index = i;
            rightCards[index].transform.SetParent(cells);
            rightCards[index].SetBack(true);
            rightCards[index].transform.DOLocalMove(Vector3.zero, 0.2f);

            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.2f);
        //反转
        rightCards.Reverse();
        for (int i = 0; i < rightCards.Count; i++)
        {
            rightCards[i].transform.SetSiblingIndex(i);

            rightCards[i].gameSteps.gameAreaType = GameAreaType.Residue_R;
            rightCards[i].gameSteps.row = 0;
            rightCards[i].gameSteps.col = i;
            rightCards[i].correctPos = Vector3.zero;
        }

        GameStepRecord.Instance.RecordTheSteps();      //更新步骤
        btn.interactable = true;
    }

    private void ResetRightCardsPos(bool isAnim = false)
    {
        if (isAnim)
        {
            coroutine = StartCoroutine(ResetRightCardsPosAnimIE());
        }
        else
        {
            for (int i = 0; i < rightCards.Count; i++)
            {
                rightCards[i].SetBack(false);
                rightCards[i].transform.SetParent(cells);
                rightCards[i].transform.SetSiblingIndex(i);
                rightCards[i].transform.localPosition = Vector3.zero;
                rightCards[i].correctPos = Vector3.zero;

                rightCards[i].gameSteps.gameAreaType = GameAreaType.Residue_R;
                rightCards[i].gameSteps.row = 0;
                rightCards[i].gameSteps.col = i;
            }
        }
    }

    private void SwitchCard()
    {
        if (rightCards.Count == 0)
        {
            foreach (var card in leftCards)
            {
                rightCards.Add(card);
            }
            leftCards.Clear();
            ResetRightCardsPos(true);
        }
        else
        {
            var card = rightCards[rightCards.Count - 1];
            rightCards.RemoveAt(rightCards.Count - 1);
            leftCards.Add(card);
            SwitchAnim();
            GameStepRecord.Instance.RecordTheSteps();      //更新步骤
            GameScenePanel gameScenePanel = UIManager.Instance.GetUI<GameScenePanel>();
            gameScenePanel.ReportLevelStep("draw");
        }
        UpdateUI();
    }

    public void UpdateUI()
    {
        cntText.text = rightCards.Count.ToString();
    }

    private void SwitchAnim()
    {
        btn.interactable = false;
        int index = 0;
        for (int i = leftCards.Count - 1; i >= 0; i--)
        {
            int temi = i;
            PlayingCard card = leftCards[temi];
            card.correctPos = Vector3.zero;
            switch (index)
            {
                case 0:
                    card.SetFront(card.frontOrBack == PlayingCard_FrontOrBack.Back, true, true);
                    card.transform.SetParent(point1);

                    card.gameSteps.gameAreaType = GameAreaType.Residue_L;
                    card.gameSteps.row = 0;
                    card.gameSteps.col = 0;
                    break;
                case 1:
                    card.SetFront(false, false, false);
                    card.transform.SetParent(point2);

                    card.gameSteps.gameAreaType = GameAreaType.Residue_L;
                    card.gameSteps.row = 1;
                    card.gameSteps.col = 0;
                    break;
                default:
                    card.SetFront(false, false, false);
                    card.transform.SetParent(point3);
                    card.transform.SetSiblingIndex(temi);

                  card.gameSteps.gameAreaType = GameAreaType.Residue_L;
                    card.gameSteps.row = 2;
                    card.gameSteps.col = card.transform.GetSiblingIndex();
                    break;
            }
            card.BackCorrectPos();
            index++;
        }

        DOTween.Sequence()
               .AppendInterval(0.2f)
               .AppendCallback(() =>
               {
                   btn.interactable = true;

                   foreach (var card in leftCards)
                   {
                       card.correctPos = Vector3.zero;
                       card.RightAwayBackCorrectPos();
                   }
                   foreach (var card in rightCards)
                   {
                       card.correctPos = Vector3.zero;
                       card.RightAwayBackCorrectPos();
                   }
               });
    }


    public void SetCanvasTop()
    {
        canvas.sortingOrder = 150;
    }
    public void SetCanvasRecover()
    {
        canvas.sortingOrder = 120;
    }
    /// <summary>
    /// 步骤专用
    /// </summary>
    public void StepR_to_L(PlayingCard playingCard, int col)
    {
        leftCards.Remove(playingCard);

        if(!rightCards.Contains(playingCard))
        {
            if (col >= 0 && col <= rightCards.Count)
            {
                rightCards.Insert(col, playingCard);
            }
            else
            {
                rightCards.Add(playingCard);
            }
        }
        UpdateUI();
        playingCard.transform.SetParent(cells);

        playingCard.residuePlayingCards = this;
        playingCard.correctPos = Vector3.zero;
        playingCard.gameSteps.gameAreaType = GameAreaType.Residue_R;
        playingCard.gameSteps.row = 0;
        playingCard.gameSteps.col = rightCards.IndexOf(playingCard);
        playingCard.lastGameSteps = new GameStep(playingCard.gameSteps);
        playingCard.BackCorrectPos();
       
        SwitchAnim();
        foreach (var leftCard in leftCards)
        {
            leftCard.lastGameSteps = new GameStep(leftCard.gameSteps);      
        }

    }

    public void StepL_to_R(List<GameStep> steps)
    {
        foreach (var step in steps)
        {
            rightCards.Remove(step.playingCard);
            if(!leftCards.Contains(step.playingCard))
            {
                if(step.playingCard.playingCardQueue != null)
                {
                    List<PlayingCard> cells = new List<PlayingCard>();
                    cells.Add(step.playingCard);
                    //当前队列移除
                    step.playingCard.playingCardQueue.RemovePlayingCard(cells);
                }
                leftCards.Add(step.playingCard);
            }
        }
        UpdateUI();
        btn.interactable = false;
        int index = 0;
        for (int i = leftCards.Count - 1; i >= 0; i--)
        {
            PlayingCard card = leftCards[i];
            card.correctPos = Vector3.zero;
            card.residuePlayingCards = this;
            switch (index)
            {
                case 0:
                    card.SetFront(card.frontOrBack == PlayingCard_FrontOrBack.Back, true, true);
                    card.transform.SetParent(point1);

                    break;
                case 1:
                    card.SetFront(false, false, false);
                    card.transform.SetParent(point2);

                    break;
                default:
                    card.SetFront(false, false, false);
                    card.transform.SetParent(point3);
                 
                    break;
            }
            card.BackCorrectPos();
            index++;
        }

        DOTween.Sequence()
               .AppendInterval(0.2f)
               .AppendCallback(() =>
               {
                   btn.interactable = true;

                   for (int i = 0; i < leftCards.Count; i++)
                   {
                       leftCards[i].correctPos = Vector3.zero;
                       leftCards[i].residuePlayingCards = this;
                       leftCards[i].transform.SetSiblingIndex(i);
                   }

                   int last1 = leftCards.Count - 1;
                   int last2 = leftCards.Count - 2;
                   for (int i = 0; i < leftCards.Count; i++)
                   {
                       leftCards[i].gameSteps.gameAreaType = GameAreaType.Residue_L;
                       if (i == last1)
                       {
                           leftCards[i].gameSteps.row = 0;
                       }
                       else if (i == last2)
                       {
                           leftCards[i].gameSteps.row = 1;
                       }
                       else
                       {
                           leftCards[i].gameSteps.row = 2;
                       }

                       leftCards[i].residuePlayingCards = this;
                       leftCards[i].gameSteps.col = leftCards[i].transform.GetSiblingIndex();
                       leftCards[i].lastGameSteps = new GameStep(leftCards[i].gameSteps);
                       leftCards[i].correctPos = Vector3.zero;
                       leftCards[i].RightAwayBackCorrectPos();
                   }
               });

      
    }

    public void StepAddR(PlayingCard card, int row, int col)
    {
        if (!rightCards.Contains(card))
        {
            if (col >= 0 && col <= rightCards.Count)
            {
                rightCards.Insert(col, card);
            }
            else
            {
                rightCards.Add(card);
            }
        }
        UpdateUI();
        card.transform.SetParent(cells);
        card.transform.SetSiblingIndex(col);

        card.residuePlayingCards = this;
        card.correctPos = Vector3.zero;
        card.gameSteps.gameAreaType = GameAreaType.Residue_R;
        card.gameSteps.row = 0;
        card.gameSteps.col = rightCards.IndexOf(card);
        card.lastGameSteps = new GameStep(card.gameSteps);
        card.BackCorrectPos(true);

        foreach (var leftCard in rightCards)
        {
            leftCard.lastGameSteps = new GameStep(leftCard.gameSteps);
        }
    }

    public void StepAddL(PlayingCard card, int row, int col)
    {
        card.residuePlayingCards = this;
        
        if(!leftCards.Contains(card))
        {
            if (col >= 0 && col <= leftCards.Count && row == 2)
            {
                leftCards.Insert(col, card);
            }
            else
            {
                leftCards.Add(card);
            }
        }

        SwitchAnim();
        foreach (var leftCard in leftCards)
        {
            leftCard.lastGameSteps = new GameStep(leftCard.gameSteps);
        }
    }
}
