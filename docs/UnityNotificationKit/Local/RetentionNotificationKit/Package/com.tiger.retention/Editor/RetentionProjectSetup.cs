using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Tiger.Retention.EditorTools
{
    /// <summary>
    /// 项目自动配置工具：把接入时最容易漏掉的两步自动化（幂等、失败安全）。
    ///   ① 通知图标：开启 Read/Write Enabled + 注册到 Mobile Notifications → Drawable Resources
    ///   ② 开启 Mobile Notifications → "Reschedule on Device Restart"（设备重启后待发通知不丢）
    ///
    /// 触发：Unity 每次域重载后自动检查一次（已配置好则零动作、零日志）；
    /// 手动重跑：菜单 Tools → Tiger Retention → Setup Project。
    ///
    /// 实现：全程通过反射调用 Unity.Notifications 的 API（避免与不同版本的通知包产生编译耦合）；
    /// 任何异常只打 Warning 日志，绝不影响项目编译与打包。
    /// </summary>
    [InitializeOnLoad]
    public static class RetentionProjectSetup
    {
        /// <summary>通知小图标路径约定（ProjectFiles/notify_small.png → 放这里）</summary>
        const string IconAssetPath = "Assets/Plugins/Android/notify_small.png";

        /// <summary>Drawable Resources 注册 Id（运行时配置里的 SmallIconId 引用同名）</summary>
        const string IconId = "notify_small";

        /// <summary>Mobile Notifications 的设置文件</summary>
        const string SettingsAssetPath = "ProjectSettings/NotificationsSettings.asset";

        const double RetryWindowSeconds = 30.0;
        static double _deadline;

        static RetentionProjectSetup()
        {
            EditorApplication.delayCall += AutoConfigureDeferred;
        }

        static void AutoConfigureDeferred()
        {
            // 编辑器忙（资产导入 / 脚本编译）时等待，最多等 30 秒
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                if (_deadline == 0) _deadline = EditorApplication.timeSinceStartup + RetryWindowSeconds;
                if (EditorApplication.timeSinceStartup < _deadline)
                {
                    EditorApplication.delayCall += AutoConfigureDeferred;
                    return;
                }
            }
            _deadline = 0;

            var actions = AutoConfigure();
            if (actions.Count > 0)
                Debug.Log("[Retention] 接入自动配置完成：\n· " + string.Join("\n· ", actions)
                          + "\n（手动重跑：菜单 Tools → Tiger Retention → Setup Project）");
        }

        [MenuItem("Tools/Tiger Retention/Setup Project")]
        public static void SetupManually()
        {
            var actions = AutoConfigure();
            string msg = actions.Count == 0
                ? "全部配置已就绪，无需修改。"
                : "已完成：\n· " + string.Join("\n· ", actions);
            if (!File.Exists(IconAssetPath))
                msg += "\n\n提示：未找到 " + IconAssetPath + "\n（图标相关配置已跳过；放入 PNG 后重跑本菜单即可）";
            EditorUtility.DisplayDialog("Tiger Retention Setup", msg, "OK");
            Debug.Log("[Retention] 手动配置检查：" + msg);
        }

        /// <summary>幂等执行全部自动项，返回本次实际完成的动作列表（空 = 无动作）</summary>
        static List<string> AutoConfigure()
        {
            var done = new List<string>();
            Safe(() => { if (EnsureIconReadable())       done.Add("开启 " + IconId + ".png 的 Read/Write Enabled"); });
            Safe(() => { if (EnsureIconRegistered())     done.Add("注册图标到 Drawable Resources（Id=" + IconId + "）"); });
            Safe(() => { if (EnsureRescheduleOnRestart()) done.Add("开启 Reschedule on Device Restart"); });
            return done;
        }

        static void Safe(Action action)
        {
            try { action(); }
            catch (Exception e) { Debug.LogWarning("[Retention] 自动配置跳过一项：" + e.Message); }
        }

        // ────────── ① 图标 Read/Write Enabled ──────────

        static bool EnsureIconReadable()
        {
            if (!File.Exists(IconAssetPath)) return false;
            var importer = AssetImporter.GetAtPath(IconAssetPath) as TextureImporter;
            if (importer == null || importer.isReadable) return false;
            importer.isReadable = true;
            importer.SaveAndReimport();
            return true;
        }

        // ────────── ② 注册到 Drawable Resources ──────────

        static bool EnsureIconRegistered()
        {
            if (!File.Exists(IconAssetPath)) return false;
            if (IsIconRegistered()) return false;                // 已注册过

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(IconAssetPath);
            if (tex == null) return false;                       // 资产尚未导入完成

            var androidSettings = FindAndroidSettingsType();
            var iconType        = FindType("Unity.Notifications.NotificationIconType");
            if (androidSettings == null || iconType == null) return false;

            var add = androidSettings.GetMethod("AddDrawableResource",
                new[] { typeof(string), typeof(Texture2D), iconType });
            if (add == null) return false;

            add.Invoke(null, new[] { (object)IconId, tex, Enum.ToObject(iconType, 0) });   // NotificationIconType.Small = 0
            return true;
        }

        static bool IsIconRegistered()
        {
            if (!File.Exists(SettingsAssetPath)) return false;
            string content = File.ReadAllText(SettingsAssetPath);
            return content.Contains("\"Id\": \"" + IconId + "\"")
                || content.Contains("\"Id\":\"" + IconId + "\"");
        }

        // ────────── ③ Reschedule on Device Restart ──────────

        static bool EnsureRescheduleOnRestart()
        {
            var androidSettings = FindAndroidSettingsType();
            if (androidSettings == null) return false;
            var prop = androidSettings.GetProperty("RescheduleOnDeviceRestart",
                BindingFlags.Public | BindingFlags.Static);
            if (prop == null) return false;
            if ((bool)prop.GetValue(null)) return false;         // 已开启
            prop.SetValue(null, true);
            return true;
        }

        // ────────── 反射工具 ──────────

        static Type FindAndroidSettingsType()
        {
            var settings = FindType("Unity.Notifications.NotificationSettings");
            return settings == null ? null : settings.GetNestedType("AndroidSettings", BindingFlags.Public);
        }

        static Type FindType(string fullName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType(fullName);
                if (t != null) return t;
            }
            try
            {
                var asm = Assembly.Load("Unity.Notifications");   // 程序集可能尚未加载
                return asm == null ? null : asm.GetType(fullName);
            }
            catch { return null; }
        }
    }
}
