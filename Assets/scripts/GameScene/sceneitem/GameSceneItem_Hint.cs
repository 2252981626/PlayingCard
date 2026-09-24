using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSceneItem_Hint : GameSceneItemBase
{
    PlayingCardControl playingCardControl;

    public override void Refresh()
    {
        base.Refresh();
        playingCardControl = UIManager.Instance.GetUI<GameScenePanel>().playingCardControl;
        cnt = GameManager.Instance.playerInfo.gameSceneItem_Hint;
        type = SceneItemType.item_hint;
        lockLv = 1;

        bool isLock = GameManager.Instance.playerInfo.level < lockLv;

        unLockTrans.gameObject.SetActive(!isLock);
        lockTrans.gameObject.SetActive(isLock);

        cntStr.text = cnt <= 0 ? "+" : GameManager.Instance.playerInfo.gameSceneItem_Hint.ToString();

    }

    public override void OnClick()
    {
        base.OnClick();
        if (cnt <= 0)
        {
           UIManager.Instance.OpenUI<AddSceneItemPanel>(this);
            return;
        }

        EventManager.Instance.TriggerEvent(GameEvent.StopHintAnim);
        bool isUseItemSucceed = TryHintAnim();
        if (isUseItemSucceed)
        {
            UIManager.Instance.GetUI<GameScenePanel>().ReportLevelStep("item_1");
            OtherSdkManager.Instance.CustomEvent("prop_use", "level_id", GameManager.Instance.playerInfo.level, "prop_id_number", "1");

            GameManager.Instance.playerInfo.Minus_item_hint(1);
            //GameManager.Instance.SavePlayerInfo();
            Refresh();
        }
        else
        {
            string str = LanguageManager.Instance.GetText("NoItemHintTips");
            UIManager.Instance.OpenUI<GeneralTipsPanel>(str);
        }
    }

    private bool TryHintAnim()
    {
        if (playingCardControl.residuePlayingCards.point1.childCount > 0)
        {
            PlayingCard card = playingCardControl.residuePlayingCards.point1.GetChild(0).GetComponent<PlayingCard>();

            foreach (var pR in playingCardControl.playingCardRecycles)
            {
                if (pR.CheckRecycleRootsCondition(card))
                {
                    card.ShowHintAnim();
                    return true;
                }
            }

            foreach (var queue in playingCardControl.playingCardQueues)
            {
                if (queue.CheckRecycleCellsCondition(card))
                {
                    card.ShowHintAnim();
                    return true;
                }
            }
        }

        foreach (var queue in playingCardControl.playingCardQueues)
        {
            if(queue.Cells.Count == 0)
            {
                continue;
            }
            //最后一张
            var zuihouCard = queue.Cells[queue.Cells.Count - 1];
            foreach (var pR in playingCardControl.playingCardRecycles)
            {
                if (pR.CheckRecycleRootsCondition(zuihouCard))
                {
                    zuihouCard.ShowHintAnim();
                    return true;
                }
            }
            //第一张正面
            PlayingCard diyizhangCard = null;
            for (int i = queue.Cells.Count - 1; i >= 0; i--)
            {
                if (queue.Cells[i].frontOrBack == PlayingCard_FrontOrBack.Front)
                {
                    diyizhangCard = queue.Cells[i];
                }
            }
            if(diyizhangCard != null)
            {
                foreach (var otherQueue in playingCardControl.playingCardQueues)
                {
                    //避免空位置来回检测
                    if (queue == otherQueue || otherQueue.Cells.Count == 0)
                    {
                        continue;
                    }

                    if (otherQueue.CheckRecycleCellsCondition(diyizhangCard))
                    {
                        diyizhangCard.ShowHintAnim();
                        return true;
                    }
                }
            }
        }

        if(playingCardControl.residuePlayingCards.rightCards.Count > 0)
        {
            int target = playingCardControl.residuePlayingCards.rightCards.Count - 1;
            playingCardControl.residuePlayingCards.rightCards[target].ShowHintAnim();
            return true;
        }

        return false;
    }
}
