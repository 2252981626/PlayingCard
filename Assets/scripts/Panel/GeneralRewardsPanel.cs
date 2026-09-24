using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GeneralRewardsPanel : UIBase
{
    public Transform root;
    public Transform itemRoot;
    public RewardAdButton rewardAdButton;
    public Button collectBtn;
    public Transform collectTrans;
    public Text collectText;

    private List<ItemData> itemDatas;
    private List<ItemBase> itemBase;

    private string page_id = "GeneralRewardsPanel";
    private string unit;
    private void Awake()
    {
        RectTransform rect = root.GetComponent<RectTransform>();
        float topBlockHeight = Screen.height - Screen.safeArea.yMax;
        rect.offsetMax = new Vector2(0, -topBlockHeight);
    }
    private void Start()
    {
        collectBtn.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayBtnMusic();
            AdManager.Instance.OnClickInterstitialAd("GameBoxRewardsPanel");
            CollectClick();
        });
    }
    private void OnEnable()
    {
        isOpen = true;
    }
    private void OnDisable()
    {
        isOpen = false;
        ResetPanel();
    }
    public override void Refresh(object data = null)
    {
        base.Refresh(data);
        OtherSdkManager.Instance.CustomEvent("rewards_show", "show", "");

        itemDatas = data as List<ItemData>;

        AudioManager.Instance.PlaySceneSingleMusic("rewardPanel");
        itemBase = GameManager.Instance.CreatItems(itemDatas, itemRoot);
        bool _isContainGold = false;
        foreach (var itemdata in itemDatas)
        {
            if (itemdata.itemType == ItemType.Gold || itemdata.itemType == ItemType.Diamond)
            {
                _isContainGold = true;
                break;
            }
        }
        if(string.IsNullOrEmpty(unit))
        {
            unit = LanguageManager.Instance.GetText_Encrypt("Special_Diamond__unit");
        }
        collectText.text = $"{LanguageManager.Instance.GetText("OnlyClaim")} {unit}{MathF.Round(itemDatas[0].count / 10f, 2)}";
        rewardAdButton.Init(AdsCallback, page_id, _isContainGold);
    }

    private void AdsCallback()
    {
        GameBox.Instance.SetBoxRewardClaimMethod("rewarded_ad");
        OtherSdkManager.Instance.CustomEvent("rewards_click", "click", "claim_two");
        OtherSdkManager.Instance.CustomEvent("general_reward_ad_claim", "level_id", GameBox.curLv);

        PlayerInfoUI playerInfoUI = UIManager.Instance.GetUI<PlayerInfoUI>();
        UIManager.Instance.OpenUIMask();
        float awaitTime = 0.1f;
        foreach (var item in itemBase)
        {
            if (item.itemType == ItemType.Gold || item.itemType == ItemType.GoldDui)
            {
                awaitTime = 2f;
                playerInfoUI.GoldCanvasTop();
            }
            else if (item.itemType == ItemType.Diamond || item.itemType == ItemType.DiamondDui)
            {
                awaitTime = 2f;
                playerInfoUI.DiamondCanvasTop();
            }

            item.GetItemReward();
            item.PlayItemAnim();
        }
        //¶¯»­
        DOTween.Sequence().AppendInterval(awaitTime).AppendCallback(() =>
        {
            playerInfoUI.GoldCanvasRecover();
            playerInfoUI.DiamondCanvasRecover();
            Hide();
        });
    }

    private void CollectClick()
    {
        GameBox.Instance.SetBoxRewardClaimMethod("normal");
        OtherSdkManager.Instance.CustomEvent("rewards_click", "click", "claim_one");

        PlayerInfoUI playerInfoUI = UIManager.Instance.GetUI<PlayerInfoUI>();
        UIManager.Instance.OpenUIMask();
        float awaitTime = 0.1f;
        foreach (var item in itemBase)
        {
            if (item.itemType == ItemType.Gold || item.itemType == ItemType.GoldDui)
            {
                awaitTime = 2f;
                playerInfoUI.GoldCanvasTop();
            }
            else if (item.itemType == ItemType.Diamond || item.itemType == ItemType.DiamondDui)
            {
                awaitTime = 2f;
                playerInfoUI.DiamondCanvasTop();
            }
            item.count = MathF.Round(item.count / 10f, 2);
            item.GetItemReward();
            item.PlayItemAnim();
        }
        //¶¯»­
        DOTween.Sequence().AppendInterval(awaitTime).AppendCallback(() =>
        {
            playerInfoUI.GoldCanvasRecover();
            playerInfoUI.DiamondCanvasRecover();
            Hide();
        });
    }

    public override void Hide()
    {
        GameManager.Instance.SavePlayerInfo();
        base.Hide();
    }

    private void ResetPanel()
    {
        foreach (Transform item in itemRoot)
        {
            Destroy(item.gameObject);
        }
        itemDatas = null;
        itemBase = null;
    }
}
