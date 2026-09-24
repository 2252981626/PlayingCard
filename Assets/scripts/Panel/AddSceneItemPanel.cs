using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AddSceneItemPanel : UIBase
{
    public Transform btnMask;
    public Image icon;
    public Image icon2;
    public Button hideBtn;
    public Button getBtn;
    public CanvasGroup getBtnCG;
    public RewardAdButton rewardAdButton;

    public Text adStr;
    public Text getStr1;
    public Text getStr2;

    private int adGetCnt = 1;
    private int getCnt = 1;
    private int expendDiamondCnt = 200;
    private string page_id = "AddSceneItemPanel";

    private GameSceneItemBase sceneItemBase;
    private void Start()
    {
        hideBtn.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayBtnMusic();
            AdManager.Instance.OnClickInterstitialAd(page_id);
            Hide();
        });
        getBtn.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayBtnMusic();
            GetOnClick();
        });

        adStr.text = LanguageManager.Instance.GetText("FREE") + $" X{getCnt}";
        getStr1.text = LanguageManager.Instance.GetText("BUY") + $" X{getCnt}";
        getStr2.text = $"{expendDiamondCnt}";
    }
    public override void Refresh(object data = null)
    {
        base.Refresh(data);
        sceneItemBase = data as GameSceneItemBase;
        icon.sprite = sceneItemBase.clickBtn.transform.GetComponent<Image>().sprite;
        icon.SetNativeSize();

        icon2.sprite = sceneItemBase.clickBtn.transform.GetComponent<Image>().sprite;
        icon2.SetNativeSize();
        icon2.transform.localPosition = icon.transform.localPosition;
        icon2.transform.localScale = Vector3.one*2f;
        icon2.gameObject.SetActive(false);

        rewardAdButton.Init(AdCallback, page_id, false);
        RefreshGetBtnUI();
        GameScenePanel.isPause = true;

        DOTween.Sequence().AppendInterval(20f / 60f).AppendCallback(() =>
        {
            Vector3 newPos = btnMask.transform.position;
            newPos.y = sceneItemBase.transform.position.y;
            btnMask.transform.position = newPos;
        });

      
    }
    public override void Hide()
    {
        callback = () =>
        {
            UIManager.Instance.GetUI<PlayerInfoUI>().DiamondCanvasRecover();
            sceneItemBase.CanvasRecover();
            GameScenePanel.isPause = false;
        };
        base.Hide();
    }

    private void RefreshGetBtnUI()
    {
        switch (sceneItemBase.type)
        {
            case SceneItemType.item_hint:
                expendDiamondCnt = 200;
                break;
            case SceneItemType.item_Extract:
                expendDiamondCnt = 200;
                break;
            case SceneItemType.item_Exchange:
                expendDiamondCnt = 200;
                break;
            case SceneItemType.item_Return:
                expendDiamondCnt = 200;
                break;
        }

        float curDiamond = GameManager.Instance.playerInfo.diamond;
        bool isTrue = curDiamond >= expendDiamondCnt;
        getBtn.interactable = isTrue;
        getBtnCG.alpha = isTrue ? 1f : 0.5f;
    }

    private void GetOnClick()
    {
        OtherSdkManager.Instance.CustomEvent("prop_buy", "prop_id_show", "buy");
        GameManager.Instance.playerInfo.Minus_diamond(expendDiamondCnt);
        RefreshGetBtnUI();

        UIManager.Instance.GetUI<PlayerInfoUI>().DiamondCanvasTop();
        UIManager.Instance.GetUI<PlayerInfoUI>().StartDiamondAnim();

        sceneItemBase.CanvasTop();
        icon2.gameObject.SetActive(true);
        UIManager.Instance.OpenUIMask();
        icon2.transform.DOScale(1f, 0.75f);
        DOTween.Sequence().Append(icon2.transform.DOMove(sceneItemBase.transform.position, 0.75f)).AppendCallback(() =>
        {
            icon2.transform.localPosition = icon.transform.localPosition;
            icon2.gameObject.SetActive(false);
            icon2.transform.localScale = Vector3.one * 2f;
            sceneItemBase.ScaleAnim();
            GetItem(getCnt);
            UIManager.Instance.HideUIMask();
        });
    }

    private void AdCallback()
    {
        OtherSdkManager.Instance.CustomEvent("prop_buy", "prop_id_show", "ad_claim");
        sceneItemBase.CanvasTop();
        icon2.gameObject.SetActive(true);
        UIManager.Instance.OpenUIMask();
        icon2.transform.DOScale(1f, 0.75f);
        DOTween.Sequence().Append(icon2.transform.DOMove(sceneItemBase.transform.position, 0.75f)).AppendCallback(() =>
        {
            icon2.transform.localPosition = icon.transform.localPosition;
            icon2.gameObject.SetActive(false);
            icon2.transform.localScale = Vector3.one * 2f;
            sceneItemBase.ScaleAnim();
            GetItem(adGetCnt);
            UIManager.Instance.HideUIMask();
        });

        DOTween.Sequence().AppendInterval(0.2f).AppendCallback(() =>
        {
            rewardAdButton.Init(AdCallback, page_id, false);
        });
    }

    private void GetItem(int _cnt)
    {
        switch (sceneItemBase.type)
        {
            case SceneItemType.item_hint:
                GameManager.Instance.playerInfo.Add_item_hint(_cnt);

                break;
            case SceneItemType.item_Extract:
                GameManager.Instance.playerInfo.Add_item_extract(_cnt);

                break;
            case SceneItemType.item_Exchange:
                GameManager.Instance.playerInfo.Add_item_exchange(_cnt);
                break;
            case SceneItemType.item_Return:
                GameManager.Instance.playerInfo.Add_item_return(_cnt);
                break;
        }
        sceneItemBase.Refresh();
        GameManager.Instance.SavePlayerInfo();
    }
}
