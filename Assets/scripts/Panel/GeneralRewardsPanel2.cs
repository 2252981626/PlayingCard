using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GeneralRewardsPanel2 : UIBase
{
    public Transform root;
    public Transform itemRoot;
    public Button collectBtn;

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
            AdManager.Instance.OnClickInterstitialAd("GameBoxRewardsPanel");
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
        OtherSdkManager.Instance.CustomEvent("rewards_show", "show", "");
        AudioManager.Instance.PlaySceneSingleMusic("rewardPanel");

        itemBase = GameManager.Instance.CreatItems(itemDatas, itemRoot);
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
