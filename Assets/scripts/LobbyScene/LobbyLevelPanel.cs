using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyLevelPanel : MonoBehaviour
{
    public LobbyLevelChildPanel child1;
    public LobbyLevelChildPanel child2;
    public Button btn;
    private void Start()
    {
        btn.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayBtnMusic();
            EnterGame();
        });
    }
    public void EnterGame()
    {
        GameManager.Instance.gameType = GameType.MainGame;
        UIManager.Instance.GetUI<LobbyScenePanel>().Hide();
        UIManager.Instance.OpenUI<GameScenePanel>();
        AudioManager.Instance.PlayBGM("BGM2");
    }
    public void Open()
    {
        gameObject.SetActive(true);

        int curlv = GameManager.Instance.playerInfo.level;
        child1.gameObject.SetActive(curlv <= 6);
        child1.Init(0);
        child2.gameObject.SetActive(curlv > 6);
        child2.Init(1);
    }

    public void hide()
    {
        gameObject.SetActive(false);
    }

  
}
