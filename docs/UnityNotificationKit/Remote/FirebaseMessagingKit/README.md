# Firebase 远程通知模块

本文件夹提供远程接收和显示代码，不附带 Firebase SDK：

- `ProjectFiles/FirebaseMessagingSetup.cs`：主题订阅、Token、消息接收和 Android 原生系统通知。
- `ProjectFiles/FirebaseNotificationBootstrap.cs`：没有已有初始化器时使用的自动入口。
- `Templates/AndroidManifest.notifications.xml`：Firebase Activity、通知渠道、图标和权限配置模板。

同时使用根目录的 `Common/ProjectFiles/`。接收方需要安装 **Firebase Unity SDK 13.2.0 的 FirebaseMessaging.unitypackage**、**Unity Mobile Notifications 2.4.0** 和 **EDM4U**；建议同时安装同版本 FirebaseAnalytics.unitypackage。

具体版本和下载见 [SDK 要求](../../SDK_REQUIREMENTS.md)，文件使用与测试见 [接入指南](../../INTEGRATION.md)。
