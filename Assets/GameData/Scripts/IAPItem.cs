using I2.Loc;
using System;
//using NUnit.Framework.Constraints;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class IAPItem : MonoBehaviour
{
	[Serializable]
	public class Subscription
	{
		public SubscriptionType type;

		public Button mainButton;
	}

	public string iapId;

	public bool isSubscription;

	public int dealValue;

	public CanvasGroup canvasGroup;

	public TextMeshProUGUI dealValueText;//额外赠送

	public TextMeshProUGUI nameText;

	public TextMeshProUGUI payoutText;//人民币价格

	public TextMeshProUGUI descriptionText;

	public Button button;//点击88元按钮

	public TextMeshProUGUI priceText;//按钮上的人民币价格

	public Product product;

	public Subscription subscriptionInfo;

	private float initTimer;

	private bool initialized;

	private Action onComplete;

	public bool IsInitialized => initialized;

   
	private void OnPointerClick()
	{
       // SDKManager.Instance.ShowAd(ShowAdType.ChaPing, 1, "点击宝石");
        //		if (product != null)
        //		{
        //			IAPManager.BuyIAP(product, onComplete);
        //		}

       
        IAPManager.BuyIAP(iapId, onComplete);
    }
    public void SetOnComplete(Action onComplete)
	{
		this.onComplete = onComplete;
	}

	public void InitIap(string id)
	{
		iapId = id;
		product = IAPManager.GetIAPInfo(id);
		if (product == null)
		{
			SetInitInProgress();
			return;
		}
		button.interactable = true;
		if (product.metadata != null)
		{
			if (priceText != null)
			{
				priceText.text = product.metadata.localizedPriceString;
			}
			if (payoutText != null)
			{
				payoutText.text = product.definition.payout.quantity.ToString();
			}
			if (descriptionText != null)
			{
				descriptionText.text = product.metadata.localizedDescription;
			}
			Refresh();
			initialized = true;
		}
		else
		{
			SetInitInProgress();
		}
	}

	public void Refresh()
	{
		if (isSubscription)
		{
			priceText.GetComponent<LocalizationParamsManager>().SetParameterValue("PRICE", product.metadata.localizedPriceString);
		}
	}

	private void SetInitInProgress()
	{
		//Debug.Log("自定义内购");
		button.interactable = true;
		//if (priceText != null&& iapId== "hillsofsteel.sub.doublechest30")
		//{
		//	priceText.text = "3元";
		//}
		////if (payoutText != null)
		////{
		////	payoutText.text = "2元";
		////}
		////if (descriptionText != null)
		////{
		////	descriptionText.text = "2元";
		////}
        switch (iapId)
        {
            case "hillsofsteel.sub.doublechest30": //购买VIP价格
                priceText.text = "500";
                break;
            case "hillsofsteel.gems1": //宝石礼包1
                priceText.text = "免费获取";
                payoutText.text = "50";
                break;
            case "hillsofsteel.gems2": //宝石礼包2
                priceText.text = "免费获取";
                payoutText.text = "50";
                break;
            case "hillsofsteel.gems3": //宝石礼包3
                priceText.text = "免费获取";
                payoutText.text = "50";
                break;
            case "hillsofsteel.gems4": //宝石礼包4
                priceText.text = "免费获取";
                payoutText.text = "50";
                break;
            case "hillsofsteel.gems5": //宝石礼包5
                priceText.text = "免费获取";
                payoutText.text = "50";
                break;
            case "hillsofsteel.gems6": //宝石礼包6
                priceText.text = "免费获取";
                payoutText.text = "50";
                break;
            case "hillsofsteel.gems7": //宝石礼包7
                priceText.text = "免费获取";
                payoutText.text = "50";
                break;
            case "hillsofsteel.gems8": //宝石礼包8
                priceText.text = "免费获取";
                payoutText.text = "50";
                break;
        }
	}

	private void OnEnable()
	{
        InitIap(iapId);
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnPointerClick);
        //button.onClick.AddListener(delegate
        //{
        //    MenuController.ShowMenu<SubscriptionOfferPopup>().offerContent.SetActive(false);
        //    MenuController.ShowMenu<SubscriptionOfferPopup>().subscribedContent.SetActive(true);
        //});

        if (isSubscription)
        {
            subscriptionInfo.mainButton.onClick.RemoveAllListeners();
            subscriptionInfo.mainButton.onClick.AddListener(OnPointerClick);
        }
    }

	private void Update()
	{
//		button.interactable = (!IAPManager.IAPTransactionInProgress || !(IAPManager.InProgressIAPID == iapId));
		if (isSubscription)
		{
			subscriptionInfo.mainButton.interactable = button.interactable;
		}
		if (!initialized)
		{
			if (initTimer > 2f)
			{
				InitIap(iapId);
			}
			initTimer += Time.deltaTime;
		}
		if (canvasGroup != null)
		{
			canvasGroup.alpha = (button.interactable ? 1f : 0.5f);
		}
	}
}
