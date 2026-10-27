# Git_PlayingCard

Unity 2022.3.62f3 项目。CI 通过 GitHub Actions 自动打包 Android APK / AAB。

---

## 玩家召回通知（Android）

通知包内置于 `Packages/com.tiger.retention`，依赖 Unity Mobile Notifications 2.4.0，
随仓库一起分发，无需引用下载目录或另行导入 `.unitypackage`。

- 项目配置：`Assets/scripts/SDK/RetentionSetup.cs`。启动及从后台返回时，按存档中的
  当前关卡重排 20 / 72 / 168 小时后的本地通知；主线通关后更新关卡文案。
- 文案跟随游戏支持的系统语言：英语、葡萄牙语、印尼语。23:00–09:00 的通知顺延至 10:00。
- 进入游戏时立即请求通知权限，每次运行只请求一次；已授权时不会重复弹窗。
- 点击回流通过现有 SolarEngine 入口上报 `retention_notification_open`，参数为
  `notification_key`（`d1` / `d3` / `d7`）及 `level_id`；SDK 尚未就绪时暂存，普通后台返回不会重复上报。
- 图标 `Assets/Plugins/Android/notify_small.png` 的注册和设备重启恢复设置由包内 Editor 工具维护。
  项目保持非精确调度，实际通知时间可能受系统调度影响。

编辑器仅输出 `[Retention]` 预览日志。自动测试位于 `Assets/Tests/Editor/RetentionNotificationTests.cs`，
通过 Test Runner 的 EditMode 运行；覆盖夜间边界、回流去重和无存档进度的多语言文案。
实际发送和点击回流需在安卓真机上验证；快速测试可临时缩短 `RetentionSetup.cs` 中的通知间隔，
同时关闭 `AvoidNightHours`，测试后恢复正式值。

---

## Firebase 后台推送（Android）

Firebase Messaging 13.2.0 随仓库分发。场景中已有的 `FirebaseAppCheckManager`
在依赖检查成功后调用 `Assets/scripts/SDK/FirebaseMessagingSetup.cs`，无需再挂组件。
Android 启动入口为 `com.google.firebase.MessagingUnityPlayerActivity`，由 SDK 的
Editor 脚本在构建前生成 Java 源码；原有 SolarEngine 和 Adjust 配置保留。

- 开局通知权限沿用 `RetentionSetup` 的请求。
- 订阅主题 `daily_reminders`；仅在异步订阅成功后输出成功日志。
- 初始 Token 和后续刷新 Token 均保存到 `PlayerPrefs` 的 `FCMDeviceToken`。
  真机日志输出 `[FCM] Device token: ...`，可用于 Firebase 控制台的发送测试消息。
- 前台接收时显示服务端发送的标题和正文，也支持 data 中的 `title` / `body`。
  后台标准 notification 消息由 Android/FCM 自动显示；点击通知不会再生成一份。
- 默认远程推送渠道为 `solitaire_casino_tycoon_push`，图标为 `notify_small`。
  本地召回通知继续使用 `solitaire_casino_tycoon_recall`。

测试时重新打包安装，启动并允许通知权限，从 `adb logcat -s Unity` 复制最新 Token，
在 Firebase 项目 `solitaire-casino-tycoon` 的 Messaging 中发送测试消息。
先把游戏按 Home 切到后台测试通知栏，再回到前台测试接收日志和前台通知。
使用带 Google Play 服务且能连接 FCM 的 Android 设备，避免使用系统强行停止应用来测试。
主题推送的目标填写 `daily_reminders`；后台 data-only 消息不保证直接显示通知。

---

## Firebase 依赖管理说明

> 这一节很重要 —— 换机器、重新克隆仓库后请先读这里。

### 现状

Firebase Unity SDK **13.2.0**（`firebase_app` + `firebase_analytics` + `firebase_messaging`）是通过官方
`.unitypackage` 导入到 `Assets/` 下的，依赖解析由 **External Dependency Manager (EDM4U)**
负责。

导入后 SDK 会把**全平台**的原生库一次性释放到项目里，总计约 **360MB**。本项目 CI 只构建
Android，其中绝大部分对 Android 打包毫无用处，因此**不纳入版本库**：

| 路径 | 体积 | 是否提交 | 说明 |
|---|---|---|---|
| `Assets/Firebase/Plugins/x86_64/` | ~300MB | ❌ 忽略 | 桌面端 (Standalone) 原生库，仅 Windows/macOS/Linux 构建需要 |
| `Assets/Plugins/iOS/Firebase/` | ~30MB | ❌ 忽略 | iOS 静态库 |
| `Assets/Plugins/tvOS/Firebase/` | ~30MB | ❌ 忽略 | tvOS 静态库 |
| `Assets/Firebase/Editor/` | ~16MB | ✅ 提交 | 编辑器 DLL + 依赖描述 XML |
| `Assets/Firebase/m2repository/` | ~22MB | ✅ 提交 | **Android 必需** —— AAR 仓库，EDM4U 从这里取依赖 |
| `Assets/Firebase/Plugins/*.dll` | ~250KB | ✅ 提交 | C# 托管包装层 |
| `Assets/ExternalDependencyManager/` | — | ✅ 提交 | EDM4U，Android 依赖解析必需 |

