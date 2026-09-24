using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GeneralRewardsPanel3 : UIBase
{
    public Transform root;
    public Transform itemRoot;
    public RewardAdButton rewardAdButton;
    public Button collectBtn;
    public Transform collectTrans;

    private List<ItemData> itemDatas;
    private List<ItemBase> itemBase;

    private string page_id = "GeneralRewardsPanel";
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
            CollectClick();
        });
    }
    private void OnDisable()
    {
        ResetPanel();
    }
    public override void Refresh(object data = null)
    {
        base.Refresh(data);
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
        OtherSdkManager.Instance.CustomEvent("luckyspin_rewards", "show", "");
        rewardAdButton.Init(AdsCallback, page_id, _isContainGold);
    }

    private void AdsCallback()
    {
        OtherSdkManager.Instance.CustomEvent("luckyspin_rewards", "claim", "");
        OtherSdkManager.Instance.CustomEvent("lucky_spin_reward_ad_claim", "level_id", GameManager.Instance.playerInfo.level);

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
        OtherSdkManager.Instance.CustomEvent("luckyspin_rewards", "no_thanks", "");
        AdManager.Instance.OnClickInterstitialAd("LuckyWheelRewardsPanel");
        Hide();
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
