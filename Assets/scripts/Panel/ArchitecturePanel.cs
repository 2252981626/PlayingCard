using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArchitecturePanel : UIBase
{
    public Image title1;
    public Image title2;

    public Image goldImg;
    public Image icon;
    public Text iconname;
    public Text lv;
    public Text goldText;

    public Button btn;

    public List<Sprite> sprites;
    public List<Vector3> vector3s;
    // Start is called before the first frame update
    void Start()
    {
        btn.onClick.AddListener(() =>
        {
            string firstGame = PlayerPrefs.GetString("GuidePanel_Architecture1", "");
            if (string.IsNullOrEmpty(firstGame))
            {
                PlayerPrefs.SetString("GuidePanel_Architecture1", "yes");
                callback = () =>
                {
                    UIManager.Instance.GetUI<GameScenePanel>().TrackLevelExit("settle_back_lobby");
                    UIManager.Instance.GetUI<GameScenePanel>().Hide();
                    UIManager.Instance.OpenUI<LobbyScenePanel>(1);
                    AudioManager.Instance.PlayBGM("BGM1");

                    DOTween.Sequence().AppendInterval(0.3f).AppendCallback(() =>
                    {
                        UIManager.Instance.OpenUI<GuidePanel_Architecture2>();
                    });
                };
            }

            AudioManager.Instance.PlayBtnMusic();
            Hide();
        });
        GameManager.Instance.UpdateAppATTToDiamond(goldImg);
    }
    public override void Refresh(object data = null)
    {
        base.Refresh(data);
        int curLv = (int)data;
        ArchitectureQueryResult architectureQueryResult = GameManager.Instance.architectureConfig.QueryFirstByLevel(curLv);
        int index = architectureQueryResult.LevelIndex;
        switch (index)
        {
            case 0:
                icon.transform.localScale = Vector3.one * 0.8f;
                break;
            case 6:
                icon.transform.localScale = Vector3.one * 0.8f;
                break;
            default:
                icon.transform.localScale = Vector3.one;
                break;
        }
        icon.sprite = sprites[index];
        icon.SetNativeSize();
        icon.transform.localPosition = vector3s[index];
        iconname.text = LanguageManager.Instance.GetText(architectureQueryResult.Name);

        title1.gameObject.SetActive(architectureQueryResult.IsFirstUnlock);
        title2.gameObject.SetActive(!architectureQueryResult.IsFirstUnlock);

        lv.text = "Lv." + (architectureQueryResult.architectureLv + 1).ToString();

        goldText.text = (architectureQueryResult.GoldPerMinute / 100f).ToString() + "/s";

        string firstGame = PlayerPrefs.GetString("GuidePanel_Architecture1", "");
        if (string.IsNullOrEmpty(firstGame))
        {
            DOTween.Sequence().AppendInterval(0.5f).AppendCallback(() =>
            {
                UIManager.Instance.OpenUI<GuidePanel_Architecture1>();
            });
        }
    }
    public override void Hide()
    {
        base.Hide();
    }

}
