using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelData
{
    public int ID { get; set; } // 关卡ID
    public List<string> queues; // 7列牌组合并集合
    public int DifType { get; set; } // 难度类型
}

public class LevelConfigData
{
    public static LevelData GetLevelData(int lv)
    {
        if(GameManager.Instance.gameType == GameType.DailyGame)
        {
            List<LevelData> realLevelsR = configs.Where(x => x.ID != 0).ToList();
            int randomIdxR = Random.Range(0, realLevelsR.Count);
            return realLevelsR[randomIdxR];
        }

        var target = configs.FirstOrDefault(d => d.ID == lv);
        if (target != null)
            return target;

        // 关卡不存在，随机返回一个正式关卡（跳过占位0）
        List<LevelData> realLevels = configs.Where(x => x.ID != 0).ToList();
        int randomIdx = Random.Range(0, realLevels.Count);
        return realLevels[randomIdx];
    }

    public static List<LevelData> configs = new List<LevelData>()
{
    // ==================== ID 0 ====================
    new LevelData() { ID = 0, DifType = 0, queues = new List<string>() { } },

    // ==================== ID 1 ====================
    new LevelData() { ID = 1, DifType = 0, queues = new List<string>() {
        "AD",
        "2D,AC",
        "3D,2C,3H",
        "6H,5S,4D,3C",
        "8D,7S,6D,5H,4H",
        "JD,10D,9D,10C,9C,8C",
        "8S,7D,6S,5C,2H,AS,AH"
    } },

    // ==================== ID 2 ====================
    new LevelData() { ID = 2, DifType = 0, queues = new List<string>() {
        "2S",
        "4S,3S",
        "3C,2C,KH",
        "QS,10H,6C,AS",
        "KD,5D,4H,3D,AC",
        "9D,8D,3H,2D,AD,KS",
        "QD,9H,8C,7H,7C,6S,AH"
    } },

    // ==================== ID 3 ====================
    new LevelData() { ID = 3, DifType = 0, queues = new List<string>() {
        "3C",
        "4H,10S",
        "9C,5S,4S",
        "KD,QC,QS,AH",
        "9S,9H,8H,6C,2D",
        "5H,7D,8S,8C,6H,AD",
        "8D,7S,6D,4C,3D,2C,AS",
    } },

    // ==================== ID 4 ====================
    new LevelData() { ID = 4, DifType = 0, queues = new List<string>() {
        "AC",
        "3H,4S",
        "7C,4H,AH",
        "10C,JH,JS,KD",
        "QS,8H,7H,7S,5C",
        "QC,10D,9H,8D,7D,2S",
        "10S,9C,9D,6D,5D,3D,2H"
    } },

    // ==================== ID 5 ====================
    new LevelData() { ID = 5, DifType = 0, queues = new List<string>() {
        "QH",
        "10C,AD",
        "9C,4C,KC",
        "QD,5D,4D,3H",
        "KH,JH,8S,2D,AC",
        "KS,JS,7D,7H,3S,2H",
        "QC,10D,8C,6D,4H,3D,AH"
    } },

    // ==================== ID 6 ====================
    new LevelData() { ID = 6, DifType = 0, queues = new List<string>() {
        "QS",
        "6C,10S",
        "KH,5D,2H",
        "JS,QD,KD,JD",
        "10D,KC,2D,7H,AC",
        "6H,5C,9C,2C,8C,7C",
        "9S,9H,6S,5S,3S,KS,AH"
    } },

    // ==================== ID 7 ====================
    new LevelData() { ID = 7, DifType = 0, queues = new List<string>() {
        "2H",
        "KH,5S",
        "5D,2D,AD",
        "10C,8S,7H,3H",
        "KD,QC,JD,7D,3C",
        "8H,6C,6D,4S,2C,AS",
        "KC,QS,JS,9H,8D,3D,AC"
    } },

    // ==================== ID 8 ====================
    new LevelData() { ID = 8, DifType = 0, queues = new List<string>() {
        "5C",
        "6H,AD",
        "QS,JS,2D",
        "QD,7C,2C,AS",
        "9S,8H,6C,6D,KH",
        "QC,10C,9C,7D,4H,KS",
        "KD,JD,10D,8S,5H,5D,3H"
    } },

    // ==================== ID 9 ====================
    new LevelData() { ID = 9, DifType = 0, queues = new List<string>() {
        "2S",
        "9C,3H",
        "QS,QD,KD",
        "6C,5C,6D,AH",
        "JS,KH,8H,7S,6S",
        "8C,7C,5H,4S,3D,2H",
        "10C,10H,9H,5D,4C,2D,AS"
    } },

    // ==================== ID 10 ====================
    new LevelData() { ID = 10, DifType = 0, queues = new List<string>() {
        "6H",
        "3C,AH",
        "JD,10D,AS",
        "7C,6D,5H,2C",
        "KC,QH,8C,2D,AC",
        "10S,9D,8D,7S,3D,AD",
        "KS,JH,10C,6C,5S,4C,4S"
    } },

    // ==================== ID 11 ====================
    new LevelData() { ID = 11, DifType = 0, queues = new List<string>() {
        "AH",
        "QH,2D",
        "9D,4D,4C",
        "7H,6C,4S,2S",
        "KC,8D,7S,3H,AC",
        "8S,6D,10D,QC,6S,KH",
        "9S,9C,10S,7D,KS,QS,2C"
    } },

    // ==================== ID 12 ====================
    new LevelData() { ID = 12, DifType = 0, queues = new List<string>() {
        "6C",
        "5S,5C",
        "3C,KH,6H",
        "10D,JS,9S,4S",
        "9H,QC,9C,6D,AC",
        "10C,8S,7D,2H,2S,4D",
        "4C,8H,JH,QD,JC,AS,AH"
    } },

    // ==================== ID 13 ====================
    new LevelData() { ID = 13, DifType = 0, queues = new List<string>() {
        "8C",
        "4S,9S",
        "QS,9H,AH",
        "5S,6S,7C,AC",
        "8D,QH,KC,3C,2D",
        "8S,7H,JH,6D,KS,2H",
        "9C,4C,3S,3D,6C,2S,AD"
    } },

    // ==================== ID 14 ====================
    new LevelData() { ID = 14, DifType = 0, queues = new List<string>() {
        "6H",
        "8H,KD",
        "10D,10H,AS",
        "8C,9C,5C,QH",
        "KS,6S,3H,7S,JC",
        "7C,10C,3C,8S,2D,AC",
        "JS,JH,6D,5H,4H,KC,3S"
    } },

    // ==================== ID 15 ====================
    new LevelData() { ID = 15, DifType = 0, queues = new List<string>() {
        "2H",
        "2D,AS",
        "9C,8D,QD",
        "10S,10H,7D,AC",
        "KD,QC,JC,2S,4D",
        "JS,4S,3S,6H,KH,AD",
        "JD,9D,9H,7H,6S,5H,AH"
    } },

    // ==================== ID 16 ====================
    new LevelData() { ID = 16, DifType = 0, queues = new List<string>() {
        "10H",
        "10D,5S",
        "9S,3H,2S",
        "QC,KS,5D,AH",
        "KH,6S,7H,7C,6D",
        "8C,6H,6C,3D,2D,AC",
        "JD,QS,8D,9H,5H,4D,AD"
    } },

    // ==================== ID 17 ====================
    new LevelData() { ID = 17, DifType = 0, queues = new List<string>() {
        "AS",
        "4S,2D",
        "KD,7D,3D",
        "QC,5C,3S,AC",
        "7H,7S,6S,KS,5D",
        "QS,QH,10C,9H,8S,AH",
        "10H,8C,KC,5S,4H,KH,2H"
    } },

    // ==================== ID 18 ====================
    new LevelData() { ID = 18, DifType = 0, queues = new List<string>() {
        "KS",
        "4H,2C",
        "8D,9D,AD",
        "6S,3H,JD,AS",
        "JH,10H,6D,3D,2D",
        "10S,10D,6H,3S,AC,AH",
        "KH,10C,9C,7S,5H,5D,2H"
    } },

    // ==================== ID 19 ====================
    new LevelData() { ID = 19, DifType = 0, queues = new List<string>() {
        "2S",
        "5C,2C",
        "7C,KH,3S",
        "5S,QC,4C,5D",
        "KC,8H,7H,10C,4S",
        "JH,JS,AS,9S,10S,AD",
        "JD,9D,10D,10H,9H,2H,AH"
    } },

    // ==================== ID 20 ====================
    new LevelData() { ID = 20, DifType = 0, queues = new List<string>() {
        "9S",
        "3C,AH",
        "6H,9C,6S",
        "JS,2H,5S,AD",
        "10H,JC,QS,4H,2S",
        "QC,8D,6D,5H,2C,AC",
        "KD,QD,7D,9D,7C,4S,4D"
    } },

    // ==================== ID 21 ====================
    new LevelData() { ID = 21, DifType = 0, queues = new List<string>() {
        "6H",
        "7C,AC",
        "2C,QS,5C",
        "6S,10H,10D,AH",
        "JC,6C,7D,4S,AS",
        "8S,4H,10C,9D,3C,KS",
        "KC,7H,9C,9H,2H,3S,AD"
    } },

    // ==================== ID 22 ====================
    new LevelData() { ID = 22, DifType = 0, queues = new List<string>() {
        "AD",
        "9D,3H",
        "4C,9S,6S",
        "7S,KC,5D,9C",
        "KS,8C,8D,3S,AC",
        "3D,4S,KH,5C,KD,AH",
        "6H,7C,10D,QC,10H,JD,2S"
    } },

    // ==================== ID 23 ====================
    new LevelData() { ID = 23, DifType = 0, queues = new List<string>() {
        "AH",
        "JH,9C",
        "QC,7D,AS",
        "9D,4H,5H,2D",
        "10D,4D,QD,5S,3S",
        "KS,9H,KD,QH,7C,JS",
        "7H,KC,JC,6H,6C,8H,5D"
    } },

    // ==================== ID 24 ====================
    new LevelData() { ID = 24, DifType = 0, queues = new List<string>() {
        "2D",
        "10H,3D",
        "KC,4S,AD",
        "8S,3C,3S,2C",
        "JH,JS,7D,5S,AS",
        "KH,8D,7H,6D,4D,AC",
        "QS,9H,5D,5H,JC,8C,AH"
    } },

    // ==================== ID 25 ====================
    new LevelData() { ID = 25, DifType = 0, queues = new List<string>() {
        "AD",
        "7S,2D",
        "JH,KC,AC",
        "9D,8C,5S,4C",
        "7H,KD,10C,4S,2C",
        "10H,10D,5C,JD,2H,9H",
        "QD,9S,7C,5D,5H,3S,AH"
    } },

    // ==================== ID 26 ====================
    new LevelData() { ID = 26, DifType = 0, queues = new List<string>() {
        "AC",
        "KD,JC",
        "9S,8D,JS",
        "5S,QD,4D,AH",
        "6H,10S,5C,4H,2S",
        "6S,KH,KC,10D,2C,9H",
        "6C,5D,9C,8H,4S,3H,AD"
    } },

    // ==================== ID 27 ====================
    new LevelData() { ID = 27, DifType = 0, queues = new List<string>() {
        "AH",
        "10D,3D",
        "JD,4C,AC",
        "4H,JC,6S,5D",
        "9C,10C,3H,2D,8D",
        "8H,6D,5C,6H,8C,AS",
        "6C,8S,QS,7D,QC,JH,10H"
    } },

    // ==================== ID 28 ====================
    new LevelData() { ID = 28, DifType = 0, queues = new List<string>() {
        "QD",
        "9D,4H",
        "10D,10S,7H",
        "4S,3S,KH,AC",
        "8D,7S,3H,AH,4D",
        "7C,2D,8S,JS,9S,8H",
        "10H,8C,QH,2C,KD,2S,AS"
    } },

    // ==================== ID 29 ====================
    new LevelData() { ID = 29, DifType = 0, queues = new List<string>() {
        "AC",
        "3H,3C",
        "2H,6C,5C",
        "QC,QH,6H,4D",
        "8D,JH,8C,7H,AS",
        "7S,JC,6D,3D,10D,9C",
        "6S,5S,10C,10H,5H,4C,4S"
    } },

    // ==================== ID 30 ====================
    new LevelData() { ID = 30, DifType = 0, queues = new List<string>() {
        "2H",
        "2D,AS",
        "QS,5H,3C",
        "JS,7C,4D,AD",
        "8C,4S,KS,9S,QD",
        "10S,4H,5C,QH,4C,8D",
        "KD,6D,QC,8S,9H,JH,AC"
    } },

    // ==================== ID 31 ====================
    new LevelData() { ID = 31, DifType = 0, queues = new List<string>() {
        "AD",
        "10H,9D",
        "8D,3H,AS",
        "7C,KD,2C,AH",
        "6H,5H,JH,JC,10S",
        "KH,5D,10C,7D,2H,2D",
        "9C,QD,8S,QH,KS,3S,5C"
    } },
};

}