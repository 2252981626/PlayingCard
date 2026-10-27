# com.tiger.retention — 次留召回本地通知

游戏启动时自动**清空上一轮 + 重排 D1 / D3 / D7 三条本地通知**。玩家没回来就依次提醒，回来了就整体重置。

- 不依赖服务器、不依赖 Google 服务框架（GMS），**国内设备同样生效**
- Android 13+ 通知权限自动处理
- 通知点击回流可挂钩埋点（区分是哪一条把人拉回来的）
- **接入自动化**：图标注册 / 关键 Unity 设置由包内 Editor 工具自动完成（见第二章）

---

## 一、新项目接入（3 步）

### 1. 引用包

`Packages/manifest.json` 的 `dependencies` 里加一行：

```json
"com.tiger.retention": "file:D:/AIGame/Packages/com.tiger.retention"
```

> 跨电脑使用（或想统一版本）时，把包推到内网 Git，改成 `"com.tiger.retention": "https://192.168.101.94:9980/xxx/com.tiger.retention.git"`。
> 包已在 `package.json` 里声明依赖 `com.unity.mobile.notifications`，Unity 会自动安装。
>
> ⚠️ 包内 `Runtime/Tiger.Retention.asmdef`（引用 `Unity.Notifications.Android`）是**必需**的：Unity 不会编译"没有 asmdef 的包内脚本"，删除它项目侧会直接报 `CS0246: 找不到 Tiger 命名空间`，且 Assembly-CSharp 整体编译失败。

### 2. 项目侧配置（注入文案）

新建一个脚本（如 `Assets/Scripts/SDK/RetentionSetup.cs`）：

```csharp
using UnityEngine;
using Tiger.Retention;

public static class RetentionSetup
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Configure()
    {
        RetentionNotification.Setup(new RetentionConfig
        {
            ChannelId   = "mygame_recall",      // 每个项目用不同的 ChannelId
            ChannelName = "Level reminders",
            SmallIconId = "notify_small",       // 图标按约定放 Assets/Plugins/Android/notify_small.png，包会自动注册（Id 同名）
            AutoLaunchOnStart = false,          // 见第 3 步
            Messages = new[]
            {
                new RetentionMessage { Key = "d1", DelayHours = 20,
                    Title = "Level {progress} is so close",
                    Body  = "You were just a few steps away. Come back and finish it!",
                    TitleNoProgress = "Your next level is waiting",
                    BodyNoProgress  = "One level takes under a minute. Jump back in!" },
                new RetentionMessage { Key = "d3", DelayHours = 72,  /* ... */ },
                new RetentionMessage { Key = "d7", DelayHours = 168, /* ... */ },
            },
        });
    }
}
```

> `{progress}` 会被替换为传入的进度值（关卡号等）。不传/传 ≤0 时使用 `TitleNoProgress` / `BodyNoProgress` 兜底文案。

### 3. 调度入口（二选一）

**方式 A · 零侵入**：配置里保持 `AutoLaunchOnStart = true`，什么都不用做。
包会在首个场景加载后自动执行（进度值取 `ProgressProvider`，可选）。

**方式 B · 显式调用（推荐）**：`AutoLaunchOnStart = false`，在启动流程里手写调用，
放在**埋点 SDK 初始化之后**，这样回流事件能当场上报：

```csharp
string src = RetentionNotification.OnGameLaunch(SaveSystem.Load().highestUnlockedLevel);
if (!string.IsNullOrEmpty(src))
    AnalyticsManager.TrackNotificationOpen(src);   // src = "d1" / "d3" / "d7"
```

---

## 二、项目配置（大部分已自动化）

> ✨ 包内置 `Editor/RetentionProjectSetup.cs`，打开项目后**自动完成**下表 ①②（幂等：已配好则零动作、零日志）。
> 手动重跑：菜单 **Tools → Tiger Retention → Setup Project**。

| 配置 | 谁来配 | 说明 |
|---|---|---|
| ① 图标 Read/Write + 注册 Drawable Resources | **自动** | 先把 PNG 放 `Assets/Plugins/Android/notify_small.png`（**纯白 + 透明**），工具会自动勾 **Read/Write Enabled** 并注册（Id=`notify_small`）。⚠️ **不要**放 `Assets/Plugins/Android/res/` —— Unity 2022.3 起会直接报 `OBSOLETE...was removed` 导致打包失败 |
| ② Reschedule on Device Restart | **自动** | 设备重启后待发通知不丢（该设置默认关闭，工具会自动开启） |
| ③ 精确闹钟（保持关闭） | 无需操作 | 2.4.0 默认即关。手动勾选会带 `SCHEDULE_EXACT_ALARM` 权限 → **Google Play 政策只允许闹钟/日历应用使用**，不要勾 |

