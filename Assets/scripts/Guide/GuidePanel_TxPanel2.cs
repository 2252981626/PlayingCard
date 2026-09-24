using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuidePanel_TxPanel2 : UIBase
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
            OtherSdkManager.Instance.CustomEvent("newbie_guide_4_complete", "step", "4");

            PlayerPrefs.SetString("Guide_TxPanel2", "yes");
            UIManager.Instance.GetUI<TxElementPanel>().btn.onClick.Invoke();
            Hide();
        });
    }


    public override void Refresh(object data = null)
    {
        base.Refresh(data);

        TxElementPanel txPanel = UIManager.Instance.GetUI<TxElementPanel>();
        mask.position = txPanel.btn.transform.position;
        trans.position = txPanel.btn.transform.position;

        var pos = trans.localPosition;
        str.transform.parent.transform.localPosition = pos + new Vector3(0, 300f, 0);
        str.text = string.Format(LanguageManager.Instance.GetText("Guide_TxPanel2"), LanguageManager.Instance.GetText_Encrypt("CHT"));

    }
    public override void Hide()
    {
        base.Hide();
    }
}
