using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayingCardQueue : MonoBehaviour
{
    public PlayingCard basePlayingCardBg;
    public Transform cellTrans;
    public Canvas canvas;

    public List<PlayingCard> Cells;
    private float baseCell0_Y = 540f;
    private void Start()
    {
        basePlayingCardBg.SetBack(false);
        basePlayingCardBg.transform.localPosition = new Vector3(0f, baseCell0_Y, 0f);
    }
    public void Init()
    {
        Cells = new List<PlayingCard>();
    }

    /// <summary>
    /// 打开最上面的背面卡
    /// </summary>
    public void ShowLastBackPlayingCard()
    {
        int lastIndex = Cells.Count;
        if(lastIndex == 0)
        {
            return;
        }

        if (Cells[lastIndex - 1].frontOrBack == PlayingCard_FrontOrBack.Back)
        {
            Cells[lastIndex - 1].SetFront(true);
        }
    }

    public void RemovePlayingCard(List<PlayingCard> _cells)
    {
        if (_cells == null || _cells.Count == 0) return;

        foreach (var cell in _cells)
        {
            if (Cells.Contains(cell))
            {
                cell.playingCardQueue = null;
                Cells.Remove(cell);
            }
        }
    }

    public void ResetCellPos()
    {
        foreach (var cell in Cells)
        {
            cell.correctPos = GetPlayingCardPos(cell);
            cell.RightAwayBackCorrectPos();
        }
    }

    public void AddPlayingCards(List<PlayingCard> _cells)
    {
        if (_cells == null || _cells.Count == 0) return;

        foreach (var cell in _cells)
        {
            cell.playingCardRecycle = null;
            cell.playingCardQueue = this;
            cell.correctPos = GetPlayingCardPos(cell);
            cell.transform.SetParent(cellTrans);
            cell.transform.SetAsLastSibling();
            Cells.Add(cell);

            cell.gameSteps.gameAreaType = GameAreaType.Queue;
            cell.gameSteps.row = transform.GetSiblingIndex();
            cell.gameSteps.col = Cells.IndexOf(cell);

        }
    }

    public void CheckQueue()
    {
        bool isback = false;
        for (int i = Cells.Count - 1; i >= 0; i--)
        {
            var cell = Cells[i];
            if(i == Cells.Count - 1)
            {
                cell.SetFront(false);
            }
            if(!isback)
            {
                isback = cell.frontOrBack == PlayingCard_FrontOrBack.Back;
            }
            else
            {
                if(cell.frontOrBack == PlayingCard_FrontOrBack.Front)
                {
                    cell.SetBack(true);
                }
            }
            cell.lastGameSteps = new GameStep(cell.gameSteps);
        }
    }

    /// <summary>
    /// 步骤专用
    /// </summary>
    /// <param name="_cell"></param>
    /// <param name="col"></param>
    public void AddPlayingCard(PlayingCard_FrontOrBack stepType, PlayingCard _cell, int col)
    {
        _cell.playingCardRecycle = null;
        _cell.playingCardQueue = this;

        int targetCol = col;
        if(targetCol >= Cells.Count)
        {
            if (stepType == PlayingCard_FrontOrBack.Back)
            {
                for (int i = Cells.Count - 1; i >= 0; i--)
                {
                    var cell = Cells[i];
                    if (cell.frontOrBack == PlayingCard_FrontOrBack.Back)
                    {
                        targetCol = i + 1;
                        break;
                    }
                }
       
                Cells.Insert(targetCol, _cell);
                Debug.Log($"{_cell.GetName()}插入错误,改变位置到：{targetCol}");
            }
            else
            {
                Cells.Add(_cell);
            }
        }
        else
        {
            Cells.Insert(targetCol, _cell);
        }

        _cell.correctPos = GetPlayingCardPos(_cell);
        _cell.transform.SetParent(cellTrans);
        _cell.transform.SetSiblingIndex(targetCol);

        _cell.gameSteps.gameAreaType = GameAreaType.Queue;
        _cell.gameSteps.row = transform.GetSiblingIndex();
        _cell.gameSteps.col = Cells.IndexOf(_cell);

        foreach (var temC in Cells)
        {
            temC.correctPos = GetPlayingCardPos(temC);

            temC.gameSteps.gameAreaType = GameAreaType.Queue;
            temC.gameSteps.row = transform.GetSiblingIndex();
            temC.gameSteps.col = Cells.IndexOf(temC);
            temC.lastGameSteps = new GameStep(temC.gameSteps);
            temC.BackCorrectPos();
        }
    }

    public List<PlayingCard> GetConnectedPlayingCards(PlayingCard _cell)
    {
        if (_cell == null) return new List<PlayingCard>();

        int index = Cells.IndexOf(_cell);
        if (index == -1) return new List<PlayingCard>(); // 没找到返回空列表

        return Cells.Skip(index).ToList();
    }

    public Vector3 GetPlayingCardPos(PlayingCard _cell)
    {
        int count = Cells.Count;
        if (Cells.Contains(_cell))
        {
            count = Cells.IndexOf(_cell);
        }

        if (count == -1) return Vector3.zero;

        Vector3 targetPos = new Vector3(0f, baseCell0_Y - 60f * count, 0f);
        return targetPos;
    }

    public bool CheckPlayingCardPosIsCorrect(PlayingCard _cell)
    {
        //取最上面一个
        PlayingCard target = basePlayingCardBg;
        if (Cells.Count > 0)
        {
            target = Cells[Cells.Count - 1];
        }
        //位置检查
        bool posYes = target.CheckPosIsCorrect(_cell);
        if(!posYes)
        {
            return false;
        }
        //花色检查
        if(CheckRecycleCellsCondition(_cell))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 检查目标是否间隔花色
    /// </summary>
    /// <param name="_cell"></param>
    /// <param name="_targetIndex"></param>
    /// <returns></returns>
    public bool CheckRecycleCellsCondition(PlayingCard _cell)
    {
        int lastIndex = Cells.Count;
        if(lastIndex == 0)
        {
            //允许任意，但是只能允许当前列的第一张正面卡
            //PlayingCardQueue temQ = _cell.playingCardQueue;
            //if(temQ != null)
            //{
            //    //找到队列第一张正面卡
            //    for (int i = 0; i < temQ.Cells.Count; i++)
            //    {
            //        if (temQ.Cells[i].frontOrBack == PlayingCard_FrontOrBack.Front)
            //        {
            //            //如果是第一张正面卡，允许
            //            return temQ.Cells[i] == _cell;
            //        }
            //    }
            //}
            //else
            //{
            //    return true;
            //}

            //只能K
            if (PlayingCardControl.GetName(_cell.order) == "K")
            {
                return true;
            }
            else
            {
                Debug.Log($"{_cell.GetName()}   当前位置需要K");
                return false;
            }
        }
        PlayingCard lastPlayingCard = Cells[lastIndex - 1];
        //花色检查
        bool isColour = false;
        if (lastPlayingCard.type == PlayingCardType.clubs || lastPlayingCard.type == PlayingCardType.spades)
        {
            isColour = _cell.type == PlayingCardType.hearts || _cell.type == PlayingCardType.diamonds;
        }
        else
        {
            isColour = _cell.type == PlayingCardType.clubs || _cell.type == PlayingCardType.spades;
        }
        if(!isColour)
        {
            Debug.Log($"{_cell.GetName()}   花色错误");
            return false;
        }
        //顺序检查
        if(lastPlayingCard.order == (_cell.order + 1))
        {
            return true;
        }
        Debug.Log($"{_cell.GetName()}   顺序错误");
        return false;
    }

    public void SetCanvasTop()
    {
        canvas.sortingOrder = 160;
    }
    public void SetCanvasRecover()
    {
        canvas.sortingOrder = 110;
    }
}
