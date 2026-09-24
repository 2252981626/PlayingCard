using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuidePanel_TxBtn : UIBase
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
            OtherSdkManager.Instance.CustomEvent("newbie_guide_2_complete", "step", "2");

            PlayerPrefs.SetString("Guide_TxBtn", "yes");
            AudioManager.Instance.PlayBtnMusic();
            UIManager.Instance.OpenUI<TxElementPanel>();
            Hide();
        });
    }


    public override void Refresh(object data = null)
    {
        base.Refresh(data);

        mask.position = UIManager.Instance.GetUI<PlayerInfoUI>().txTrans.transform.position;
        trans.position = UIManager.Instance.GetUI<PlayerInfoUI>().txTrans.transform.position;

        var pos = trans.localPosition;
        str.transform.parent.transform.localPosition = pos - new Vector3(0, 350f, 0);
        str.text = string.Format(LanguageManager.Instance.GetText("Guide_TxBtn"), LanguageManager.Instance.GetText_Encrypt("CH"));

    }
    public override void Hide()
    {
        base.Hide();
    }
}
