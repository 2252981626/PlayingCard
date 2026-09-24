
using System.ComponentModel;

// 创建一个 partial 类 SROptions
public partial class SROptions
{
    // 定义一个可在面板中调整的数字选项



    // 定义一个可点击的按钮方法
    //[Category("基本功能"), DisplayName("增加100金币")]
    //public void Add100Coins()
    //{
    //    EventManager.Instance.TriggerEvent(GameEvent.GetGold, 100f);
    //    EventManager.Instance.TriggerEvent(GameEvent.RefreshTxPanel);
    //}

    private int debugTargetLv { get; set; } = 1;
    [Category("自定义关卡"), DisplayName("关卡")]
    public int DebugTargetLv
    { 
        get
        {
            return debugTargetLv;
        }
        set
        {
            debugTargetLv = value;
            GameManager.Instance.playerInfo.level = value;
        }
            
    }
}