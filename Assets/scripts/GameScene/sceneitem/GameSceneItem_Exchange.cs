using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameSceneItem_Exchange : GameSceneItemBase
{
    PlayingCardControl playingCardControl;

    public override void Refresh()
    {
        base.Refresh();
        playingCardControl = UIManager.Instance.GetUI<GameScenePanel>().playingCardControl;
        cnt = GameManager.Instance.playerInfo.gameSceneItem_Exchange;
        type = SceneItemType.item_Exchange;
        lockLv = 1;

        bool isLock = GameManager.Instance.playerInfo.level < lockLv;

        unLockTrans.gameObject.SetActive(!isLock);
        lockTrans.gameObject.SetActive(isLock);

        cntStr.text = cnt <= 0 ? "+" : GameManager.Instance.playerInfo.gameSceneItem_Exchange.ToString();

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
        bool isUseItemSucceed = TryExchangeAllPlayingCard();
        if (isUseItemSucceed)
        {
            OtherSdkManager.Instance.CustomEvent("prop_use", "level_id", GameManager.Instance.playerInfo.level, "prop_id_number", "3");
            UIManager.Instance.GetUI<GameScenePanel>().ReportLevelStep("item_3");
            GameManager.Instance.playerInfo.Minus_item_exchange(1);
            //GameManager.Instance.SavePlayerInfo();
            Refresh();
        }
    }

    public void ExchangePlayingCard(PlayingCard c1, PlayingCard c2)
    {
        Sprite temSp = c1.cardImg.sprite;
        int temOd = c1.order;
        PlayingCardType temTy = c1.type;

        c1.Init(c2.cardImg.sprite, c2.order, c2.type);
        c1.gameObject.name = PlayingCardControl.GetName(c1.order);
        c2.Init(temSp, temOd, temTy);
        c2.gameObject.name = PlayingCardControl.GetName(c2.order);

    }

    public bool TryExchangeAllPlayingCard()
    {
        //随机
        List<PlayingCard> cards = new List<PlayingCard>();
        List<PlayingCard> cardsAnim = new List<PlayingCard>();
        foreach (var queue in playingCardControl.playingCardQueues)
        {
            foreach (var cell in queue.Cells)
            {
                if (cell.frontOrBack == PlayingCard_FrontOrBack.Back)
                {
                    cards.Add(cell);
                    cardsAnim.Add(cell);
                }
            }
        }

        playingCardControl.residuePlayingCards.AllL_to_R();
        foreach (var card in playingCardControl.residuePlayingCards.rightCards)
        {
            if (card.frontOrBack == PlayingCard_FrontOrBack.Back)
            {
                cards.Add(card);
            }
        }
        //交换
        foreach (var currentCard in cards)
        {
            // 1. 排除当前这张card，生成剩余卡牌集合
            List<PlayingCard> otherCards = cards.Where(c => c != currentCard).ToList();

            // 判断是否还有其他卡牌
            if (otherCards.Count == 0)
            {
                // 只剩当前一张，无其他卡牌可取
                return false;
            }

            // 2. 随机取一张
            int randomIndex = Random.Range(0, otherCards.Count);
            PlayingCard randomPick = otherCards[randomIndex];

            ExchangePlayingCard(currentCard, randomPick);
            // 后续使用 randomPick
        }

        StartCoroutine(ExchangeAllPlayingCardAnimIE(cardsAnim));

        return true;
    }

    IEnumerator ExchangeAllPlayingCardAnimIE(List<PlayingCard> cards)
    {
        UIManager.Instance.OpenUIMask();

        foreach (var card in cards)
        {
            card.isExchange = true;

            AudioManager.Instance.SetAudioSource(card.audioSource, "fapai");
            card.transform.DOMove(playingCardControl.residuePlayingCards.cells.transform.position, 0.2f);
            yield return new WaitForSeconds(0.1f);
        }
        foreach (var card in cards)
        {
            AudioManager.Instance.SetAudioSource(card.audioSource, "fapai");
            card.transform.DOLocalMove(card.correctPos, 0.2f).OnComplete(() =>
            {
                card.isExchange = false;
            });
            yield return new WaitForSeconds(0.1f);
        }

        UIManager.Instance.HideUIMask();
        GameStepRecord.Instance.ResetSteps();
    }
}
