# Git_PlayingCard

Unity 2022.3.62f3 项目。CI 通过 GitHub Actions 自动打包 Android APK / AAB。

---

## Firebase 依赖管理说明

> 这一节很重要 —— 换机器、重新克隆仓库后请先读这里。

### 现状

Firebase Unity SDK **13.2.0**（`firebase_app` + `firebase_analytics`）是通过官方
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
   `FirebaseAnalytics.unitypackage` + `FirebaseApp.unitypackage`
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

1. 检查 13 个关键文件（托管 DLL、依赖描述 XML、m2repository 里的 srcaar、EDM4U、
   `mainTemplate.gradle`、`google-services.json`）是否存在
2. 检查 `mainTemplate.gradle` 里是否已经写入了 `com.google.firebase:firebase-analytics`
   等 Android 依赖 —— **这一条专门用来拦截「忘记跑 Force Resolve」**

任一项失败都会用 `::error::` 在日志里标出具体原因并立即中止，避免出现「编译通过、
打出来的包 Firebase 起不来」这种最难查的情况。
