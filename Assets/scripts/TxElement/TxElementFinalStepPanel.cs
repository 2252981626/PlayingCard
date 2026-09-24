using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TxElementFinalStepPanel : UIBase
{
    public Button hideBtn;
    public Text explain;
    public Text countText;
    public Text explain2;

    public Text goldText;

    public GameObject z1;
    public GameObject z1_have;
    public GameObject z2;
    public GameObject z2_have;
    public GameObject z3;
    public GameObject z3_have;
    public GameObject z4;
    public GameObject z4_have;
    public GameObject z5;
    public GameObject z5_have;
    // Start is called before the first frame update
    void Start()
    {
        hideBtn.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayBtnMusic();
            Hide();
        });
    }
    public override void Refresh(object data = null)
    {
        base.Refresh(data);
        OtherSdkManager.Instance.CustomEvent("withdraw_two_show", "show", "");

        explain.text = LanguageManager.Instance.GetText("FinalStep_explain");
        explain2.text = string.Format(LanguageManager.Instance.GetText("FinalStep_explain2"),LanguageManager.Instance.GetText_Encrypt("CH"));

        string str4 = LanguageManager.Instance.GetText_Encrypt("Special_Diamond__unit");
        goldText.text = str4 + GameManager.Instance.playerInfo.gold;

        TxElementTaskInfo _info = TxElementMananger.Instance.info.taskInfo;

        z1.gameObject.SetActive(!_info.isHave_1);
        z1_have.gameObject.SetActive(_info.isHave_1);

        z2.gameObject.SetActive(!_info.isHave_2);
        z2_have.gameObject.SetActive(_info.isHave_2);

        z3.gameObject.SetActive(!_info.isHave_3);
        z3_have.gameObject.SetActive(_info.isHave_3);

        z4.gameObject.SetActive(!_info.isHave_4);
        z4_have.gameObject.SetActive(_info.isHave_4);

        z5.gameObject.SetActive(!_info.isHave_5);
        z5_have.gameObject.SetActive(_info.isHave_5);
    }
    public override void Hide()
    {
        base.Hide();
    }
}
