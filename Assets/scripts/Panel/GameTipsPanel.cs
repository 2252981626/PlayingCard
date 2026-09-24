using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameTipsPanel : UIBase
{
    public List<Sprite> sPs;
    public Image neirong;
    public Button hideBtn;
    public Button leftBtn;
    public Button rightBtn;
    public Text ex;
    public Text xuanzeText;

    private int index;

    private void Start()
    {
        hideBtn.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayBtnMusic();
            Hide();
        });
        leftBtn.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayBtnMusic();
            SwitchPanel(-1);
        });
        rightBtn.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayBtnMusic();
            SwitchPanel(1);
        });
    }
    public override void Refresh(object data = null)
    {
        base.Refresh(data);

        index = 0;
        RefreshUI();
    }

    private void SwitchPanel(int _lorR)
    {
        index += _lorR;
        if (index < 0)
        {
            index = 5;
        }
        else if (index > 5)
        {
            index = 0;
        }
        RefreshUI();
    }

    private void RefreshUI()
    {
        neirong.sprite = sPs[index];
        ex.text = LanguageManager.Instance.GetText("GameTipsPanel_" + (index + 1));
        xuanzeText.text = $"{index + 1}/6";
    }

    public override void Hide()
    {
        base.Hide();
    }
}
