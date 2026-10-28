using DG.Tweening;
using System;
using UnityEngine;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance;

    public static bool ShowAdIcon = true;
    //-----------------------------------------------------------
    public static bool inter_enable = true;
    // True skips the shared interstitial cooldown.
    public static bool mau_inter = false;
    public static float ad_mau_inter_time = 45;
    public static float insertTime = 60;
    public static float startInsertTime = 120;

    public float insertTimer { get; private set; }
    public float insertClickCoolingTime { get; private set; }

    private bool isInit;


    //public ApplovinMaxRewardOperator applovinMaxRewardOperator;
    //public ApplovinMaxInterstitialOperator applovinMaxInterstitialOperator;
    //private string SDK_key = "PbbJng_h8aD16wZWrSaHN5gtVDExorX-b1ywfx8Gal1WlU7kvbWVDpzsPARTTLwex_cbeU8SGZanUXSoA1WDMx";//测试
    private string SDK_key = "4xI8Wexro+t5Cg53ZFj2t6ML1GMEiPi99oBHbP4pfnxdjOou+X/9faU05XjKHFRTL+/wq9t4rKX99il4AwD0REdQSNVF9rAgymZUi6GipA353uCPvO1ejwgWXKDF+rSOoSOnA07lTT8=";

    private void Awake()
    {
        Instance = this;
        Init();
    }

    public void Init()
    {
        if (isInit) return;

        ResetInterstitialTimer(startInsertTime);
        insertClickCoolingTime = 0f;
        isInit = true;
    }

    private void Update()
    {
        if (!CanProcessInterstitial()) return;

        insertTimer = Mathf.Max(0f, insertTimer - Time.deltaTime);
        insertClickCoolingTime = Mathf.Max(0f, insertClickCoolingTime - Time.deltaTime);

        if (insertTimer <= 0f && insertClickCoolingTime <= 0f)
        {
            ResetInterstitialTimer(insertTime);
            OnClickInterstitialAd("auto_interstitial_show", false);
        }
    }

    private bool CanProcessInterstitial()
    {
        return isInit && inter_enable && !GameLoadingPanel.isOpenStatic;
    }

    public void ResetInterstitialTimer(float delay)
    {
        insertTimer = Mathf.Max(0f, delay);
    }

    public void ResetInterstitialAfterEvaluation()
    {
        insertClickCoolingTime = 0f;
        ResetInterstitialTimer(insertTime);
    }

    /// <summary>
    /// 激励广告(有)
    /// </summary>
    public void ShowRewardedAd(string _page_id, Action _rewardCallback, Action _displayErrorCallback)
    {
        if (ShootSdk.Instance == null)
        {
            Debug.LogWarning("[AdManager] ShootSdk is not initialized; add ShootSdk to the startup scene.");
            _displayErrorCallback?.Invoke();
            return;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        ShootSdk.Instance.PlayRewardAds(() =>
        {
            insertClickCoolingTime = Mathf.Max(0f, ad_mau_inter_time);
            ResetInterstitialTimer(insertTime);
            _rewardCallback?.Invoke();
        }, _page_id, _displayErrorCallback);
#else
        Debug.Log("[AdManager] Rewarded ads are available in Android players only.");
        _displayErrorCallback?.Invoke();
#endif

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
    public void OnClickInterstitialAd(string _page_id, bool isClick = true)
    {
        if (!CanProcessInterstitial()) return;

        // Both request types obey cooldown unless mau_inter is enabled.
        if (!mau_inter && insertClickCoolingTime > 0f) return;
        if (string.IsNullOrEmpty(PlayerPrefs.GetString("EvaluationGame", ""))) return;

        if (ShootSdk.Instance == null || Platform.Instance == null)
        {
            Debug.LogWarning("[AdManager] ShootSdk is not initialized; interstitial request skipped.");
            return;
        }

        insertClickCoolingTime = Mathf.Max(0f, ad_mau_inter_time);
        Debug.Log($"[AdManager] Interstitial request: page_id={_page_id}, isClick={isClick}");

        ShootSdk.Instance.PlayInterstitialAds(null);
    }

}