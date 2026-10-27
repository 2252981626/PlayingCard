using System;

namespace Tiger.Retention
{
    /// <summary>通知重要级别（映射到 Android Notification Importance）</summary>
    public enum RetentionImportance
    {
        /// <summary>无声音、无横幅，只在通知栏静默出现</summary>
        Low,
        /// <summary>有声音，不弹横幅（推荐，避免打扰式推送）</summary>
        Default,
        /// <summary>有声音 + 屏幕顶部横幅（更醒目，但更打扰）</summary>
        High,
    }

    /// <summary>单条召回通知的配置</summary>
    [Serializable]
    public class RetentionMessage
    {
        /// <summary>埋点标识（如 d1 / d3 / d7）。玩家点击回流时会把这个 key 带回，用于区分是哪一条通知把人拉回来的</summary>
        public string Key = "d1";

        /// <summary>距当前时刻多少小时后触发</summary>
        public double DelayHours = 20;

        /// <summary>标题模板，{progress} 会被替换为进度值（如关卡号）</summary>
        public string Title = "";

        /// <summary>正文模板，{progress} 会被替换为进度值</summary>
        public string Body = "";

        /// <summary>进度值无效（≤0）时的标题兜底文案</summary>
        public string TitleNoProgress = "";

        /// <summary>进度值无效（≤0）时的正文兜底文案</summary>
        public string BodyNoProgress = "";
    }

    /// <summary>
    /// 召回通知配置。项目侧 new 一个实例改掉需要的字段，通过 RetentionNotification.Setup() 注入即可。
    /// </summary>
    public class RetentionConfig
    {
        // ────────── 通知渠道（Android 8.0+ 必需）──────────
        // 注意：ChannelId 一旦在设备上创建，其属性（名称/重要级别）不可更改。
        // 如果后续要大改渠道文案，请换一个新的 ChannelId。

        public string ChannelId = "retention_recall";
        public string ChannelName = "Level reminders";
        public string ChannelDescription = "Progress reminders for your game";
        public RetentionImportance Importance = RetentionImportance.Default;

        // ────────── 通知小图标 ──────────
        // Android 要求小图标是「纯白 + 透明背景」的 PNG（系统会自动着色）。
        // 用法：图片放 Assets/Plugins/Android/notify_small.png（Inspector 勾 Read/Write Enabled），
        // 再到 Project Settings → Mobile Notifications → Android → Drawable Resources 注册（Id=notify_small），
        // 然后这里填 "notify_small"。留空则使用系统默认（桌面上可能显示成白色方块）。
        // ⚠️ 不要放 Assets/Plugins/Android/res/ —— Unity 2022.3 起该目录已废弃，会直接导致打包失败。

        public string SmallIconId = "";

        // ────────── 深夜保护 ──────────
        // 触发时间落在 [NightStartHour, 24) ∪ [0, NightEndHour) 时，
        // 自动顺延到当天/次日 NightFallbackHour，避免凌晨吵醒玩家。

        public bool AvoidNightHours = true;
        public int NightStartHour = 23;
        public int NightEndHour = 9;
        public int NightFallbackHour = 10;

        // ────────── 召回链 ──────────
        // 默认 D1 / D3 / D7 三条，玩家每次启动游戏都会整体重置。

        public RetentionMessage[] Messages = CreateDefaultMessages();

        // ────────── 自动化钩子（可选）──────────

        /// <summary>进度值提供者：自动启动时调用它取个性化文案的进度值（如当前关卡号）。返回 ≤0 表示不使用个性化。</summary>
        public Func<int> ProgressProvider;

        /// <summary>玩家通过通知打开游戏时的回调，参数为通知的 Key（d1/d3/d7）。适合挂钩埋点上报。</summary>
        public Action<string> OnLaunchFromNotification;

        /// <summary>是否在首个场景加载后自动执行一次 OnGameLaunch（推荐开启，零侵入接入）。</summary>
        public bool AutoLaunchOnStart = true;

        public static RetentionMessage[] CreateDefaultMessages()
        {
            return new[]
            {
                new RetentionMessage
                {
                    Key = "d1",
                    DelayHours = 20,
                    Title = "Level {progress} is so close",
                    Body = "You were just a few steps away. Come back and finish it!",
                    TitleNoProgress = "Your levels are waiting",
                    BodyNoProgress = "One level takes a minute. Jump back in!",
                },
                new RetentionMessage
                {
                    Key = "d3",
                    DelayHours = 72,
                    Title = "Your progress is still there",
                    Body = "Pick up level {progress} right where you left off.",
                    TitleNoProgress = "Your progress is still there",
                    BodyNoProgress = "Pick up right where you left off.",
                },
                new RetentionMessage
                {
                    Key = "d7",
                    DelayHours = 168,
                    Title = "Level {progress} is still waiting for you",
                    Body = "Can you beat it in under a minute? Give it a try!",
                    TitleNoProgress = "New levels are waiting",
                    BodyNoProgress = "Can you beat one in under a minute? Give it a try!",
                },
            };
        }
    }
}
