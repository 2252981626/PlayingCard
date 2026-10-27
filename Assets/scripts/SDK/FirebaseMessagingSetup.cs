using System;
using System.Collections.Generic;
using System.Linq;
using Firebase.Extensions;
using Firebase.Messaging;
using UnityEngine;

/// <summary>Firebase push integration based on the notification implementation supplied by the project owner.</summary>
public sealed class FirebaseMessagingSetup : MonoBehaviour
{
    public const string Topic = "daily_reminders";
    public const string ChannelId = "solitaire_casino_tycoon_push";
    public const string TokenPrefsKey = "FCMDeviceToken";
    private static FirebaseMessagingSetup instance;
    private bool initialized;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetState()
    {
        instance = null;
    }

    public static void Initialize()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (instance != null) return;
        var host = new GameObject("FirebaseMessaging");
        DontDestroyOnLoad(host);
        instance = host.AddComponent<FirebaseMessagingSetup>();
        instance.InitializeFcm();
#else
        Debug.Log("[FCM] Remote push reception is enabled in the Android player.");
#endif
    }

    // 文案列表（与控制台推送的消息内容对应）
    private readonly List<string> notificationTexts = new List<string>
        {
            "Don’t miss out on earnings while offline 💰\nCome online to claim your in-game rewards 🎮",
            "You have unclaimed in-game funds 💸\nHurry online to cash them out 🚀",
            "Don’t let your earnings go to waste ❌\nPlay online and start earning right away 💸",
            "Your in-game earnings are ready ✅\nClick to go online and redeem now 💳",
            "Funds you missed while offline 💴\nGet them back by going online 🆘",
            "Your in-game \"pocket money\" has arrived 🧧\nClaim it once you’re online 👛",
            "Earning opportunities won’t wait ⏱️\nPlay online and get rewards instantly 💸",
            "There’s money in your in-game account 💵\nGo online and withdraw it with a simple tap 💳"
        };

    // 已展示的文案索引（本地存储）
    private List<int> shownIndexes = new List<int>();

    // 初始化FCM（订阅主题 + 注册消息接收回调）
    private void InitializeFcm()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (initialized) return;
        initialized = true;
        LoadShownIndexes();
        Unity.Notifications.Android.AndroidNotificationCenter.RegisterNotificationChannel(
            new Unity.Notifications.Android.AndroidNotificationChannel
            {
                Id = ChannelId,
                Name = "游戏提醒",
                Description = "游戏通知",
                Importance = Unity.Notifications.Android.Importance.Default,
            });
        FirebaseMessaging.TokenReceived += OnTokenReceived;
        // 请求通知权限（iOS必需）
        FirebaseMessaging.RequestPermissionAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
                Debug.LogWarning("[FCM] 通知权限请求失败: " + task.Exception);
        });

        // 订阅主题（方便控制台批量推送）
        FirebaseMessaging.SubscribeAsync(Topic).ContinueWithOnMainThread(task =>
        {
            if (this == null) return;
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("[FCM] 主题订阅失败: " + task.Exception);
                return;
            }
            Debug.Log("[FCM] 已订阅主题: " + Topic);
        });

        // 获取设备FCM令牌（用于控制台定向推送）
        FirebaseMessaging.GetTokenAsync().ContinueWithOnMainThread(tokenTask =>
        {
            if (this == null) return;
            if (tokenTask.IsCanceled || tokenTask.IsFaulted)
            {
                Debug.LogError("[FCM] 获取设备令牌失败: " + tokenTask.Exception);
                return;
            }
            SaveDeviceToken(tokenTask.Result);
        });

        // 注册消息接收事件（核心：处理远程推送的消息）
        FirebaseMessaging.MessageReceived += OnMessageReceived;
