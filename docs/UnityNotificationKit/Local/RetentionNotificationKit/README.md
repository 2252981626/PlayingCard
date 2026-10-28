# 本地召回通知模块

本文件夹提供本地通知所需源码：

- `Package/com.tiger.retention/`：本地通知内嵌包。
- `ProjectFiles/RetentionSetup.cs`：通用启动、语言文案、进度同步和点击回流接口。
- `Templates/AndroidManifest.notifications.xml`：仅本地模式需要合并的权限片段。

同时使用根目录的 `Common/ProjectFiles/`。接收方安装 Unity Mobile Notifications 2.4.0 即可，不需要 Firebase SDK、Google Play 服务或推送后台。

完整操作见 [接入指南](../../INTEGRATION.md)，依赖见 [SDK 要求](../../SDK_REQUIREMENTS.md)。
