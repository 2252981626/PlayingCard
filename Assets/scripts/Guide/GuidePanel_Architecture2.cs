using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuidePanel_Architecture2 : UIBase
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
            OtherSdkManager.Instance.CustomEvent("newbie_guide_8_complete", "step", "8");

            UIManager.Instance.GetUI<LobbyScenePanel>().ShowShouzhi();
            Hide();
        });
        tagetBtn.onClick.AddListener(() =>
        {
    
        });
    }


    public override void Refresh(object data = null)
    {
        base.Refresh(data);

        LobbyHomePanel lobbyHomePanel = UIManager.Instance.GetUI<LobbyScenePanel>().lobbyHomePanel;
        mask.position = lobbyHomePanel.architectureCells[3].transform.position;
        trans.position = lobbyHomePanel.architectureCells[3].transform.position;

        var pos = trans.localPosition;
        str.transform.parent.transform.localPosition = pos - new Vector3(0, 550f, 0);
        str.text = string.Format(LanguageManager.Instance.GetText("GuidePanel_Architecture2"), LanguageManager.Instance.GetText_Encrypt("CH"));

    }
    public override void Hide()
    {
        base.Hide();
    }
}
