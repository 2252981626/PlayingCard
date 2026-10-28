# Unity Android 本地与远程通知接入指南

接收方准备好 SDK 后，使用本文件夹内的源码和模板接入自己的项目。下表的“本包文件”都已随包提供；“目标位置”指接收方自己的 Unity 项目。

## 1. 选择模块并复制文件

先安装 [SDK_REQUIREMENTS.md](SDK_REQUIREMENTS.md) 对应依赖。建议先接 Common，再选择 Local / Remote，最后统一 Android 配置。

### 公共文件

| 本包文件 | 目标位置 |
| --- | --- |
| `Common/ProjectFiles/NotificationProjectConfig.cs` | `Assets/NotificationKit/NotificationProjectConfig.cs` |
| `Common/ProjectFiles/NotificationPermissionBootstrap.cs` | `Assets/NotificationKit/NotificationPermissionBootstrap.cs` |
| `Common/ProjectFiles/notify_small.png` | `Assets/Plugins/Android/notify_small.png` |

两个脚本无需挂到场景，通知权限在首次场景加载后自动申请。Android 13+ 弹窗由系统和用户授权状态决定；已授权时跳过。本地与远程共用这一处申请，详见 [Android 通知权限说明](https://developer.android.com/develop/ui/views/notifications/notification-permission)。

### 本地通知

| 本包文件 / 文件夹 | 目标位置 |
| --- | --- |
| `Local/RetentionNotificationKit/Package/com.tiger.retention/` | 完整复制成 `Packages/com.tiger.retention/` |
| `Local/RetentionNotificationKit/ProjectFiles/RetentionSetup.cs` | `Assets/NotificationKit/RetentionSetup.cs` |
| `Local/RetentionNotificationKit/Templates/AndroidManifest.notifications.xml` | 仅本地模式使用的权限片段，合并到自己的 Android Manifest |

合并以下两项到接收方 `Packages/manifest.json` 的 `dependencies` 中，保留其他已有内容：

```json
"com.tiger.retention": "file:com.tiger.retention",
"com.unity.mobile.notifications": "2.4.0"
```

这里的 RetentionNotificationKit 就是本地召回模块的完整源码，不需要原游戏工程，也不引用下载目录。`RetentionSetup` 自动创建常驻对象，不用挂场景。

### 远程通知

| 本包文件 | 目标位置 / 操作 |
| --- | --- |
| `Remote/FirebaseMessagingKit/ProjectFiles/FirebaseMessagingSetup.cs` | `Assets/NotificationKit/FirebaseMessagingSetup.cs` |
| `Remote/FirebaseMessagingKit/ProjectFiles/FirebaseNotificationBootstrap.cs` | 没有 Firebase 初始化入口时复制到 `Assets/NotificationKit/`；自动启动，无需挂场景 |
| `Remote/FirebaseMessagingKit/Templates/AndroidManifest.notifications.xml` | 参考片段，按第 5 节合并到自己的 Android Manifest |

Firebase SDK 自行下载导入，明确名称为 **Firebase Unity SDK 13.2.0 的 `FirebaseMessaging.unitypackage`**；建议同时导入同版本 `FirebaseAnalytics.unitypackage`。链接和 EDM4U 要求见 [SDK_REQUIREMENTS.md](SDK_REQUIREMENTS.md)。

如果游戏已有 Firebase 初始化入口，**不用复制 `FirebaseNotificationBootstrap.cs`**。在自己的 `CheckAndFixDependenciesAsync()` 成功且结果为 `DependencyStatus.Available` 后，在 Unity 主线程调用一次：

```csharp
FirebaseMessagingSetup.Initialize();
```

`FirebaseMessagingSetup` 会自动创建常驻对象，不需要手动挂场景。不要只复制这个脚本却漏掉初始化调用。

## 2. 每个游戏先改公共配置

编辑随包提供的 `NotificationProjectConfig.cs`：

```csharp
public const string LocalChannelId = "your_game_recall";
public const string RemoteChannelId = "your_game_push";
public const string RemoteTopic = "your_game_daily_reminders";
```

把 `your_game` 换成目标游戏的标识。`RemoteChannelId` 还要同步到 Manifest 的 `com.google.firebase.messaging.default_notification_channel_id`；主题发送端要与 `RemoteTopic` 相同。

Android 渠道以应用为范围，跨应用同名不代表会串到另一个游戏。使用游戏前缀便于维护；实际隔离还要靠正确的应用包名、Firebase 配置和发送目标。同一 Firebase 项目下不同游戏若订阅同一主题，可能一起收到该主题消息。更换已上线的主题时还要退订旧主题，见 [Firebase Topic Messaging](https://firebase.google.com/docs/cloud-messaging/topic-messaging)。

本地文案在随包的 `RetentionSetup.CreateConfig()` 内，提供英语、葡萄牙语、印尼语，其他系统语言回退英语。接收方可以直接改为自己游戏的文案。

## 3. 本地进度与回流回调

本地适配脚本没有 GameManager、PlayerInfo、SolarEngine 等工程依赖。它用独立的 `PlayerPrefs` 键 `NotificationKit_CurrentProgress` 保存传入的进度；没有进度时使用通用文案。

在游戏读取完自己的存档后同步一次，之后在真正通关并更新关卡时调用：

```csharp
// 读取自己的存档后：savedLevel 换成自己的变量。
RetentionSetup.SetProgress(savedLevel);

// 通关解锁下一关后：nextLevel 换成最新关卡数。
RetentionSetup.OnLevelCompleted(nextLevel);
```

传 `0` 使用不带关卡号的文案。不要每帧调用；每次更新会重排通知时间。

可选的本地回流回调：

```csharp
RetentionSetup.NotificationOpened += OnLocalNotificationOpened;

private void OnLocalNotificationOpened(string key, int progress)
{
    // key 为 d1 / d3 / d7，在这里接收方自行上报埋点。
}
```

请在首个场景的 `Awake()` 或其他足够早的初始化阶段订阅，以便收到冷启动点击；对象销毁时退订。回调同步执行，不负责等待接收方的埋点 SDK 初始化，接收方可自行排队。

### 本地通知调度规则

- 默认 `d1 / d3 / d7` 分别在最近一次调度后的 **20 / 72 / 168 小时**提醒；`d1` 不是自然日判定。
- 启动、真实暂停后返回、更新进度时取消本模块旧预约并重新排期。
- 设备本地时间 23:00（含）至次日 09:00（不含）的时间顺延至接下来的 10:00。
- 使用非精确调度，系统省电或休眠可能延迟；本地通知无需联网和 Google Play 服务。
- 本地核心只按自己保存的 ID 取消通知，没有调用全应用 CancelAll。同一应用有多套通知时仍需协调通知 ID。

## 4. 图标设置

打开 `Edit > Project Settings > Mobile Notifications > Android`，在 Drawable Resources 注册：

| 项目 | 值 |
| --- | --- |
| Id | `notify_small` |
| Type | Small |
| Texture | 本包提供的 `notify_small.png`，或自己替换的透明底白色小图标 |

接入 Local 后可执行 `Tools > Tiger Retention > Setup Project`，其 Editor 工具会开启图标 Read/Write、注册图标并开启设备重启恢复。只接 Remote 时按上表手动配置即可。

不要把图片放到旧式 `Assets/Plugins/Android/res/` 目录。图标由 Mobile Notifications 导出为 Android drawable；源码和模板默认均使用 `notify_small`。

## 5. Android Activity 与 Manifest

### 接了 Firebase 远程通知

导入 SDK 后，把包内 `Remote/FirebaseMessagingKit/Templates/AndroidManifest.notifications.xml` 的内容合并到自己的 `Assets/Plugins/Android/AndroidManifest.xml`。如果已有 Manifest，保留广告和统计 SDK 的其他声明；已有 MAIN / LAUNCHER 入口时统一修改入口，不再额外添加一个。

该模板已提供以下必要内容：

| 配置 | 值 |
| --- | --- |
| 权限 | `INTERNET`、`POST_NOTIFICATIONS` |
| Unity 启动入口 | `com.google.firebase.MessagingUnityPlayerActivity` |
| `custom_notification_android_activity` | 同上 |
| `com.google.firebase.messaging.default_notification_channel_id` | 改成自己的 `NotificationProjectConfig.RemoteChannelId` |
| `com.google.firebase.messaging.default_notification_icon` | `@drawable/notify_small` |

在 `Project Settings > Mobile Notifications > Android` **开启 Use Custom Activity**，填入完整类名 `com.google.firebase.MessagingUnityPlayerActivity`。同时开启 Reschedule Notifications on Device Restart，保持非精确调度。

如果目标项目有必须保留的自定义 Activity，应让它继承 Firebase 入口或按官方要求处理生命周期和 Intent，再让 Manifest、Mobile Notifications、`custom_notification_android_activity` 全部指向最终入口。参考 [Firebase 自定义 Activity 文档](https://firebase.google.com/docs/cloud-messaging/unity/get-started#configure_a_custom_entry_point_activity)。

13.2.0 SDK 的 Editor 生成器会在构建前生成 Firebase Activity，相关 Editor 文件不能漏掉。FCM 服务由 SDK 原生 AAR 的 Manifest 合并提供，先检查合并结果，避免手写重复或混入旧版本服务声明。

### 只接本地通知

不需要 Firebase Activity，也不需要 `google-services.json`。声明 `POST_NOTIFICATIONS`、配置通知图标、开启设备重启恢复即可。只有目标项目存在自定义入口 / 多 Activity 选择问题时，才把 Mobile Notifications 的 Use Custom Activity 指向**这个游戏实际的有效入口类**；不要填写未安装的 Firebase 类。

### 为什么要统一入口

`Multiple choices for activity for notifications` 表示 Mobile Notifications 无法确定通知点击入口。渠道注册可能因此异常，中断后续初始化。接了 Firebase 和其他 SDK 的项目必须显式核对以上配置，不能只看 C# 编译是否通过。设置含义见 [Unity Mobile Notifications 设置](https://docs.unity3d.com/Packages/com.unity.mobile.notifications@2.4/manual/Settings.html)。

## 6. Firebase 项目与依赖解析

1. Unity Player Settings 配置自己的 Android Application Identifier。
2. 在自己的 Firebase 项目注册相同包名的 Android 应用，下载该应用的 `google-services.json`，放到自己的 `Assets/` 根目录。
3. 导入 Messaging SDK；按需导入同版本 Analytics，保留 EDM4U、Editor 生成器和 Android 原生依赖。
4. 执行 `Assets > External Dependency Manager > Android Resolver > Force Resolve`，等待成功。
5. 使用本包的自动初始化入口，或在自己已有的成功初始化分支调用 `FirebaseMessagingSetup.Initialize()`，然后打包。

13.2.0 的 Android 解析结果包含 `firebase-app-unity:13.2.0`、`firebase-messaging-unity:13.2.0` 和原生 `firebase-messaging:25.0.0` 等依赖，以 EDM4U 实际解析结果为准。不要只复制 Messaging DLL，也不要从其他游戏照搬整套 Gradle 文件。

## 7. 远程通知当前做了什么

| 场景 | 当前处理 |
| --- | --- |
| 前台收到标准 notification | 进入 `OnMessageReceived`，读取服务端标题正文，通过原生 `NotificationCompat.Builder` 发到系统通知栏 |
| 后台收到标准 notification | Android / FCM 显示系统通知栏，不要求立刻出现 Unity 接收日志 |
| 点击已有后台通知 | 打开当前应用，回调标记为 `NotificationOpened` 时不重复发一份 |
| 纯 data-only 消息 | 只记录消息日志，当前代码不生成可见通知 |
| Token 获取 / 刷新 | 写入 `FCMDeviceToken`，输出 `[FCM] Device token: ...` |
| 主题订阅 | 订阅 `NotificationProjectConfig.RemoteTopic`，异步成功后才打印成功日志 |

保留原实现的八条随机文案和 `ShownNotificationIndexes` 索引轮换；接收方按产品需求修改这些文案。**实际展示正文仍是服务端 `Notification.Body`**，随机结果当前只用于日志和索引轮换。这里生成的是系统通知，不是游戏内评分或奖励弹窗。

本地与远程均使用 Default 重要级别，应查看下拉通知栏，不保证顶部横幅。显示还受系统权限、渠道开关、免打扰等设置影响。前后台规则见 [Firebase Unity 消息投递说明](https://firebase.google.com/docs/cloud-messaging/unity/get-started#message_delivery_on_android)。

## 8. 本地通知快速测试

1. 在 `NotificationProjectConfig.cs` 临时改 `FirstRecallDelayHours = 0.02`（约 72 秒），`AvoidNightHours = false`。
2. 打包安装，进入游戏允许通知，确认 `[Retention] 已预约 d1` 和通知链就绪日志。
3. 按 Home 切到后台，等待通知。非精确调度可能延迟；等待期间不要反复进入游戏，否则会重排。
4. 核对通知所属应用、图标、文案，点击应回到当前游戏；核对 `d1` 回流日志 / 回调。
5. 同步一次进度并通关，核对预约文案中的关卡号。
6. 测试后恢复 20 / 72 / 168 小时和 `AvoidNightHours = true`，再打正式包。

Editor 只输出本地预览，不实际发通知。不要用系统“强行停止”或 `adb shell am force-stop` 模拟正常切后台；强停后重新打开再测。Android 15 强停还会取消 PendingIntent，见 [Android 停止状态规则](https://developer.android.com/about/versions/15/behavior-changes-all#stopped-state)。

## 9. Firebase 控制台与测试

远程测试设备需要具备可用 Google Play 服务，网络能连接 FCM。游戏可以未上架，安装后先打开完成初始化和授权即可，不要求一直保持前台。

### 按应用受众发送

在自己的 Firebase 项目 Messaging 中创建通知，填写标题和正文，目标选择正确的 Android 应用及受众，选择立即发送。可以给该应用的全部用户发送，不强制要求 Token；其他测试安装也可能在目标范围内。

### 按主题发送

选择主题目标，填写与 `RemoteTopic` 一致的主题名。先确认手机日志出现 `[FCM] 已订阅主题: ...`。主题发送和按应用受众发送是不同的目标选择方式。

### 可选的单设备测试

在 `adb logcat -s Unity` 中复制当前 `[FCM] Device token: ...`，在控制台“发送测试消息”中填入完整 Token。Token 对应设备上的应用安装，重装、清数据或刷新后可能变化。官方步骤见 [Firebase 发送测试通知](https://firebase.google.com/docs/cloud-messaging/android/get-started#send_a_notification_message)。

先 Home 切后台发一条，应在系统通知栏看到消息；再保持前台发一条，应看到 `[FCM] Message received:`、通知调试日志及系统通知。两种状态都要测试点击是否回到当前游戏。代码日志中的“通知显示成功”表示已调用系统 `notify()`，最终可见性以通知栏为准。

## 10. 常见问题与交付检查

| 现象 | 检查 |
| --- | --- |
| 没有 Token | 初始化入口是否调用、依赖是否 Available、前面是否已有渠道 / Activity 异常 |
| 多 Activity 选择异常 | Manifest、Mobile Notifications 自定义 Activity 和元数据是否一致 |
| Firebase 入口类不存在 | SDK 的 Editor Activity 生成器、asmdef、原生 AAR 是否完整 |
| 编译成功但手机初始化失败 | Force Resolve、原生依赖、SDK 版本和自己的 Firebase 配置 |
| 收到回调但没有通知 | 是否为纯 data-only；权限、渠道开关、图标资源、JNI 异常 |
| 有通知栏消息但无顶部横幅 | Default 重要级别、渠道设置与免打扰；这不等于收不到 |
| 本地测试没有按 72 秒出现 | 夜间保护是否关闭、是否被重排或强停、系统是否延迟调度 |
| 通知属于另一个游戏 | 长按通知查看所属应用，核对应用包、Token、Firebase 目标和主题 |
| 同一游戏通知相互覆盖 | 不同渠道仍共享应用内 ID 空间；当前远程前台使用随机 ID，没有单独 tag |

交付前在接收方打出的新包上确认：开局授权、本地出现与回流、进度更新、远程前后台显示、点击打开正确、配置和测试延时已恢复。源码编译检查不替代真机收发验收。
