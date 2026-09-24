using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ArchitectureInfo
{
    public string Name;
    public List<int> LockLevels;
    public List<int> Golds;
    public List<int> MinutesList;
}

[Serializable]
public class ArchitectureConfig
{
    public List<ArchitectureInfo> Architectures;

    public ArchitectureConfig()
    {
        Architectures = new List<ArchitectureInfo>
    {
        // government building 政府大楼
        new ArchitectureInfo
        {
            Name = "Architecture1",
            LockLevels = new List<int> { 1, 50, 99, 139, 179 },
            Golds = new List<int> { 1, 2, 2, 3, 4 },
            MinutesList = new List<int> { 600, 900, 1200, 1800, 2700 }
        },
        // flower shop 花店
        new ArchitectureInfo
        {
            Name = "Architecture2",
            LockLevels = new List<int> { 5, 55, 103, 143, 183 },
            Golds = new List<int> { 1, 2, 2, 3, 4 },
            MinutesList = new List<int> { 600, 900, 1200, 1800, 2700 }
        },
        // police station 警局
        new ArchitectureInfo
        {
            Name = "Architecture3",
            LockLevels = new List<int> { 10, 60, 107, 147, 187 },
            Golds = new List<int> { 1, 2, 2, 3, 4 },
            MinutesList = new List<int> { 600, 900, 1200, 1800, 2700 }
        },
        // restaurant 饭店
        new ArchitectureInfo
        {
            Name = "Architecture4",
            LockLevels = new List<int> { 15, 65, 111, 151, 191 },
            Golds = new List<int> { 1, 2, 2, 3, 4 },
            MinutesList = new List<int> { 600, 900, 1200, 1800, 2700 }
        },
        // bank 银行
        new ArchitectureInfo
        {
            Name = "Architecture5",
            LockLevels = new List<int> { 20, 70, 115, 155, 195 },
            Golds = new List<int> { 2, 3, 3, 4, 5 },
            MinutesList = new List<int> { 600, 900, 1200, 1800, 2700 }
        },
        // shopping mall 商城
        new ArchitectureInfo
        {
            Name = "Architecture6",
            LockLevels = new List<int> { 25, 75, 119, 159, 199 },
            Golds = new List<int> { 2, 3, 3, 4, 5 },
            MinutesList = new List<int> { 600, 900, 1200, 1800, 2700 }
        },
        // hospital 医院
        new ArchitectureInfo
        {
            Name = "Architecture7",
            LockLevels = new List<int> { 30, 80, 123, 163, 203 },
            Golds = new List<int> { 2, 3, 3, 4, 5 },
            MinutesList = new List<int> { 600, 900, 1200, 1800, 2700 }
        },
        // school 学校
        new ArchitectureInfo
        {
            Name = "Architecture8",
            LockLevels = new List<int> { 35, 85, 127, 167, 207 },
            Golds = new List<int> { 2, 3, 3, 4, 5 },
            MinutesList = new List<int> { 600, 900, 1200, 1800, 2700 }
        },
        // iron gym 健身铁馆
        new ArchitectureInfo
        {
            Name = "Architecture9",
            LockLevels = new List<int> { 40, 90, 131, 171, 211 },
            Golds = new List<int> { 2, 3, 3, 4, 5 },
            MinutesList = new List<int> { 600, 900, 1200, 1800, 2700 }
        },
        // amusement park 游乐园
        new ArchitectureInfo
        {
            Name = "Architecture10",
            LockLevels = new List<int> { 45, 95, 135, 175, 215 },
            Golds = new List<int> { 2, 3, 3, 4, 5 },
            MinutesList = new List<int> { 600, 900, 1200, 1800, 2700 }
        }
    };
    }

    /// <summary>
    /// 根据建筑索引获取配置
    /// </summary>
    public ArchitectureInfo GetInfoByIndex(int index)
    {
        if (index >= 0 && index < Architectures.Count)
        {
            return Architectures[index];
        }
        return null;
    }

