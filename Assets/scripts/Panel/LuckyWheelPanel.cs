using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LuckyWheelPanel : UIBase
{
    public Transform root;
    public Button hideBtn;
    public Button spinBtn;
    public CanvasGroup canvasGroup;

    public Transform wheel;
    public Transform cellTrans;

    private List<ItemBase> cells;
    List<ItemData> itemDatas;
    List<ItemData> configs;
    List<int> weights;

    bool _isContainGold = false;
    int targetIndex;
    bool isInit = false;

    private string page_id = "LuckyWheelPanel";
    private void Awake()
    {
        RectTransform rect = root.GetComponent<RectTransform>();
        float topBlockHeight = Screen.height - Screen.safeArea.yMax;
        rect.offsetMax = new Vector2(0, -topBlockHeight);
    }
    void Start()
    {
        hideBtn.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayBtnMusic();
            AdManager.Instance.OnClickInterstitialAd(page_id);
            Hide();
        });
        spinBtn.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayBtnMusic();
            AdRewardCallback();
        });
        
    }

    private void Init()
    {
        if (isInit)
            return;

        isInit = true;

        configs = new List<ItemData>();
        configs.Add(new ItemData(ItemType.GoldDui, 10f));
        configs.Add(new ItemData(ItemType.GoldDui, 25f));
        configs.Add(new ItemData(ItemType.DiamondDui, 150));
        configs.Add(new ItemData(ItemType.DiamondDui, 300));
        configs.Add(new ItemData(ItemType.Hint, 1));
        configs.Add(new ItemData(ItemType.Exchange, 1));
        configs.Add(new ItemData(ItemType.Extract, 1));
        configs.Add(new ItemData(ItemType.Return, 1));

        weights = new List<int>() { 5, 1, 3, 2, 5, 5, 5, 5 };

        cells = GameManager.Instance.CreatItems(configs, transform);
        for (int i = 0; i < cells.Count; i++)
        {
            cells[i].transform.SetParent(cellTrans.GetChild(i).transform);
            cells[i].transform.localEulerAngles = Vector3.zero;
            cells[i].transform.localPosition = Vector3.zero;
            cells[i].transform.localScale = Vector3.one;
            cells[i].effect.gameObject.SetActive(false);
        }
    }

    private void RandomReward()
    {
        itemDatas = new List<ItemData>();

        targetIndex = GameManager.GetWeightIndex(configs, weights);
        itemDatas.Add(configs[targetIndex]);
        _isContainGold = false;
        foreach (var itemdata in itemDatas)
        {
            Debug.Log(itemdata.itemType.ToString());
            if (itemdata.itemType == ItemType.Gold || itemdata.itemType == ItemType.Diamond)
            {
                _isContainGold = true;
                break;
            }
        }
    }

    public override void Refresh(object data = null)
    {
        base.Refresh(data);

        OtherSdkManager.Instance.CustomEvent("luckyspin", "show", "");
        Init();

    }
    public override void Hide()
    {
        base.Hide();
    }
    private void AdRewardCallback()
    {
        RandomReward();
        spinBtn.interactable = false;
        canvasGroup.alpha = 0.5f;
        SpinWheel();
        OtherSdkManager.Instance.CustomEvent("luckyspin", "spin", "");
    }


    private void SpinWheel()
    {
        AudioManager.Instance.PlaySceneSingleMusic("wheelrotate");
        AudioManager.Instance.BGMLitter();
        UIManager.Instance.OpenUIMask();
        float targetaugle = targetIndex * 45f;
        DOTween.Sequence()
            .Append(wheel.transform.DORotate(new Vector3(0, 0, 360 * 1f), 0.5f, RotateMode.FastBeyond360).SetEase(Ease.InCubic))
            .Append(wheel.transform.DORotate(new Vector3(0, 0, 360 * 3f + targetaugle), 2f, RotateMode.FastBeyond360).SetEase(Ease.OutCubic))
            .AppendCallback(() =>
            {
                AudioManager.Instance.BGMRecover();
            })
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                UIManager.Instance.HideUIMask();
                UIManager.Instance.OpenUI<GeneralRewardsPanel3>(itemDatas, () =>
                {
                    spinBtn.interactable = true;
                    canvasGroup.alpha = 1f;
                });

            });
    }



}
