using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LuckyWheel : MonoBehaviour
{
    public Button clickBtn;

    void Start()
    {
        clickBtn.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayBtnMusic();
            UIManager.Instance.OpenUI<LuckyWheelPanel>();
        });
    }
}
