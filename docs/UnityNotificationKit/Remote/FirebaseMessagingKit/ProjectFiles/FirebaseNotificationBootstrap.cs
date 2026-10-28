using Firebase;
using Firebase.Extensions;
using UnityEngine;

/// <summary>Automatic Firebase entry for projects without an existing Firebase initializer.</summary>
public sealed class FirebaseNotificationBootstrap : MonoBehaviour
{
#if UNITY_ANDROID && !UNITY_EDITOR
    private static FirebaseNotificationBootstrap instance;
    private FirebaseApp app;
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetState()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        instance = null;
#endif
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (instance != null) return;
        var host = new GameObject("FirebaseNotificationBootstrap");
        DontDestroyOnLoad(host);
        instance = host.AddComponent<FirebaseNotificationBootstrap>();
#endif
    }

    private void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (this == null) return;
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("[Firebase] Dependency check failed: " + task.Exception);
                return;
            }
            Debug.Log("[Firebase] DependencyStatus = " + task.Result);
            if (task.Result != DependencyStatus.Available)
            {
                Debug.LogError("[Firebase] Dependencies unavailable: " + task.Result);
                return;
            }
            app = FirebaseApp.DefaultInstance;
            FirebaseMessagingSetup.Initialize();
        });
#endif
    }
}
