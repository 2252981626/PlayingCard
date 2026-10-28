# NF Shoot SDK — Unity 2022.3.62f3c1

按当前 Android 文件和你补充的 `ShotSdk` 封装。Java 包名、广告参数、广告 ID、事件 JSON、Application/Activity 广告初始化链，以及 `ShootSdk.Instance` 的游戏调用接口均保留。两个供应商 AAR 原样交付，13 个原 Java 文件和一个 Firebase Intent 适配类编译成 `nf-unity-bridge.aar`，未将 Unity 引擎或供应商代码重复合入。MainActivity 仅追加推送回调，原 onCreate 广告初始化和 UnityActivity 广告生命周期代码保持原样。

## 导入与构建

1. 在原 Unity 工程导入 `dist/NFShootSDK-Unity2022.3.unitypackage`，或者将 ZIP 中的 `Assets/NFShootSDK` 复制到工程的 `Assets`。
2. 包中的 `Runtime/ShotSdk` 是你提供的四个 C# 文件的接入副本，保留原 `.meta` GUID。**替换原四个脚本，不要并存同名类**。如果原工程已经有这些 GUID，导入时确认其目标路径并替换，避免重复 GUID。原文件快照仍位于 `UnitySDK/ShotSdk`，未修改。
3. 清理旧的同名广告 AAR，以及单独导入的 `com.common.*`、`com.nf.java.Shoot.*`、`com.example.myapplication.MainActivity/MyApplication` Java/AAR，避免重复类。
4. 原工程需要保留现有 **Adjust Unity SDK 5.x（`AdjustSdk` 命名空间）**，继续执行收入回传。本包不重复安装第三方 Unity 插件。OPS Obfuscator 为可选依赖：执行 Configure Android Player 时若检测到 OPS，会添加 `NFSDK_OPS_OBFUSCATOR` 启用原混淆保护属性；未安装 OPS 也可以编译。IL2CPP 保留规则始终启用。
5. Preferences > External Tools 中保留 **Unity 自带 JDK 11**，将 Gradle 改为外部 **Gradle 8.13**。执行 **Tools > NF Shoot SDK > Select Gradle JDK 17**，选择另一个 JDK 17 的根目录（macOS 为 `Contents/Home`）。插件通过 `org.gradle.java.home` 让 Gradle daemon 使用 JDK 17，避免 Unity 2022 对编辑器 JDK 版本的校验冲突。
6. NDK 使用 Unity 自带 `23.1.7779620`。Android SDK 安装 `platforms;android-36` 和 `build-tools;36.1.0`，并保留 Unity 能识别的 Command-line Tools（Unity 自带为 `6.0`）。可以在 Unity 自带 SDK 中补齐这两个包。仅有平台和 Build Tools、没有 `sdkmanager` 的目录会被 Unity 拒绝。
7. 执行 **Tools > NF Shoot SDK > Configure Android Player**，设置 IL2CPP、ARMv7 + ARM64、minSdk 28、targetSdk 36。保留游戏包名、版本号、图标和签名配置，直接 Build APK/AAB。

编辑器脚本只修改本次生成的 Gradle 工程，接入依赖、仓库、AGP 8.13.1、SDK 版本和 Manifest，不覆盖原工程的 Gradle 模板。直接启动 Unity 的环境可以设置 `NFSDK_JAVA_HOME`（Gradle daemon JDK）、`NFSDK_GRADLE_HOME`、`NFSDK_ANDROID_SDK_ROOT`。GameCI Docker 构建请使用下面的工作流和命令行参数。

## GitHub Actions / GameCI v4

GameCI v4 在 Docker 内运行 Unity，只挂载工作区和少数目录，并且只转发固定的环境变量。`actions/setup-java` 在宿主机安装的 `/usr/lib/jvm/...` 不会因此出现在容器内，步骤的 `env: NFSDK_JAVA_HOME` 也不会自动传入。仅设置这两项仍会触发插件的 JDK 路径校验。

使用包内的示例 **`Assets/NFShootSDK/Editor/CI/UnityAndroid.yml`**，将它复制到原 Unity 仓库的 **`.github/workflows/UnityAndroid.yml`**，替换旧打包工作流。SDK 和工作流必须一起更新。原流程如果配置了可用的 `customImage`，请保留该镜像设置；此方案不切换 Unity 编辑器版本。

工作流先运行 `select-gameci-java.sh` 自动选择 runner 已安装的兼容 JDK，随后准备 SDK 组件并运行 `prepare-gameci-tools.sh`，把可移植的 Linux JDK、Gradle 8.13 和完整 SDK 组件放入 `.ci-tools`。GameCI 将工作区挂载为 `/github/workspace`，因此 Unity 可以用以下容器内路径访问工具：

