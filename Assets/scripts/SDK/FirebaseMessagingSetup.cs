using System;
using Firebase.Extensions;
using Firebase.Messaging;
using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using Unity.Notifications.Android;
#endif

/// <summary>Receives Firebase console pushes after the shared Firebase dependency check.</summary>
public static class FirebaseMessagingSetup
{
    public const string Topic = "daily_reminders";
    public const string ChannelId = "solitaire_casino_tycoon_push";
    public const string TokenPrefsKey = "FCMDeviceToken";

    private static bool initialized;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetState()
    {
        Shutdown();
    }

    public static void Initialize()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (initialized) return;

        AndroidNotificationCenter.RegisterNotificationChannel(new AndroidNotificationChannel
        {
            Id = ChannelId,
            Name = "Game notifications",
            Description = "Notifications sent by Solitaire Casino Tycoon",
            Importance = Importance.High,
        });

        // RetentionSetup already requests Android notification permission on game entry.
        FirebaseMessaging.TokenRegistrationOnInitEnabled = true;
        FirebaseMessaging.TokenReceived += OnTokenReceived;
        FirebaseMessaging.MessageReceived += OnMessageReceived;
        initialized = true;
        Application.quitting += Shutdown;

        FirebaseMessaging.GetTokenAsync().ContinueWithOnMainThread(task =>
        {
            if (!initialized) return;
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("[FCM] GetToken failed: " + task.Exception);
                return;
            }
            SaveToken(task.Result);
        });

        FirebaseMessaging.SubscribeAsync(Topic).ContinueWithOnMainThread(task =>
        {
            if (!initialized) return;
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("[FCM] Topic subscription failed: " + task.Exception);
                return;
            }
            Debug.Log("[FCM] Subscribed to topic: " + Topic);
        });

        Debug.Log("[FCM] Messaging initialized. Package: " + Application.identifier);
#else
        Debug.Log("[FCM] Remote push reception is enabled in the Android player.");
#endif
    }

    private static void OnTokenReceived(object sender, TokenReceivedEventArgs args)
    {
        if (initialized && args != null) SaveToken(args.Token);
    }

    private static void SaveToken(string token)
    {
        if (string.IsNullOrEmpty(token)) return;
        PlayerPrefs.SetString(TokenPrefsKey, token);
        PlayerPrefs.Save();
        Debug.Log("[FCM] Device token: " + token);
    }

    private static void OnMessageReceived(object sender, MessageReceivedEventArgs args)
    {
        if (!initialized || args == null || args.Message == null) return;

        FirebaseMessage message = args.Message;
        Debug.Log("[FCM] Message received: " + message.MessageId
            + ", opened=" + message.NotificationOpened);

        // Background notification messages are already displayed by Android/FCM.
        // A notification tap must not generate another copy of the notification.
        if (message.NotificationOpened || !Application.isFocused) return;

        string title = message.Notification == null ? null : message.Notification.Title;
        string body = message.Notification == null ? null : message.Notification.Body;
        if (message.Data != null)
        {
            if (string.IsNullOrEmpty(title) && message.Data.TryGetValue("title", out string dataTitle))
                title = dataTitle;
            if (string.IsNullOrEmpty(body) && message.Data.TryGetValue("body", out string dataBody))
                body = dataBody;
        }
        if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(body)) return;

#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidNotificationCenter.SendNotification(new AndroidNotification
        {
            Title = string.IsNullOrEmpty(title) ? Application.productName : title,
            Text = body ?? "",
            FireTime = DateTime.Now,
            SmallIcon = "notify_small",
            IntentData = "fcm:" + message.MessageId,
            ShouldAutoCancel = true,
            ShowInForeground = true,
        }, ChannelId);
        Debug.Log("[FCM] Foreground notification displayed.");
#endif
    }

    private static void Shutdown()
    {
        if (!initialized) return;
        initialized = false;
        FirebaseMessaging.TokenReceived -= OnTokenReceived;
        FirebaseMessaging.MessageReceived -= OnMessageReceived;
        Application.quitting -= Shutdown;
    }
}
