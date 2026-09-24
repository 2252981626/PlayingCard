using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LobbyLevelChildPanel : MonoBehaviour
{
    public Transform bg;
    public List<LobbyLevel> lobbyLevels;

    public LobbyLevel bu1;
    public LobbyLevel bu2;

    // Start is called before the first frame update
    public void Init(int panelinedx)
    {
        int curlv = GameManager.Instance.playerInfo.level;
        bool isThis = curlv <= 6;
        if (panelinedx == 0)
        {
            for (int i = 0; i < lobbyLevels.Count; i++)
            {
                lobbyLevels[i].Init(i + 1);
            }
            if(curlv >= 5)
            {
                bg.transform.localPosition = new Vector3(0f, -1372f, 0f);
            }
            else
            {
                bg.transform.localPosition = new Vector3(0f, -777f, 0f);
            }
        }
        else if (panelinedx == 1)
        {
            int offset = curlv - 6;
            int page = (offset - 1) / 8;
            page = Mathf.Max(page, 0);
            int startLv = page * 8  + 6 + 1;

            bu1.Init(startLv - 1);
            bu2.Init(startLv - 2);
            int shipeiIndex = 0;
            for (int i = 0; i < lobbyLevels.Count; i++)
            {
                lobbyLevels[i].Init(startLv);
                if (curlv == startLv)
                {
                    shipeiIndex = i;
                }
                 startLv++;
            }
            if (shipeiIndex >= 2 && shipeiIndex < 5)
            {
                bg.transform.localPosition = new Vector3(0f, -1100f, 0f);
            }
            else if (shipeiIndex >= 5)
            {
                bg.transform.localPosition = new Vector3(0f, -2135f, 0f);
            }
            else
            {
                bg.transform.localPosition = new Vector3(0f, -72f, 0f);
            }
        }
    }

}
