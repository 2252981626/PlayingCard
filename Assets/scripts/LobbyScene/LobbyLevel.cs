using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyLevel : MonoBehaviour
{
    public Transform unlock;
    public Transform curLv;
    public Transform lockLv;
    public Transform tips;

    public Text t1;
    public Text t2;
    public Text t3;
    public Text tipstext;
    public void Init(int _lv)
    {
        int curlv = GameManager.Instance.playerInfo.level;

        t1.text = _lv.ToString();
        t2.text = _lv.ToString();
        t3.text = _lv.ToString();

        unlock.gameObject.SetActive(curlv > _lv);
        curLv.gameObject.SetActive(curlv == _lv);
        lockLv.gameObject.SetActive(curlv < _lv);

      
        ArchitectureQueryResult architectureQueryResult = GameManager.Instance.architectureConfig.QueryFirstByLevel(_lv);
        if(architectureQueryResult == null)
        {
            tips.gameObject.SetActive(false);
        }
        else
        {
            tips.gameObject.SetActive(true);
            string str = "";
            if(architectureQueryResult.IsFirstUnlock)
            {
                str = LanguageManager.Instance.GetText("UnlockArchitecture");
            }
            else
            {
                str = LanguageManager.Instance.GetText("UpgradeArchitecture");
            }
            string name = LanguageManager.Instance.GetText(architectureQueryResult.Name);
            tipstext.text = string.Format(str, name);
            if (_lv % 2 == 0)
            {
                // Å¼Êý£¨Ë«£©
                tipstext.transform.localPosition = new Vector3(-18f, 0f, 0f);
            }
            else
            {
                // ÆæÊý£¨µ¥£©
                tipstext.transform.localPosition = new Vector3(15f, 0f, 0f);
            }
        }
    }


}