```yaml
with:
  dockerWorkspacePath: /github/workspace
  customParameters: >-
    -nfSdkCi
    -nfSdkJavaHome /github/workspace/.ci-tools/jdk
    -nfSdkGradleHome /github/workspace/.ci-tools/gradle-8.13
    -nfSdkAndroidSdkRoot /github/workspace/.ci-tools/android-sdk
```

命令行参数优先于同名环境变量，再回退到本地 JDK 偏好设置。`-nfSdkCi` 仅为本次 Android 构建设置 minSdk 至少 28、targetSdk 36，兼容 GameCI 的 `AndroidApiLevelAuto` 输入。Unity 本身仍使用镜像内置的 JDK 11。准备脚本保留可执行权限并校验 Gradle 下载的 SHA-256，不需要另外构建 Docker 镜像。

工作流不再使用 `setup-java` 固定安装某个 JDK 版本：优先使用当前 `JAVA_HOME`，再检查 `JAVA_HOME_*` 和 `/usr/lib/jvm` 中的已有 JDK。所选版本必须在 17–23 范围内；只有 JDK 11 时会明确报错，不会在本步骤自动安装新 JDK。这个范围来自当前 AGP 和 Gradle 的兼容要求，自动检测并不取消工具链的最低版本限制。

### AAB 工作流版本

