using UnityEngine;
#if UNITY_ANDROID || UNITY_EDITOR
using System;
using System.Collections.Generic;
using Tiger.Retention;
#endif

/// <summary>Connects local recall notifications to saved progress and SolarEngine analytics.</summary>
public sealed class RetentionSetup : MonoBehaviour
{
#if UNITY_ANDROID || UNITY_EDITOR
    private static RetentionSetup _instance;
    private readonly Queue<KeyValuePair<string, int>> _pendingOpens = new Queue<KeyValuePair<string, int>>();
    private int _progress;
    private bool _started;
    private bool _wasPaused;
    private bool _permissionRequested;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetState()
    {
        _instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Configure()
    {
        // Disable the package's automatic boot before its AfterSceneLoad callback runs.
        RetentionNotification.Setup(CreateConfig(Application.systemLanguage));
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        if (_instance != null) return;

        var host = new GameObject("RetentionNotifications");
        DontDestroyOnLoad(host);
        _instance = host.AddComponent<RetentionSetup>();
    }

    private void Start()
    {
        _progress = ReadProgress();
        _started = true;
        RefreshSchedule();
        RequestPermissionOnce();
    }

    private void OnApplicationPause(bool paused)
    {
        if (!_started) return;
        if (paused)
        {
            _wasPaused = true;
            return;
        }

        if (!_wasPaused) return;
        _wasPaused = false;
        RefreshSchedule();
    }

    private void Update()
    {
        if (_pendingOpens.Count == 0 || !OtherSdkManager.IsInit || OtherSdkManager.Instance == null)
            return;

        var opened = _pendingOpens.Dequeue();
        try
        {
            OtherSdkManager.Instance.CustomEvent("retention_notification_open",
                "notification_key", opened.Key, "level_id", opened.Value);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("[Retention] Notification-open analytics failed: " + exception.Message);
        }
    }

    private void RefreshSchedule()
    {
        string source = RetentionNotification.OnGameLaunch(_progress, autoRequestPermission: false);
        if (!string.IsNullOrEmpty(source))
            _pendingOpens.Enqueue(new KeyValuePair<string, int>(source, _progress));
    }

    private static int ReadProgress()
    {
        if (GameManager.Instance == null) return 0;
        try
        {
            // GameManager.Init loads playerInfo later; the scene's serialized value may be stale.
            PlayerInfo player = GameManager.Instance.GetPlayerInfo();
            return player == null ? 0 : Mathf.Max(1, player.level);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("[Retention] Could not read saved progress: " + exception.Message);
            return 0;
        }
    }

    private void RequestPermissionOnce()
    {
        if (_permissionRequested) return;
        _permissionRequested = true;
        RetentionNotification.RequestPermission();
    }

    private static RetentionConfig CreateConfig(SystemLanguage language)
    {
        RetentionMessage[] messages;
        string channelName;
        string channelDescription;
        switch (language)
        {
            case SystemLanguage.Portuguese:
                channelName = "Lembretes de paciência";
                channelDescription = "Lembretes para continuar sua partida";
                messages = new[]
                {
                    Message("d1", 20, "O nível {progress} está esperando",
                        "Volte ao Solitaire Casino Tycoon e tente sua próxima partida!",
                        "Sua próxima partida está esperando"),
                    Message("d3", 72, "Seu progresso está salvo",
                        "Continue do nível {progress}. As cartas estão esperando por você!",
                        "Seu progresso está salvo", "As cartas estão esperando por você!"),
                    Message("d7", 168, "Que tal uma partida de paciência?",
                        "O nível {progress} espera por você. Volte e continue sua aventura!",
                        "Que tal uma partida de paciência?", "Volte e continue sua aventura!"),
                };
                break;
            case SystemLanguage.Indonesian:
                channelName = "Pengingat solitaire";
                channelDescription = "Pengingat untuk melanjutkan permainan";
                messages = new[]
                {
                    Message("d1", 20, "Level {progress} menunggumu",
                        "Kembali ke Solitaire Casino Tycoon dan mainkan babak berikutnya!",
                        "Permainan berikutnya menunggumu"),
                    Message("d3", 72, "Progresmu masih tersimpan",
                        "Lanjutkan dari level {progress}. Kartu-kartunya menunggumu!",
                        "Progresmu masih tersimpan", "Kartu-kartunya menunggumu!"),
                    Message("d7", 168, "Saatnya bermain solitaire lagi?",
                        "Level {progress} menunggumu. Kembali dan lanjutkan petualanganmu!",
                        "Saatnya bermain solitaire lagi?", "Kembali dan lanjutkan petualanganmu!"),
                };
                break;
            default:
                channelName = "Solitaire reminders";
                channelDescription = "Reminders to continue your solitaire game";
                messages = new[]
                {
                    Message("d1", 20, "Level {progress} is waiting",
                        "Come back to Solitaire Casino Tycoon for your next game!",
                        "Your next solitaire game is waiting"),
                    Message("d3", 72, "Your progress is still there",
                        "Pick up at level {progress}. The cards are waiting for you!",
                        "Your progress is still there", "The cards are waiting for you!"),
                    Message("d7", 168, "Ready for another game of solitaire?",
                        "Level {progress} is waiting. Come back and continue your adventure!",
                        "Ready for another game of solitaire?", "Come back and continue your adventure!"),
                };
                break;
        }

        return new RetentionConfig
        {
            ChannelId = "solitaire_casino_tycoon_recall",
            ChannelName = channelName,
            ChannelDescription = channelDescription,
            SmallIconId = "notify_small",
            AutoLaunchOnStart = false,
            AvoidNightHours = true,
            NightStartHour = 23,
            NightEndHour = 9,
            NightFallbackHour = 10,
            Messages = messages,
        };
    }

    private static RetentionMessage Message(string key, double hours, string title,
        string body, string titleNoProgress, string bodyNoProgress = null)
    {
        return new RetentionMessage
        {
            Key = key,
            DelayHours = hours,
            Title = title,
            Body = body,
            TitleNoProgress = titleNoProgress,
            BodyNoProgress = bodyNoProgress ?? body,
        };
    }
#endif

    /// <summary>Called after a main-game win, when the next level has been unlocked.</summary>
    public static void OnLevelCompleted(int nextLevel)
    {
#if UNITY_ANDROID || UNITY_EDITOR
        if (_instance == null) return;
        _instance._progress = Mathf.Max(1, nextLevel);
        RetentionNotification.UpdateProgress(_instance._progress);
#endif
    }
}
