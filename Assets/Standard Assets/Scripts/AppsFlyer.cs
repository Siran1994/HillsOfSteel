using System;
using System.Collections.Generic;
using UnityEngine;

public class AppsFlyer : MonoBehaviour
{
//	private static AndroidJavaClass obj = new AndroidJavaClass("com.appsflyer.AppsFlyerLib");

//	private static AndroidJavaObject cls_AppsFlyer = obj.CallStatic<AndroidJavaObject>("getInstance", Array.Empty<object>());

//	private static AndroidJavaClass propertiesClass = new AndroidJavaClass("com.appsflyer.AppsFlyerProperties");

//	private static AndroidJavaClass cls_AppsFlyerHelper = new AndroidJavaClass("com.appsflyer.AppsFlyerUnityHelper");

	private static string devKey;

//	private static AndroidJavaClass cls_UnityShareHelper = new AndroidJavaClass("com.appsflyer.UnityShareHelper");

//	private static AndroidJavaObject ShareHelperInstance = cls_UnityShareHelper.CallStatic<AndroidJavaObject>("getInstance", Array.Empty<object>());

//	private static AndroidJavaClass cls_AndroidShare = new AndroidJavaClass("com.appsflyer.share.CrossPromotionHelper");

	public static void trackEvent(string eventName, string eventValue)
	{
		MonoBehaviour.print("AF.cs this is deprecated method. please use trackRichEvent instead. nothing is sent.");
	}

	public static void setCurrencyCode(string currencyCode)
	{
//		cls_AppsFlyer.Call("setCurrencyCode", currencyCode);
	}

	public static void setCustomerUserID(string customerUserID)
	{
//		cls_AppsFlyer.Call("setAppUserId", customerUserID);
	}

	public static void loadConversionData(string callbackObject)
	{
//		using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
//		{
//			using (AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity"))
//			{
//				cls_AppsFlyerHelper.CallStatic("createConversionDataListener", androidJavaObject, callbackObject);
//			}
//		}
	}

	[Obsolete("Use loadConversionData(string callbackObject)")]
	public static void loadConversionData(string callbackObject, string callbackMethod, string callbackFailedMethod)
	{
		loadConversionData(callbackObject);
	}

	public static void setCollectIMEI(bool shouldCollect)
	{
//		cls_AppsFlyer.Call("setCollectIMEI", shouldCollect);
	}

	public static void setCollectAndroidID(bool shouldCollect)
	{
		MonoBehaviour.print("AF.cs setCollectAndroidID");
//		cls_AppsFlyer.Call("setCollectAndroidID", shouldCollect);
	}

	public static void init(string key, string callbackObject)
	{
		init(key);
		if (callbackObject != null)
		{
			loadConversionData(callbackObject);
		}
	}

	public static void init(string key)
	{
		MonoBehaviour.print("AF.cs init");
		devKey = key;
//		using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
//		{
//			using (AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity"))
//			{
//				androidJavaObject.Call("runOnUiThread", new AndroidJavaRunnable(init_cb));
//			}
//		}
	}

	private static void init_cb()
	{
		MonoBehaviour.print("AF.cs start tracking");
		trackAppLaunch();
	}

	public static void setAppsFlyerKey(string key)
	{
		MonoBehaviour.print("AF.cs setAppsFlyerKey");
	}

	public static void trackAppLaunch()
	{
		MonoBehaviour.print("AF.cs trackAppLaunch");
//		using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
//		{
//			using (AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity"))
//			{
//				AndroidJavaObject androidJavaObject2 = androidJavaObject.Call<AndroidJavaObject>("getApplication", Array.Empty<object>());
////				cls_AppsFlyer.Call("startTracking", androidJavaObject2, devKey);
////				cls_AppsFlyer.Call("trackAppLaunch", androidJavaObject, devKey);
//			}
//		}
	}

	public static void setAppID(string packageName)
	{
//		cls_AppsFlyer.Call("setAppId", packageName);
	}

