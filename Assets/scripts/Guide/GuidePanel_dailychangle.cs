using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuidePanel_dailychangle : UIBase
{
    public Button maskBtn;
    public Button tagetBtn;
    public Text str;

    public Transform mask;
    public Transform trans;

    void Start()
    {
        maskBtn.onClick.AddListener(() =>
        {
        });
        tagetBtn.onClick.AddListener(() =>
        {
            OtherSdkManager.Instance.CustomEvent("newbie_guide_10_complete", "step", "10");

            PlayerPrefs.SetString("GuidePanel_dailychangle", "yes");
            UIManager.Instance.GetUI<LobbyScenePanel>().dailyBtn.onClick.Invoke();
            Hide();
        });
    }


    public override void Refresh(object data = null)
    {
        base.Refresh(data);

        LobbyScenePanel lobbyScenePanel = UIManager.Instance.GetUI<LobbyScenePanel>();
        mask.position = lobbyScenePanel.dailyBtn.transform.position;
        trans.position = lobbyScenePanel.dailyBtn.transform.position;

        var pos = trans.localPosition;
        pos.x = 0;
        str.transform.parent.transform.localPosition = pos + new Vector3(0, 300f, 0);
        str.text = LanguageManager.Instance.GetText("GuidePanel_dailychangle");

    }
    public override void Hide()
    {
        base.Hide();
    }
}
