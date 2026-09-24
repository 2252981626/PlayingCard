using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuidePanel_firstGame : UIBase
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
            OtherSdkManager.Instance.CustomEvent("newbie_guide_1_complete", "step", "1");

            DailySignInPanel dailySignInPanel = UIManager.Instance.GetUI<DailySignInPanel>();
            dailySignInPanel.dailySignInCells[0].clickBtn.onClick.Invoke();
            Hide();
        });
    }


    public override void Refresh(object data = null)
    {
        base.Refresh(data);


        DailySignInPanel dailySignInPanel = UIManager.Instance.GetUI<DailySignInPanel>();

        mask.position = dailySignInPanel.dailySignInCells[0].transform.position;
        trans.position = dailySignInPanel.dailySignInCells[0].transform.position;
    }
    public override void Hide()
    {
        base.Hide();
    }
}
