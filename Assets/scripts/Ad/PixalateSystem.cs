using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using ShootFramework;
using UnityEngine;
using UnityEngine.Networking;

namespace ShootFramework.SDK
{
	public class PixalateSystem 
	{
		private const string FallbackUserIp = "0.0.0.0";
		private const int UserIpRequestTimeoutSeconds = 3;
		private const int CountryLookupTimeoutSeconds = 3;
		private const int PixalateRequestTimeoutSeconds = 10;
		private const int PixalateMaxRetryCount = 3;
		private const float PixalateRetryDelaySeconds = 3f;
		private const string IpifyUrl = "https://api.ipify.org";
		private const string IpWhoUrl = "https://ipwho.is/";
		private static readonly string[] FallbackIpServiceUrls =
		{
			"https://api64.ipify.org",
			"https://checkip.amazonaws.com"
		};

		public string CachedUserIp { get; private set; } = FallbackUserIp;
		public string CachedCountryCode { get; private set; } = string.Empty;
		public bool HasResolvedUserIp { get; private set; }
		public bool HasResolvedCountryCode { get; private set; }
		public bool IsRequestingUserIp { get; private set; }

		public void Init()
		{
			StartPrefetchUserIp();
		}

		public void StartPrefetchUserIp()
		{
			if (HasResolvedUserIp || IsRequestingUserIp)
				return;

            OtherSdkManager.Instance.StartCoroutine(FetchUserIpAtLaunch());
		}

		public void SendPixalateRequest(string AdUnitId)
		{
			if (string.IsNullOrEmpty(AdUnitId))
			{
				Debug.LogError("[Pixalate] Skip request: AdUnitId is null.");
				return;
			}

			string publisherId = OtherSdkManager.CurrentAdjustAdgroup;
			Debug.Log($"[Pixalate] Start.AdUnitId = {AdUnitId} ");
            //Debug.Log($"[Pixalate] Start. AdType={adParam.AdType}, AdStatue={adParam.AdStatue}, sourceName={adParam.sourceName}, AdPlace={adParam.AdPlace}, UIntIdStr={adParam.UIntIdStr}, showId={adParam.showId}, revenue={adParam.revenue}");
            OtherSdkManager.Instance.StartCoroutine(SendPixalateRequestCoroutine(AdUnitId, publisherId));
		}

		private string BuildPixalateUrl(string AdUnitId, string userIp, string publisherId, string userAgent)
		{
//			https://adrta.com/i?clid=sml&paid=max&publisherId=[PUBLISHER_ID]&siteId=[SITE_ID]&kv1=[CREATIVE_SIZE]&kv3=[USER_ID]&kv4=[USER_IP]&kv11=[IMPRESSION_ID]&kv12=[PLACEMENT_ID]&kv15=[GEOGRAPHIC_REGION]&kv18=[APP_ID]&kv19=[DEVICE_ID]&kv26=[DEVICE_OS]&kv27=[USERAGENT]&kv24=Mobile_InApp
	//		https://adrta.com/i?clid=sml&paid=max&publisherId=&siteId=e86acdf1c24a2948&kv1=1440x3200&kv3=17c69aed1d030c4b57f7f527b69cfdce&kv4=119.13.90.246&kv11=e86acdf1c24a2948&kv12=e86acdf1c24a2948&kv15=ChineseSimplified&kv18=com.arrowcash.withdraw.game&kv19=17c69aed1d030c4b57f7f527b69cfdce&kv26=Android+OS+12+%2f+API-32+(V417IR%2f1323)&kv27=Mozilla%2f5.0+(Linux%3b+Android+12%3b+PJJ110+Build%2fV417IR%3b+wv)+AppleWebKit%2f537.36+(KHTML%2c+like+Gecko)+Version%2f4.0+Chrome%2f110.0.5481.154+Safari%2f537.36&kv24=Mobile_InApp

			// 字段映射参考 Macro Mapping sheet_ShootMedia_Aug2026.xlsx。
			// clid / paid / kv24 由 Pixalate 固定分配；其余字段优先使用广告回调和 Unity 运行时可获得的数据。
			Dictionary<string, string> queryParams = new Dictionary<string, string>
			{
				{ "clid", "sml" },
				{ "paid", "max" },
				{ "publisherId", publisherId },
				{ "siteId", AdUnitId },
				{ "kv1", GetCreativeSize() },
				{ "kv3", GetUserId() },
				{ "kv4", GetRequiredUserIp(userIp) },
				{ "kv11", Guid.NewGuid().ToString("N") },
				{ "kv12", AdUnitId },
				{ "kv15", GetCountryCode() },
				{ "kv18", Application.identifier },
				{ "kv19", SystemInfo.deviceUniqueIdentifier },
				{ "kv26", SystemInfo.operatingSystem },
				{ "kv27", userAgent },
				{ "kv24", "Mobile_InApp" },
				{ "kv62", Application.version }
			};

			return "https://adrta.com/i?" + BuildQueryString(queryParams);
		}

