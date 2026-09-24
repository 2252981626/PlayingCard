using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class TxElementGameStartPanel : UIBase
{
    public Text t1;
    public Text t2;
    public Text t3;
    public Button btn;

    private string Wh = "";
    private string CH;
    private void Start()
    {
        btn.onClick.AddListener(() =>
        {
            Hide();
        });
    }
    public override void Refresh(object data = null)
    {
        base.Refresh(data);
        if(string.IsNullOrEmpty(Wh))
        {
            Wh = LanguageManager.Instance.GetText_Encrypt("Wh");
            CH = LanguageManager.Instance.GetText_Encrypt("CH");
        }

        t1.text = LanguageManager.Instance.GetText("TxElementGameStartPanel_t1");
        t2.text = string.Format(LanguageManager.Instance.GetText("TxElementGameStartPanel_t2"), Wh, CH);
        float temV = TxElementMananger.Instance.GetProbByLevel(GameManager.Instance.playerInfo.level);
        t3.text = string.Format(LanguageManager.Instance.GetText("TxElementGameStartPanel_t3"), $"{temV / 100f}%");

        DOTween.Sequence().AppendInterval(5f).AppendCallback(() =>
        {
            Hide();
        })
        .SetTarget(transform);
    }

    public override void Hide()
    {
        transform.DOKill();
        base.Hide();
    }
}
