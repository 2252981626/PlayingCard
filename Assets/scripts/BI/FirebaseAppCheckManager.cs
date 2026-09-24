using Firebase;
using Firebase.Extensions;
using UnityEngine;

public class FirebaseAppCheckManager : MonoBehaviour
{
    private void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;
            // 无论成功失败都打出来，方便真机 logcat 验证 Firebase 是否真的起来了
            Debug.Log("[Firebase] DependencyStatus = " + dependencyStatus);
            if (dependencyStatus == DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                // app = Firebase.FirebaseApp.DefaultInstance;
                // Set a flag here to indicate whether Firebase is ready to use by your app.
                FirebaseAdAnalytics.IsReady = true;
            }
            else
            {
                FirebaseAdAnalytics.IsReady = false;
                Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }

}
