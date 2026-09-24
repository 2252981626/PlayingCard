using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeadPanel : UIBase
{
    public Button hideBtn;
    public Transform cells;
    public HeadCell HeadCellPrefab;
    private List<HeadCell> headCells;
    [HideInInspector] public HeadCell curSelect;
    private void Start()
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
       
        if(headCells == null)
        {
            headCells = new List<HeadCell>();
            PlayerInfoUI playerInfoUI = UIManager.Instance.GetUI<PlayerInfoUI>();
            for (int i = 0; i < playerInfoUI.heads.Count; i++)
            {
                var obj = Instantiate(HeadCellPrefab, cells);
                HeadCell temcell = obj.GetComponent<HeadCell>();
                temcell.Init(i,1);
                headCells.Add(temcell);
            }
        }
        UpdateCells();
        UpdateDimmondLockState();
    }
    
    public void UpdateCells()
    {
        int newindex = 0;
        foreach (int index in GameManager.Instance.playerInfo.heads)
        {
            headCells[index].transform.SetSiblingIndex(newindex);
            newindex++;
        }
    }  

    public void UpdateDimmondLockState()
    {
        foreach (var item in headCells)
        {
            item.UpdateDimmondLockState();
        }
    }

    public override void Hide()
    {
        callback = () =>
        {
            GameManager.Instance.playerInfo.head = curSelect.index;
            UIManager.Instance.GetUI<PlayerInfoUI>().RefreshHeadUI();
            GameManager.Instance.SavePlayerInfo();
        };
        base.Hide();
    }
}
