using I2.Loc;
using Newtonsoft.Json;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.Purchasing;
using UnityEngine.UI;

#pragma warning disable 0219
public class ShopMenu : MenuBase<ShopMenu>
{
	public enum Section
	{
		SpecialOffer,
		DailyOffers,
		Subscriptions,
		Chests,
		Gems,
		Coins
	}

	public enum ShopItemType
	{
		Chest,
		Coin,
		Gem,
		TankCard,
		BoosterCard,
		PremiumCard
	}

	[Serializable]
	public struct SpecialOfferElement
	{
		public TextMeshProUGUI text;

		public Image image;
	}

	[Serializable]
	public class ShopItem
	{
		public int index
		{
			get;
			set;
		}

		public ShopItemType type
		{
			get;
			set;
		}

		public Rarity rarity
		{
			get;
			set;
		}

		public CurrencyType currency
		{
			get;
			set;
		}

		public int price
		{
			get;
			set;
		}

		public int discount
		{
			get;
			set;
		}

		public string id
		{
			get;
			set;
		}

		public int count
		{
			get;
			set;
		}

		public bool bought
		{
			get;
			set;
		}

		public bool isNew
		{
			get;
			set;
		}

		public static ShopItem FromPayoutDefinition(PayoutDefinition definition)
		{
			ShopItem shopItem = new ShopItem();
			shopItem.count = (int)definition.quantity;
			if (definition.subtype.Contains("gems"))
			{
				shopItem.type = ShopItemType.Gem;
			}
			else if (definition.subtype.Contains("coins"))
			{
				shopItem.type = ShopItemType.Coin;
			}
			else if (definition.subtype.Contains("tank") && definition.subtype.Contains("Cards"))
			{
				shopItem.type = ShopItemType.TankCard;
				shopItem.id = definition.subtype.Replace("Cards", "");
			}
			return shopItem;
		}

		public ShopItemSerialized Serialize()
		{
			return new ShopItemSerialized
			{
				Index = index,
				Type = (int)type,
				Rarity = (int)rarity,
				Currency = (int)currency,
				Price = price,
				Discount = discount,
				Id = int.Parse(id.Replace("tank", "")),
				Count = count,
				Bought = (bought ? 1 : 0)
			};
		}
	}

	[Serializable]
	[JsonObject]
	public class ShopItemSerialized
	{
		[JsonProperty]
		public int Index
		{
			get;
			set;
		}

		[JsonProperty]
		public int Type
		{
			get;
			set;
		}

		[JsonProperty]
		public int Rarity
		{
			get;
			set;
		}

		[JsonProperty]
		public int Currency
		{
			get;
			set;
		}

		[JsonProperty]
		public int Price
		{
			get;
			set;
		}

		[JsonProperty]
		public int Discount
		{
			get;
			set;
		}

		[JsonProperty]
		public int Id
		{
			get;
			set;
		}

		[JsonProperty]
		public int Count
		{
			get;
			set;
		}

		[JsonProperty]
		public int Bought
		{
			get;
			set;
		}

		public ShopItem Deserialize()
		{
			return new ShopItem
			{
				index = Index,
				type = (ShopItemType)Type,
				rarity = (Rarity)Rarity,
				currency = (CurrencyType)Currency,
				price = Price,
				discount = Discount,
				id = "tank" + Id,
				count = Count,
				bought = (Bought == 1)
			};
		}
	}

	[Serializable]
	public class ChestItem
	{
		public Rarity rarity;

		public Image image;

		//public TextMeshProUGUI name;

		public TextMeshProUGUI price;

		public Button button;

		public Button graphicButton;

		public GameObject claimedButton;
	}

	[Serializable]
	public class CoinItem
	{
		public int gemPrice;

		public int coinAmount;

		public int dealValue;

		public TextMeshProUGUI dealValueText;

		public TextMeshProUGUI coinAmountText;

		public TextMeshProUGUI priceText;

		public Button button;

		public Button graphicButton;
	}

	[Header("Sprites")]
	public Sprite buttonEnabledSprite;

	public Sprite buttonDisabledSprite;

	public Sprite cardBackgroundWhite;

	public Sprite cardBackgroundBlue;

	public Sprite cardBackgroundOrange;

