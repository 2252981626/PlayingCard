using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using Unity.Notifications.Android;
#endif

namespace Tiger.Retention
{
    /// <summary>
    /// 次留召回本地通知（通用模块，可多项目复用）。
    ///
    /// 生命周期：
    ///   ① 游戏启动 → OnGameLaunch()：读取回流来源 → 清空上一轮通知链 → 按进度重排 D1/D3/D7
    ///   ② 玩家没回来 → 到点依次弹通知
    ///   ③ 玩家点通知回来 → 再次进入 ①，链条重置
    ///
    /// 特性：不依赖服务器、不依赖 Google 服务框架，国内设备同样生效。
    /// 接入：项目侧只需调用一次 Setup()（推荐放在 [RuntimeInitializeOnLoadMethod(BeforeSceneLoad)] 里），
    ///       其余全自动（AutoLaunchOnStart）。
    /// </summary>
    public static class RetentionNotification
    {
        const string SourcePrefix        = "retention:";
        const string PrefsScheduledIds   = "tiger_retention_scheduled_ids";
        const string PermPostNotification = "android.permission.POST_NOTIFICATIONS";

        static RetentionConfig _cfg = new RetentionConfig();
        static readonly HashSet<int> ConsumedLaunchIds = new HashSet<int>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetState()
        {
            _cfg = new RetentionConfig();
            ConsumedLaunchIds.Clear();
        }

        /// <summary>当前生效的召回消息列表（配置缺失时回退到默认文案）</summary>
        static RetentionMessage[] Messages
        {
            get
            {
                var m = _cfg != null ? _cfg.Messages : null;
                return (m != null && m.Length > 0) ? m : RetentionConfig.CreateDefaultMessages();
            }
        }

        /// <summary>注入项目侧配置（文案 / 图标 / 回调）。不调用则使用包默认配置。</summary>
        public static void Setup(RetentionConfig config)
        {
            if (config != null) _cfg = config;
        }

        /// <summary>
        /// 启动主入口：注册渠道 → 读取回流来源 → 清空上一轮通知链 → 重排全部召回 → （可选）请求权限。
        /// </summary>
        /// <param name="progressValue">个性化进度值（如最高解锁关卡），≤0 时使用无进度兜底文案</param>
        /// <param name="autoRequestPermission">是否自动请求通知权限。若想延后到「首关通过」再请求，传 false 并自行调用 RequestPermission()</param>
        /// <returns>本次启动是否由通知打开：返回通知 Key（如 "d1"），否则 null</returns>
        public static string OnGameLaunch(int progressValue = 0, bool autoRequestPermission = true)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            RegisterChannel();
            // Read the clicked notification before cancellation removes its stored data.
            string source = ReadLaunchSource();
            CancelScheduled();                          // 玩家已回来 → 清掉上一轮
            ScheduleAll(progressValue);                 // 重排 D1/D3/D7
            if (autoRequestPermission) RequestPermission();
            return source;
#else
            Debug.Log("[Retention] Editor / 非 Android 环境：不实际调度通知，以下为预览");
            PrintPreview(progressValue);
            return null;
#endif
        }

        /// <summary>进度变化时同步通知链（可选，例如每次通关后调用，让文案里的关卡号保持最新）</summary>
        public static void UpdateProgress(int progressValue)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            CancelScheduled();
            ScheduleAll(progressValue);
#endif
        }

        /// <summary>请求通知权限（Android 13+）。建议在玩家首关通过后的合适时机调用，通过率更高。</summary>
        public static void RequestPermission()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (AndroidNotificationCenter.UserPermissionToPost == PermissionStatus.Allowed) return;   // 已允许或系统无需权限
            if (UnityEngine.Android.Permission.HasUserAuthorizedPermission(PermPostNotification)) return;
            UnityEngine.Android.Permission.RequestUserPermission(PermPostNotification);
            Debug.Log("[Retention] 已请求通知权限（POST_NOTIFICATIONS）");
#endif
        }

        /// <summary>是否已获得通知权限（Android 13+ 之外的设备通常为 true）</summary>
        public static bool HasPermission()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return AndroidNotificationCenter.UserPermissionToPost == PermissionStatus.Allowed;
#else
            return false;
#endif
        }

        /// <summary>取消本模块排出的所有待发通知（不影响其他模块的通知）</summary>
        public static void CancelScheduled()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            foreach (int id in LoadIds())
                AndroidNotificationCenter.CancelNotification(id);
            SaveIds(new List<int>());