		private string BuildQueryString(Dictionary<string, string> queryParams)
		{
			List<string> pairs = new List<string>();
			foreach (KeyValuePair<string, string> pair in queryParams)
			{
				pairs.Add($"{UnityWebRequest.EscapeURL(pair.Key)}={UnityWebRequest.EscapeURL(pair.Value ?? string.Empty)}");
			}

			return string.Join("&", pairs);
		}

		private string GetCreativeSize()
		{
			return $"{Screen.width}x{Screen.height}";
		}

		private string GetUserId()
		{
			return SystemInfo.deviceUniqueIdentifier;
		}

		private string GetCountryCode()
		{
			if (!string.IsNullOrEmpty(CachedCountryCode))
				return CachedCountryCode;

			try
			{
				return RegionInfo.CurrentRegion.TwoLetterISORegionName;
			}
			catch (CultureNotFoundException ex)
			{
				Debug.LogError($"[Pixalate] Get country code failed: {ex.Message}");
				return string.Empty;
			}
		}

		private string GetRequiredUserIp(string userIp)
		{
			return string.IsNullOrWhiteSpace(userIp) ? FallbackUserIp : userIp.Trim();
		}


		private string GetUserAgent()
		{
#if UNITY_ANDROID && !UNITY_EDITOR
			try
			{
				using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
				using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
				using (AndroidJavaClass webSettings = new AndroidJavaClass("android.webkit.WebSettings"))
				{
					string userAgent = webSettings.CallStatic<string>("getDefaultUserAgent", activity);
					if (!string.IsNullOrWhiteSpace(userAgent))
						return userAgent;
				}
			}
			catch (Exception ex)
			{
				Debug.LogError($"[Pixalate] Get Android userAgent failed: {ex.Message}");
			}
#endif
			return SystemInfo.operatingSystem;
		}

		private IEnumerator SendPixalateRequestCoroutine(string AdUnitId, string publisherId)
		{
			while (IsRequestingUserIp)
				yield return null;

			Debug.Log($"[Pixalate] Use launch-cached user ip: {CachedUserIp}");
			string userAgent = GetUserAgent();
			string url = BuildPixalateUrl(AdUnitId, CachedUserIp, publisherId, userAgent);
			Debug.Log($"[Pixalate] Final request url: {url}");
			yield return SendPixalateRequestCoroutine2(url, userAgent);
		}

		private IEnumerator FetchUserIpAtLaunch()
		{
			IsRequestingUserIp = true;
			Debug.Log("[Pixalate] Launch user ip prefetch start.");

			try
			{
				using (UnityWebRequest request = UnityWebRequest.Get(IpifyUrl))
				{
					Debug.Log($"[Pixalate] Try user ip service: {IpifyUrl}");
					request.timeout = UserIpRequestTimeoutSeconds;
					yield return request.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
					bool failed = request.result != UnityWebRequest.Result.Success;
#else
					bool failed = request.isNetworkError || request.isHttpError;
#endif
					string responseIp = request.downloadHandler != null ? request.downloadHandler.text.Trim() : string.Empty;
					if (!failed && IPAddress.TryParse(responseIp, out _))
					{
						CachedUserIp = responseIp;
						HasResolvedUserIp = true;
						yield return ResolveCountryCode(responseIp);
						Debug.Log($"[Pixalate] Launch user ip prefetch success. service={IpifyUrl}, ip={CachedUserIp}, country={GetCountryCode()}");
						yield break;
					}

					Debug.LogError($"[Pixalate] User ip service failed. service={IpifyUrl}, responseCode={request.responseCode}, error={request.error}, response={responseIp}");
				}

				Debug.LogError($"[Pixalate] Primary IP lookup failed. Try fallback service: {IpWhoUrl}");
				yield return ResolveIpAndCountryFromIpWho();

				if (!HasResolvedUserIp)
				{
					foreach (string serviceUrl in FallbackIpServiceUrls)
					{
						yield return ResolveIpFromService(serviceUrl);
						if (HasResolvedUserIp)
						{
							yield return ResolveCountryCode(CachedUserIp);
							break;
						}
					}
				}

				if (!HasResolvedUserIp)
					Debug.LogError($"[Pixalate] All user ip services failed. Keep fallback ip: {FallbackUserIp}");
			}
			finally
			{
				IsRequestingUserIp = false;
			}
		}

