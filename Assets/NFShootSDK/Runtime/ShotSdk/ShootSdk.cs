using AdjustSdk;
//using Firebase.Analytics;
#if NFSDK_OPS_OBFUSCATOR
using OPS.Obfuscator.Attribute;
#endif
using System;
using UnityEngine;

[Serializable]
#if NFSDK_OPS_OBFUSCATOR
[DoNotObfuscateClassAttribute]
#endif
public class SootSdkInit
{
#if NFSDK_OPS_OBFUSCATOR
    [DoNotRenameAttribute]
#endif
    public string MaxAppkey;
#if NFSDK_OPS_OBFUSCATOR
    [DoNotRenameAttribute]
#endif
    public string[] MaxRewardArr;
#if NFSDK_OPS_OBFUSCATOR
    [DoNotRenameAttribute]
#endif
    public string[] MaxInterstitialArr;
}

#if NFSDK_OPS_OBFUSCATOR
[DoNotObfuscateClassAttribute]
#endif
public class ShootSdk : MonoBehaviour
{
#if NFSDK_OPS_OBFUSCATOR
    [DoNotRenameAttribute]
#endif
    public static ShootSdk Instance;


    // 广告观看计数器
#if NFSDK_OPS_OBFUSCATOR
    [DoNotRenameAttribute]
#endif
    private int rewardAdWatchCount = 0;
#if NFSDK_OPS_OBFUSCATOR
    [DoNotRenameAttribute]
#endif
    private int interstitialAdWatchCount = 0;

    // PlayerPrefs存储键
#if NFSDK_OPS_OBFUSCATOR
    [DoNotRenameAttribute]
#endif
    private const string RewardAdCountKey = "Ads_RewardCount";
#if NFSDK_OPS_OBFUSCATOR
    [DoNotRenameAttribute]
#endif
    private const string InterstitialAdCountKey = "Ads_InsCount";

#if NFSDK_OPS_OBFUSCATOR
    [DoNotRenameAttribute]
#endif
    private Action onPlayInterstitial;


#if NFSDK_OPS_OBFUSCATOR
    [DoNotRenameAttribute]
#endif
    private Action onPlayReward;
    private Action onPlayRewardError;

    private SootSdkInit mInitData = new SootSdkInit();

#if NFSDK_OPS_OBFUSCATOR
    [DoNotObfuscateMethodBodyAttribute]
#endif
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 从PlayerPrefs加载广告观看计数
        rewardAdWatchCount = PlayerPrefs.GetInt(RewardAdCountKey, 0);
        interstitialAdWatchCount = PlayerPrefs.GetInt(InterstitialAdCountKey, 0);

        MaxEventExecutor.InitializeIfNeeded();
        if (Platform.Instance == null)
            new GameObject("Platform").AddComponent<Platform>();
    }

#if NFSDK_OPS_OBFUSCATOR
    [DoNotObfuscateMethodBodyAttribute]
#endif
    private void Start()
    {
        mInitData.MaxAppkey = "mcdCxZ8EO8DhhIA5ZGDPflPiLRpa55smRy7JrL0PWhamoDLNiguHTaPAUgaDyFID5VJz273KySTLWZ_NwN69UP";
        mInitData.MaxRewardArr = new string[] { "427e6fb29a8bc97e" };
        mInitData.MaxInterstitialArr = new string[] { "9c9dce817740a47c" };
        string str = JsonUtility.ToJson(mInitData);
        Platform.Instance.InitSdk(str);
    }

#if NFSDK_OPS_OBFUSCATOR
    [DoNotObfuscateMethodBodyAttribute]
#endif
    public void PlayInterstitialAds(Action callBack)
    {
        onPlayInterstitial = callBack;
        Platform.Instance.OnShowAd(AdType.Interstitial.ToString());
    }

#if NFSDK_OPS_OBFUSCATOR
    [DoNotObfuscateMethodBodyAttribute]
