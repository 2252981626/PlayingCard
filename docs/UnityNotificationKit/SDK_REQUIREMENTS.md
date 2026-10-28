# 通知接入需要哪些 SDK

## 依赖选择

| 需要的功能 | 接收方需要安装 |
| --- | --- |
| 仅本地通知 | Unity Mobile Notifications 2.4.0；本包已提供 RetentionNotificationKit 源码 |
| 仅 Firebase 远程通知 | Unity Mobile Notifications 2.4.0、Firebase Unity SDK 的 Cloud Messaging 13.2.0、EDM4U；建议同时导入 Analytics 13.2.0 |
| 本地 + 远程 | 上述 SDK 各安装一份，共用 Mobile Notifications 和 Android 通知权限 |

## Unity Mobile Notifications

- 完整名称：**Unity Mobile Notifications**。
- UPM 包名：`com.unity.mobile.notifications`。
- 本次接入版本：**2.4.0**。
- 用途：本地通知调度、渠道注册、Android 权限状态和通知图标导出。当前远程代码也使用它，所以只接远程时仍要安装。
- 安装：Unity 菜单 `Window > Package Manager > + > Add package by name`，名称填 `com.unity.mobile.notifications`，版本填 `2.4.0`。
- 官方说明：[Android 用法](https://docs.unity3d.com/Packages/com.unity.mobile.notifications@2.4/manual/Android.html)、[设置与自定义 Activity](https://docs.unity3d.com/Packages/com.unity.mobile.notifications@2.4/manual/Settings.html)。

## Firebase Cloud Messaging for Unity

- SDK 产品：**Firebase Unity SDK**。
- 需要的模块：**Firebase Cloud Messaging，简称 FCM / Messaging**。
- 本次接入版本：**13.2.0**。
- Unity 导入包名称：**`FirebaseMessaging.unitypackage`**。
- 官方指定版本下载：[FirebaseMessaging 13.2.0 .unitypackage](https://dl.google.com/firebase/sdk/unity/dotnet4/FirebaseMessaging_13.2.0.unitypackage)。
- 官方归档：[Google packages for Unity](https://developers.google.com/unity/archive)。
- 官方接入文档：[FCM for Unity](https://firebase.google.com/docs/cloud-messaging/unity/get-started)。

下载后在 Unity 使用 `Assets > Import Package > Custom Package` 导入，保留该包附带的 Firebase App 基础依赖、Android 原生库、Editor 依赖描述和 Messaging Activity 生成器。这里需要的是 **Unity 的 FirebaseMessaging 导入包**，不能用 Firebase JavaScript SDK、Android Studio 的 Java 接入步骤或一个单独 DLL 代替。

本包远程脚本使用 13.2.0 的 `TokenReceived`、`GetTokenAsync()`、`MessageReceived` API。接收方已有 Firebase 时，把 App / Messaging / Analytics 统一到兼容的同一版本；升级后需重新核对 API 和构建结果。

## Firebase Analytics for Unity

- 模块：**Google Analytics for Firebase / Firebase Analytics**。
- 本次版本：**13.2.0**。
- 导入包：**`FirebaseAnalytics.unitypackage`**。
- 官方下载：[FirebaseAnalytics 13.2.0 .unitypackage](https://dl.google.com/firebase/sdk/unity/dotnet4/FirebaseAnalytics_13.2.0.unitypackage)。

这是 Firebase 官方推荐搭配的模块；需要复用控制台受众、运营统计方案时一并导入。纯按 Token / Topic 接收的脚本没有直接调用 Analytics C# API。App 基础依赖由官方模块导入包提供，不需要 Firebase Auth、Firestore 或 In-App Messaging。

## External Dependency Manager for Unity

- 全称：**External Dependency Manager for Unity**，简称 **EDM4U**。
- 本次基线版本：**1.2.186**。
- 用途：解析 Firebase 的 Android Maven / Gradle 依赖。
- 导入 Firebase 官方 Unity 包时保留其附带的 External Dependency Manager 相关文件；已有 EDM4U 的项目保留兼容的一套。
- 官方项目：[googlesamples/unity-jar-resolver](https://github.com/googlesamples/unity-jar-resolver)。

导入完成后必须执行：

```text
Assets > External Dependency Manager > Android Resolver > Force Resolve
```

解析会下载 Android 原生依赖，因此需要网络。本包不提供这些 SDK 二进制，也不要求从其他游戏复制 Gradle 解析结果。接收方在自己的项目里执行解析即可。

## 接收方自行准备的 Firebase 配置

在自己的 Firebase 项目里注册 Android 应用，包名必须与 Unity Player Settings 中的 Android Application Identifier 一致。下载该应用的 **`google-services.json`**，放到接收方 Unity 项目的 **`Assets/google-services.json`**。

该文件不是通用 SDK，不能复用其他游戏的文件。普通控制台通知无需自建推送后台，也无需把服务账号私钥放入客户端。
