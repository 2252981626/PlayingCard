
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum GameAreaType
{
    Queue,
    Recycle,
    Residue_R,
    Residue_L,
}
[Serializable]
public class GameStep
{
    public PlayingCard playingCard;
    public int row;
    public int col;
    public GameAreaType gameAreaType;
    public PlayingCard_FrontOrBack frontOrBack;

    public GameStep(GameStep _steps)
    {
        row = _steps.row;
        col = _steps.col;
        gameAreaType = _steps.gameAreaType;
        frontOrBack = _steps.frontOrBack;
        playingCard = _steps.playingCard;
    }

    public bool IsSame(GameStep _steps)
    {
        if (row == _steps.row && col == _steps.col && gameAreaType == _steps.gameAreaType && frontOrBack == _steps.frontOrBack)
            return true;

        return false;
    }
}
public class GameStepRecord :MonoBehaviour
{
    public static GameStepRecord Instance;

    public List<List<GameStep>> steps;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        steps = new List<List<GameStep>>();
    }

    public void ResetSteps()
    {
        steps.Clear();
    }

    public bool  ReturnSteps()
    {
        if (steps.Count == 0)
        {
            Debug.Log("没有步骤");
            return false;
        }

        StartCoroutine(ReturnStepsIE());
        return true;
    }

    private IEnumerator ReturnStepsIE()
    {
        UIManager.Instance.OpenUIMask();
        int index = steps.Count - 1;
        List<GameStep> curStep = steps[index];
        steps.RemoveAt(index);
        EventManager.Instance.TriggerEvent(GameEvent.Update_ItemReturnInfo);

        bool isotherBool = true;
        foreach (var cardStep in curStep)
        {
            if (cardStep.playingCard.playingCardQueue != null)
            {
                isotherBool = false;
                break;
            }
            if (cardStep.playingCard.playingCardRecycle != null)
            {
                isotherBool = false;
                break;
            }
            if (cardStep.gameAreaType != GameAreaType.Residue_L)
            {
                isotherBool = false;
                break;
            }
        }

        if (isotherBool)
        {
            // 第一步：拿出 row == 2
            var filteredList = curStep.Where(x => x.row == 2).ToList();
            Debug.Log($"第一步：过滤 row==2，共 {filteredList.Count} 个");
            Debug.Log(string.Join(", ", filteredList.Select(x => $"{x.playingCard.name}")));

            // 第二步：按 col 排序
            filteredList = filteredList.OrderBy(x => x.col).ToList();
            //filteredList = filteredList.OrderByDescending(x => x.col).ToList();
            Debug.Log($"第二步：按 col 排序后");
            Debug.Log(string.Join(", ", filteredList.Select(x => $"{x.playingCard.name}")));

            // 第三步：把 row == 1 的放在后面
            var row1List = curStep.Where(x => x.row == 1).ToList();
            Debug.Log($"第三步：拿出 row==1，共 {row1List.Count} 个");
            Debug.Log(string.Join(", ", row1List.Select(x => $"{x.playingCard.name}")));
            filteredList.AddRange(row1List);

            // 第四步：把 row == 0 的放在后面
            var row2List = curStep.Where(x => x.row == 0).ToList();
            Debug.Log($"第四步：拿出 row==0，共 {row2List.Count} 个");
            Debug.Log(string.Join(", ", row2List.Select(x => $"{x.playingCard.name}")));
            filteredList.AddRange(row2List);

            PlayingCardControl playingCardControl = UIManager.Instance.GetUI<GameScenePanel>().playingCardControl;
            ResiduePlayingCards residuePlayingCards = playingCardControl.residuePlayingCards;
            residuePlayingCards.StepL_to_R(filteredList);

        }
        else
        {
            foreach (var cardStep in curStep)
            {
                RecoverStep(cardStep);
            }
        }



        yield return new WaitForSeconds(0.2f);
        UIManager.Instance.HideUIMask();
    }

    private void RecoverStep(GameStep step)
    {
        Debug.Log(step.playingCard.name + "   " + step.gameAreaType + "  --" +step.col);
        PlayingCardControl playingCardControl = UIManager.Instance.GetUI<GameScenePanel>().playingCardControl;
        if (step.gameAreaType == GameAreaType.Queue)
        {
            PlayingCardQueue queue = playingCardControl.playingCardQueues[step.row];
            //队列->回收
            if (step.playingCard.playingCardQueue == null)
            {
                //回收不管
                //队列增加
                queue.AddPlayingCard(step.frontOrBack, step.playingCard, step.col);
                Debug.Log("队列->回收  ");
            }
            else
            {
                Debug.Log("队列->另一队列");
                //队列->另一队列
                if (queue != step.playingCard.playingCardQueue)
                {
                    List<PlayingCard> cells = new List<PlayingCard>();
                    cells.Add(step.playingCard);
                    //当前队列移除
                    step.playingCard.playingCardQueue.RemovePlayingCard(cells);
                    //步骤队列增加
                    queue.AddPlayingCard(step.frontOrBack, step.playingCard, step.col);

                }
             
            }
        }
        else if (step.gameAreaType == GameAreaType.Recycle)
        {
            PlayingCardRecycle recycle = playingCardControl.playingCardRecycles[step.row];
            //回收->队列
            if (step.playingCard.playingCardRecycle == null)
            {
                Debug.Log("回收->队列");

                List<PlayingCard> cells = new List<PlayingCard>();
                cells.Add(step.playingCard);
                //当前队列移除
                step.playingCard.playingCardQueue.RemovePlayingCard(cells);
                //步骤回收增加
                recycle.AddPlayingCards(step.playingCard);

                step.playingCard.BackCorrectPos();
            }
            else
            {
                Debug.Log("回收->回收");
                //回收->回收
                //步骤回收增加
                recycle.AddPlayingCards(step.playingCard);
                step.playingCard.BackCorrectPos();
            }
        }
        else if (step.gameAreaType == GameAreaType.Residue_R)
        {
            ResiduePlayingCards residuePlayingCards = playingCardControl.residuePlayingCards;
            if (step.playingCard.playingCardRecycle != null)
            {
                step.playingCard.playingCardRecycle = null;
                Debug.Log("//右->回收");
                //右->回收
                residuePlayingCards.StepAddR(step.playingCard, step.row, step.col);
            }
            else
            {
                Debug.Log("右到左");

                //右到左
                residuePlayingCards.StepR_to_L(step.playingCard, step.col);
            }
        }
        else if (step.gameAreaType == GameAreaType.Residue_L)
        {
            ResiduePlayingCards residuePlayingCards = playingCardControl.residuePlayingCards;

            //左->队列
            if (step.playingCard.playingCardQueue != null)
            {
                Debug.Log("左->队列");

                List<PlayingCard> cells = new List<PlayingCard>();
                cells.Add(step.playingCard);
                //当前队列移除
                step.playingCard.playingCardQueue.RemovePlayingCard(cells);

                residuePlayingCards.StepAddL(step.playingCard, step.row, step.col);

            }
            else if (step.playingCard.residuePlayingCards != null)
            {
                //左->左
                residuePlayingCards.StepAddL(step.playingCard, step.row, step.col);
                Debug.Log("//左->左");
            }
            else if (step.playingCard.playingCardRecycle != null)
            {
                step.playingCard.playingCardRecycle = null;
                Debug.Log("//左->回收");
                //左->回收
                residuePlayingCards.StepAddL(step.playingCard, step.row, step.col);
            }

        }
        if (step.frontOrBack == PlayingCard_FrontOrBack.Back)
        {
            step.playingCard.SetBack(step.playingCard.frontOrBack != PlayingCard_FrontOrBack.Back);
        }
        else
        {
            step.playingCard.SetFront(step.playingCard.frontOrBack != PlayingCard_FrontOrBack.Front);
        }
        step.playingCard.lastGameSteps = new GameStep(step.playingCard.gameSteps);
        if(step.playingCard.playingCardQueue != null)
        {
          //  step.playingCard.playingCardQueue.CheckQueue();
        }
    }

    public void RecordTheSteps()
    {
        var temSteps = new List<GameStep>();

        PlayingCardControl playingCardControl = UIManager.Instance.GetUI<GameScenePanel>().playingCardControl;
        foreach (var card in playingCardControl.GetAllPlayingCard())
        {
            if (!card.gameSteps.IsSame(card.lastGameSteps))
            {
                Debug.Log(card.name + "   " + card.lastGameSteps.frontOrBack);
                temSteps.Add(card.lastGameSteps);
                card.lastGameSteps = new GameStep(card.gameSteps);
            }
        }

        if (temSteps.Count > 0)
        {
            steps.Add(temSteps);
            Debug.Log($"记录步骤 :{steps.Count},{temSteps.Count}个");
            EventManager.Instance.TriggerEvent(GameEvent.Update_ItemReturnInfo);

            UIManager.Instance.GetUI<GameScenePanel>().AddMove(1);
        }

    }
}