	public static void createValidateInAppListener(string aObject, string callbackMethod, string callbackFailedMethod)
	{
		MonoBehaviour.print("AF.cs createValidateInAppListener called");
//		using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
//		{
//			using (AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity"))
//			{
//				cls_AppsFlyerHelper.CallStatic("createValidateInAppListener", androidJavaObject, aObject, callbackMethod, callbackFailedMethod);
//			}
//		}
	}

	public static void validateReceipt(string publicKey, string purchaseData, string signature, string price, string currency, Dictionary<string, string> extraParams)
	{
		MonoBehaviour.print("AF.cs validateReceipt pk = " + publicKey + " data = " + purchaseData + "sig = " + signature);
//		using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
//		{
//			using (AndroidJavaObject androidJavaObject2 = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity"))
//			{
//				AndroidJavaObject androidJavaObject = null;
//				if (extraParams != null)
//				{
//					androidJavaObject = ConvertHashMap(extraParams);
//				}
//				MonoBehaviour.print("inside cls_activity");
//				cls_AppsFlyer.Call("validateAndTrackInAppPurchase", androidJavaObject2, publicKey, signature, purchaseData, price, currency, androidJavaObject);
//			}
//		}
	}

	public static void trackRichEvent(string eventName, Dictionary<string, string> eventValues)
	{
//		using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
//		{
//			using (AndroidJavaObject androidJavaObject2 = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity"))
//			{
//				AndroidJavaObject androidJavaObject = ConvertHashMap(eventValues);
//				cls_AppsFlyer.Call("trackEvent", androidJavaObject2, eventName, androidJavaObject);
//			}
//		}
	}

	private static AndroidJavaObject ConvertHashMap(Dictionary<string, string> dict)
	{
		AndroidJavaObject androidJavaObject = new AndroidJavaObject("java.util.HashMap");
		IntPtr methodID = AndroidJNIHelper.GetMethodID(androidJavaObject.GetRawClass(), "put", "(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;");
		object[] array = new object[2];
		foreach (KeyValuePair<string, string> item in dict)
		{
			using (AndroidJavaObject androidJavaObject2 = new AndroidJavaObject("java.lang.String", item.Key))
			{
				using (AndroidJavaObject androidJavaObject3 = new AndroidJavaObject("java.lang.String", item.Value))
				{
					array[0] = androidJavaObject2;
					array[1] = androidJavaObject3;
					AndroidJNI.CallObjectMethod(androidJavaObject.GetRawObject(), methodID, AndroidJNIHelper.CreateJNIArgArray(array));
				}
			}
		}
		return androidJavaObject;
	}

	public static void setImeiData(string imeiData)
	{
		MonoBehaviour.print("AF.cs setImeiData");
//		cls_AppsFlyer.Call("setImeiData", imeiData);
	}

	public static void setAndroidIdData(string androidIdData)
	{
		MonoBehaviour.print("AF.cs setImeiData");
//		cls_AppsFlyer.Call("setAndroidIdData", androidIdData);
	}

	public static void setIsDebug(bool isDebug)
	{
		MonoBehaviour.print("AF.cs setDebugLog");
//		cls_AppsFlyer.Call("setDebugLog", isDebug);
	}

	public static void setIsSandbox(bool isSandbox)
	{
	}

	public static void getConversionData()
	{
	}

	public static void handleOpenUrl(string url, string sourceApplication, string annotation)
	{
	}

	public static string getAppsFlyerId()
	{
		return "";
//		using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
//		{
//			using (AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity"))
//			{
//				return cls_AppsFlyer.Call<string>("getAppsFlyerUID", new object[1]
//				{
//					androidJavaObject
//				});
//			}
//		}
	}

	public static void setGCMProjectNumber(string googleGCMNumber)
	{
//		cls_AppsFlyer.Call("setGCMProjectNumber", googleGCMNumber);
	}

