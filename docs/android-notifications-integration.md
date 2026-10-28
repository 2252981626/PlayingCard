# Unity Android 通知独立接入包

对外发送整个 **UnityNotificationKit** 文件夹或它的 ZIP 即可。文件夹已提供本地通知源码、远程通知接收代码、公共配置与 Android 模板；接收方自行安装文档指定的 SDK。

- [接入包首页与目录](UnityNotificationKit/README.md)
- [SDK 名称、版本与下载方式](UnityNotificationKit/SDK_REQUIREMENTS.md)
- [接入步骤与真机测试](UnityNotificationKit/INTEGRATION.md)
- [本地 RetentionNotificationKit](UnityNotificationKit/Local/RetentionNotificationKit/README.md)
- [远程 FirebaseMessagingKit](UnityNotificationKit/Remote/FirebaseMessagingKit/README.md)

本地适配已去掉原游戏存档、GameManager 和埋点接口依赖。远程使用 Firebase Unity SDK 的 Cloud Messaging 13.2.0；SDK 本体、游戏专属 Firebase 配置和签名文件不在分发包内。
