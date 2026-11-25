using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public sealed class NativeMaxSdk : MonoBehaviour
{
    private const string BridgeClass = "com.citydaheng.solitaire.ads.NativeMaxBridge";
    private static NativeMaxSdk instance;
    private static bool initializationRequested;
    private static string interstitialAdUnitId;
    private static string rewardedAdUnitId;
    private static bool interstitialReady;
    private static bool rewardedReady;

    public static bool IsInitialized { get; private set; }
    public static event Action<SdkConfiguration> InitializationEvent;
    public static event Action<ErrorInfo> InitializationFailedEvent;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static void InitializeSdk(string sdkKey, string interId, string rewardId, string sndoId)
    {
        if (IsInitialized)
        {
            InitializationEvent?.Invoke(new SdkConfiguration());
            return;
        }
        if (initializationRequested) return;

        if (instance == null)
        {
            var receiver = new GameObject(nameof(NativeMaxSdk));
            instance = receiver.AddComponent<NativeMaxSdk>();
        }

        interstitialAdUnitId = interId;
        rewardedAdUnitId = rewardId;
        interstitialReady = false;
        rewardedReady = false;
        initializationRequested = true;

#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var bridge = new AndroidJavaClass(BridgeClass))
            {
                bridge.CallStatic("initialize", activity, sdkKey, interId, rewardId, sndoId, instance.gameObject.name);
            }
        }
        catch (Exception error)
        {
            FailInitialization(error.Message);
        }
#else
        FailInitialization("Native MAX ads are available in Android players only.");
#endif
    }

    public static void LoadInterstitial(string adUnitId)
    {
        ExecuteNative("loadInterstitial", "interstitial", adUnitId, false);
    }

    public static bool IsInterstitialReady(string adUnitId)
    {
        return IsInitialized && interstitialReady && adUnitId == interstitialAdUnitId;
    }

    public static void ShowInterstitial(string adUnitId)
    {
        interstitialReady = false;
        ExecuteNative("showInterstitial", "interstitial", adUnitId, true);
    }

    public static void LoadRewardedAd(string adUnitId)
    {
        ExecuteNative("loadRewarded", "rewarded", adUnitId, false);
    }

    public static bool IsRewardedAdReady(string adUnitId)
    {
        return IsInitialized && rewardedReady && adUnitId == rewardedAdUnitId;
    }

    public static void ShowRewardedAd(string adUnitId)
    {
        rewardedReady = false;
        ExecuteNative("showRewarded", "rewarded", adUnitId, true);
    }

    private static void ExecuteNative(string method, string adType, string adUnitId, bool display)
    {
        string failure = "Native MAX SDK is not initialized.";
#if UNITY_ANDROID && !UNITY_EDITOR
        if (IsInitialized)
        {
            try
            {
                using (var bridge = new AndroidJavaClass(BridgeClass))
                {
                    bridge.CallStatic(method);
                }
                return;
            }
            catch (Exception error)
            {
                failure = error.Message;
            }
        }
#endif
        DispatchEvent(new NativeEvent
        {
            @event = display ? "display_failed" : "load_failed",
            adType = adType,
            adUnitId = adUnitId,
            errorCode = -1,
            errorMessage = failure
        });
    }

    private static void FailInitialization(string message)
    {
        IsInitialized = false;
        initializationRequested = false;
        interstitialReady = false;
        rewardedReady = false;
        InitializationFailedEvent?.Invoke(new ErrorInfo(-1, message));
    }

    [Preserve]
    public void OnNativeAdEvent(string json)
    {
        NativeEvent nativeEvent;
        try
        {
            nativeEvent = JsonUtility.FromJson<NativeEvent>(json);
        }
        catch (Exception error)
        {
            Debug.LogError("[NativeMaxSdk] Invalid native callback: " + error.Message);
            return;
        }

        if (nativeEvent != null) DispatchEvent(nativeEvent);
    }

    private static void DispatchEvent(NativeEvent nativeEvent)
    {
        if (nativeEvent.@event == "initialized")
        {
            IsInitialized = true;
            InitializationEvent?.Invoke(new SdkConfiguration());
            return;
        }

        if (nativeEvent.@event == "initialization_failed")
        {
            FailInitialization(nativeEvent.errorMessage);
            return;
        }

        bool rewarded = nativeEvent.adType == "rewarded";
        if (!rewarded && nativeEvent.adType != "interstitial") return;

        if (string.IsNullOrEmpty(nativeEvent.adUnitId))
        {
            nativeEvent.adUnitId = rewarded ? rewardedAdUnitId : interstitialAdUnitId;
        }

        switch (nativeEvent.@event)
        {
            case "loaded":
                if (rewarded) rewardedReady = true;
                else interstitialReady = true;
                break;
            case "load_failed":
            case "displayed":
            case "display_failed":
            case "hidden":
                if (rewarded) rewardedReady = false;
                else interstitialReady = false;
                break;
        }

        var adInfo = new AdInfo(nativeEvent);
        var error = new ErrorInfo(nativeEvent.errorCode, nativeEvent.errorMessage);
        if (rewarded) Rewarded.Dispatch(nativeEvent, adInfo, error);
        else Interstitial.Dispatch(nativeEvent, adInfo, error);
    }

    private void OnDestroy()
    {
        if (instance != this) return;
        instance = null;
        IsInitialized = false;
        initializationRequested = false;
        interstitialReady = false;
        rewardedReady = false;
    }

    public static class Interstitial
    {
        public static event Action<string, AdInfo> OnAdLoadedEvent;
        public static event Action<string, ErrorInfo> OnAdLoadFailedEvent;
        public static event Action<string, AdInfo> OnAdDisplayedEvent;
        public static event Action<string, AdInfo> OnAdClickedEvent;
        public static event Action<string, AdInfo> OnAdHiddenEvent;
        public static event Action<string, ErrorInfo, AdInfo> OnAdDisplayFailedEvent;
        public static event Action<string, AdInfo> OnAdRevenuePaidEvent;

        internal static void Dispatch(NativeEvent nativeEvent, AdInfo adInfo, ErrorInfo error)
        {
            string adUnitId = adInfo.AdUnitIdentifier;
            switch (nativeEvent.@event)
            {
                case "loaded": OnAdLoadedEvent?.Invoke(adUnitId, adInfo); break;
                case "load_failed": OnAdLoadFailedEvent?.Invoke(adUnitId, error); break;
                case "displayed": OnAdDisplayedEvent?.Invoke(adUnitId, adInfo); break;
                case "clicked": OnAdClickedEvent?.Invoke(adUnitId, adInfo); break;
                case "hidden": OnAdHiddenEvent?.Invoke(adUnitId, adInfo); break;
                case "display_failed": OnAdDisplayFailedEvent?.Invoke(adUnitId, error, adInfo); break;
                case "revenue": OnAdRevenuePaidEvent?.Invoke(adUnitId, adInfo); break;
            }
        }
    }

    public static class Rewarded
    {
        public static event Action<string, AdInfo> OnAdLoadedEvent;
        public static event Action<string, ErrorInfo> OnAdLoadFailedEvent;
        public static event Action<string, AdInfo> OnAdDisplayedEvent;
        public static event Action<string, AdInfo> OnAdClickedEvent;
        public static event Action<string, AdInfo> OnAdHiddenEvent;
        public static event Action<string, ErrorInfo, AdInfo> OnAdDisplayFailedEvent;
        public static event Action<string, AdInfo> OnAdRevenuePaidEvent;
        public static event Action<string, Reward, AdInfo> OnAdReceivedRewardEvent;

        internal static void Dispatch(NativeEvent nativeEvent, AdInfo adInfo, ErrorInfo error)
        {
            string adUnitId = adInfo.AdUnitIdentifier;
            switch (nativeEvent.@event)
            {
                case "loaded": OnAdLoadedEvent?.Invoke(adUnitId, adInfo); break;
                case "load_failed": OnAdLoadFailedEvent?.Invoke(adUnitId, error); break;
                case "displayed": OnAdDisplayedEvent?.Invoke(adUnitId, adInfo); break;
                case "clicked": OnAdClickedEvent?.Invoke(adUnitId, adInfo); break;
                case "hidden": OnAdHiddenEvent?.Invoke(adUnitId, adInfo); break;
                case "display_failed": OnAdDisplayFailedEvent?.Invoke(adUnitId, error, adInfo); break;
                case "revenue": OnAdRevenuePaidEvent?.Invoke(adUnitId, adInfo); break;
                case "rewarded":
                    OnAdReceivedRewardEvent?.Invoke(adUnitId,
                        new Reward(nativeEvent.rewardLabel, nativeEvent.rewardAmount), adInfo);
                    break;
            }
        }
    }

    [Preserve]
    public sealed class SdkConfiguration { }

    [Preserve]
    public sealed class AdInfo
    {
        public string AdUnitIdentifier { get; private set; }
        public string NetworkName { get; private set; }
        public string NetworkPlacement { get; private set; }
        public string Placement { get; private set; }
        public double Revenue { get; private set; }

        internal AdInfo(NativeEvent nativeEvent)
        {
            AdUnitIdentifier = nativeEvent.adUnitId ?? string.Empty;
            NetworkName = nativeEvent.networkName ?? string.Empty;
            NetworkPlacement = nativeEvent.networkPlacement ?? string.Empty;
            Placement = nativeEvent.placement ?? string.Empty;
            Revenue = nativeEvent.revenue;
        }
    }

    [Preserve]
    public sealed class ErrorInfo
    {
        public int Code { get; private set; }
        public string Message { get; private set; }

        public ErrorInfo(int code, string message)
        {
            Code = code;
            Message = message ?? string.Empty;
        }
    }

    [Preserve]
    public struct Reward
    {
        public string Label;
        public int Amount;

        public Reward(string label, int amount)
        {
            Label = label ?? string.Empty;
            Amount = amount;
        }
    }

    [Serializable, Preserve]
    internal sealed class NativeEvent
    {
        public string @event;
        public string adType;
        public string adUnitId;
        public string networkName;
        public string networkPlacement;
        public string placement;
        public double revenue = -1d;
        public int errorCode;
        public string errorMessage;
        public string rewardLabel;
        public int rewardAmount;
    }
}