    /// <summary>
    /// 根据建筑索引获取名称
    /// </summary>
    public string GetNameByIndex(int index)
    {
        var info = GetInfoByIndex(index);
        return info != null ? info.Name : string.Empty;
    }

    /// <summary>
    /// 获取建筑总数量
    /// </summary>
    public int GetArchitectureCount()
    {
        return Architectures.Count;
    }

    /// <summary>
    /// 根据建筑索引和当前等级获取产出速度
    /// </summary>
    public int GetGoldByArchIndex(int archIndex)
    {
        var info = GetInfoByIndex(archIndex);
        if (info == null) return 0;

        int levelIndex = GetCurrentArchLevel(archIndex);
        if (levelIndex >= 0 && levelIndex < info.Golds.Count)
        {
            return info.Golds[levelIndex];
        }

        return 0;
    }

    /// <summary>
    /// 根据建筑索引和当前等级获取产出时间上限
    /// </summary>
    public int GetMinutesByArchIndex(int archIndex)
    {
        var info = GetInfoByIndex(archIndex);
        if (info == null) return 0;

        int levelIndex = GetCurrentArchLevel(archIndex);
        if (levelIndex >= 0 && levelIndex < info.MinutesList.Count)
        {
            return info.MinutesList[levelIndex];
        }
        return 0;
    }

    /// <summary>
    /// 根据建筑索引获取解锁等级列表
    /// </summary>
    public List<int> GetLockLevelsByArchIndex(int archIndex)
    {
        var info = GetInfoByIndex(archIndex);
        return info != null ? info.LockLevels : null;
    }

    /// <summary>
    /// 检查建筑是否已解锁
    /// </summary>
    public bool IsArchitectureUnlocked(int archIndex)
    {
        var info = GetInfoByIndex(archIndex);
        if (info == null) return false;

        int currentLevel = GameManager.Instance.playerInfo.level;
        return currentLevel > info.LockLevels[0];
    }

    /// <summary>
    /// 获取建筑当前等级（0-based，-1表示未解锁）
    /// </summary>
    public int GetCurrentArchLevel(int archIndex)
    {
        var info = GetInfoByIndex(archIndex);
        if (info == null) return -1;

        int currentLevel = GameManager.Instance.playerInfo.level;
        int levelIndex = -1;

        for (int i = 0; i < info.LockLevels.Count; i++)
        {
            if (currentLevel > info.LockLevels[i])
            {
                levelIndex = i;
            }
        }
        return levelIndex;
    }

    /// <summary>
    /// 根据传入等级，获取第一个匹配的建筑（适用于只有一个建筑在该等级解锁的场景）
    /// </summary>
    /// <param name="level">要查询的等级</param>
    /// <returns>匹配的建筑信息，如果没有返回 null</returns>
    public ArchitectureQueryResult QueryFirstByLevel(int level)
    {
        for (int i = 0; i < Architectures.Count; i++)
        {
            var info = Architectures[i];

            int levelIndex = info.LockLevels.IndexOf(level);
            if (levelIndex >= 0)
            {
                return new ArchitectureQueryResult
                {
                    Name = info.Name,
                    IsFirstUnlock = (levelIndex == 0),
                    GoldPerMinute = info.Golds[levelIndex],
                    MaxMinutes = info.MinutesList[levelIndex],
                    LevelIndex = i,
                    architectureLv = levelIndex
                };
            }
        }

        return null;
    }
}

/// <summary>
/// 根据等级查询到的建筑信息
/// </summary>
public class ArchitectureQueryResult
{
    public string Name;              // 建筑名称
    public bool IsFirstUnlock;       // 是否为第一个解锁等级
    public int GoldPerMinute;        // 产出速度（金币/分钟）
    public int MaxMinutes;           // 产出时间上限（分钟）
    public int LevelIndex;           // 在配置中的索引位置
    public int architectureLv;       //建筑等级
}
