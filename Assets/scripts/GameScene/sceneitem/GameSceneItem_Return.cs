using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSceneItem_Return : GameSceneItemBase,IEventListener
{
    private void OnEnable()
    {
        EventManager.Instance.RegisterListener(GameEvent.Update_ItemReturnInfo, this);
    }
    private void OnDisable()
    {
        EventManager.Instance.UnregisterListener(GameEvent.Update_ItemReturnInfo, this);
    }
    public override void Refresh()
    {
        base.Refresh();
        cnt = GameManager.Instance.playerInfo.gameSceneItem_Return;
        type = SceneItemType.item_Return;
        lockLv = 1;

        bool isLock = GameManager.Instance.playerInfo.level < lockLv;

        unLockTrans.gameObject.SetActive(!isLock);
        lockTrans.gameObject.SetActive(isLock);

        cntStr.text = cnt <= 0 ? "+" : GameManager.Instance.playerInfo.gameSceneItem_Return.ToString();
        Update_ItemReturnInfo();
    }

    private void Update_ItemReturnInfo()
    {
        clickBtn.interactable = GameStepRecord.Instance.steps.Count > 0;
    }

    public override void OnClick()
    {
        base.OnClick();
        if (cnt <= 0)
        {
            UIManager.Instance.OpenUI<AddSceneItemPanel>(this);
            return;
        }

        EventManager.Instance.TriggerEvent(GameEvent.StopHintAnim);
        bool isUseItemSucceed = GameStepRecord.Instance.ReturnSteps();
        if (isUseItemSucceed)
        {
            OtherSdkManager.Instance.CustomEvent("prop_use", "level_id", GameManager.Instance.playerInfo.level, "prop_id_number", "4");
            UIManager.Instance.GetUI<GameScenePanel>().ReportLevelStep("item_4");
            GameManager.Instance.playerInfo.Minus_item_return(1);
            //GameManager.Instance.SavePlayerInfo();
            Refresh();
        }
    }

    public void OnEventTriggered(GameEvent eventType, object data = null)
    {
        if(eventType == GameEvent.Update_ItemReturnInfo)
        {
            Update_ItemReturnInfo();
        }
    }
}
