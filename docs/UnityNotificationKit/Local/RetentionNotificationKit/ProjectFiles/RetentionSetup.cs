using UnityEngine;
#if UNITY_ANDROID || UNITY_EDITOR
using System;
using Tiger.Retention;
#endif

/// <summary>Standalone local notification entry. No game manager or analytics SDK is required.</summary>
public sealed class RetentionSetup : MonoBehaviour
{
    private const string SavedProgressKey = "NotificationKit_CurrentProgress";
#if UNITY_ANDROID || UNITY_EDITOR
    private static RetentionSetup instance;
    private int progress;
    private bool started;
    private bool wasPaused;

    /// <summary>Optional analytics hook: notification key and latest stored progress.</summary>
    public static event Action<string, int> NotificationOpened;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetState()
    {
        instance = null;
        NotificationOpened = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Configure()
    {
        RetentionNotification.Setup(CreateConfig(Application.systemLanguage));
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        if (instance != null) return;
        var host = new GameObject("RetentionNotifications");
        DontDestroyOnLoad(host);
        instance = host.AddComponent<RetentionSetup>();
    }

    private void Start()
    {
        progress = Mathf.Max(0, PlayerPrefs.GetInt(SavedProgressKey, 0));
        started = true;
        RefreshSchedule();
        NotificationPermissionBootstrap.RequestOnce();
    }

    private void OnApplicationPause(bool paused)
    {
        if (!started) return;
        if (paused)
        {
            wasPaused = true;
            return;
        }
        if (!wasPaused) return;
        wasPaused = false;
        RefreshSchedule();
    }

    private void RefreshSchedule()
    {
        string source = RetentionNotification.OnGameLaunch(progress, autoRequestPermission: false);
        if (string.IsNullOrEmpty(source)) return;
        try
        {
            NotificationOpened?.Invoke(source, progress);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("[Retention] Notification-open callback failed: " + exception.Message);
        }
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
                    Message("d1", NotificationProjectConfig.FirstRecallDelayHours, "O nível {progress} está esperando",
                        "Volte e tente sua próxima partida!",
                        "Sua próxima partida está esperando"),
                    Message("d3", NotificationProjectConfig.ThirdRecallDelayHours, "Seu progresso está salvo",
                        "Continue do nível {progress}. As cartas estão esperando por você!",
                        "Seu progresso está salvo", "As cartas estão esperando por você!"),
                    Message("d7", NotificationProjectConfig.SeventhRecallDelayHours, "Que tal uma partida de paciência?",
                        "O nível {progress} espera por você. Volte e continue sua aventura!",
                        "Que tal uma partida de paciência?", "Volte e continue sua aventura!"),
                };
                break;
            case SystemLanguage.Indonesian:
                channelName = "Pengingat solitaire";
                channelDescription = "Pengingat untuk melanjutkan permainan";
                messages = new[]
                {
                    Message("d1", NotificationProjectConfig.FirstRecallDelayHours, "Level {progress} menunggumu",
                        "Kembali dan mainkan babak berikutnya!",
                        "Permainan berikutnya menunggumu"),
                    Message("d3", NotificationProjectConfig.ThirdRecallDelayHours, "Progresmu masih tersimpan",
                        "Lanjutkan dari level {progress}. Kartu-kartunya menunggumu!",
                        "Progresmu masih tersimpan", "Kartu-kartunya menunggumu!"),
                    Message("d7", NotificationProjectConfig.SeventhRecallDelayHours, "Saatnya bermain solitaire lagi?",
                        "Level {progress} menunggumu. Kembali dan lanjutkan petualanganmu!",
                        "Saatnya bermain solitaire lagi?", "Kembali dan lanjutkan petualanganmu!"),
                };
                break;
            default:
                channelName = "Solitaire reminders";
                channelDescription = "Reminders to continue your solitaire game";
                messages = new[]
                {
                    Message("d1", NotificationProjectConfig.FirstRecallDelayHours, "Level {progress} is waiting",
                        "Come back for your next game!",
                        "Your next solitaire game is waiting"),
                    Message("d3", NotificationProjectConfig.ThirdRecallDelayHours, "Your progress is still there",
                        "Pick up at level {progress}. The cards are waiting for you!",
                        "Your progress is still there", "The cards are waiting for you!"),
                    Message("d7", NotificationProjectConfig.SeventhRecallDelayHours, "Ready for another game of solitaire?",
                        "Level {progress} is waiting. Come back and continue your adventure!",
                        "Ready for another game of solitaire?", "Come back and continue your adventure!"),
                };
                break;
        }

        return new RetentionConfig
        {
            ChannelId = NotificationProjectConfig.LocalChannelId,
            ChannelName = channelName,
            ChannelDescription = channelDescription,
            SmallIconId = NotificationProjectConfig.SmallIconId,
            AutoLaunchOnStart = false,
            AvoidNightHours = NotificationProjectConfig.AvoidNightHours,
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

    /// <summary>Call once when saved progress is loaded, and again when it changes. Zero uses generic text.</summary>
    public static void SetProgress(int currentProgress)
    {
        int value = Mathf.Max(0, currentProgress);
        PlayerPrefs.SetInt(SavedProgressKey, value);
        PlayerPrefs.Save();
#if UNITY_ANDROID || UNITY_EDITOR
        if (instance == null) return;
        instance.progress = value;
        if (instance.started) RetentionNotification.UpdateProgress(value);
#endif
    }

    /// <summary>Call after a completed level has unlocked the next level.</summary>
    public static void OnLevelCompleted(int nextLevel)
    {
        SetProgress(nextLevel);
    }
}
