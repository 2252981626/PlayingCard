using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum ItemType
{
    Gold,
    GoldDui,
    Diamond,
    DiamondDui,
    Hint,// 提示
    Exchange,// 洗牌
    Extract,// 魔法棒
    Return,// 撤回
    Z1,
    Z2,
    Z3,
    Z4,
    Z5,
}

public class ItemData
{
    public ItemType itemType;
    public float count;
    public ItemData(ItemType _itemType, float _count)
    {
        itemType = _itemType;
        count = _count;
    }
}
public class ItemBase : MonoBehaviour
{
    public List<Sprite> icons;
    public List<float> iconScales;
    public Image icon;
    public Text cntText;
    public Transform effect;

    [HideInInspector] public ItemType itemType;
    [HideInInspector] public float count;

    public virtual void Init(ItemData _itemData)
    {
        itemType = _itemData.itemType;
        count = _itemData.count;

        icon.sprite = icons[(int)itemType];
        icon.SetNativeSize();
        icon.transform.localScale = Vector3.one * iconScales[(int)itemType];

        string unit = "";
        if(itemType == ItemType.Hint || itemType == ItemType.Exchange || itemType == ItemType.Extract || itemType == ItemType.Return)
        {
            unit = "x";
        }
        else if (itemType == ItemType.Gold || itemType == ItemType.GoldDui)
        {
            unit = LanguageManager.Instance.GetText_Encrypt("Special_Diamond__unit");
        }

        cntText.text = unit + count.ToString();

        if(itemType == ItemType.Gold)
        {
            GameManager.Instance.UpdateAppATTToDiamond(icon);
        }
        else if(itemType == ItemType.GoldDui)
        {
            GameManager.Instance.UpdateAppATTToDiamondDui(icon);
            if (GameManager.appATTtype == 1)
            {
                icon.transform.localScale = Vector3.one * 0.5f;
            }
        }
    }

    public  void GetItemReward()
    {
        switch (itemType)
        {
            case ItemType.Gold:
                GameManager.Instance.playerInfo.Add_gold(count);
                break;
            case ItemType.GoldDui:
                GameManager.Instance.playerInfo.Add_gold(count);
                break;
            case ItemType.Diamond:
                GameManager.Instance.playerInfo.Add_diamond(count);
                break;
            case ItemType.DiamondDui:
                GameManager.Instance.playerInfo.Add_diamond(count);
                break;
            case ItemType.Hint:
                GameManager.Instance.playerInfo.Add_item_hint((int)count);
                break;
            case ItemType.Exchange:
                GameManager.Instance.playerInfo.Add_item_exchange((int)count);
                break;
            case ItemType.Extract:
                GameManager.Instance.playerInfo.Add_item_extract((int)count);
                break;
            case ItemType.Return:
                GameManager.Instance.playerInfo.Add_item_return((int)count);
                break;
            case ItemType.Z1:
                AddZIMu();
                break;
            case ItemType.Z2:
                AddZIMu();
                break;
            case ItemType.Z3:
                AddZIMu();
                break;
            case ItemType.Z4:
                AddZIMu();
                break;
            case ItemType.Z5:
                AddZIMu();
                break;
        }


    }

    public void PlayItemAnim() 
    {
        switch (itemType)
        {
            case ItemType.Gold:
                UIManager.Instance.GetUI<PlayerInfoUI>().GoldFlyAnim(transform.position);
                break;
            case ItemType.GoldDui:
                UIManager.Instance.GetUI<PlayerInfoUI>().GoldFlyAnim(transform.position);
                break;
            case ItemType.Diamond:
                UIManager.Instance.GetUI<PlayerInfoUI>().DiamondFlyAnim(transform.position);
                break;
            case ItemType.DiamondDui:
                UIManager.Instance.GetUI<PlayerInfoUI>().DiamondFlyAnim(transform.position);
                break;
            case ItemType.Hint:
                UIManager.Instance.GetUI<GameScenePanel>()?.gameSceneItem_Hint.Refresh();
                break;
            case ItemType.Exchange:
                UIManager.Instance.GetUI<GameScenePanel>()?.gameSceneItem_Exchange.Refresh();
                break;
            case ItemType.Extract:
                UIManager.Instance.GetUI<GameScenePanel>()?.gameSceneItem_Extract.Refresh();
                break;
            case ItemType.Return:
                UIManager.Instance.GetUI<GameScenePanel>()?.gameSceneItem_Return.Refresh();
                break;
        }
    }
    
    public void AddZIMu()
    {
        if(TxElementMananger.Instance == null)
        {
            return;
        }
        switch (itemType)
        {
            case ItemType.Z1:
                if(!TxElementMananger.Instance.info.taskInfo.isHave_1)
                {
                    TxElementMananger.Instance.info.taskInfo.isHave_1 = true;
                }
                else
                {
                    GameManager.Instance.playerInfo.Add_gold(1);
                    UIManager.Instance.GetUI<PlayerInfoUI>().GoldFlyAnim(transform.position);
                }
                break;
            case ItemType.Z2:
                if (!TxElementMananger.Instance.info.taskInfo.isHave_2)
                {
                    TxElementMananger.Instance.info.taskInfo.isHave_2 = true;
                }
                else
                {
                    GameManager.Instance.playerInfo.Add_gold(1);
                    UIManager.Instance.GetUI<PlayerInfoUI>().GoldFlyAnim(transform.position);
                }
                break;
            case ItemType.Z3:
                if (!TxElementMananger.Instance.info.taskInfo.isHave_3)
                {
                    TxElementMananger.Instance.info.taskInfo.isHave_3 = true;
                }
                else
                {
                    GameManager.Instance.playerInfo.Add_gold(1);
                    UIManager.Instance.GetUI<PlayerInfoUI>().GoldFlyAnim(transform.position);
                }
                break;
            case ItemType.Z4:
                if (!TxElementMananger.Instance.info.taskInfo.isHave_4)
                {
                    TxElementMananger.Instance.info.taskInfo.isHave_4 = true;
                }
                else
                {
                    GameManager.Instance.playerInfo.Add_gold(1);
                    UIManager.Instance.GetUI<PlayerInfoUI>().GoldFlyAnim(transform.position);
                }
                break;
            case ItemType.Z5:
                if (!TxElementMananger.Instance.info.taskInfo.isHave_5)
                {
                    TxElementMananger.Instance.info.taskInfo.isHave_5 = true;
                }
                else
                {
                    GameManager.Instance.playerInfo.Add_gold(1);
                    UIManager.Instance.GetUI<PlayerInfoUI>().GoldFlyAnim(transform.position);
                }
                break;
        }
        TxElementMananger.Instance.SaveElementManangerInfo();
    }
}
