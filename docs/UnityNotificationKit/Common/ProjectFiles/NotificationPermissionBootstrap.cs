using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using Unity.Notifications.Android;
#endif

/// <summary>Requests Android notification permission at game entry for either module.</summary>
public static class NotificationPermissionBootstrap
{
#if UNITY_ANDROID && !UNITY_EDITOR
    private static bool requested;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetState()
    {
        requested = false;
    }
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void RequestOnce()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (requested) return;
        requested = true;
        const string permission = "android.permission.POST_NOTIFICATIONS";
        if (AndroidNotificationCenter.UserPermissionToPost == PermissionStatus.Allowed
            || UnityEngine.Android.Permission.HasUserAuthorizedPermission(permission))
            return;
        UnityEngine.Android.Permission.RequestUserPermission(permission);
        Debug.Log("[NotificationKit] Requested POST_NOTIFICATIONS at game entry.");
#endif
    }
}
