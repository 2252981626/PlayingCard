using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoUI : UIBase
{
    public Image bg;
    public Image bg2;

    public Transform headTrans;
    [Header("Gold")]
    public Transform goldTrans;
    public Image goldIcon;
    public Text goldCnt;
    public Canvas goldCanvas;
    public Transform txTrans;
    [Header("Diamond")]
    public Transform diamondTrans;
    public Image diamondIcon;
    public Text diamondCnt;
    public Canvas diamondCanvas;
    [Header("Head")]
    public Image headIcon;
    public Button headBtn;
    public List<Sprite> heads;

    public Button settingBtn;



    private void Awake()
    {
        RectTransform rect = GetComponent<RectTransform>();
        float topBlockHeight = Screen.height - Screen.safeArea.yMax;
        rect.offsetMax = new Vector2(0, -topBlockHeight);
    }
    private void OnEnable()
    {
        isOpen = false;
    }
    private void OnDisable()
    {
        isOpen = false;
    }
    private void Start()
    {
        settingBtn.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayBtnMusic();
            UIManager.Instance.OpenUI<SettingPanel>();
        });

        headBtn.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayBtnMusic();
            UIManager.Instance.OpenUI<HeadPanel>();
        });
        GameManager.Instance.UpdateAppATTToDiamond(goldIcon);

        if(TxElementMananger.Instance != null)
        {
           var obj = Instantiate(TxElementMananger.Instance.TxProgressPrefab, txTrans);
            obj.transform.localPosition = Vector3.zero;
        }
    }

    public override void Refresh(object data = null)
    {
        base.Refresh(data);

        RefreshGoldUI();
        RefreshDiamondUI();
        SwitchUI(true);
        RefreshHeadUI();
    }
    public override void Hide()
    {
        base.Hide();
    }

    public void RefreshHeadUI()
    {
        headIcon.sprite = heads[GameManager.Instance.playerInfo.head];
        headIcon.SetNativeSize();
    }

    public void RefreshGoldUI()
    {
        goldCnt.text = GameManager.Instance.playerInfo.gold.ToString();
    }

    public void RefreshDiamondUI()
    {
        diamondCnt.text = GameManager.Instance.playerInfo.diamond.ToString();
    }

    public void SwitchUI(bool ishome)
    {
        headTrans.gameObject.SetActive(ishome);
        bg.gameObject.SetActive(ishome);
        bg2.gameObject.SetActive(!ishome);

        if (ishome)
        {
            goldTrans.transform.localPosition = new Vector3(-177f, 0f, 0f);
            diamondTrans.transform.localPosition = new Vector3(357f, 0f, 0f);
            settingBtn.transform.localPosition = new Vector3(450f, -140f, 0f);
        }
        else
        {
            goldTrans.transform.localPosition = new Vector3(-356f, 0f, 0f);
            diamondTrans.transform.localPosition = new Vector3(186f, 0f, 0f);
            settingBtn.transform.localPosition = new Vector3(450f, 0f, 0f);
        }
    }

    //------------------------------------------
    public void GoldFlyAnim(Vector3 start)
    {
        GoldCollectEffect.Instance.StartEffect(ItemType.Gold, start, goldIcon.transform.position);
        DOTween.Sequence().AppendInterval(0.8f).AppendCallback(() =>
        {
            StartGoldAnim();
        });
    }
    public void GoldCanvasTop()
    {
        goldCanvas.sortingOrder = 510;
    }
    public void GoldCanvasRecover()
    {
        goldCanvas.sortingOrder = 410;
    }

    public void StartGoldAnim()
    {
        goldTrans.DOKill();
        float _currentValue = float.Parse(goldCnt.text);
        float targetGold = GameManager.Instance.playerInfo.gold;
        bool hasDecimal1 = targetGold != Mathf.RoundToInt(targetGold); // true（有小数）
        int unit = hasDecimal1 ? 2 : 0;

        DOTween.To(
          () => _currentValue,
          x =>
          {
              _currentValue = (float)Math.Round(x, unit);
              goldCnt.text = _currentValue.ToString();
          },
          targetGold, // 目标值
          1f // 时长
        ).SetTarget(goldTrans)
        .OnComplete(() =>
        {
            goldCnt.text = GameManager.Instance.playerInfo.gold.ToString();
        });
    }

    //-----------------------------------------------------

    public void DiamondFlyAnim(Vector3 start)
    {
        GoldCollectEffect.Instance.StartEffect(ItemType.Diamond, start, diamondIcon.transform.position);
        DOTween.Sequence().AppendInterval(0.8f).AppendCallback(() =>
        {
            StartDiamondAnim();
        });
    }
    public void DiamondCanvasTop()
    {
        diamondCanvas.sortingOrder = 510;
    }
    public void DiamondCanvasRecover()
    {
        diamondCanvas.sortingOrder = 410;
    }

    public void StartDiamondAnim()
    {
        diamondTrans.DOKill();
        float _currentValue = float.Parse(diamondCnt.text);
        float targetDiamond = GameManager.Instance.playerInfo.diamond;
        bool hasDecimal1 = targetDiamond != Mathf.RoundToInt(targetDiamond); // true（有小数）
        int unit = hasDecimal1 ? 2 : 0;

        DOTween.To(
          () => _currentValue,
          x =>
          {
              _currentValue = (float)Math.Round(x, unit);
              diamondCnt.text = _currentValue.ToString();
          },
          targetDiamond, // 目标值
          1f // 时长
        ).SetTarget(diamondTrans)
        .OnComplete(() =>
        {
            diamondCnt.text = GameManager.Instance.playerInfo.diamond.ToString();
        });
    }
}
