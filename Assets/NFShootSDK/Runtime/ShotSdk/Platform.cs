#if NFSDK_OPS_OBFUSCATOR
using OPS.Obfuscator.Attribute;
#endif
using UnityEngine;


public class JavaProxyCallback : AndroidJavaProxy
{
    public JavaProxyCallback() : base("com.common.CallBack.JavaProxyCallback") { }

    public void IntCallBack(int num)
    {

    }

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
public class Platform : MonoBehaviour
{
    private static JavaProxyCallback mJavaProxyCallback = null;
    public static Platform Instance;


    AndroidJavaClass jniServer;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        jniServer = new AndroidJavaClass("com.common.JniServer.JniServer");

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        mJavaProxyCallback = new JavaProxyCallback();
#if UNITY_ANDROID && !UNITY_EDITOR
 		jniServer.CallStatic("SetJavaProxyCallback", mJavaProxyCallback);
#endif
    }

    public void InitSdk(string str)
    {
        jniServer.CallStatic("InitSdk", str);
    }

    public void OnShowAd(string str)
    {
        jniServer.CallStatic("ShowAd", str);
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
                    ShootSdk.Instance.OnAdDisplayed(adParam);
                    break;
                case 4:
                    ShootSdk.Instance.OnInterstitialCallbackNull();
                    break;
                case 5:
                    ShootSdk.Instance.OnAdClicked(adParam);
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
                    ShootSdk.Instance.OnAdDisplayed(adParam);
                    break;
                case 4:
                    ShootSdk.Instance.OnRewardedAdFailed();
                    break;
                case 5:
                    ShootSdk.Instance.OnAdClicked(adParam);
                    break;
                case 6:
                    ShootSdk.Instance.OnRewardedAdClosed();
                    break;
                case 7:
                    ShootSdk.Instance.OnRewardedAdFailed();
                    break;
                case 8:
                    break;
                case 9:
                    break;
                case 10:
                    ShootSdk.Instance.OnAwardCallback();
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
