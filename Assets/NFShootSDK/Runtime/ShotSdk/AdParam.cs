
#if NFSDK_OPS_OBFUSCATOR
using OPS.Obfuscator.Attribute;
#endif
using System;
[Serializable]
#if NFSDK_OPS_OBFUSCATOR
[DoNotObfuscateClassAttribute]
#endif
public class AdParam
{
    // // 1： Success  2：  AdFailed  3: show 5: onAdClick   6:onAdClose   
#if NFSDK_OPS_OBFUSCATOR
    [DoNotRenameAttribute]
#endif
    public int AdStatue;
#if NFSDK_OPS_OBFUSCATOR
    [DoNotRenameAttribute]
#endif
    public String AdPlace; // 广告placeid
#if NFSDK_OPS_OBFUSCATOR
    [DoNotRenameAttribute]
#endif
    public int AdType; // 1: 插屏 2：激励

#if NFSDK_OPS_OBFUSCATOR
    [DoNotRenameAttribute]
#endif
    public String UIntIdStr; //

#if NFSDK_OPS_OBFUSCATOR
    [DoNotRenameAttribute]
#endif
    public String showId;
#if NFSDK_OPS_OBFUSCATOR
    [DoNotRenameAttribute]
#endif
    public String sourceName;
#if NFSDK_OPS_OBFUSCATOR
    [DoNotRenameAttribute]
#endif
    public double revenue;

    public AdParam()
    {
        AdStatue = 0;
        AdPlace = "";
        AdType = 0;
        showId = "";
        sourceName = "";
        revenue = 0f;
    }

}

public class AdType
{
    public const int Interstitial = 1;
    public const int Reward = 2;
}
