using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuidePanel_Architecture1 : UIBase
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
            OtherSdkManager.Instance.CustomEvent("newbie_guide_7_complete", "step", "7");
            UIManager.Instance.GetUI<ArchitecturePanel>().btn.onClick.Invoke();
            Hide();
        });
    }


    public override void Refresh(object data = null)
    {
        base.Refresh(data);

        ArchitecturePanel architecturePanel = UIManager.Instance.GetUI<ArchitecturePanel>();
        mask.position = architecturePanel.btn.transform.position;
        trans.position = architecturePanel.btn.transform.position;

        // var pos = trans.localPosition;
        //str.transform.parent.transform.localPosition = pos + new Vector3(0, 300f, 0);
        //str.text = string.Format(LanguageManager.Instance.GetText("Guide_TxPanel2"), LanguageManager.Instance.GetText_Encrypt("CHT"));

    }
    public override void Hide()
    {
        base.Hide();
    }
}
