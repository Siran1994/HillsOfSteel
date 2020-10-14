//using GameAnalyticsSDK;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;

public class IAPManager : Manager<IAPManager>, IStoreListener
{
	public class Receipt
	{
		public string Store;

		public string TransactionID;

		public string Payload;

		public Receipt()
		{
			Store = (TransactionID = (Payload = ""));
		}

		public Receipt(string store, string transactionID, string payload)
		{
			Store = store;
			TransactionID = transactionID;
			Payload = payload;
		}
	}

	public class PayloadAndroid
	{
		public string json;

		public string signature;

		public PayloadAndroid()
		{
			json = (signature = "");
		}

		public PayloadAndroid(string _json, string _signature)
		{
			json = _json;
			signature = _signature;
		}
	}

	public Variables variables;

	private Action iapCallback;

	private IStoreController storeController;

	private IExtensionProvider extensionProvider;

	public static bool IAPTransactionInProgress
	{
		get;
		private set;
	}

	public static string InProgressIAPID
	{
		get;
		private set;
	}

	public static bool PurchasingInitialized
	{
		get;
		private set;
	}

	public static bool PurchasingInitializationFailed
	{
		get;
		private set;
	}

	public static bool RetryPurchasingInitialization
	{
		get;
		private set;
	}

	public static bool RestoringPurchases
	{
		get;
		private set;
	}

	public void Initialize()
	{
		Manager<IAPManager>.instance = this;
		RestoringPurchases = true;
		PurchasingInitialized = false;
		PurchasingInitializationFailed = false;
		RetryPurchasingInitialization = false;
		StandardPurchasingModule first = StandardPurchasingModule.Instance();
		ProductCatalog catalog = ProductCatalog.LoadDefaultCatalog();
		ConfigurationBuilder builder = ConfigurationBuilder.Instance(first);
//		IAPConfigurationHelper.PopulateConfigurationBuilder(ref builder, catalog);
		UnityPurchasing.Initialize(this, builder);
	}

	public void OnInitializeFailed(InitializationFailureReason error)
	{
		PurchasingInitialized = false;
		PurchasingInitializationFailed = true;
		RestoringPurchases = false;
		RetryPurchasingInitialization = (error != InitializationFailureReason.PurchasingUnavailable);
	}

	public void onInAppBillingSuccess()
	{
		ProcessPurchase(InProgressIAPID);
	}

	public void onInAppBillingFailure(string error)
	{
	}
   

    
    
    public void ProcessPurchase(string id)
	{
		Debug.Log("购买的物品ID是:"+id);
		Product iAPInfo = GetIAPInfo(id);
		bool flag = true;
        if (id.Contains("gems")) //宝石
        {
            // Debug.Log("Getgems 自定义数量");
            //PlayerDataManager.AddGems((int)50);
            //MenuController.UpdateTopMenu();

        }
        else if (id.Contains("tank") &&id.Contains("Cards"))
		{
			PlayerDataManager.AddTankCards(id.Replace("Cards", ""), (int)50);
		}
		else if (id.Contains("coins"))
		{
			Debug.Log("Getcoins 自定义数量");
			PlayerDataManager.AddCoins((int)50);
		}
//		if (id.Contains("sub"))
//		{
//			UnityEngine.Debug.Log("Product is a subscription");
////			
//		}
//		else
//		{
//			UnityEngine.Debug.Log("Product is NOT a subscription");
//		}
		if (flag)
		{
			SetSomeIAPBought();
		}
		TankPrefs.SaveAndSendToCloud( true);
		if (iapCallback != null)
		{
			iapCallback();
			iapCallback = null;
		}
		IAPTransactionInProgress = false;
	}

	private void SetSomeIAPBought()
	{
		TankPrefs.SetInt("someIAPBought", 1);
	}

	public static bool GetSomeIAPBought()
	{
		return TankPrefs.GetInt("someIAPBought") == 1;
	}