		private IEnumerator ResolveCountryCode(string userIp)
		{
			string url = IpWhoUrl + userIp;
			using (UnityWebRequest request = UnityWebRequest.Get(url))
			{
				request.timeout = CountryLookupTimeoutSeconds;
				yield return request.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
				bool failed = request.result != UnityWebRequest.Result.Success;
#else
				bool failed = request.isNetworkError || request.isHttpError;
#endif
				if (failed || request.downloadHandler == null)
				{
					Debug.LogError($"[Pixalate] Country lookup failed. url={url}, responseCode={request.responseCode}, error={request.error}");
					yield break;
				}

				IpWhoResponse response = ParseIpWhoResponse(request.downloadHandler.text, url);
				if (response == null)
					yield break;

				SetCountryCode(response.country_code);
			}
		}

		private IEnumerator ResolveIpAndCountryFromIpWho()
		{
			using (UnityWebRequest request = UnityWebRequest.Get(IpWhoUrl))
			{
				request.timeout = UserIpRequestTimeoutSeconds;
				yield return request.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
				bool failed = request.result != UnityWebRequest.Result.Success;
#else
				bool failed = request.isNetworkError || request.isHttpError;
#endif
				if (failed || request.downloadHandler == null)
				{
					Debug.LogError($"[Pixalate] Fallback IP lookup failed. url={IpWhoUrl}, responseCode={request.responseCode}, error={request.error}");
					yield break;
				}

				IpWhoResponse response = ParseIpWhoResponse(request.downloadHandler.text, IpWhoUrl);
				if (response == null || !IPAddress.TryParse(response.ip, out _))
					yield break;

				CachedUserIp = response.ip;
				HasResolvedUserIp = true;
				SetCountryCode(response.country_code);
				Debug.Log($"[Pixalate] Fallback IP lookup success. service={IpWhoUrl}, ip={CachedUserIp}, country={GetCountryCode()}");
			}
		}

		private IEnumerator ResolveIpFromService(string serviceUrl)
		{
			using (UnityWebRequest request = UnityWebRequest.Get(serviceUrl))
			{
				request.timeout = UserIpRequestTimeoutSeconds;
				yield return request.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
				bool failed = request.result != UnityWebRequest.Result.Success;
#else
				bool failed = request.isNetworkError || request.isHttpError;
#endif
				string responseIp = request.downloadHandler != null ? request.downloadHandler.text.Trim() : string.Empty;
				if (!failed && IPAddress.TryParse(responseIp, out _))
				{
					CachedUserIp = responseIp;
					HasResolvedUserIp = true;
					Debug.Log($"[Pixalate] Fallback IP lookup success. service={serviceUrl}, ip={CachedUserIp}");
					yield break;
				}

				Debug.LogError($"[Pixalate] Fallback IP service failed. service={serviceUrl}, responseCode={request.responseCode}, error={request.error}, response={responseIp}");
			}
		}

		private IpWhoResponse ParseIpWhoResponse(string json, string url)
		{
			try
			{
				return JsonUtility.FromJson<IpWhoResponse>(json);
			}
			catch (Exception ex)
			{
				Debug.LogError($"[Pixalate] Country response parse failed. url={url}, error={ex.Message}");
				return null;
			}
		}

		private void SetCountryCode(string countryCode)
		{
			if (!string.IsNullOrEmpty(countryCode) && countryCode.Length == 2)
			{
				CachedCountryCode = countryCode.ToUpperInvariant();
				HasResolvedCountryCode = true;
			}
		}

		[Serializable]
		private class IpWhoResponse
		{
			public string ip;
			public string country_code;
		}

		private IEnumerator SendPixalateRequestCoroutine2(string url, string userAgent)
		{
			for (int retryCount = 0; ; retryCount++)
			{
				using (UnityWebRequest request = UnityWebRequest.Get(url))
				{
					request.timeout = PixalateRequestTimeoutSeconds;
					request.SetRequestHeader("User-Agent", userAgent);
					Debug.Log($"[Pixalate] Request headers. attempt={retryCount + 1}, User-Agent={userAgent}");
					yield return request.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
					bool failed = request.result != UnityWebRequest.Result.Success;
#else
					bool failed = request.isNetworkError || request.isHttpError;
#endif
					if (!failed)
					{
					Debug.Log($"[Pixalate] Request success. responseCode={request.responseCode}, url={url}");
						yield break;
					}

					bool canRetry = request.responseCode == 0 && retryCount < PixalateMaxRetryCount;
					if (!canRetry)
					{
						Debug.LogError($"[Pixalate] Request failed. attempts={retryCount + 1}, responseCode={request.responseCode}, error={request.error}, url={url}");
						yield break;
					}

					float retryDelay = PixalateRetryDelaySeconds * (retryCount + 1);
					Debug.LogError($"[Pixalate] Request failed. attempts={retryCount + 1}, responseCode=0, error={request.error}. Retry in {retryDelay:F0}s.");
					yield return new WaitForSecondsRealtime(retryDelay);
				}
			}
		}
	}
}





