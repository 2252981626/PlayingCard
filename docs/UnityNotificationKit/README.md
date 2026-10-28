# Unity Android 通知接入包

这是可以单独转交的通知代码与接入说明。接收方使用本文件夹中的文件即可接入自己的 Unity 项目。

## 内容

| 目录 / 文件 | 内容 |
| --- | --- |
| [Local/RetentionNotificationKit](Local/RetentionNotificationKit) | 本地召回通知完整源码包，以及通用 `RetentionSetup.cs` |
| [Remote/FirebaseMessagingKit](Remote/FirebaseMessagingKit) | 远程通知接收与显示代码、自动初始化脚本、Android Manifest 配置模板 |
| [Common/ProjectFiles](Common/ProjectFiles) | 两类通知共用的配置、开局权限申请脚本和小图标 |
| [SDK_REQUIREMENTS.md](SDK_REQUIREMENTS.md) | SDK 全称、版本、用途、官方下载链接和导入方式 |
| [INTEGRATION.md](INTEGRATION.md) | 文件放置、项目配置、进度回调、发送测试和排错步骤 |

## 接收方从哪里开始

1. 先阅读 [SDK_REQUIREMENTS.md](SDK_REQUIREMENTS.md)，安装所选模块的依赖。
2. 按 [INTEGRATION.md](INTEGRATION.md) 的复制表使用本包文件；只要本地通知就选 Common + Local，只要远程就选 Common + Remote。
3. 修改 `NotificationProjectConfig.cs` 中的游戏渠道和主题，使用自己的 Firebase 配置，再做真机测试。

本地模块已去掉特定游戏的存档类和埋点类，提供 `SetProgress()`、`OnLevelCompleted()` 和 `NotificationOpened` 回调。远程保留原来的原生 Android 通知显示流程。

Firebase SDK、Unity Mobile Notifications、EDM4U 由接收方自行安装，本包不附带这些 SDK 二进制，也不包含任何游戏的 `google-services.json`、设备 Token 或签名文件。

适用基线：Unity 2022.3、Android。本包不是 iOS 接入包。
