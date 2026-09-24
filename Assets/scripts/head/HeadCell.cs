using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HeadCell : MonoBehaviour
{
    public Button btn;
    public Button diamondBtn;
    public Text diamondText;
    public CanvasGroup canvasGroup;
    public RewardAdButton rewardAdButton;

    public Image icon;
    public Image select;
    public Image select2;

    private float expendDiamond = 200;
    private HeadPanel headPanel;
    [HideInInspector] public int index;
    private int lockType;
    private string page_id = "HeadPanel";
    private void Start()
    {
        diamondBtn.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayBtnMusic();
            DiamondJieSuo();
        });
        btn.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayBtnMusic();
            Select(true);
        });
    }
    public void Init(int _index,int _lockType)
    {
        index = _index;
        lockType = _lockType;
        headPanel = UIManager.Instance.GetUI<HeadPanel>();

        var heads = GameManager.Instance.playerInfo.heads;
        bool unLock = heads.Contains(index);
        btn.gameObject.SetActive(unLock);
        rewardAdButton.gameObject.SetActive(false);
        diamondBtn.gameObject.SetActive(false);
        if (!unLock)
        {
            rewardAdButton.gameObject.SetActive(lockType == 1);
            diamondBtn.gameObject.SetActive(lockType == 2);
            UpdateDimmondLockState();
        }
        if (index == GameManager.Instance.playerInfo.head)
        {
            Select(true);
        }

        icon.sprite = UIManager.Instance.GetUI<PlayerInfoUI>().heads[index];
        icon.SetNativeSize();

        rewardAdButton.Init(AdCallback, page_id, false);
    }

    public void UpdateDimmondLockState()
    {
        if (!diamondBtn.gameObject.activeSelf)
            return;

        diamondText.text = expendDiamond.ToString();
        bool isyes = GameManager.Instance.playerInfo.diamond >= expendDiamond;
        diamondBtn.interactable = isyes;
        canvasGroup.alpha = isyes ? 1f : 0.5f;
    }
    public void DiamondJieSuo()
    {
        if(GameManager.Instance.playerInfo.diamond < expendDiamond)
        {
            UIManager.Instance.OpenUI<GeneralTipsPanel>(LanguageManager.Instance.GetText("InsufficientDiamond"));
        }
        else
        {
            UIManager.Instance.OpenUIMask();
            GameManager.Instance.playerInfo.Minus_diamond(expendDiamond);
            PlayerInfoUI playerInfoUI = UIManager.Instance.GetUI<PlayerInfoUI>();
            playerInfoUI.DiamondCanvasTop();
            playerInfoUI.StartDiamondAnim();
            DOTween.Sequence().AppendInterval(1f).AppendCallback(() =>
            {
                playerInfoUI.DiamondCanvasRecover();
                UIManager.Instance.HideUIMask();
            });
            Select(true);
            Debug.Log("解锁头像：" + index);
            GameManager.Instance.playerInfo.heads.Add(index);
            headPanel.UpdateCells();
            diamondBtn.gameObject.SetActive(false);
            btn.gameObject.SetActive(true);

            headPanel.UpdateDimmondLockState();
        }
    }

    public void AdCallback()
    {
        Select(true);
        Debug.Log("解锁头像：" + index);
        GameManager.Instance.playerInfo.heads.Add(index);
        headPanel.UpdateCells();
        rewardAdButton.gameObject.SetActive(false);
        btn.gameObject.SetActive(true);
    }

    public void Select(bool isSelect)
    {
        if (isSelect)
        {
            if (headPanel.curSelect != null && headPanel.curSelect != this)
            {
                headPanel.curSelect.Select(false);
            }
            headPanel.curSelect = this;
        }
        select.gameObject.SetActive(isSelect);
        select2.gameObject.SetActive(isSelect);
    }
}