	public static void updateServerUninstallToken(string token)
	{
//		AndroidJavaObject androidJavaObject = new AndroidJavaClass("com.appsflyer.AppsFlyerLib").CallStatic<AndroidJavaObject>("getInstance", Array.Empty<object>());
//		using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
//		{
//			using (AndroidJavaObject androidJavaObject2 = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity"))
//			{
//				androidJavaObject.Call("updateServerUninstallToken", androidJavaObject2, token);
//			}
//		}
	}

	public static void enableUninstallTracking(string senderId)
	{
		new AndroidJavaClass("com.appsflyer.AppsFlyerLib").CallStatic<AndroidJavaObject>("getInstance", Array.Empty<object>()).Call("enableUninstallTracking", senderId);
	}

	public static void setDeviceTrackingDisabled(bool state)
	{
//		cls_AppsFlyer.Call("setDeviceTrackingDisabled", state);
	}

	public static void setAdditionalData(Dictionary<string, string> extraData)
	{
//		using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
//		{
//			using (androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity"))
//			{
//				AndroidJavaObject androidJavaObject = ConvertHashMap(extraData);
//				cls_AppsFlyer.Call("setAdditionalData", androidJavaObject);
//			}
//		}
	}

	public static void stopTracking(bool isStopTracking)
	{
//		using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
//		{
//			using (AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity"))
//			{
//				AndroidJavaObject androidJavaObject2 = androidJavaObject.Call<AndroidJavaObject>("getApplication", Array.Empty<object>());
//				cls_AppsFlyer.Call("stopTracking", isStopTracking, androidJavaObject2);
//			}
//		}
	}

	public static void setAppInviteOneLinkID(string oneLinkID)
	{
//		cls_AppsFlyer.Call("setAppInviteOneLink", oneLinkID);
	}

	public static void generateUserInviteLink(Dictionary<string, string> parameters, string callbackObject, string callbackMethod, string callbackFailedMethod)
	{
//		using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
//		{
//			using (AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity"))
//			{
//				AndroidJavaObject androidJavaObject2 = androidJavaObject.Call<AndroidJavaObject>("getApplication", Array.Empty<object>());
//				AndroidJavaObject androidJavaObject3 = ConvertHashMap(parameters);
//				ShareHelperInstance.Call("createOneLinkInviteListener", androidJavaObject2, androidJavaObject3, callbackObject, callbackMethod, callbackFailedMethod);
//			}
//		}
	}

	public static void trackCrossPromoteImpression(string appId, string campaign)
	{
//		using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
//		{
//			using (AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity"))
//			{
//				AndroidJavaObject androidJavaObject2 = androidJavaObject.Call<AndroidJavaObject>("getApplication", Array.Empty<object>());
//				cls_AndroidShare.CallStatic("trackCrossPromoteImpression", androidJavaObject2, appId, campaign);
//			}
//		}
	}

	public static void trackAndOpenStore(string promotedAppId, string campaign, Dictionary<string, string> customParams)
	{
//		using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
//		{
//			using (AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity"))
//			{
//				AndroidJavaObject androidJavaObject2 = androidJavaObject.Call<AndroidJavaObject>("getApplication", Array.Empty<object>());
//				AndroidJavaObject androidJavaObject3 = null;
//				if (customParams != null)
//				{
//					androidJavaObject3 = ConvertHashMap(customParams);
//				}
//				ShareHelperInstance.Call("trackAndOpenStore", androidJavaObject2, promotedAppId, campaign, androidJavaObject3);
//			}
//		}
	}

	public static void setPreinstallAttribution(string mediaSource, string campaign, string siteId)
	{
//		cls_AppsFlyer.Call("setPreinstallAttribution", mediaSource, campaign, siteId);
	}

	public static void setMinTimeBetweenSessions(int seconds)
	{
//		cls_AppsFlyer.Call("setMinTimeBetweenSessions", seconds);
	}
}
