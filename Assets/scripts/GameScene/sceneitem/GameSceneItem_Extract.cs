using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 魔法棒
/// </summary>
public class GameSceneItem_Extract : GameSceneItemBase
{
    PlayingCardControl playingCardControl;
    public Transform guideTrans;

    public void ShowGuide()
    {
        guideTrans.gameObject.SetActive(true);
    }

    public void HideGuide()
    {
        guideTrans.gameObject.SetActive(false);
    }

    public override void Refresh()
    {
        base.Refresh();
        HideGuide();
        playingCardControl = UIManager.Instance.GetUI<GameScenePanel>().playingCardControl;
        cnt = GameManager.Instance.playerInfo.gameSceneItem_Extract;
        type = SceneItemType.item_Extract;
        lockLv = 1;

        bool isLock = GameManager.Instance.playerInfo.level < lockLv;

        unLockTrans.gameObject.SetActive(!isLock);
        lockTrans.gameObject.SetActive(isLock);

        cntStr.text = cnt <= 0 ? "+" : GameManager.Instance.playerInfo.gameSceneItem_Extract.ToString();

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
        bool isUseItemSucceed = TryExtractCard();
        if (isUseItemSucceed)
        {
            OtherSdkManager.Instance.CustomEvent("prop_use", "level_id", GameManager.Instance.playerInfo.level, "prop_id_number", "2");

            UIManager.Instance.GetUI<GameScenePanel>().AddisEffectualStepAwaitCallback_Item2();
            GameManager.Instance.playerInfo.Minus_item_extract(1);
            // GameManager.Instance.SavePlayerInfo();
            Refresh();
            playingCardControl.residuePlayingCards.UpdateUI();
        }
        else
        {
            string str = LanguageManager.Instance.GetText("NoItemHintTips");
            UIManager.Instance.OpenUI<GeneralTipsPanel>(str);
        }
    }

    private bool TryExtractCard()
    {
        List<PlayingCard> cards = new List<PlayingCard>();
        foreach (var card in playingCardControl.residuePlayingCards.rightCards)
        {
            cards.Add(card);

        }
        foreach (var card in playingCardControl.residuePlayingCards.leftCards)
        {
            cards.Add(card);
        }
        foreach (var queue in playingCardControl.playingCardQueues)
        {
            foreach (var cell in queue.Cells)
            {
                if (cell.frontOrBack == PlayingCard_FrontOrBack.Back)
                {
                    cards.Add(cell);
                }
            }
            if(queue.Cells.Count > 0)
            {
                var lastCard = queue.Cells[queue.Cells.Count - 1];
                cards.Add(lastCard);
            }
        }

        int targetIndex = 99;
        PlayingCardType targetType = PlayingCardType.clubs;
        PlayingCard target = null;

        foreach (var recycle in playingCardControl.playingCardRecycles)
        {
            if (recycle.recycleRoot.childCount == 0)
            {
                targetIndex = 0;
            }
            else
            {
                PlayingCard _c = recycle.recycleRoot.GetChild(recycle.recycleRoot.childCount - 1).GetComponent<PlayingCard>();
                int _order = _c.order + 1;
                if (targetIndex > _order)
                {
                    targetIndex = _order;
                    targetType = _c.type;
                }
            }
        }

        foreach (var card in cards)
        {
            if (targetIndex == 0 && card.order == 0)
            {
                target = card;
                break;
            }
            else
            {
                if (card.order == targetIndex && card.type == targetType)
                {
                    target = card;
                    break;
                }
            }
        }

        if (target != null)
        {
            foreach (var pR in playingCardControl.playingCardRecycles)
            {
                if (pR.CheckRecycleRootsCondition(target))
                {
                    if (target.playingCardQueue != null)
                    {
                        List<PlayingCard> thiscards = new List<PlayingCard>();
                        thiscards.Add(target);
                        var curQueue = target.playingCardQueue;
                        target.playingCardQueue.RemovePlayingCard(thiscards);

                        for (int i = 0; i < curQueue.Cells.Count; i++)
                        {
                            var cell = curQueue.Cells[i];
                            cell.correctPos = curQueue.GetPlayingCardPos(cell);
                            cell.BackCorrectPos(false);

                            cell.gameSteps.col = i;
                            cell.lastGameSteps = new GameStep(cell.gameSteps);
                        }
                    }

                    if (target.residuePlayingCards != null)
                    {
                        target.residuePlayingCards.RemovePlayingCard(target);
                    }

                    target.SetFront(target.frontOrBack == PlayingCard_FrontOrBack.Back);
                    pR.AddPlayingCards(target);
                    target.BackCorrectPosCoroutine(false, true);
                    return true;
                }
            }
        }
        Debug.Log("魔法棒使用失败");
        return false;
    }
}