#endif
        }

        // ══════════════ 内部实现 ══════════════

        internal static void AutoLaunchIfEnabled()
        {
            if (_cfg == null || !_cfg.AutoLaunchOnStart) return;

            int progress = 0;
            if (_cfg.ProgressProvider != null)
            {
                try { progress = _cfg.ProgressProvider(); }
                catch (Exception e) { Debug.LogWarning($"[Retention] ProgressProvider 异常：{e.Message}"); }
            }

            string source = OnGameLaunch(progress);
            if (!string.IsNullOrEmpty(source) && _cfg.OnLaunchFromNotification != null)
            {
                try { _cfg.OnLaunchFromNotification(source); }
                catch (Exception e) { Debug.LogWarning($"[Retention] 回流回调异常：{e.Message}"); }
            }
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        static void RegisterChannel()
        {
            var channel = new AndroidNotificationChannel
            {
                Id          = _cfg.ChannelId,
                Name        = _cfg.ChannelName,
                Description = _cfg.ChannelDescription,
                Importance  = ToImportance(_cfg.Importance),
            };
            AndroidNotificationCenter.RegisterNotificationChannel(channel);
        }

        static void ScheduleAll(int progressValue)
        {
            var ids = new List<int>();
            DateTime now = DateTime.Now;

            foreach (var m in Messages)
            {
                if (m == null || string.IsNullOrEmpty(m.Key)) continue;

                DateTime fireTime = now.AddHours(m.DelayHours);
                if (_cfg.AvoidNightHours) fireTime = SafeFireTime(fireTime);

                var notification = new AndroidNotification
                {
                    Title      = Format(m.Title, m.TitleNoProgress, progressValue),
                    Text       = Format(m.Body, m.BodyNoProgress, progressValue),
                    FireTime   = fireTime,
                    IntentData = SourcePrefix + m.Key,
                };
                if (!string.IsNullOrEmpty(_cfg.SmallIconId))
                    notification.SmallIcon = _cfg.SmallIconId;

                int id = AndroidNotificationCenter.SendNotification(notification, _cfg.ChannelId);
                ids.Add(id);
                Debug.Log($"[Retention] 已预约 {m.Key} → {fireTime:MM-dd HH:mm}「{notification.Title}」");
            }

            SaveIds(ids);
            Debug.Log($"[Retention] 通知链就绪：共 {ids.Count} 条（渠道 {_cfg.ChannelId}）");
        }

        static string ReadLaunchSource()
        {
            try
            {
                var data = AndroidNotificationCenter.GetLastNotificationIntent();
                if (data == null) return null;   // AndroidNotification 是 struct，不能与 null 比较

                string source = ConsumeLaunchSource(data.Id, data.Notification.IntentData);
                if (source == null) return null;
                AndroidNotificationCenter.CancelDisplayedNotification(data.Id);   // 清掉玩家刚点开的那条
                Debug.Log($"[Retention] 玩家通过通知回流：{source}");
                return source;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Retention] 读取通知来源失败：{e.Message}");
                return null;
            }
        }

        static Importance ToImportance(RetentionImportance value)
        {
            switch (value)
            {
                case RetentionImportance.Low:  return Importance.Low;
                case RetentionImportance.High: return Importance.High;
                default:                       return Importance.Default;
            }
        }

        static List<int> LoadIds()
        {
            var list = new List<int>();
            string raw = PlayerPrefs.GetString(PrefsScheduledIds, "");
            if (string.IsNullOrEmpty(raw)) return list;
            foreach (string part in raw.Split(','))
                if (int.TryParse(part, out int id)) list.Add(id);
            return list;
        }

        static void SaveIds(List<int> ids)
        {
            PlayerPrefs.SetString(PrefsScheduledIds, string.Join(",", ids));
            PlayerPrefs.Save();
        }
#endif

        // Android can return the same Activity intent again on an ordinary resume.
        static string ConsumeLaunchSource(int notificationId, string payload)
        {
            if (string.IsNullOrEmpty(payload)
                || !payload.StartsWith(SourcePrefix, StringComparison.Ordinal)
                || payload.Length == SourcePrefix.Length
                || !ConsumedLaunchIds.Add(notificationId))
                return null;
            return payload.Substring(SourcePrefix.Length);
        }

        /// <summary>深夜保护：落在 [NightStartHour, 24) ∪ [0, NightEndHour) 的时间顺延到 NightFallbackHour</summary>
        static DateTime SafeFireTime(DateTime t)
        {
            if (t.Hour >= _cfg.NightStartHour || t.Hour < _cfg.NightEndHour)
            {
                DateTime fallback = t.Date.AddHours(_cfg.NightFallbackHour);
                if (fallback <= t) fallback = fallback.AddDays(1);
                return fallback;
            }
            return t;
        }

        static string Format(string template, string noProgressTemplate, int progress)
        {
            if (string.IsNullOrEmpty(template)) return "";
            if (progress > 0) return template.Replace("{progress}", progress.ToString());
            if (!string.IsNullOrEmpty(noProgressTemplate)) return noProgressTemplate;
            return template.Replace("{progress}", "").Replace("  ", " ").Trim();
        }

#if !UNITY_ANDROID || UNITY_EDITOR
        static void PrintPreview(int progressValue)
        {
            DateTime now = DateTime.Now;
            foreach (var m in Messages)
            {
                if (m == null) continue;
                DateTime fireTime = now.AddHours(m.DelayHours);
                if (_cfg.AvoidNightHours) fireTime = SafeFireTime(fireTime);
                Debug.Log($"[Retention]   {m.Key} → {fireTime:MM-dd HH:mm}「{Format(m.Title, m.TitleNoProgress, progressValue)}」");
            }
        }
#endif
    }

    /// <summary>自动启动钩子：首个场景加载后执行一次 OnGameLaunch（可在配置里关闭）</summary>
    internal static class RetentionAutoBoot
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            RetentionNotification.AutoLaunchIfEnabled();
        }
    }
}