#endif
    public void PlayRewardAds(Action callBack, string sceneid = "0", Action displayErrorCallback = null)
    {
        onPlayReward = callBack;
        onPlayRewardError = displayErrorCallback;
        if (Platform.Instance == null)
        {
            OnRewardedAdFailed();
            return;
        }
        Platform.Instance.OnShowAd(AdType.Reward.ToString());
    }


#if NFSDK_OPS_OBFUSCATOR
    [DoNotObfuscateMethodBodyAttribute]
#endif
    public void OnAwardCallback()
    {
        Action callback = onPlayReward;
        onPlayReward = null;
        onPlayRewardError = null;
        callback?.Invoke();
    }

    public void OnRewardedAdFailed()
    {
        Action callback = onPlayRewardError;
        onPlayReward = null;
        onPlayRewardError = null;
        callback?.Invoke();
    }

    public void OnRewardedAdClosed()
    {
        // A real reward callback may arrive after close; keep its original callback.
        Action callback = onPlayRewardError;
        onPlayRewardError = null;
        callback?.Invoke();
    }

    public void OnAdDisplayed(AdParam param)
    {
        if (param == null || OtherSdkManager.Instance == null) return;
        OtherSdkManager.Instance.SendPixalateRequest(param.UIntIdStr);
    }

    public void OnAdClicked(AdParam param)
    {
        if (param == null || !OtherSdkManager.IsInit) return;
        var attributes = new SolarEngine.AdClickAttributes();
        attributes.ad_platform = param.sourceName;
        attributes.mediation_platform = "max";
        attributes.ad_id = param.UIntIdStr;
        attributes.ad_type = param.AdType == AdType.Reward ? 1 : 3;
        SolarEngine.Analytics.trackAdClick(attributes);
    }

#if NFSDK_OPS_OBFUSCATOR
    [DoNotObfuscateMethodBodyAttribute]
#endif
    public void OnInterstitialCallback()
    {
        onPlayInterstitial?.Invoke();
        onPlayInterstitial = null;
    }

#if NFSDK_OPS_OBFUSCATOR
    [DoNotObfuscateMethodBodyAttribute]
#endif
    public void OnInterstitialCallbackNull()
    {
        onPlayInterstitial = null;
    }

#if NFSDK_OPS_OBFUSCATOR
    [DoNotObfuscateMethodBodyAttribute]
#endif
    public void OnAwardRevenut(AdParam param)
    {
        if (param == null) return;

        double revenue = param.revenue;

        if (revenue > 0)
        {


            AdjustAdRevenue adRevenueEvent = new AdjustAdRevenue("applovin_max_sdk");
            adRevenueEvent.SetRevenue(revenue, "USD");
            adRevenueEvent.AdRevenueNetwork = param.sourceName;
            adRevenueEvent.AdRevenueUnit = param.UIntIdStr;
            adRevenueEvent.AdRevenuePlacement = param.AdType == 1 ? "INTER" : "REWARDED";
            Adjust.TrackAdRevenue(adRevenueEvent);
            Debug.Log($"【激励广告-收益】 回传 广告位: {param.UIntIdStr}, 收益: {revenue:F4} 广告源: {param.sourceName}");
            // 发送Firebase事件
            //var impressionParameters = new[] {
            //	new Parameter("ad_platform", "AppLovin"),
            //	//new Parameter("ad_source", param.NetworkName),
            //	//new Parameter("ad_unit_name", adInfo.AdUnitIdentifier),
            //	//new Parameter("ad_format", adInfo.AdFormat),

            //	new Parameter("ad_source", param.sourceName),
            //	new Parameter("ad_unit_name", param.UIntIdStr),
            //	new Parameter("ad_format", param.AdType ==1 ? "INTER":"REWARDED"),
            //	new Parameter("value", revenue),
            //	new Parameter("currency", "USD")
            //};
            //FirebaseAnalytics.LogEvent("ad_impression", impressionParameters);
        }

        FirebaseAdAnalytics.LogAdImpression(
            param.sourceName, param.UIntIdStr, revenue, FirebaseAdAnalytics.AdFormatRewarded);

        if (OtherSdkManager.IsInit)
        {
            try
            {
                var impressionAttributes = new SolarEngine.ImpressionAttributes();
                impressionAttributes.ad_platform = param.sourceName;
                impressionAttributes.ad_id = param.UIntIdStr;
                impressionAttributes.ad_type = 1;
                impressionAttributes.ad_ecpm = revenue * 1000;
                impressionAttributes.currency_type = "USD";
                impressionAttributes.mediation_platform = "max";
                impressionAttributes.is_rendered = true;
                SolarEngine.Analytics.trackAdImpression(impressionAttributes);
            }
            catch (Exception error)
            {
                Debug.LogWarning("[ShootSdk] Rewarded impression reporting failed: " + error.Message);
            }
        }
    }