	public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
	{
		//CrossPlatformValidator crossPlatformValidator = new CrossPlatformValidator(GooglePlayTangle.Data(), AppleTangle.Data(), Application.identifier);
		string receipt = e.purchasedProduct.receipt;
		string isoCurrencyCode = e.purchasedProduct.metadata.isoCurrencyCode;
		int amount = decimal.ToInt32(e.purchasedProduct.metadata.localizedPrice * 100m);
		bool num = e.purchasedProduct.definition.id.Contains("coins");
		string itemType = num ? "coins" : "tank";
		string cartType = num ? "shop" : "garage";
		string storeSpecificId = e.purchasedProduct.definition.storeSpecificId;
		try
		{
			//crossPlatformValidator.Validate(receipt);
			ProcessPurchase(storeSpecificId);
		}
		catch (Exception)
		{
			TankAnalytics.InvalidPurchase();
		}
		PayloadAndroid payloadAndroid = JsonUtility.FromJson<PayloadAndroid>(JsonUtility.FromJson<Receipt>(receipt).Payload);
		//GameAnalytics.NewBusinessEventGooglePlay(isoCurrencyCode, amount, itemType, storeSpecificId, cartType, payloadAndroid.json, payloadAndroid.signature);
		AppsFlyer.validateReceipt("MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEArDKTBCIaRoT2n1QI1wS/JRep9iSS3AcqQ4Z0Udr5bCxcf0594aY3P39rpfnbRdJUCFjo150j9c1ZKuL0V8rnWOzwRNqdSvPtwiyQNztsQrMG5LkirW61VLCt8kIhpLd0osBeH585s4VV7FBe1m7u6+S98bM1TVm+Ows2/OoXXydyDpcaRaMsNAcM0bt4hWFcRTndwLalmyWHUYJ1tj9N/agtsX6nbBG2uF1Lc7iEH2UoY0VtaXGZdo5tCEHdx+58abxfcuVwHauH7ZzX8jlURDXPy1K0Fi9Ag6IKzrWFEmAjqAjYnmrD3tgHMJEIsKx/OoVI6+AaEfF4nRmejdt6KQIDAQAB", payloadAndroid.json, payloadAndroid.signature, e.purchasedProduct.metadata.localizedPrice.ToString(), e.purchasedProduct.metadata.isoCurrencyCode, null);
		return PurchaseProcessingResult.Complete;
	}

	public void OnPurchaseFailed(Product i, PurchaseFailureReason p)
	{
		IAPTransactionInProgress = false;
	}

	public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
	{
		storeController = controller;
		extensionProvider = extensions;
		PurchasingInitialized = true;
		RestoringPurchases = false;
	}

	public static void BuyIAP(Product product, Action onComplete)
	{
		if (!IAPTransactionInProgress)
		{
			if (onComplete != null)
			{
				Manager<IAPManager>.instance.iapCallback = onComplete;
			}
			InProgressIAPID = product.definition.id;
			IAPTransactionInProgress = true;
			Manager<IAPManager>.instance.storeController.InitiatePurchase(product);
		}
	}
	public static void BuyIAP(string id, Action onComplete)
	{
//		if (!IAPTransactionInProgress)
//		{
//			
//		}
		if (onComplete != null)
		{
			Manager<IAPManager>.instance.iapCallback = onComplete;
		}
		InProgressIAPID = id;
		IAPTransactionInProgress = true;
		Manager<IAPManager>.instance.ProcessPurchase(InProgressIAPID);
		
	}
	public static Product GetIAPInfo(string id)
	{
		if (Manager<IAPManager>.instance.storeController != null && Manager<IAPManager>.instance.storeController.products != null)
		{
			return Manager<IAPManager>.instance.storeController.products.WithID(id);
		}
		return null;
	}

	public static void RestorePurchases()
	{
		RestoringPurchases = true;
		Manager<IAPManager>.instance.extensionProvider.GetExtension<IAppleExtensions>().RestoreTransactions(delegate(bool result)
		{
			if (result)
			{
				MenuController.ShowMenu<PurchaseRestorePopup>().SetSucceeded();
			}
			else
			{
				MenuController.ShowMenu<PurchaseRestorePopup>().SetFailed();
			}
			RestoringPurchases = false;
		});
	}

	private static string SubscriptionTypeToId(SubscriptionType sub)
	{
		if (sub == SubscriptionType.DoubleChestRewards)
		{
			return "hillsofsteel.sub.doublechest30";
		}
		return string.Empty;
	}

	private static SubscriptionType SubscriptionIdToType(string id)
	{
		if (string.Equals(id, SubscriptionTypeToId(SubscriptionType.DoubleChestRewards), StringComparison.Ordinal))
		{
			UnityEngine.Debug.LogFormat("Type found! {0}", SubscriptionType.DoubleChestRewards.ToString());
			return SubscriptionType.DoubleChestRewards;
		}
		UnityEngine.Debug.LogFormat("Type NOT found!");
		return SubscriptionType.None;
	}

