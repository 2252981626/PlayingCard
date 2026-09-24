using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

public class TxElementMananger : MonoBehaviour
{
    public static TxElementMananger Instance;

    public GameObject TxProgressPrefab;
    public TxElementManangerInfo info;
    public List<Sprite> accountTypeSprites;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        InitElementManangerInfo();
    }

    public int GetJIndu()
    {
        int lv = GameManager.Instance.playerInfo.level - 1;
        if (lv < 1) return 0;

        int total = 0;

        // ========== 1~10级 ==========
        if (lv >= 1) total += GetProbByLevel(1);   // 1级
        if (lv >= 2) total += GetProbByLevel(2);   // 2级
        if (lv >= 3) total += GetProbByLevel(3);   // 3级
        if (lv >= 4) total += GetProbByLevel(4);   // 4级
        if (lv >= 5) total += GetProbByLevel(5);   // 5级
        if (lv >= 6) total += GetProbByLevel(6);   // 6级
        if (lv >= 7) total += GetProbByLevel(7);   // 7级
        if (lv >= 8) total += GetProbByLevel(8);  // 8级
        if (lv >= 9) total += GetProbByLevel(9);   // 9级
        if (lv >= 10) total += GetProbByLevel(10); // 10级

        // ========== 11级以后 ==========
        if (lv >= 11) total += Mathf.Min(lv - 10, 10) * GetProbByLevel(11); // 11~20级
        if (lv >= 21) total += Mathf.Min(lv - 20, 10) * GetProbByLevel(21); // 21~30级
        if (lv >= 31) total += Mathf.Min(lv - 30, 10) * GetProbByLevel(31); // 31~40级
        if (lv >= 41) total += Mathf.Min(lv - 40, 10) * GetProbByLevel(41); // 41~50级

        return total;
    }
    public int GetProbByLevel(int level)
    {
        if (level <= 0) return 0;

        // 1~10级
        if (level == 1) return 3000;
        if (level == 2) return 2500;
        if (level == 3) return 2000;
        if (level == 4) return 1000;
        if (level == 5) return 600;
        if (level == 6) return 300;
        if (level == 7) return 200;
        if (level == 8) return 150;
        if (level == 9) return 100;
        if (level == 10) return 50;

        // 11级以后
        if (level <= 20) return 4;
        if (level <= 30) return 3;
        if (level <= 40) return 2;
        return 1; // 41级以后
    }

    private void InitElementManangerInfo()
    {
        string jsonStr = PlayerPrefs.GetString("TxElementManangerInfo", "");
        if (string.IsNullOrEmpty(jsonStr))
        {
            info = new TxElementManangerInfo();
            info.accountInfo = new TxElementAccountInfo();
            info.taskInfo = new TxElementTaskInfo();
        }
        else
        {
            info = JsonConvert.DeserializeObject<TxElementManangerInfo>(jsonStr);
        }
    }

    public void SaveElementManangerInfo()
    {
        string jsonStr = JsonConvert.SerializeObject(info, Formatting.Indented);
        PlayerPrefs.SetString("TxElementManangerInfo", jsonStr);
        Debug.Log("TxElementManangerInfo :" + jsonStr);
    }
}

[Serializable]
public class TxElementManangerInfo
{
    public TxElementAccountInfo accountInfo;
    public TxElementTaskInfo taskInfo;
}

[Serializable]
public class TxElementAccountInfo
{
    public TxElementAccountType type;
    public string email;
    public TxElementAccountInfo()
    {
        type = TxElementAccountType.type1;
        email = "";
    }
}

[Serializable]
public class TxElementTaskInfo
{
    public bool isHave_1;
    public bool isHave_2;
    public bool isHave_3;
    public bool isHave_4;
    public bool isHave_5;

    public TxElementTaskInfo()
    {
        Init();
    }
    public void Init()
    {
        isHave_1 = false;
        isHave_2 = false;
        isHave_3 = false;
        isHave_4 = false;
        isHave_5 = false;
    }
}

public enum TxElementAccountType
{
    type1,
    type2,
    type3,
    type4,
    type5,
    type6,
    type7,
    type8
}