#if NFSDK_OPS_OBFUSCATOR
    [DoNotObfuscateMethodBodyAttribute]
#endif
    public void OnInterstitialAdRevenuePaid(AdParam param)
    {
        if (param == null) return;

        double revenue = param.revenue;

        if (revenue > 0)
        {
            // 发送Singular收益数据
            AdjustAdRevenue adRevenueEvent = new AdjustAdRevenue("applovin_max_sdk");
            adRevenueEvent.SetRevenue(revenue, "USD");
            adRevenueEvent.AdRevenueNetwork = param.sourceName;
            adRevenueEvent.AdRevenueUnit = param.UIntIdStr;
            adRevenueEvent.AdRevenuePlacement = param.AdType == 1 ? "INTER" : "REWARDED";
            Adjust.TrackAdRevenue(adRevenueEvent);
            Debug.Log($"【插屏广告-收益】 回传 广告位: {param.UIntIdStr}, 收益: {revenue:F4} 广告源: {param.sourceName}");
            //AdjustAdRevenue adRevenueEvent = new AdjustAdRevenue("applovin_max_sdk");
            //adRevenueEvent.SetRevenue(revenue, "USD");
            //adRevenueEvent.AdRevenueNetwork = adInfo.NetworkName;
            //adRevenueEvent.AdRevenueUnit = adUnitId;
            //adRevenueEvent.AdRevenuePlacement = adInfo.AdFormat;
            //Adjust.TrackAdRevenue(adRevenueEvent);

            // 发送Firebase事件
            //var impressionParameters = new[] {
            //	new Parameter("ad_platform", "AppLovin"),
            //	//new Parameter("ad_source", adInfo.NetworkName),
            //	//new Parameter("ad_unit_name", adInfo.AdUnitIdentifier),
            //	//new Parameter("ad_format", adInfo.AdFormat),
            //	new Parameter("ad_source", param.sourceName),
            //	new Parameter("ad_unit_name", param.UIntIdStr),
            //	new Parameter("ad_format", param.AdType ==1 ? "INTER":"REWARDED"),
            //	new Parameter("value", revenue),
            //	new Parameter("currency", "USD")
            //};
            //FirebaseAnalytics.LogEvent("ad_impression", impressionParameters);
        }

        FirebaseAdAnalytics.LogAdImpression(
            param.sourceName, param.UIntIdStr, revenue, FirebaseAdAnalytics.AdFormatInterstitial);

        if (OtherSdkManager.IsInit)
        {
            try
            {
                var impressionAttributes = new SolarEngine.ImpressionAttributes();
                impressionAttributes.ad_platform = param.sourceName;
                impressionAttributes.ad_id = param.UIntIdStr;
                impressionAttributes.ad_type = 3;
                impressionAttributes.ad_ecpm = revenue * 1000;
                impressionAttributes.currency_type = "USD";
                impressionAttributes.mediation_platform = "max";
                impressionAttributes.is_rendered = true;
                SolarEngine.Analytics.trackAdImpression(impressionAttributes);
            }
            catch (Exception error)
            {
                Debug.LogWarning("[ShootSdk] Interstitial impression reporting failed: " + error.Message);
            }
        }

    }
}