	public Sprite cardBackgroundPurple;

	public Sprite cardBackgroundBlueSmall;

	public Sprite cardBackgroundOrangeSmall;

	public Sprite cardBackgroundPurpleSmall;

	public Sprite commonCoinsSprite;

	public Sprite rareCoinsSprite;

	public Sprite epicCoinsSprite;

	public Sprite commonGemsSprite;

	public Sprite rareGemsSprite;

	public Sprite epicGemsSprite;

	[Header("Sections")]
	public RectTransform contentContainer;

	public RectTransform specialOfferSection;

	public RectTransform dailyOfferSection;

	public RectTransform subscriptionSection;//VIP面板

	public RectTransform chestsSection;

	public RectTransform gemsSection;

	public RectTransform coinsSection;

	[Header("Special Offers")]
	public GameObject specialOfferContainer;

	public SpecialOfferElement[] specialOfferItems;

	public Button specialOfferPurchaseButton;

	[Header("Daily Offers")]
	public RectTransform dailyOfferContainer;

	public GameObject offerPrefab;

	public DailyOfferElement[] dailyOffers;

	[Header("Chests")]
	public GameObject dailyChestAlert;

	[ArrayElementTitle("rarity")]
	public ChestItem[] chestItems;//点击装甲

	[Header("Gems")]
	public IAPItem[] gemIAPItems;

	[Header("Subscriptions")]
	public IAPItem[] subscriptionItems;

	[Header("Coins")]
	public CoinItem[] coinItems;

	[Space(10f)]
	public GameObject cardElementPrefab;

	private float contentHeight;

	private Coroutine scrollRoutine;

	private float adCheckTime = 1f;

	private float adCheckTimer;

	private void Awake()
	{
		MenuBase<ShopMenu>.instance = this;
		contentHeight = 0f - contentContainer.offsetMin.y;
	}

	private void OnEnable()
	{
        SDKManager.Instance.RepeatShowBan(2, 30);//展示Bannel
        if (PlayerDataManager.HasActiveDailyOffer())
		{
			InitOfferItems(PlayerDataManager.GetActiveDailyOffers());
		}
		else
		{
			GenerateDailyOffers();
		}
		if (dailyOffers.Length == 0)
		{
			HideSection(Section.DailyOffers);
		}
		if (PlayerDataManager.IsSubscribed())
		{
			HideSection(Section.Subscriptions);
		}

        if (PlayerPrefs.GetInt("Isbought")==1)//是否购买
        {
            HideSection(Section.Subscriptions);
        }

        UpdateChestItems();
		TankAnalytics.NewAttributionEvent(TankAnalytics.AttributionEvent.ShopOpened);
	}

	private void OnDisable()
	{
        SDKManager.Instance.CloseBanner();//关闭Banner
        MenuController.GetMenu<MainMenu>().UpdatePlayMenu();
		TankPrefs.CheckAndCreateLongtermBackup();
	}