---

## 三、API 速查

| 方法 | 说明 |
|---|---|
| `Setup(config)` | 注入项目配置（不调用则使用包默认文案） |
| `OnGameLaunch(progress, autoRequestPermission)` | 启动主入口：注册渠道 → 请求权限 → 清空旧链 → 读回流来源 → 重排全部。**返回回流来源 Key 或 null** |
| `UpdateProgress(progress)` | 进度变化时同步通知链（可选，例如每次通关后调用，让文案里的关卡号保持最新） |
| `RequestPermission()` | 单独请求通知权限（想延后到「首关通过」再弹时用） |
| `HasPermission()` | 是否已获得通知权限 |
| `CancelScheduled()` | 取消本模块排出的所有待发通知（不影响其他模块） |

---

## 四、验证通知真的会发

1. **看日志**：logcat 过滤 `[Retention]`，启动时应出现
   `[Retention] 已预约 d1 → 09-17 11:30「...」` 与 `[Retention] 通知链就绪：共 3 条`
2. **看系统闹钟**：`adb shell dumpsys alarm | grep <包名>`
3. **看通知栏 + 文案**：`adb shell dumpsys notification --noredact | grep NotificationRecord`（再 `grep android.title` 可核对标题文案）
4. **快速实测**：把某条 `DelayHours` 临时改成 `0.02`（约 72 秒），打包后杀掉进程等通知
   - ⚠️ 实际弹出可能**延迟 1~3 分钟**：为合规未启用精确闹钟（系统按非精确窗口触发），属正常现象；真实 20h 场景无感
5. **模拟点击回流**（验证埋点链路，2026-09-16 实测通过）：先 `adb shell am force-stop <包名>` 杀掉游戏，再执行
   `adb shell am start -n <包名>/com.unity3d.player.UnityPlayerActivity --ei com.unity.NotificationID <通知id>`
   （通知 id 见第 3 步输出里的 `id=xxx`）→ 日志应出现 `[Retention] 玩家通过通知回流：d1` 且通知链自动重排

---

## 五、常见问题

| 现象 | 原因 / 处理 |
|---|---|
| 打包后一条通知都收不到 | ① Android 13+ 没给 `POST_NOTIFICATIONS` 权限 → 检查 `RequestPermission()` 是否执行、系统设置里是否被关；② 包体没重新解析成功 → 看 Console 报错 |
| 玩家在线还收到召回通知 | 说明启动时没执行到 `OnGameLaunch`（检查日志有没有"通知链就绪"） |
| 通知图标显示成白色方块 | ① 图标不是纯白透明 PNG；② Drawable Resources 里没注册，或 Id 与 `SmallIconId` 不一致；③ PNG 未勾 Read/Write Enabled（构建时会静默 warning "Failed exporting"） |
| 打包报 `OBSOLETE - Providing Android resources in Assets/Plugins/Android/res was removed` | `Assets/Plugins/Android/res/` 已废弃，清掉该目录，图标改走 Drawable Resources（见上表） |
| 改了渠道名称没生效 | Android 限制：渠道属性创建后不可更改。换一个新的 `ChannelId` 即可 |
| Editor 里看不到通知 | Editor 不实际调度，只打印预览日志；必须真机打包验证 |
| 测试时通知延迟几分钟才弹 | 正常：为合规未启用精确闹钟（非精确调度），系统在到点后的窗口触发，延迟 1~3 分钟属预期；真实 20h 场景无感 |
| 图标/设置自动配置没生效 | 手动跑菜单 **Tools → Tiger Retention → Setup Project**（会弹窗报告结果）；也可按第二章表格手动配置 |
| 想提高 Android 13+ 权限通过率 | 冷启动弹权限框通过率低：把 `OnGameLaunch(progress, autoRequestPermission: false)`，等玩家首关通过后再调 `RequestPermission()` |

---

## 六、实现要点（为什么这么设计）

1. **预约放在「进游戏时」，不依赖退出事件** —— 进程被系统杀死时 `OnApplicationQuit` 不保证执行，"进游戏就排好明天的提醒"永远可靠。
2. **进游戏先清空旧链** —— 避免玩家在线还收到召回弹窗。
3. **精确取消（记录通知 ID）** —— 只取消本模块排出的通知，不误杀项目里其他通知模块。
4. **深夜保护** —— 触发时间落在 23:00–09:00 自动顺延到 10:00，避免凌晨吵醒玩家。
5. **`Importance.Default`** —— 有声音、无横幅。改成 `High` 会更醒目但更打扰，按游戏调性取舍。