	public static SubscriptionInfo GetSubscriptionInfo(SubscriptionType type)
	{
		UnityEngine.Debug.Log("Getting subscription info...");
		if (!PurchasingInitialized)
		{
			UnityEngine.Debug.Log("Purchasing not initialized!");
			return null;
		}
		Product iAPInfo = GetIAPInfo(SubscriptionTypeToId(type));
		if (iAPInfo == null || !iAPInfo.definition.type.Equals(ProductType.Subscription))
		{
			UnityEngine.Debug.LogError("GetIAPInfo failed.");
			return null;
		}
		UnityEngine.Debug.LogFormat("Product receipt from IAP: {0}", iAPInfo.receipt);
		Dictionary<string, string> dictionary = null;
		dictionary = Manager<IAPManager>.instance.extensionProvider.GetExtension<IGooglePlayStoreExtensions>().GetProductJSONDictionary();
		string intro_json = (dictionary == null || !dictionary.ContainsKey(iAPInfo.definition.storeSpecificId)) ? null : dictionary[iAPInfo.definition.storeSpecificId];
		try
		{
			UnityEngine.Debug.Log("Starting SubscriptionManager...");
			SubscriptionManager subscriptionManager = new SubscriptionManager(iAPInfo, intro_json);
			UnityEngine.Debug.Log(" SubscriptionManager successfully initialized. Getting info...");
			SubscriptionInfo subscriptionInfo = subscriptionManager.getSubscriptionInfo();
			UnityEngine.Debug.Log("product id is: " + subscriptionInfo.getProductId());
			UnityEngine.Debug.Log("purchase date is: " + subscriptionInfo.getPurchaseDate().ToLocalTime());
			UnityEngine.Debug.Log("subscription next billing date is: " + subscriptionInfo.getExpireDate().ToLocalTime());
			UnityEngine.Debug.Log("is subscribed? " + subscriptionInfo.isSubscribed().ToString());
			UnityEngine.Debug.Log("is expired? " + subscriptionInfo.isExpired().ToString());
			UnityEngine.Debug.Log("is cancelled? " + subscriptionInfo.isCancelled());
			UnityEngine.Debug.Log("cancel date: " + subscriptionInfo.getCancelDate().ToLocalTime());
			UnityEngine.Debug.Log("product is in free trial period? " + subscriptionInfo.isFreeTrial());
			UnityEngine.Debug.Log("product is auto renewing? " + subscriptionInfo.isAutoRenewing());
			UnityEngine.Debug.Log("subscription remaining valid time until next billing date is: " + subscriptionInfo.getRemainingTime());
			UnityEngine.Debug.Log("is this product in introductory price period? " + subscriptionInfo.isIntroductoryPricePeriod());
			UnityEngine.Debug.Log("the product introductory localized price is: " + subscriptionInfo.getIntroductoryPrice());
			UnityEngine.Debug.Log("the product introductory price period is: " + subscriptionInfo.getIntroductoryPricePeriod());
			UnityEngine.Debug.Log("the number of product introductory price period cycles is: " + subscriptionInfo.getIntroductoryPricePeriodCycles());
			return subscriptionInfo;
		}
		catch (NullReceiptException)
		{
		}
		UnityEngine.Debug.Log("GetSubscriptionInfo() returned null");
		return null;
	}

	private bool IsSubscriptionAvailable(string receipt)
	{
		if (string.IsNullOrEmpty(receipt))
		{
			UnityEngine.Debug.Log("Subscription receipt is null");
			return false;
		}
		Dictionary<string, object> dictionary = (Dictionary<string, object>)MiniJson.JsonDecode(receipt);
		if (!dictionary.ContainsKey("Store") || !dictionary.ContainsKey("Payload"))
		{
			UnityEngine.Debug.Log("The product receipt does not contain enough information");
			return false;
		}
		string a = (string)dictionary["Store"];
		string text = (string)dictionary["Payload"];
		if (text != null)
		{
			if (!(a == "GooglePlay"))
			{
				if (a == "AppleAppStore" || a == "AmazonApps" || a == "MacAppStore")
				{
					return true;
				}
				return false;
			}
			Dictionary<string, object> dictionary2 = (Dictionary<string, object>)MiniJson.JsonDecode(text);
			if (!dictionary2.ContainsKey("json"))
			{
				UnityEngine.Debug.Log("The product receipt does not contain enough information, the 'json' field is missing");
				return false;
			}
			Dictionary<string, object> dictionary3 = (Dictionary<string, object>)MiniJson.JsonDecode((string)dictionary2["json"]);
			if (dictionary3 == null || !dictionary3.ContainsKey("developerPayload"))
			{
				UnityEngine.Debug.Log("The product receipt does not contain enough information, the 'developerPayload' field is missing");
				return false;
			}
			Dictionary<string, object> dictionary4 = (Dictionary<string, object>)MiniJson.JsonDecode((string)dictionary3["developerPayload"]);
			if (dictionary4 == null || !dictionary4.ContainsKey("is_free_trial") || !dictionary4.ContainsKey("has_introductory_price_trial"))
			{
				UnityEngine.Debug.Log("The product receipt does not contain enough information, the product is not purchased using 1.19 or later");
				return false;
			}
			return true;
		}
		return false;
	}

	public static bool IsActiveSubscription(SubscriptionType type)
	{
		SubscriptionInfo subscriptionInfo = GetSubscriptionInfo(type);
		if (subscriptionInfo == null)
		{
			UnityEngine.Debug.Log("Subscription is null [IsActiveSubscription()]");
			return false;
		}
		return subscriptionInfo.isExpired().Equals(Result.False);
	}
}