	private void Start()
	{
		InitChestItems();
		InitCoinItems();
		InitIAPItems();
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			StopScrolling();
		}
		if (adCheckTimer >= adCheckTime)
		{
			UpdateChestItems();
			adCheckTimer = 0f;
		}
		adCheckTimer += Time.deltaTime;
	}

	public void InitOfferItems(ShopItem[] items)
	{
		if (dailyOffers.Length != 0)
		{
			ResetOfferItems();
		}
		dailyOffers = new DailyOfferElement[items.Length];
		for (int i = 0; i != items.Length; i++)
		{
			if (items[i].count > 0)
			{
				DailyOfferElement component = UnityEngine.Object.Instantiate(offerPrefab, dailyOfferContainer).GetComponent<DailyOfferElement>();
				component.SetValues(i, items[i]);
				dailyOffers[i] = component;
			}
		}
	}

	public void UpdateOfferItems()
	{
		if (!PlayerDataManager.HasActiveDailyOffer())
		{
			dailyOfferSection.gameObject.SetActive(value: false);
			return;
		}
		ShopItem[] activeDailyOffers = PlayerDataManager.GetActiveDailyOffers();
		for (int i = 0; i != activeDailyOffers.Length; i++)
		{
			if (activeDailyOffers[i].currency == CurrencyType.Gems && !activeDailyOffers[i].bought && !PlayerDataManager.IsTankLocked(Variables.instance.GetTank(activeDailyOffers[i].id)))
			{
				dailyOffers[i].gameObject.SetActive(value: false);
				continue;
			}
			dailyOffers[i].mainButton.interactable = !activeDailyOffers[i].bought;
			dailyOffers[i].graphicButton.image.sprite = (activeDailyOffers[i].bought ? buttonDisabledSprite : buttonEnabledSprite);
			dailyOffers[i].SetValues(i, activeDailyOffers[i]);
		}
	}

	public void ResetOfferItems()
	{
		for (int i = 0; i != dailyOffers.Length; i++)
		{
			UnityEngine.Object.Destroy(dailyOffers[i].gameObject);
		}
		dailyOffers = new DailyOfferElement[0];
	}

	public void GenerateDailyOffers()
	{
		ShopItem[] items = PlayerDataManager.GenerateDailyOffers();
		InitOfferItems(items);
		PlayerDataManager.SetDailyOfferTime();
		PlayerDataManager.SetDailyOffers(items);
	}

   

    public void GetFreeChestItem()
    {
        if (PlayerDataManager.IsDailyChestReady())
        {
            MenuController.ShowMenu<ChestPopup>().OpenChest(Rarity.Common, UpdateOfferItems);
            PlayerDataManager.DailyChestOpened();
            UpdateChestItems();
            PlayerDataManager.HandleDailyChestNotification();
        }
        UpdateChestItems();
    }
    #region
    bool isCanShow1 = false;
    int index1 = 1;
    int time1 = 0;
    public void CanShow1()
    {
        isCanShow1 = true;
    }
    void Timer1()
    {
        time1++;
        if (time1 == 60)
        {
            time1 = 0;
        }
        else
        {
            Invoke("Timer1", 1.0f);
        }
    }
    #endregion
    private void InitChestItems()
	{
		Variables variables = MenuController.instance.variables;
		for (int i = 0; i != chestItems.Length; i++)
		{
			ChestItem chest = chestItems[i];
			if (i == 0)
			{
				chest.button.onClick.AddListener(delegate
				{
                    GetFreeChestItem();
                    if (isCanShow1 || index1 == 1)
                    {
                        index1++;
                        isCanShow1 = false;
                        SDKManager.Instance.ShowAd(ShowAdType.Reward, 1, "免费点击装甲",(bool IsComplete)=> 
                        {
                            if (IsComplete)
                            {
                               // GetFreeChestItem();
                            }
                        });                        
                        Invoke("CanShow1", 60);
                        Invoke("Timer1", 1.0f);
                    }
                    else
                    {
                        Debug.Log("广告请求过于频繁,请在" + (60 - time1) + "秒后再试!");
                        if (60 - time1 == 0)
                        {
                            CancelInvoke("Timer1");
                        }
                        SDKManager.Instance.MakeToast("广告请求过于频繁, 请在" + (60 - time1) + "秒后再试!");
                    }

                });
				continue;
			}
			int price = variables.GetChest(chest.rarity).gemValue;
			chest.price.text = price.ToString();
			chest.button.onClick.AddListener(delegate
			{
                TryPurchase(price, CurrencyType.Gems, delegate
				{
					TankPrefs.CloudSyncComplete = true;
					MenuController.ShowMenu<ChestPopup>().OpenChest(chest.rarity, UpdateOfferItems);
					TankAnalytics.BoughtChest(chest.rarity);
					TankAnalytics.BoughtWithPremiumCurrency("chest", chest.rarity.ToString(), price);
				});
			});
		}
		UpdateChestItems();
	}

	public void UpdateChestItems()
	{
		dailyChestAlert.SetActive(PlayerDataManager.IsTimeForDailyChestAlert());
		bool flag = PlayerDataManager.IsDailyChestReady();
		bool flag2 = flag /*&& AdsManager.VideoAdAvailable(VideoAdPlacement.DailyChest)*/;
		chestItems[0].claimedButton.SetActive(!flag);
		chestItems[0].graphicButton.gameObject.SetActive(flag);
		chestItems[0].graphicButton.image.sprite = (flag2 ? MenuBase<ShopMenu>.instance.buttonEnabledSprite : MenuBase<ShopMenu>.instance.buttonDisabledSprite);
	}

	public void InitCoinItems()
	{
        Debug.Log("金币总量为:" + PlayerDataManager.GetCoins());
        for (int i = 0; i != coinItems.Length; i++)
		{
			CoinItem item = coinItems[i];
			item.coinAmountText.text = item.coinAmount.ToString();
			if (item.dealValue > 0)
			{
				item.dealValueText.GetComponent<LocalizationParamsManager>().SetParameterValue("AMOUNT", $"+{item.dealValue.ToString()}");
                item.dealValueText.text = "<color=#9806FD>" + item.dealValue.ToString() + "%" + "额外赠送!";
            }
			item.priceText.text = item.gemPrice.ToString();
            
			item.button.onClick.AddListener(delegate
			{
                TryPurchase(item.gemPrice, CurrencyType.Gems, delegate
				{
					AnimatedCurrencyController.AnimateCoins(item.coinAmount, MenuController.UICamera.WorldToViewportPoint(item.graphicButton.transform.position), MenuController.TotalCoinsPositionViewport, 1, null, delegate(int tc)
					{
                    });
                    MenuController.instance.topTotalGemsText.Tick(-item.gemPrice);//宝石扣除
					PlayerDataManager.AddCoins(item.coinAmount, sync: true, cloudSync: false);//金币添加
                    MenuController.UpdateTopMenu();
                    Debug.Log("金币总量为:" + PlayerDataManager.GetCoins());
                    Debug.Log("钻石总量为:" + PlayerDataManager.GetGems());
                });
              
            });
		}
	}
    
    public void GetFreeGems()
    {
        LeanTween.delayedCall(1.2f, (Action)delegate
        {
            ShopMenu shopMenu = this;
            AnimatedCurrencyController.AnimateGems((int)1, MenuController.UICamera.WorldToViewportPoint(shopMenu.gemIAPItems[1].transform.position), MenuController.TotalGemsPositionViewport, 1, null, delegate (int tc)
            {
            });
            AudioMap.PlayClipAt(AudioMap.instance["gemCollect"], Vector3.zero, AudioMap.instance.uiMixerGroup);
            MenuController.UpdateTopMenu();
            Debug.Log(PlayerDataManager.GetGems());
        });
    }
    #region
    bool isCanShow = false;
    int index = 1;
    int time = 0;
    public void CanShow()
    {
        isCanShow = true;
    }
    void Timer()
    {
        time++;
        if (time == 60)
        {
            time = 0;
        }
        else
        {
            Invoke("Timer", 1.0f);
        }
    }
    #endregion
    public void InitIAPItems()
	{
        Action<int> action = delegate(int index)
		{
            Debug.Log(index);
			gemIAPItems[index].SetOnComplete(delegate
            {
                if (isCanShow || index == 1)
                {
                    index++;
                    isCanShow = false;
                    SDKManager.Instance.ShowAd(ShowAdType.Reward, 1, "点击宝石", (bool IsComplete) =>
                    {
                        if (IsComplete)
                        {
                            GetFreeGems();
                        }
                    });
                    Invoke("CanShow", 60);
                    Invoke("Timer", 1.0f);
                }
                else
                {
                    Debug.Log("广告请求过于频繁,请在" + (60 - time) + "秒后再试!");
                    if (60 - time == 0)
                    {
                        CancelInvoke("Timer");
                    }
                    SDKManager.Instance.MakeToast("广告请求过于频繁, 请在" + (60 - time) + "秒后再试!");
                }
            });
		};
        for (int j = 0; j != gemIAPItems.Length; j++)
        {
            action(j);
        }
        for (int k = 0; k != subscriptionItems.Length; k++)
        {
            subscriptionItems[k].SetOnComplete(delegate
            {
                HideSection(Section.Subscriptions);
                ScrollToSection(Section.Chests);
                MenuController.ShowMenu<SubscriptionOfferPopup>().ShowSubscribed();
                TankAnalytics.BoughtDoubleSubscription("Shop");
            });
        }
    }
	public static void TryPurchase(int price, CurrencyType currency, Action onSuccess)
	{
        if (PlayerDataManager.GetCurrency(currency) >= price)
		{
			PlayerDataManager.TakeCurrency(currency, price);
			onSuccess?.Invoke();
		}
		else
		{
			MenuController.ShowMenu<OutOfCurrencyPopup>().SetCurrency(currency);
		}
	}

	public Sprite GetCardBackground(Rarity rarity, bool small = false)
	{
		if (small)
		{
			switch (rarity)
			{
			default:
				return cardBackgroundBlueSmall;
			case Rarity.Rare:
				return cardBackgroundOrangeSmall;
			case Rarity.Epic:
				return cardBackgroundPurpleSmall;
			}
		}
		switch (rarity)
		{
		default:
			return cardBackgroundWhite;
		case Rarity.Common:
			return cardBackgroundBlue;
		case Rarity.Rare:
			return cardBackgroundOrange;
		case Rarity.Epic:
			return cardBackgroundPurple;
		}
	}

	public void HideSection(Section section)
	{
		RectTransform rectTransform;
		switch (section)
		{
		case Section.DailyOffers:
			rectTransform = dailyOfferSection;
			break;
		case Section.Chests:
			rectTransform = chestsSection;
			break;
		case Section.Subscriptions:
			rectTransform = subscriptionSection;
			break;
		case Section.Gems:
			rectTransform = gemsSection;
			break;
		default:
			rectTransform = coinsSection;
			break;
		}
		if (rectTransform.gameObject.activeSelf)
		{
			contentContainer.offsetMin = new Vector2(contentContainer.offsetMin.x, 0f - contentHeight);
			contentContainer.offsetMax = new Vector2(contentContainer.offsetMax.x, 0f);
			Vector2 offsetMin = contentContainer.offsetMin;
			offsetMin.y += rectTransform.rect.height;
			contentContainer.offsetMin = offsetMin;
			contentHeight = 0f - contentContainer.offsetMin.y;
			rectTransform.gameObject.SetActive(value: false);
		}
	}

	public void ScrollToSection(Section section)
	{
		if (!base.gameObject.activeInHierarchy)
		{
			MenuController.ShowMenu<ShopMenu>();
		}
		StopScrolling();
		scrollRoutine = StartCoroutine(ScrollRoutine(section));
	}

	public void ScrollToCurrency(CurrencyType currency)
	{
		switch (currency)
		{
		case CurrencyType.Coins:
			ScrollToSection(Section.Coins);
			break;
		case CurrencyType.Gems:
			ScrollToSection(Section.Gems);
			break;
		}
	}

	private IEnumerator ScrollRoutine(Section section)
	{
		Vector2 min = contentContainer.offsetMin;
		Vector2 max = contentContainer.offsetMax;
		Vector2 targetMin = new Vector2(min.x, 0f);
		Vector2 targetMax = new Vector2(max.x, contentHeight);
		switch (section)
		{
		case Section.DailyOffers:
			targetMin.y = 0f - contentHeight;
			targetMax.y = 0f;
			break;
		case Section.Chests:
			targetMin.y -= chestsSection.rect.height;
			targetMax.y -= chestsSection.rect.height;
			goto case Section.Subscriptions;
		case Section.Subscriptions:
			if (subscriptionSection.gameObject.activeSelf)
			{
				targetMin.y -= subscriptionSection.rect.height;
				targetMax.y -= subscriptionSection.rect.height;
			}
			goto case Section.Gems;
		case Section.Gems:
			targetMin.y -= gemsSection.rect.height;
			targetMax.y -= gemsSection.rect.height;
			break;
		}
		float time = 0.4f;
		for (float t = 0f; t < time; t += Time.deltaTime)
		{
			float t2 = LeanTween.easeInOutCirc(0f, 1f, t / time);
			contentContainer.offsetMin = Vector2.Lerp(min, targetMin, t2);
			contentContainer.offsetMax = Vector2.Lerp(max, targetMax, t2);
			yield return null;
		}
		contentContainer.offsetMin = targetMin;
		contentContainer.offsetMax = targetMax;
		scrollRoutine = null;
	}

	private void StopScrolling()
	{
		if (scrollRoutine != null)
		{
			StopCoroutine(scrollRoutine);
		}
		scrollRoutine = null;
	}
}
