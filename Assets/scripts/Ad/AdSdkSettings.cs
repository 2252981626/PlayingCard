using System;
using UnityEngine;

[CreateAssetMenu(fileName = "AdSdkSettings", menuName = "SDK/广告接入配置")]
public sealed class AdSdkSettings : ScriptableObject
{
    [Header("MAX SDK Key（密文）")]
    [TextArea(2, 4)]
    public string encryptedMaxSdkKey;

    [Header("Android 插屏广告位 ID")]
    public string androidInterstitialAdUnitId;

    [Header("Android 激励广告位 ID")]
    public string androidRewardedAdUnitId;

    [Header("SndoRun 应用 ID")]
    public string sndoRunAppId;

    [Header("Adjust App Token")]
    public string adjustAppToken;

    [Header("SolarEngine App Key")]
    public string solarEngineAppKey;

    private static AdSdkSettings instance;

    public static AdSdkSettings Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<AdSdkSettings>("AdSdkSettings");
                if (instance == null)
                {
                    throw new InvalidOperationException("缺少 Resources/AdSdkSettings 广告接入配置。");
                }
            }

            return instance;
        }
    }
}
