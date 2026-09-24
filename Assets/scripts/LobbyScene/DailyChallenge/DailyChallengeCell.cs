using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DailyChallengeCell : MonoBehaviour
{
    public Button btn;
    public Text str;
    public Text todaystr;
    public GameObject last;
    public GameObject today;
    public GameObject signIn;
    private DailyChallengePanel dailyChallengePanel;
    private int thisDay;

    private void Start()
    {
        btn.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayBtnMusic();
            Select(true);
        });
    }
    public void Init(int day, DailyChallengePanel panel)
    {
        str.text = day.ToString();
        todaystr.text = day.ToString();
        str.gameObject.SetActive(true);
        todaystr.gameObject.SetActive(false);
        last.gameObject.SetActive(false);
        today.gameObject.SetActive(false);
        signIn.gameObject.SetActive(false);
        signIn.GetComponent<Image>().color = new Color(1, 1, 1, 1);

        thisDay = day;
        dailyChallengePanel = panel;
    }

    public void SignIn(bool _isPlayAnim)
    {
        signIn.gameObject.SetActive(true);

        if (_isPlayAnim)
        {
            Select(true);
            DOTween.Sequence()
                .Append(signIn.GetComponent<Image>().DOFade(0.5f, 0.5f))
                .Append(signIn.GetComponent<Image>().DOFade(1f, 0.5f))
                .SetLoops(5);
        }
    }

    public void LastDay()
    {
        last.gameObject.SetActive(true);
    }
    public void Select(bool isSelect)
    {
        if(isSelect)
        {
            dailyChallengePanel.curSelectDay = thisDay;
            if(dailyChallengePanel.lastCell != null)
            {
                dailyChallengePanel.lastCell.Select(false);
            }
            dailyChallengePanel.lastCell = this;
            dailyChallengePanel.UpdatePlayBtnState();
        }
        today.gameObject.SetActive(isSelect);
        str.gameObject.SetActive(!isSelect);
        todaystr.gameObject.SetActive(isSelect);
    }
}
