using System;
using Firebase.Analytics;
using UnityEngine;

/// <summary>
/// Firebase Analytics 广告价值打点（激励视频 / 插屏）。
///
/// 统一使用 GA4 推荐事件 <c>ad_impression</c>，参数与 GA4「广告收入」报表对齐：
/// <list type="bullet">
///   <item><description>ad_platform  —— 聚合平台（本项固定为 AppLovin）</description></item>
///   <item><description>ad_source    —— 实际变现的广告网络</description></item>
///   <item><description>ad_format    —— rewarded / interstitial</description></item>
///   <item><description>ad_unit_name —— 广告位 ID</description></item>
///   <item><description>currency     —— USD（GA4 规定：传 value 必须同时传 currency）</description></item>
///   <item><description>value        —— 本次展示的广告收入，即「广告价值」</description></item>
/// </list>
///
/// 参数名刻意使用字面量而非 SDK 常量类，避免不同 Firebase SDK 版本间常量类归属差异导致编译失败。
/// </summary>
public static class FirebaseAdAnalytics
{
    // ---------------- GA4 事件与参数名 ----------------
    public const string EventAdImpression = "ad_impression";

    private const string ParamAdPlatform = "ad_platform";
    private const string ParamAdSource = "ad_source";
    private const string ParamAdFormat = "ad_format";
    private const string ParamAdUnitName = "ad_unit_name";
    private const string ParamCurrency = "currency";
    private const string ParamValue = "value";

    // ---------------- ad_format 取值（GA4 规范） ----------------
    /// <summary>激励视频</summary>
    public const string AdFormatRewarded = "rewarded";
    /// <summary>插屏</summary>
    public const string AdFormatInterstitial = "interstitial";

    /// <summary>聚合平台标识，写入 ad_platform</summary>
    private const string MediationPlatform = "AppLovin";

    /// <summary>收入币种</summary>
    private const string RevenueCurrency = "USD";

    /// <summary>
    /// Firebase 依赖是否已就绪。
    /// 由 <see cref="FirebaseAppCheckManager"/> 在 CheckAndFixDependenciesAsync 返回
    /// Available 之后置为 true；未就绪时不上报，避免抛异常。
    /// </summary>
    public static bool IsReady { get; set; }

    private static bool _warnedNotReady;

    /// <summary>
    /// 上报一次广告展示（广告价值）。
    /// </summary>
    /// <param name="networkName">广告网络名称</param>
    /// <param name="adUnitIdentifier">广告位 ID</param>
    /// <param name="adRevenue">本次展示的广告收入（USD）</param>
    /// <param name="adFormat">ad_format 取值，见 <see cref="AdFormatRewarded"/> / <see cref="AdFormatInterstitial"/></param>
    public static void LogAdImpression(string networkName, string adUnitIdentifier, double adRevenue, string adFormat)
    {
        if (!IsReady)
        {
            // 只提示一次，避免广告频繁触发时刷屏
            if (!_warnedNotReady)
            {
                _warnedNotReady = true;
                Debug.LogWarning("[FirebaseAdAnalytics] Firebase 尚未就绪，ad_impression 暂不上报（仅提示一次）");
            }
            return;
        }

        try
        {
            string source = string.IsNullOrEmpty(networkName) ? "unknown" : networkName;
            string unit = string.IsNullOrEmpty(adUnitIdentifier) ? "unknown" : adUnitIdentifier;
            // 收入不可用或为负数时归零，但依然记录一次展示。
            double revenue = adRevenue > 0d ? adRevenue : 0d;

            FirebaseAnalytics.LogEvent(
                EventAdImpression,
                new Parameter(ParamAdPlatform, MediationPlatform),
                new Parameter(ParamAdSource, source),
                new Parameter(ParamAdFormat, adFormat),
                new Parameter(ParamAdUnitName, unit),
                new Parameter(ParamCurrency, RevenueCurrency),
                new Parameter(ParamValue, revenue));

            Debug.Log($"[FirebaseAdAnalytics] ad_impression 上报: format={adFormat}, source={source}, unit={unit}, value={revenue}");
        }
        catch (Exception e)
        {
            Debug.LogWarning("[FirebaseAdAnalytics] ad_impression 上报异常: " + e.Message);
        }
    }
}
