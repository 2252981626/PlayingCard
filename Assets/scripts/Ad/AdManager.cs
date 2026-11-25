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
    public void ShowRewardedAd(string _page_id, Action _rewardCallback, Action _displayErrorCallback)
    {
        if (!CanPlayAds)
        {
            ShowRewardedAd2(_page_id, _rewardCallback, _displayErrorCallback);
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
    /// 激励广告(无)
    /// </summary>
    public void ShowRewardedAd2(string _page_id, Action _rewardCallback, Action _displayErrorCallback)
    {
        DOTween.Sequence().AppendInterval(0.5F).AppendCallback(() =>
        {
            _rewardCallback?.Invoke();

        });
    }

    /// <summary>
    /// 插屏广告
    /// </summary>
    public void OnClickInterstitialAd(string _page_id, bool isClick = true, bool isLevelFourSettlement = false)
    {
        applovinMaxInterstitialOperator.OnClickInterstitialAd(_page_id, isClick, isLevelFourSettlement);
    }


}