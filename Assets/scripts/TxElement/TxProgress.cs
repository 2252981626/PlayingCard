using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TxProgress : MonoBehaviour,IEventListener
{
    public Button btn;
    public Text btnText;
    public Text tipsText;
    public Text progressText;

    public Slider Slider;

    private void OnEnable()
    {
        EventManager.Instance.RegisterListener(GameEvent.UpdateTxProgress, this);
    }
    private void OnDisable()
    {
        EventManager.Instance.UnregisterListener(GameEvent.UpdateTxProgress, this);
    }
    // Start is called before the first frame update
    void Start()
    {
        btn.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayBtnMusic();
            UIManager.Instance.OpenUI<TxElementPanel>();
        });
        if (LanguageManager.Instance.type == MultilingualType.Portuguese)
        {
            btnText.text = LanguageManager.Instance.GetText_Encrypt("CHT");
        }
        else
        {
            btnText.text = LanguageManager.Instance.GetText_Encrypt("WD");
        }
        tipsText.text = string.Format(LanguageManager.Instance.GetText("TxProgress_Tips"), LanguageManager.Instance.GetText_Encrypt("wd"));
        RefreshProgressUI();
    }

    public void RefreshProgressUI()
    {
        float curV = TxElementMananger.Instance.GetJIndu();
        Slider.value = curV / 10000f;
        progressText.text = $"{curV / 100f}%";
    }

    public void OnEventTriggered(GameEvent eventType, object data = null)
    {
        if(eventType == GameEvent.UpdateTxProgress)
        {
            RefreshProgressUI();
        }
    }
}