将 `Assets/NFShootSDK/Editor/CI/UnityAndroidAAB.yml` 复制到 Unity 仓库的 `.github/workflows/UnityAndroidAAB.yml`，在 Actions 中手动运行 **Unity Android AAB Build & Sign**。它沿用 APK 版的工具准备、磁盘检查和签名 Secrets，并按照 [GameCI 的 AAB 参数](https://game.ci/docs/github/builder/#androidexporttype)设置 `androidExportType: androidAppBundle`。产物从 `build/Android/**/*.aab` 上传到 **Android-Release-AAB**。

已有工程只需添加这个工作流；需要补齐 CI 文件时，可导入 `dist/NFShootSDK-AAB-CITools.unitypackage`，其中只有 AAB 示例工作流和三个工具脚本。两份示例工作流均保留，手动选择需要的版本；SDK 广告文件没有变化。此版本已检查 YAML、参数、上传路径及脚本语法，尚未从本环境运行完整远程 AAB 构建。

### Gradle 报 cacerts 不存在或不可读

Runner 上的 JDK 可能将 `lib/security/cacerts` 链接到 `/etc/ssl/certs/java/cacerts`，也可能有其他运行时配置指向宿主机目录。保留这些链接复制 JDK 时，宿主机路径不一定存在于 GameCI 容器中，Gradle 会报 `Trust store file .../lib/security/cacerts does not exist or is not readable`。

准备脚本现使用 `stage-gameci-jdk.py` 复制链接指向的实际文件，并要求 staged JDK 的 `cacerts` 是可读、非空的普通文件。其他缺失的运行时配置也会在准备阶段报错；仅允许跳过未安装的可选 `src.zip` 源码链接。不关闭 TLS 校验，也不改变原 JDK 的信任证书集合。

更新 `Editor/CI` 目录中的脚本和工作流，保持容器参数指向 `/github/workspace/.ci-tools/jdk`。旧流程里的 `.ci-tools/jdk-17` 名称不要与新版目录混用。此证书库用于 Java 下载 HTTPS 依赖，与 APK 签名 keystore 无关。

Secrets 名称沿用原流程：`UNITY_LICENSE`、`UNITY_EMAIL`、`UNITY_PASSWORD`、`CI_BUNDLE_ID`、`ANDROID_KEYSTORE_BASE64`、`ANDROID_KEYSTORE_PASS`、`ANDROID_KEY_ALIAS`、`ANDROID_KEY_PASS`。签名文件通过 GameCI 原生 `androidKeystoreBase64` 输入提供，不需要把密钥内容放入 shell 命令。

### Docker 解压镜像时磁盘不足

`docker: failed to register layer ... Unity_s.debug: no space left on device` 发生在 Unity 启动之前，是 Runner 的 Docker 存储空间不足。降低 JDK、AGP 或修改游戏代码不能修复这个阶段。

工作流已固定使用 `ubuntu-24.04`，在拉取镜像前清理 GitHub-hosted Runner 的预装 .NET、Haskell、CodeQL 和完整 Android SDK，然后在独立临时目录仅安装需要的 SDK 组件。组件复制进 `.ci-tools` 后会清理临时源目录。

首次构建请保持 Run workflow 中的 **restore_library_cache=false**（默认值）。整个 `Library` 缓存可能很大；只有首次成功且磁盘有余量后再开启。工作流在镜像拉取前记录 Docker 所在文件系统的容量、inode 和目录大小，默认预留 **35 GiB**。这是用于镜像解压与后续编译的保守预算，不是 Unity 的官方固定最低要求，可在工作流 `NFSDK_MIN_FREE_GIB` 调整。

如果清理后仍未达到预算，或镜像解压仍耗尽空间，需要采用更大磁盘的 Runner 或自托管 Runner。此时下载 `Android-Build-Logs` 中的 `nfsdk-disk-*.log` 查看实际占用。此次修复只需更新工作流，现有 SDK 的 JDK 容器路径方案继续使用。

### 启动后加载页卡住，日志出现 Failed to determine Activity

`Unity.Notifications.Android` 初始化时需要确定通知点击后打开的 Activity。此 SDK 使用自定义 `com.example.myapplication.MainActivity`，广告 SDK 同时会声明其他 Activity，通知插件的自动查找可能失败。如果游戏在关闭加载页时初始化通知，异常会中断关闭流程。

构建脚本现会在生成的 `unityLibrary/src/main/AndroidManifest.xml` 的 `<application>` 中设置：

```xml
<meta-data android:name="custom_notification_android_activity"
           android:value="com.example.myapplication.MainActivity"
           tools:replace="android:value" />
```

这里的 `tools` 命名空间由构建脚本自动写入，已有通知元数据会更新，重复构建不会重复添加。若手动设置 Unity 的 Mobile Notifications > Android > Use Custom Activity，Activity 名称也应为上述完整类名。

### 本次通知、推送和入口补丁

> **⚠ 历史记录，非当前工程操作步骤**
>
> 以下原文仅保留用于追溯旧工程的通知、推送和入口适配。文中的“本次”“当前游戏”均为历史语境，不代表当前工程需要执行这些步骤，也不是本次修改 AndroidManifest 广告 ID 后重新打包的变更清单。**不要仅凭本节自动导入增量包、替换 `KlondikeRoundBootView.cs`，或改动 SDK 初始化、广告及推送流程。** 当前操作应以工程实际文件和用户明确提出的修改要求为准。

已接入 SDK 的当前游戏使用这两个增量包：

1. 导入 `dist/NFShootSDK-ActivityFix.unitypackage`：仅更新桥接 AAR、Editor 构建脚本及两个 Gradle 脚本。该补丁不包含 ShootSdk/Platform 等 C# 广告脚本、广告位参数或供应商 AAR。
2. 导入 `dist/NFShootSDK-SolitaireStartupFix.unitypackage`：仅替换原 `Assets/Game/Scripts/Panel/KlondikeRoundBootView.cs`，保留原脚本 GUID。若该文件已有其他改动，只合并 ZIP 中 `IntegrationPatches` 对应文件的 `Dismiss()` 方法；通知初始化和关闭页埋点各自记录异常，最后仍执行 `base.Dismiss()`。游戏核心资源加载、Configure 和广告流程不变。
3. 提交 Unity 仓库并重新运行已有 Actions，安装新 APK 验证。

自定义入口仍为 `MainActivity -> UnityActivity -> UnityPlayerActivity`。通知、推送使用同一个 MainActivity。MainActivity 新增 `onNewIntent`，先执行 Unity 原逻辑，再将带 `google.message_id` 或 `message_id` 的 FCM Intent 转发给宿主 Firebase Messaging 的 `MessageForwardingService.enqueueWork`。保留附加数据及 URI；普通深链和本地通知不转发。Firebase 未安装或转发失败时记录错误，不中断回到游戏；冷启动保持宿主 Firebase 读取启动 Intent 的流程。适配依据：[Firebase 自定义 Activity 官方说明](https://firebase.google.com/docs/cloud-messaging/unity/get-started#configure_a_custom_entry_point_activity)。

构建脚本合并已知 Unity/Firebase 主入口，并迁移生成的 `.androidlib` 中已知入口的设置和深链；通过 Manifest 合并规则移除旧入口声明。合并后的应用 Manifest 会检查实际启动入口、Application 和通知入口。未知自定义主入口会明确报错，需要先合并其生命周期。现有广告 Java 类、初始化、焦点处理、C# 广告调用和供应商 AAR 均保持不变。

## 游戏接入

原场景已经挂有 `ShootSdk` / `Platform` 时保留组件。如果没有，在一个启动场景的 GameObject 上挂载 `ShootSdk`；它会在需要时创建 `Platform`。不要每个场景都重新初始化。

`ShootSdk.Start()` 保留你提供的当前 MaxAppkey、激励/插屏广告位，自动调用原初始化流程。游戏继续使用：

```csharp
ShootSdk.Instance.PlayRewardAds(() => {
    // 原有激励奖励发放逻辑。
});

ShootSdk.Instance.PlayInterstitialAds(() => {
    // 原有插屏关闭后的继续逻辑。
});
```

本包没有额外接管回调的新包装层。SDK 沿用 `Platform -> JniServer -> JavaProxyCallback -> ShootSdk` 的原流程，避免两个包装层争用一个 Java 回调槽。

接入副本做了以下适配：

- `Platform.Awake` 提前建立并注册 Java 回调，消除与 `ShootSdk.Start` 之间的启动顺序问题。
- Java 调用排队到 Android UI 线程；所有原生回调先进入 `MaxEventExecutor`，再由 Unity 主线程处理，包括奖励、关闭、计数和 Adjust 收入回传。
- JNI 仅在 Android Player 中执行，Editor 不尝试加载 Java 类，也不模拟成功广告或发放奖励。
- 保留原四个脚本 GUID、类名、公共方法、广告参数、计数和 Adjust 回传规则；回调增加 IL2CPP 保留标记。

`MyApplication` 负责底层广告库的进程初始化，`MainActivity` 设置 Activity 上下文，不要再从 C# 调用 `ShootAdManager.Init()`。

## 依赖及宿主边界

- Maven 声明快照来自当前 `NFShootAdLib/build.gradle`，保留版本及 `analytics -> util` 排除规则；BidMachine 和其 ExploreStack protobuf 依赖通过 Appodeal 仓库解析。不要由另一个声明重新引入 `io.bidmachine:util`，也不要再次导入同一广告平台的完整 SDK。
- Firebase、Adjust、SolarEngine、Mobile Notifications 的 Unity 插件仍由原游戏管理。本包不复制导出工程中的 IL2CPP、Firebase C++ 库、游戏资源或签名文件。原 Firebase Unity 12.5.0/13.9.0 混用仍需在原工程统一处理。
- 保留当前 `MainActivity -> UnityActivity -> UnityPlayerActivity` 及 `getShad()` 行为。默认 UnityPlayerActivity 和 Firebase 的 MessagingUnityPlayerActivity 声明统一至 MainActivity；其他自定义 Application 或启动 Activity 仍会明确报出冲突，需要合并初始化/生命周期后再构建。
- 桥接 AAR 的 Manifest 保留当前广告 App ID。更换广告账户时修改 `AndroidBridge/AndroidManifest.xml` 并重新封装；C# 广告位位于 `Runtime/ShotSdk/ShootSdk.cs`。
- Java / IL2CPP 保留规则随包提供。供应商 AAR 和 native 库原样保留；正式发布前真机验证广告恢复、奖励只发一次、收入回传和推送。

## 文件与重新封装

```text
Assets/NFShootSDK/
  Plugins/Android/                  桥接 AAR + 两个供应商 AAR
  Runtime/ShotSdk/                  适配后的原四个 C# 文件（保留 GUID）
  Editor/NFShootAndroidBuild.cs      构建接入和工具检查
  Editor/Android/                   依赖快照及 Gradle 配置
ShotSdk/                            用户提供的 C# 原件，未修改
AndroidBridge/src/main/java/         当前自有 Java 源码快照
AndroidBridge/compile-only/          仅编译引用的 UnityPlayerActivity，绝不打入 AAR
Tools/build_package.py               桥接编译和包导出脚本
IntegrationPatches/Assets/Game/      当前游戏加载页异常兜底，单独导入
Tools/Tests/                        通知、推送和 Manifest 回归测试
```

修改 SDK 目录中的桥接或接入源码后，在仓库根目录运行（替换为本机路径）：

```sh
python3 UnitySDK/Tools/build_package.py \
  --javac /path/to/jdk-17/bin/javac \
  --android-jar /path/to/android-sdk/platforms/android-36/android.jar \
  --unity-jar /path/to/Unity2022.3/PlaybackEngines/AndroidPlayer/Variations/il2cpp/Release/Classes/classes.jar
```

输出在 `UnitySDK/dist`：`.unitypackage`、ZIP、SHA-256 校验值。`SOURCE-SNAPSHOT.json` 记录原文件哈希。源 Android 工程后续更新不会自动改变已导出的 SDK。

## 验证范围

SDK 基线已在本机 Unity 2022.3.62f3c1 通过实际导入及脚本编译，使用原游戏的真实 Adjust 源码。Java 编译、重复类检查、Manifest 合并及 ARMv7/ARM64 IL2CPP 编译也已通过。GameCI 适配更新通过了 C# 编译、参数解析、工作流结构和工具准备脚本的隔离验证，尚未从本环境触发远程 Docker/Actions 构建。完整 APK 的资源/dex 验证此前因磁盘空间不足中断，真机广告也尚未验证。详细记录在导入包内 `Assets/NFShootSDK/VALIDATION.txt`。
