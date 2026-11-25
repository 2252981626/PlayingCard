using SolarEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplovinMaxRewardOperator : MonoBehaviour
{
    protected string _iosRewardedAdUnitId = "";     // iOS测试ID
    int retryAttempt;
    //-------------------------------
    protected string RewardedAdUnitId = "";
    public Action RewardReceivedCallback = null;
    public Action RewardDisplayErrorCallback = null;
    public static bool isPlayRewardAds = false;
    private bool callbacksRegistered;
    private bool requestActive;
    private bool rewardReceived;

    public virtual void Init()
    {
        if (callbacksRegistered) return;
        isPlayRewardAds = false;
        InitializeRewardedAds();
    }
    public void InitializeRewardedAds()
    {
        if (callbacksRegistered) return;
        callbacksRegistered = true;
        RewardedAdUnitId = Application.platform == RuntimePlatform.IPhonePlayer ? _iosRewardedAdUnitId : AdSdkSettings.Instance.androidRewardedAdUnitId;

        // Attach callback
        NativeMaxSdk.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
        NativeMaxSdk.Rewarded.OnAdLoadFailedEvent += OnRewardedAdLoadFailedEvent;
        NativeMaxSdk.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
        NativeMaxSdk.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
        NativeMaxSdk.Rewarded.OnAdRevenuePaidEvent += OnRewardedAdRevenuePaidEvent;
        NativeMaxSdk.Rewarded.OnAdHiddenEvent += OnRewardedAdHiddenEvent;
        NativeMaxSdk.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
        NativeMaxSdk.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;

        // Load the first rewarded ad
        LoadRewardedAd();
    }

    protected void LoadRewardedAd()
    {
        Debug.Log("激励视频加载 :" + RewardedAdUnitId);

        NativeMaxSdk.LoadRewardedAd(RewardedAdUnitId);
    }

    // 展示激励视频（调用此方法触发广告展示）
    public void ShowRewardedAd()
    {
        if (isPlayRewardAds) return;

        requestActive = true;
        rewardReceived = false;
        if (NativeMaxSdk.IsRewardedAdReady(RewardedAdUnitId))
        {
            isPlayRewardAds = true;
            NativeMaxSdk.ShowRewardedAd(RewardedAdUnitId);
        }
        else
        {
            isPlayRewardAds = false;
            ExecutionRewardDisplayErrorCallback();
            string message = LanguageManager.Instance.GetText("AdsNotReady");
            UIManager.Instance.OpenUI<GeneralTipsPanel>(message);
            LoadRewardedAd();
        }
    }
    private void ExecutionRewardDisplayErrorCallback()
    {
        requestActive = false;
        rewardReceived = false;
        Action callback = RewardDisplayErrorCallback;
        RewardDisplayErrorCallback = null;
        RewardReceivedCallback = null;
        callback?.Invoke();
    }
    private void ExecutionRewardReceivedCallback()
    {
        requestActive = false;
        rewardReceived = false;
        Action callback = RewardReceivedCallback;
        RewardReceivedCallback = null;
        RewardDisplayErrorCallback = null;
        AdManager.Instance.applovinMaxInterstitialOperator.insertClickCoolingTime = ApplovinMaxInterstitialOperator.ad_mau_inter_time;
        AdManager.Instance.applovinMaxInterstitialOperator.insertTimer = ApplovinMaxInterstitialOperator.insertTime;
        callback?.Invoke();
    }
    private void OnRewardedAdLoadedEvent(string adUnitId, NativeMaxSdk.AdInfo adInfo)
    {
        // Rewarded ad is ready for you to show. NativeMaxSdk.IsRewardedAdReady(adUnitId) now returns 'true'.
        // Reset retry attempt
        retryAttempt = 0;
        Debug.Log("激励视频加载完成");
    }

    private void OnRewardedAdLoadFailedEvent(string adUnitId, NativeMaxSdk.ErrorInfo errorInfo)
    {
        // Rewarded ad failed to load
        // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds).
        retryAttempt++;
        double retryDelay = Math.Pow(2, Math.Min(6, retryAttempt));

        Debug.Log("激励视频加载失败 :" + errorInfo.Message);
        Invoke("LoadRewardedAd", (float)retryDelay);
    }

    private void OnRewardedAdDisplayedEvent(string adUnitId, NativeMaxSdk.AdInfo adInfo)
    {
        Debug.Log("激励视频展示");
        OtherSdkManager.Instance.SendPixalateRequest(adInfo.AdUnitIdentifier);
    }

    private void OnRewardedAdFailedToDisplayEvent(string adUnitId, NativeMaxSdk.ErrorInfo errorInfo, NativeMaxSdk.AdInfo adInfo)
    {
        if (!requestActive) return;
        // Rewarded ad failed to display. AppLovin recommends that you load the next ad.

        isPlayRewardAds = false;
        Debug.Log("激励视频展示失败");
        ExecutionRewardDisplayErrorCallback();
        LoadRewardedAd();


    }

    private void OnRewardedAdClickedEvent(string adUnitId, NativeMaxSdk.AdInfo adInfo)
    {
        Debug.Log("激励视频点击");
        if (!OtherSdkManager.IsInit)
        {
            return;
        }
        AdClickAttributes AdClickAttributes = new AdClickAttributes();
        AdClickAttributes.ad_platform = adInfo.NetworkName;
        AdClickAttributes.mediation_platform = "max";
        AdClickAttributes.ad_id = adInfo.AdUnitIdentifier;
        AdClickAttributes.ad_type = 1; 
        SolarEngine.Analytics.trackAdClick(AdClickAttributes);
    }

    private void OnRewardedAdHiddenEvent(string adUnitId, NativeMaxSdk.AdInfo adInfo)
    {
        if (!requestActive) return;
        isPlayRewardAds = false;
        if (rewardReceived) ExecutionRewardReceivedCallback();
        else ExecutionRewardDisplayErrorCallback();
        LoadRewardedAd();
    }

    private void OnRewardedAdReceivedRewardEvent(string adUnitId, NativeMaxSdk.Reward reward, NativeMaxSdk.AdInfo adInfo)
    {
        if (!requestActive || rewardReceived) return;
        rewardReceived = true;
    }

    private void OnRewardedAdRevenuePaidEvent(string adUnitId, NativeMaxSdk.AdInfo adInfo)
    {
        OnRewardedAdRevenuePaidEvent(adInfo);

    }
    //广告成功展示且产生有效收益
    private void OnRewardedAdRevenuePaidEvent(NativeMaxSdk.AdInfo adInfo)
    {
        // Ad revenue paid. Use this callback to track user revenue.
       
        //----------------热力---------------------------
        //----------------Firebase 广告价值---------------------------
        FirebaseAdAnalytics.LogAdImpression(adInfo, FirebaseAdAnalytics.AdFormatRewarded);
        if (!OtherSdkManager.IsInit)
        {
            return;
        }
        ImpressionAttributes impressionAttributes = new ImpressionAttributes();
        impressionAttributes.ad_platform = adInfo.NetworkName;
        impressionAttributes.ad_id = adInfo.AdUnitIdentifier;
        impressionAttributes.ad_type = 1;
        impressionAttributes.ad_ecpm = adInfo.Revenue * 1000;
        impressionAttributes.currency_type = "USD";
        impressionAttributes.mediation_platform = "max";//填入您所使用的聚合平台
        impressionAttributes.is_rendered = true;
        SolarEngine.Analytics.trackAdImpression(impressionAttributes);
    }

    private void OnDestroy()
    {
        CancelInvoke();
        if (callbacksRegistered)
        {
            NativeMaxSdk.Rewarded.OnAdLoadedEvent -= OnRewardedAdLoadedEvent;
            NativeMaxSdk.Rewarded.OnAdLoadFailedEvent -= OnRewardedAdLoadFailedEvent;
            NativeMaxSdk.Rewarded.OnAdDisplayedEvent -= OnRewardedAdDisplayedEvent;
            NativeMaxSdk.Rewarded.OnAdClickedEvent -= OnRewardedAdClickedEvent;
            NativeMaxSdk.Rewarded.OnAdRevenuePaidEvent -= OnRewardedAdRevenuePaidEvent;
            NativeMaxSdk.Rewarded.OnAdHiddenEvent -= OnRewardedAdHiddenEvent;
            NativeMaxSdk.Rewarded.OnAdDisplayFailedEvent -= OnRewardedAdFailedToDisplayEvent;
            NativeMaxSdk.Rewarded.OnAdReceivedRewardEvent -= OnRewardedAdReceivedRewardEvent;
        }
        isPlayRewardAds = false;
        RewardReceivedCallback = null;
        RewardDisplayErrorCallback = null;
    }
}