即：**提交约 38MB，忽略约 358MB**。

> 备注：`Assets/Plugins/iOS.meta` / `Assets/Plugins/tvOS.meta` 也一并被忽略 —— 这两个
> 目录里除了 Firebase 静态库没有别的东西，留着空目录的 meta 没有意义。若以后要往这两个
> 目录放非 Firebase 的 iOS/tvOS 插件，记得先把对应忽略规则从 `.gitignore` 里去掉。

### 新机器 / 全新克隆后

Android 打包**无需任何额外操作** —— 克隆下来直接就能在 CI 或本地出包。

只有当你需要构建 **iOS / tvOS / Windows / macOS / Linux** 时才需要补齐：

1. 下载 Firebase Unity SDK 13.2.0
   （<https://firebase.google.cn/download/unity>，或从
   <https://developers.google.cn/unity/archive> 取单个 `.unitypackage`）
2. Unity 中 `Assets > Import Package > Custom Package`，导入
   `FirebaseAnalytics.unitypackage` + `FirebaseMessaging.unitypackage`（均包含 App 依赖）
3. 被忽略的文件会自动重新释放到上述路径

> ⚠️ 重新导入后**不要** `git add -f` 那些大文件。`.gitignore` 已经拦好了，
> 如果 `git status` 里又冒出它们，说明忽略规则被改坏了。

### ⚠️ 改动 Firebase 依赖后必须重新解析

本项目的 `ProjectSettings/GvhProjectSettings.xml` 里
`GooglePlayServices.AutoResolverEnabled = False`（自动解析是关的），所以**依赖解析结果
靠提交固化**，不会在构建时自动刷新。

`Assets/Plugins/Android/mainTemplate.gradle` 的
`// Android Resolver Dependencies Start/End` 区间，以及
`ProjectSettings/AndroidResolverDependencies.xml`，都是 EDM4U 的**输出**。

**当你新增/升级/移除任何带 `Dependencies.xml` 的 SDK（Firebase、Adjust、MaxSdk、
SolarEngine 等）后，必须手动跑一次：**

```
Unity 菜单：Assets > External Dependency Manager > Android Resolver > Force Resolve
```

然后把这两个文件的变化一起提交。**否则 CI 打出来的包里不会有新 SDK 的 Android 依赖**，
表现为编译能过、但运行时初始化失败。

### 为什么不用 UPM / Git LFS

评估过（2026-09 结论）：

- **Firebase 官方 Git 仓库不支持直接当 UPM 源**。`firebase/firebase-unity-sdk` 仓库里
  **没有** `firebase_analytics_upm` 这类目录（UPM 包需要用仓库自带的
  `build_package.py --output_upm=True` 自行构建），所以
  `"com.google.firebase.analytics": "https://github.com/firebase/firebase-unity-sdk.git?path=/firebase_analytics_upm"`
  这种写法**解析必然失败**。
- 官方支持的 UPM 路线是「下载 `.tgz` → 放 `GooglePackages/` → `file:` 引用」，但
  `com.google.firebase.app` 的 `.tgz` 本身就有几十 MB，提交它省不下多少体积；改成 CI
  构建时下载则引入外部网络依赖，反而更脆。
- **Git LFS**：GitHub 免费额度仅 1GB 存储 / 1GB 月流量，360MB 二进制一推就爆，不适合。

结论：**保留 `.unitypackage` 导入结构，只把与 Android 无关的平台二进制排除出版本库**，
是体积、构建稳定性、迁移成本三者最优的组合。

---

## 本地构建

1. Unity Hub 安装 **2022.3.62f3**
2. 打开本仓库根目录
3. 首次打开会自动解析包并运行 EDM4U 的 Android 依赖解析（需要联网）
4. `File > Build Settings > Android` 出包

签名文件 `Assets/mySignKey.keystore` 随仓库一起提交（密码走 GitHub Secrets）。

## CI

- `.github/workflows/build-apk.yml` —— 手动触发，产出 APK
- `.github/workflows/build-aab.yml` —— 手动触发，产出 AAB

两个 workflow 在构建前都会跑一步 **Firebase 依赖完整性校验**，做两件事：

1. 检查关键文件（托管 DLL、依赖描述 XML、m2repository 里的 srcaar、EDM4U、Messaging Activity 生成器、
   `mainTemplate.gradle`、`google-services.json`）是否存在
2. 检查 `mainTemplate.gradle` 里是否已经写入了 `com.google.firebase:firebase-analytics`
   等 Android 依赖 —— **这一条专门用来拦截「忘记跑 Force Resolve」**

任一项失败都会用 `::error::` 在日志里标出具体原因并立即中止，避免出现「编译通过、
打出来的包 Firebase 起不来」这种最难查的情况。
