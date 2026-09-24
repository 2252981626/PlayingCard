using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayingCardRecycle : MonoBehaviour
{
    public Canvas canvas;
    public Transform recycleRoot;
    public PlayingCard recycleArea;
    public void Init()
    {
        recycleArea.SetBack(false);
    }

    public void AddPlayingCards(PlayingCard _cell)
    {
        _cell.playingCardRecycle = this;
        _cell.correctPos = Vector3.zero;
        _cell.transform.SetParent(recycleRoot);
        _cell.transform.SetAsLastSibling();

        _cell.gameSteps.gameAreaType = GameAreaType.Recycle;
        _cell.gameSteps.row = transform.GetSiblingIndex();
        _cell.gameSteps.col = _cell.transform.GetSiblingIndex();
    }

    public bool CheckPlayingCardPosIsCorrect(PlayingCard _cell)
    {
        //位置检查
        bool posYes = recycleArea.CheckPosIsCorrect(_cell);
        if (!posYes)
        {
            return false;
        }
        //回收检查
        if (CheckRecycleRootsCondition(_cell))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 检查目标是否为A或者是否跟A同花色
    /// </summary>
    /// <param name="_cell"></param>
    /// <param name="_targetIndex"></param>
    /// <returns></returns>
    public bool CheckRecycleRootsCondition(PlayingCard _cell)
    {
        int childCnt = recycleRoot.childCount;
        if (childCnt == 0)
        {
            //检查是否为A
            if (PlayingCardControl.GetName(_cell.order) == "A")
            {
                return true;
            }
            Debug.Log($"{_cell.GetName()}   当前位置需要A");
        }
        else
        {
            //是否同花色
            bool isColour = false;
            PlayingCard playingCard = recycleRoot.GetChild(0).GetComponent<PlayingCard>();
            isColour = playingCard.type == _cell.type;
            if (!isColour)
            {
                Debug.Log($"{_cell.GetName()}   花色错误");
                return false;
            }

            int lastIndex = recycleRoot.childCount;
            PlayingCard lastPlayingCard = recycleRoot.GetChild(lastIndex - 1).GetComponent<PlayingCard>();
            if (lastPlayingCard.order == (_cell.order - 1))
            {
                return true;
            }
            Debug.Log($"{_cell.GetName()}   顺序错误");
        }
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
