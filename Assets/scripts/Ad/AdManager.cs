using DG.Tweening;
using System;
using UnityEngine;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance;

    public static bool ShowAdIcon = true;
    //-----------------------------------------------------------
    public ApplovinMaxRewardOperator applovinMaxRewardOperator;
    public ApplovinMaxInterstitialOperator applovinMaxInterstitialOperator;

    public const int FirstAdLevel = 5;

    public static bool CanPlayAds
    {
        get
        {
            int progressLevel = GameManager.Instance?.playerInfo?.level ?? 0;
            if (progressLevel != FirstAdLevel) return progressLevel > FirstAdLevel;

            // Progress advances before the previous round's settlement closes.
            GameScenePanel gameScene = UIManager.Instance?.GetUI<GameScenePanel>(logIfMissing: false);
            return gameScene == null || !gameScene.isActiveAndEnabled || gameScene.LevelId >= FirstAdLevel;
        }
    }

    private bool initializationStarted;

    private void Awake()
    {
        Instance = this;
        Init();
    }

    private void Update()
    {
        var interstitial = applovinMaxInterstitialOperator;
        if (interstitial == null || !interstitial.isActiveAndEnabled || !interstitial.IsInitializedAndIdle) return;
        if (!CanPlayAds || GameLoadingPanel.isOpenStatic || ApplovinMaxRewardOperator.isPlayRewardAds) return;

        float deltaTime = Time.deltaTime;
        interstitial.insertTimer -= deltaTime;
        if (interstitial.insertTimer <= 0 && interstitial.insertClickCoolingTime <= 0)
        {
            interstitial.insertTimer = ApplovinMaxInterstitialOperator.insertTime;
            OnClickInterstitialAd("auto_interstitial_show", false);
        }
        if (interstitial.insertClickCoolingTime > 0)
        {
            interstitial.insertClickCoolingTime -= deltaTime;
        }
    }

    public void Init()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (initializationStarted) return;

        AdSdkSettings settings;
        string sdkKey;
        try
        {
            settings = AdSdkSettings.Instance;
            sdkKey = EncryptSDKKey.DecryptWithRandomSalt(settings.encryptedMaxSdkKey);
        }
        catch (Exception)
        {
            Debug.LogError("[AdManager] \u5e7f\u544a\u914d\u7f6e\u8bfb\u53d6\u6216 MAX Key \u89e3\u5bc6\u5931\u8d25\u3002");
            return;
        }

        if (string.IsNullOrWhiteSpace(sdkKey) ||
            string.IsNullOrWhiteSpace(settings.androidInterstitialAdUnitId) ||
            string.IsNullOrWhiteSpace(settings.androidRewardedAdUnitId) ||
            string.IsNullOrWhiteSpace(settings.sndoRunAppId))
        {
            Debug.LogError("[AdManager] \u5e7f\u544a\u53c2\u6570\u4e0d\u5b8c\u6574\uff0c\u521d\u59cb\u5316\u5df2\u505c\u6b62\u3002");
            return;
        }

        initializationStarted = true;
        NativeMaxSdk.InitializationEvent += OnNativeSdkInitialized;
        NativeMaxSdk.InitializationFailedEvent += OnNativeSdkInitializationFailed;
        NativeMaxSdk.InitializeSdk(sdkKey, settings.androidInterstitialAdUnitId,
            settings.androidRewardedAdUnitId, settings.sndoRunAppId);
#endif
    }

    private void OnNativeSdkInitialized(NativeMaxSdk.SdkConfiguration configuration)
    {
        applovinMaxRewardOperator.Init();
        applovinMaxInterstitialOperator.Init();
    }

    private void OnNativeSdkInitializationFailed(NativeMaxSdk.ErrorInfo error)
    {
        NativeMaxSdk.InitializationEvent -= OnNativeSdkInitialized;
        NativeMaxSdk.InitializationFailedEvent -= OnNativeSdkInitializationFailed;
        initializationStarted = false;
        Debug.LogError("[AdManager] \u539f\u751f MAX \u521d\u59cb\u5316\u5931\u8d25\u3002");
    }

    private void OnDestroy()
    {
        NativeMaxSdk.InitializationEvent -= OnNativeSdkInitialized;
        NativeMaxSdk.InitializationFailedEvent -= OnNativeSdkInitializationFailed;
        if (Instance == this) Instance = null;
    }

    /// <summary>
    /// 激励广告(有)
    /// </summary>
    public void ShowRewardedAd(string _page_id, Action _rewardCallback, Action _displayErrorCallback, bool ignoreLevelLimit = false)
    {
        if (!ignoreLevelLimit && !CanPlayAds)
        {
            _rewardCallback?.Invoke();
            return;
        }

        if (ApplovinMaxRewardOperator.isPlayRewardAds)
        {
            _displayErrorCallback?.Invoke();
            return;
        }

        applovinMaxRewardOperator.RewardReceivedCallback = _rewardCallback;
        applovinMaxRewardOperator.RewardDisplayErrorCallback = _displayErrorCallback;
        applovinMaxRewardOperator.ShowRewardedAd();
    }

    /// <summary>
    /// 插屏广告
    /// </summary>
    public void OnClickInterstitialAd(string _page_id, bool isClick = true)
    {
        var interstitial = applovinMaxInterstitialOperator;
        if (!interstitial.IsInitializedAndIdle || !ApplovinMaxInterstitialOperator.inter_enable) return;
        if (!CanPlayAds || interstitial.insertClickCoolingTime > 0) return;

        interstitial.insertClickCoolingTime = ApplovinMaxInterstitialOperator.ad_mau_inter_time;
        if (string.IsNullOrEmpty(PlayerPrefs.GetString("EvaluationGame", ""))) return;

        interstitial.ShowInterstitialAd(_page_id, isClick);
    }

    public void ShowInterstitialDirectly(string pageId)
    {
        var interstitial = applovinMaxInterstitialOperator;
        if (!interstitial.IsInitializedAndIdle || !ApplovinMaxInterstitialOperator.inter_enable) return;
        if (ApplovinMaxRewardOperator.isPlayRewardAds || !interstitial.IsReady) return;

        interstitial.insertTimer = ApplovinMaxInterstitialOperator.insertTime;
        interstitial.insertClickCoolingTime = ApplovinMaxInterstitialOperator.ad_mau_inter_time;
        interstitial.ShowInterstitialAd(pageId, true);
    }


}