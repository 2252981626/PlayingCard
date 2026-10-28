#if NFSDK_OPS_OBFUSCATOR
using OPS.Obfuscator.Attribute;
#endif
using UnityEngine;


[UnityEngine.Scripting.Preserve]
#if NFSDK_OPS_OBFUSCATOR
[DoNotObfuscateClassAttribute]
#endif
public class JavaProxyCallback : AndroidJavaProxy
{
    public JavaProxyCallback() : base("com.common.CallBack.JavaProxyCallback") { }

    [UnityEngine.Scripting.Preserve]
#if NFSDK_OPS_OBFUSCATOR
    [DoNotRenameAttribute]
#endif
    public void IntCallBack(int num)
    {

    }

    [UnityEngine.Scripting.Preserve]
#if NFSDK_OPS_OBFUSCATOR
    [DoNotRenameAttribute]
#endif
    public void StringCallback(string jsonStr)
    {
        MaxEventExecutor.ExecuteOnMainThread(() =>
        {
            if (Platform.Instance != null) Platform.Instance.AdListenBack(jsonStr);
        }, "nf_ad_callback");
    }
}
#if NFSDK_OPS_OBFUSCATOR
[DoNotObfuscateClassAttribute]
#endif
[DefaultExecutionOrder(-1000)]
public class Platform : MonoBehaviour
{
#if UNITY_ANDROID && !UNITY_EDITOR
    private static JavaProxyCallback mJavaProxyCallback = null;
    AndroidJavaClass jniServer;
#endif
    public static Platform Instance;


    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        MaxEventExecutor.InitializeIfNeeded();
#if UNITY_ANDROID && !UNITY_EDITOR
        jniServer = new AndroidJavaClass("com.common.JniServer.JniServer");
        mJavaProxyCallback = new JavaProxyCallback();
        jniServer.CallStatic("SetJavaProxyCallback", mJavaProxyCallback);
#endif

        DontDestroyOnLoad(gameObject);
    }

    public void InitSdk(string str)
    {
        CallOnAndroidUiThread("InitSdk", str);
    }

    public void OnShowAd(string str)
    {
        CallOnAndroidUiThread("ShowAd", str);
    }

    private void CallOnAndroidUiThread(string method, string value)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaObject activity = null;
        try
        {
            using (var unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                activity = unity.GetStatic<AndroidJavaObject>("currentActivity");
            var retained = activity;
            activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                try { jniServer.CallStatic(method, value); }
                catch (System.Exception error)
                {
                    MaxEventExecutor.ExecuteOnMainThread(() => Debug.LogException(error), "nf_native_error");
                }
                finally { retained.Dispose(); }
            }));
        }
        catch (System.Exception error)
        {
            if (activity != null) activity.Dispose();
            Debug.LogException(error);
        }
#else
        Debug.Log("NF Shoot SDK: " + method + " is available in Android players only.");
#endif
    }

    public void AdListenBack(string str)
    {
        AdParam adParam = JsonUtility.FromJson<AdParam>(str);
        Debug.Log("AdListenBack:   " + str);
        if (adParam.AdType == 1)
        {
            // 插屏
            switch (adParam.AdStatue)
            {
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    ShootSdk.Instance.AddAwardAddSave();
                    break;
                case 4:
                    ShootSdk.Instance.OnInterstitialCallbackNull();
                    break;
                case 5:
                    break;
                case 6:
                    ShootSdk.Instance.OnInterstitialCallback();
                    break;
                case 7:
                    break;
                case 8:
                    break;
                case 9:
                    break;
                case 10:
                    ShootSdk.Instance.OnAwardCallback();
                    break;
                case 11:
                    ShootSdk.Instance.OnInterstitialAdRevenuePaid(adParam);
                    break;
                default:
                    break;
            }
        }
        else if (adParam.AdType == 2)
        {
            // 激励
            switch (adParam.AdStatue)
            {
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    break;
                case 4:
                    break;
                case 5:
                    break;
                case 6:
                    break;
                case 7:
                    break;
                case 8:
                    break;
                case 9:
                    break;
                case 10:
                    MaxEventExecutor.ExecuteOnMainThread(() =>
                    {
                        ShootSdk.Instance.OnAwardCallback();
                    }, "cb");
                    break;
                case 11:
                    ShootSdk.Instance.OnAwardRevenut(adParam);
                    break;
                default:
                    break;
            }
        }
    }
}
