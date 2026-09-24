using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TxElementBtn : MonoBehaviour
{
    public Button clickBtn;
    public Text text;
    // Start is called before the first frame update
    void Start()
    {
        clickBtn.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayBtnMusic();
            UIManager.Instance.OpenUI<TxElementPanel>();
        });

        text.text = LanguageManager.Instance.GetText_Encrypt("CHT");
    }
}