#endif
    }

    // 处理接收到的FCM消息
    private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        Debug.Log("收到FCM消息");
        Debug.Log("【FCM】1. 进入 OnMessageReceived 方法");

        // 步骤1：检查脚本存活
        if (this == null)
        {
            Debug.LogError("【FCM】2. 脚本已销毁");
            return;
        }

        Debug.Log("【FCM】2. 脚本存活");

        // 步骤2：检查 sender
        if (sender == null)
        {
            Debug.Log("【FCM】3. sender 为 null（SDK 回调允许为空）");
        }

        Debug.Log("【FCM】3. sender 检查完成");

        // 步骤3：检查 e 和 e.Message
        if (e == null || e.Message == null)
        {
            Debug.LogWarning("【FCM】4. 消息为空");
            return;
        }

        Debug.Log("【FCM】4. e 不为 null");
        Debug.Log("[FCM] Message received: " + e.Message.MessageId);
        if (e.Message.NotificationOpened)
        {
            Debug.Log("[FCM] 点击已有通知返回游戏，不重复显示通知。");
            return;
        }

        // 步骤4：检查 e.Message.Notification
        string title = "默认标题";

        // 步骤5：检查 e.Message.Data
        if (e.Message.Notification != null)
        {
            Debug.Log("【FCM】7. 通知不为空，准备调用 GetRandomUnshownText");
            title = e.Message.Notification.Title ?? title;
            Debug.Log($"【FCM】6. 提取 Notification 标题：{title}");

            string displayText = GetRandomUnshownText();
            Debug.Log($"【FCM】8. GetRandomUnshownText 返回：{displayText}");

            string content = e.Message.Notification.Body;
            Debug.Log("【FCM】9. 准备调用 ShowNotificationPopup");
            ShowNotificationPopup(title, content);
            Debug.Log("【FCM】10. ShowNotificationPopup 调用完成");
        }

        Debug.Log("【FCM】11. OnMessageReceived 处理完成");
    }

    // 显示通知弹窗
    private void ShowNotificationPopup(string title, string content)
    {
        Debug.Log($"【通知调试】开始处理通知 - 标题: {title}, 内容: {content}");
#if UNITY_ANDROID && !UNITY_EDITOR
            if (Unity.Notifications.Android.AndroidNotificationCenter.UserPermissionToPost
                != Unity.Notifications.Android.PermissionStatus.Allowed)
            {
                Debug.LogWarning("[FCM] 已收到消息，但系统通知权限未允许。");
                return;
            }
            try
            {
                Debug.Log($"【通知调试】进入Android平台逻辑");

                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    Debug.Log($"【通知调试】获取UnityPlayer类成功");

                    using (var context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                    {
                        if (context == null)
                        {
                            Debug.LogError($"【通知调试】获取上下文失败：currentActivity为null");
                            return;
                        }

                        string packageName = context.Call<string>("getPackageName");
                        Debug.Log($"【通知调试】获取上下文成功，包名: {packageName}");

                        // -------- 步骤 1：创建通知渠道（Android 8.0+ 必需） --------
                        string channelId = ChannelId;
                        string channelName = "游戏提醒";
                        int channelImportance = 3; // 对应NotificationManager.IMPORTANCE_DEFAULT
                        Debug.Log($"【通知调试】开始创建通知渠道 - ID: {channelId}, 名称: {channelName}");

                        // 获取系统版本
                        using var buildVersionClass = new AndroidJavaClass("android.os.Build$VERSION");
                        int sdkInt = buildVersionClass.GetStatic<int>("SDK_INT");
                        Debug.Log($"【通知调试】当前Android版本SDK_INT: {sdkInt}");

                        // 仅在Android 8.0+创建渠道
                        if (sdkInt >= 26)
                        {
                            using (var notificationManager =
                                   context.Call<AndroidJavaObject>("getSystemService", "notification"))
                            {
                                if (notificationManager == null)
                                {
                                    Debug.LogError($"【通知调试】获取NotificationManager失败");
                                    return;
                                }

                                using (var channel = new AndroidJavaObject("android.app.NotificationChannel",
                                           channelId, channelName, channelImportance))
                                {
                                    channel.Call("setDescription", "游戏内定时提醒通知");
                                    channel.Call("setShowBadge", true);
                                    notificationManager.Call("createNotificationChannel", channel);
                                    Debug.Log($"【通知调试】通知渠道创建完成（API 26+）");
                                }
                            }
                        }
                        else
                        {
                            Debug.Log($"【通知调试】API 26以下，无需创建通知渠道");
                        }

                        // -------- 步骤 2：构建Intent --------
                        using (var intentClass = new AndroidJavaClass("android.content.Intent"))
                        {
                            using (var packageManager = context.Call<AndroidJavaObject>("getPackageManager"))
                            using (var intent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", packageName))
                            {
                                if (intent == null)
                                {
                                    Debug.LogError($"【通知调试】创建Intent失败");
                                    return;
                                }

                                int flagNewTask = intentClass.GetStatic<int>("FLAG_ACTIVITY_NEW_TASK");
                                int flagClearTop = intentClass.GetStatic<int>("FLAG_ACTIVITY_CLEAR_TOP");
                                intent.Call<AndroidJavaObject>("addFlags", flagNewTask);
                                intent.Call<AndroidJavaObject>("addFlags", flagClearTop);
                                Debug.Log(
                                    $"【通知调试】Intent创建完成，已添加标志 - NEW_TASK: {flagNewTask}, CLEAR_TOP: {flagClearTop}");

                                // -------- 步骤 3：创建 PendingIntent --------
                                using (var pendingIntentClass = new AndroidJavaClass("android.app.PendingIntent"))
                                {
                                int pendingIntentFlags = pendingIntentClass.GetStatic<int>("FLAG_UPDATE_CURRENT");
                                if (sdkInt >= 23)
                                    pendingIntentFlags |= pendingIntentClass.GetStatic<int>("FLAG_IMMUTABLE");

                                using (var pendingIntent = pendingIntentClass.CallStatic<AndroidJavaObject>(
                                           "getActivity", context, 0, intent, pendingIntentFlags))
                                {
                                    if (pendingIntent == null)
                                    {
                                        Debug.LogError($"【通知调试】创建PendingIntent失败");
                                        return;
                                    }

                                    Debug.Log($"【通知调试】PendingIntent创建成功");

                                    // -------- 步骤 4：构建通知（使用NotificationCompat兼容类） --------
                                    try
                                    {
                                        // 关键修改：使用AndroidX的NotificationCompat.Builder（兼容性更好）
                                        AndroidJavaObject builder;
                                        if (sdkInt >= 26)
                                        {
                                            // Android 8.0+ 传入channelId
                                            builder =
                                                new AndroidJavaObject("androidx.core.app.NotificationCompat$Builder",
                                                    context, channelId);
                                            Debug.Log($"【通知调试】创建NotificationCompat.Builder（带channelId）");
                                        }
                                        else
                                        {
                                            // Android 7.1及以下 不传入channelId
                                            builder =
                                                new AndroidJavaObject("androidx.core.app.NotificationCompat$Builder",
                                                    context);
                                            Debug.Log($"【通知调试】创建NotificationCompat.Builder（不带channelId）");
                                        }

                                        using var builderLifetime = builder;
                                        // 设置通知基本信息
                                        builder.Call<AndroidJavaObject>("setContentTitle", title);
                                        builder.Call<AndroidJavaObject>("setContentText", content ?? "");
                                        builder.Call<AndroidJavaObject>("setAutoCancel", true);
                                        builder.Call<AndroidJavaObject>("setContentIntent", pendingIntent);
                                        builder.Call<AndroidJavaObject>("setPriority", 0); // 优先级：默认（兼容低版本）
                                        Debug.Log($"【通知调试】通知基本信息设置完成");

                                        // 设置通知图标
                                        using var resources = context.Call<AndroidJavaObject>("getResources");
                                        int iconId = resources.Call<int>("getIdentifier",
                                            "notify_small", "drawable", packageName);

                                        if (iconId != 0)
                                        {
                                            builder.Call<AndroidJavaObject>("setSmallIcon", iconId);
                                            Debug.Log($"【通知调试】成功设置通知图标，资源ID: {iconId}");
                                        }
                                        else
                                        {
                                            Debug.LogWarning($"【通知调试】未找到通知图标资源 - notify_small，尝试使用应用默认图标");
                                            try
                                            {
                                                // 步骤1：获取 ApplicationInfo 对象（应用信息容器）
                                                using var appInfo =
                                                    context.Call<AndroidJavaObject>("getApplicationInfo");
                                                if (appInfo != null)
                                                {
                                                    // 步骤2：从 ApplicationInfo 中提取默认图标 ID
                                                    int defaultIconId = appInfo.Get<int>("icon");
                                                    if (defaultIconId != 0)
                                                    {
                                                        builder.Call<AndroidJavaObject>("setSmallIcon", defaultIconId);
                                                        Debug.Log($"【通知调试】成功使用应用默认图标，资源ID: {defaultIconId}");
                                                    }
                                                    else
                                                    {
                                                        Debug.LogError($"【通知调试】应用默认图标资源ID为0，设置失败");
                                                    }
                                                }
                                                else
                                                {
                                                    Debug.LogError($"【通知调试】获取 ApplicationInfo 失败");
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                Debug.LogError($"【通知调试】设置应用默认图标时出错: {e.Message}");
                                            }
                                        }

                                        // 设置通知声音（使用系统默认）
                                        using (var ringtoneManager =
                                               new AndroidJavaClass("android.media.RingtoneManager"))
                                        {
                                            int typeNotification = ringtoneManager.GetStatic<int>("TYPE_NOTIFICATION");
                                            using var soundUri =
                                                ringtoneManager.CallStatic<AndroidJavaObject>("getDefaultUri",
                                                    typeNotification);
                                            builder.Call<AndroidJavaObject>("setSound", soundUri);
                                            Debug.Log($"【通知调试】通知声音设置完成");
                                        }

                                        // -------- 步骤 5：显示通知 --------
                                        using (var notificationManager =
                                               context.Call<AndroidJavaObject>("getSystemService", "notification"))
                                        {
                                            int notificationId = UnityEngine.Random.Range(1000, 9999);
                                            using var notification = builder.Call<AndroidJavaObject>("build");
                                            notificationManager.Call("notify", notificationId, notification);
                                            Debug.Log($"【通知调试】通知显示成功，通知ID: {notificationId}");
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Debug.LogError($"【通知调试】创建NotificationCompat.Builder失败: {e.Message}");
                                        Debug.LogError($"【通知调试】Builder错误堆栈: {e.StackTrace}");
                                    }
                                }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"【通知调试】通知创建失败 - 错误消息: {e.Message}");
                Debug.LogError($"【通知调试】错误堆栈: {e.StackTrace}");
            }

#else
        Debug.Log($"【通知调试】非Android平台，跳过通知处理");
#endif
    }

    // 获取随机未展示的文案（核心业务逻辑）
    private string GetRandomUnshownText()
    {
        var unshownIndexes = Enumerable.Range(0, notificationTexts.Count)
            .Where(i => !shownIndexes.Contains(i))
            .ToList();

        if (unshownIndexes.Count == 0)
        {
            // 所有文案已展示，重置
            shownIndexes.Clear();
            SaveShownIndexes();
            unshownIndexes = Enumerable.Range(0, notificationTexts.Count).ToList();
        }

        int randomIndex = unshownIndexes[UnityEngine.Random.Range(0, unshownIndexes.Count)];
        shownIndexes.Add(randomIndex);
        SaveShownIndexes();

        return notificationTexts[randomIndex];
    }

    // 本地存储已展示的索引
    private void SaveShownIndexes()
    {
        PlayerPrefs.SetString("ShownNotificationIndexes", string.Join(",", shownIndexes));
        PlayerPrefs.Save();
    }
    private void OnDestroy()
    {
        // 取消消息接收事件注册
        if (initialized)
        {
            FirebaseMessaging.MessageReceived -= OnMessageReceived;
            FirebaseMessaging.TokenReceived -= OnTokenReceived;
        }
        if (instance == this) instance = null;
    }

    private void OnTokenReceived(object sender, TokenReceivedEventArgs args)
    {
        if (this != null && args != null) SaveDeviceToken(args.Token);
    }

    private void SaveDeviceToken(string token)
    {
        if (string.IsNullOrEmpty(token)) return;
        Debug.Log("[FCM] Device token: " + token);
        PlayerPrefs.SetString(TokenPrefsKey, token);
        PlayerPrefs.Save();
    }

    private void LoadShownIndexes()
    {
        shownIndexes.Clear();
        foreach (string part in PlayerPrefs.GetString("ShownNotificationIndexes", "").Split(','))
        {
            if (int.TryParse(part, out int index) && index >= 0
                && index < notificationTexts.Count && !shownIndexes.Contains(index))
                shownIndexes.Add(index);
        }
    }
}
