using I2.Loc;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChestPopup : MenuBase<ChestPopup>
{
	[Serializable]
	public class ChestDisplayOptions
	{
		public Rarity rarity;

		public Material backgroundMaterial;

		public GameObject enable;

		public ParticleSystem explosion;

		[Range(0f, 2f)]
		public float shakeTime = 0.5f;

		[Range(0.367f, 2f)]
		public float windupTime = 1f;

		[Range(0f, 5f)]
		public float postExplosionWaitTime = 1f;
	}

	private const float WindupSoundDuration = 0.367f;

	public Image globalBackground;

	public Material defaultBackgroundMaterial;

	public RectTransform contents;

	public Image chest;

	public Sprite commonChestSprite;

	public Sprite rareChestSprite;

	public Sprite epicChestSprite;

	[ArrayElementTitle("rarity")]
	public ChestDisplayOptions[] chestDisplayOptions;

	public Image glowImage;

	public GridLayoutGroup contentGridLayoutGroup;

	public GameObject cardPrefab;//³é½±µÄ°ü

	public Button doubleRewardsOfferButton;//Ë«±¶½±Àø

	public GameObject doubleRewardsActiveContainer;

	public RectTransform doubleRewardsTextAnchor;

	public RectTransform doubleRewardsTextContainer;

	private bool chestExploded;

	private bool rewardingDone;

	private bool isSubscribed;

	private bool offerPressed;

	private bool doubleRewardingDone;

	private ChestRewards currentChestRewards;

	private List<CardElement> elements;

	private List<ShopMenu.ShopItem> shopItems;

	private Stack<KeyValuePair<Collectible, CardElement>> newCards;

	private ChestDisplayOptions currentOptions;

	private Action onComplete;

	private Coroutine rewardRoutine;

	private Coroutine doubleRewardRoutine;
    public Button Exit;
	private void Start()
	{
        if (PlayerPrefs.GetInt("Isbought") == 1)
        {
            isSubscribed = true;
        }
        else
        {
            doubleRewardsOfferButton.onClick.AddListener(delegate
            {
                offerPressed = true;
                AnimatedCurrencyController.CancelAllAnimations();
                MenuController.ShowMenu<SubscriptionOfferPopup>().Init("ChestPopup");
                isSubscribed = true;
            });
        }
        Exit.onClick.AddListener(delegate { MenuController.HideMenu<ChestPopup>(); });

    }

	private void OnEnable()
	{
		elements = new List<CardElement>();
		shopItems = new List<ShopMenu.ShopItem>();
		newCards = new Stack<KeyValuePair<Collectible, CardElement>>();
		glowImage.color = new Color(1f, 1f, 1f, 0f);
		MenuController.backButtonOverrideAction = WaitForAnimation;
		isSubscribed = PlayerDataManager.IsSubscribed();
		doubleRewardsActiveContainer.SetActive(value: false);
		doubleRewardsOfferButton.gameObject.SetActive(value: false);
	}

	private void OnDisable()
	{
		FinishAnimation();
		for (int i = 0; i != elements.Count; i++)
		{
			UnityEngine.Object.Destroy(elements[i].gameObject);
		}
		globalBackground.material = defaultBackgroundMaterial;
		if (currentOptions.enable != null)
		{
			currentOptions.enable.SetActive(value: false);
		}
		AnimatedCurrencyController.CancelAllAnimations();
		if (onComplete != null)
		{
			onComplete();
		}
		MenuController.UpdateTopMenu();
		currentOptions = null;
		MenuController.backButtonOverrideAction = null;
		doubleRewardRoutine = null;
	}

	private void Update()
	{
        if (PlayerPrefs.GetInt("Isbought") == 1)
        {
            isSubscribed = true;
        }
        else
        {
            isSubscribed = false;
        }

        if (!Input.GetMouseButtonUp(0) || offerPressed || MenuController.GetMenu<SubscriptionOfferPopup>().isActiveAndEnabled || MenuController.GetMenu<NewCardPopup>().isActiveAndEnabled || !chestExploded)
		{
			return;
		}
		if (!rewardingDone)
		{
			FinishAnimation();
		}
		else if (isSubscribed && !doubleRewardingDone)
		{
			if (doubleRewardRoutine == null)
			{
				doubleRewardRoutine = StartCoroutine(DoubleRewardsRoutine());
			}
		}
		else if (newCards.Count > 0 && chestExploded)
		{
			ShowNewCard();
		}
		else if (rewardingDone && doubleRewardingDone)
		{
			MenuController.HideMenu<ChestPopup>();
		}
	}

	public void OnNewCardRewarded()
	{
		if (isSubscribed)
		{
			foreach (CardElement element in elements)
			{
                if (element.doubleCardContainer==null)
                {
                    return;
                }
                else
                {
                    element.doubleCardContainer.SetActive(value: true);
                }

                
			}
		}
		rewardingDone = true;
		if (currentOptions != null)
		{
			globalBackground.material = currentOptions.backgroundMaterial;
			if ((bool)currentOptions.enable)
			{
				currentOptions.enable.SetActive(value: true);
			}
		}
	}

	private void AddElement(CardElement element)
	{
		element.container.SetParent(contents);
		element.container.localScale = Vector3.one;
		elements.Add(element);
	}

	public void OpenChest(Rarity rarity, Action callback = null)
	{
		onComplete = callback;
		Init(rarity);
		rewardRoutine = MenuBase<ChestPopup>.instance.StartCoroutine(MenuBase<ChestPopup>.instance.ChestOpenAnimationRoutine(rarity));
	}

	private IEnumerator ChestOpenAnimationRoutine(Rarity chestRarity)
	{
		rewardingDone = false;
		chestExploded = false;
		currentChestRewards = Variables.instance.GenerateChestRewards(chestRarity);
		PlayerDataManager.AddChestRewards(currentChestRewards);
		contentGridLayoutGroup.enabled = true;
		for (int j = 0; j != currentChestRewards.cards.Count; j++)
		{
			CardElement component = UnityEngine.Object.Instantiate(cardPrefab).GetComponent<CardElement>();
			Card card = currentChestRewards.cards[j];
			ShopMenu.ShopItem item = new ShopMenu.ShopItem
			{
				rarity = card.rarity,
				count = card.count,
				id = card.id,
				type = ((card.type == CardType.TankCard) ? ShopMenu.ShopItemType.TankCard : ShopMenu.ShopItemType.BoosterCard),
				isNew = card.isNew
			};
			shopItems.Add(item);
			AddElement(component);
			if (card.type == CardType.TankCard)
			{
				Tank tank = Variables.instance.GetTank(card.id);
				if (card.isNew)
				{
					newCards.Push(new KeyValuePair<Collectible, CardElement>(tank, component));
				}
			}
		}
		if (currentChestRewards.gems > 0)
		{
			CardElement component2 = UnityEngine.Object.Instantiate(cardPrefab).GetComponent<CardElement>();
			ShopMenu.ShopItem item2 = new ShopMenu.ShopItem
			{
				rarity = Rarity.Epic,
				count = currentChestRewards.gems,
				type = ShopMenu.ShopItemType.Gem
			};
			shopItems.Add(item2);
			AddElement(component2);
		}
		CardElement component3 = UnityEngine.Object.Instantiate(cardPrefab).GetComponent<CardElement>();
		ShopMenu.ShopItem item3 = new ShopMenu.ShopItem
		{
			rarity = chestRarity,
			count = currentChestRewards.coins,
			type = ShopMenu.ShopItemType.Coin
		};
		shopItems.Add(item3);
		AddElement(component3);
		LayoutRebuilder.ForceRebuildLayoutImmediate(contentGridLayoutGroup.GetComponent<RectTransform>());
		contentGridLayoutGroup.enabled = false;
		foreach (CardElement element in elements)
		{
			element.gameObject.SetActive(value: false);
		}
		chest.gameObject.SetActive(value: true);
		Image image = chest;
		object sprite;
		switch (chestRarity)
		{
		default:
			sprite = commonChestSprite;
			break;
		case Rarity.Rare:
			sprite = rareChestSprite;
			break;
		case Rarity.Epic:
			sprite = epicChestSprite;
			break;
		}
		image.sprite = (Sprite)sprite;
		chest.SetNativeSize();
		yield return new WaitForSeconds(currentOptions.shakeTime);
		AudioMap.PlayClipAt(AudioMap.instance["chestShake"], Vector3.zero, AudioMap.instance.uiMixerGroup);
		LeanTween.delayedCall(currentOptions.windupTime - 0.367f, (Action)delegate
		{
			AudioMap.PlayClipAt(AudioMap.instance["chestWindup"], Vector3.zero, AudioMap.instance.uiMixerGroup);
		});
		for (float time = 0f; time <= currentOptions.windupTime; time += Time.deltaTime)
		{
			chest.transform.rotation = Quaternion.Euler(0f, 0f, 50f * (Mathf.PingPong(time / 0.2f, 0.3f) - 0.2f));
			glowImage.color = new Color(1f, 1f, 1f, LeanTween.easeInExpo(0f, 1f, time / currentOptions.windupTime));
			yield return null;
		}
		chest.gameObject.SetActive(value: false);
		chest.transform.rotation = Quaternion.identity;
		currentOptions.explosion.Play();
		AudioMap.PlayClipAt(AudioMap.instance["chestOpen"], Vector3.zero, AudioMap.instance.uiMixerGroup);
		if (chestRarity == Rarity.Epic)
		{
			AudioMap.PlayClipAt(AudioMap.instance["chestOpenChime"], Vector3.zero, AudioMap.instance.uiMixerGroup);
		}
		yield return new WaitForSeconds(currentOptions.postExplosionWaitTime);
		doubleRewardsOfferButton.gameObject.SetActive(!isSubscribed);
		doubleRewardingDone = !isSubscribed;
		chestExploded = true;
		MenuController.backButtonOverrideAction = FinishAnimation;
		int num;
		for (int i = 0; i < elements.Count; i = num)
		{
			if (!(elements[i] == null))
			{
				CardElement cardElement = elements[i];
				AudioMap.PlayClipAt(AudioMap.instance["cardReveal"], Vector3.zero, AudioMap.instance.uiMixerGroup);
				cardElement.gameObject.SetActive(value: true);
				cardElement.SetValues(shopItems[i], animateRoll: true, deductCount: true, isSubscribed);
				if (shopItems[i].type == ShopMenu.ShopItemType.TankCard && shopItems[i].isNew)
				{
					SetNewCard(i);
				}
				if (shopItems[i].type == ShopMenu.ShopItemType.Coin)
				{
					AnimatedCurrencyController.AnimateCoins(isSubscribed ? (shopItems[i].count / 2) : shopItems[i].count, MenuController.UICamera.WorldToViewportPoint(cardElement.transform.position), MenuController.TotalCoinsPositionViewport, 5, null, delegate(int ts)
					{
						MenuController.instance.topTotalCoinsText.Tick(ts);
					});
				}
				else if (shopItems[i].type == ShopMenu.ShopItemType.Gem)
				{
					AnimatedCurrencyController.AnimateGems(isSubscribed ? (shopItems[i].count / 2) : shopItems[i].count, MenuController.UICamera.WorldToViewportPoint(cardElement.transform.position), MenuController.TotalGemsPositionViewport, 1, null, delegate(int ts)
					{
						MenuController.instance.topTotalGemsText.Tick(ts);
					});
				}
				yield return new WaitForSeconds(0.25f);
			}
			num = i + 1;
		}
		if (isSubscribed)
		{
			doubleRewardRoutine = StartCoroutine(DoubleRewardsRoutine());
		}
		FinishAnimation();
	}

	private IEnumerator DoubleRewardsRoutine()
	{
		yield return new WaitForSeconds(0.25f);
		doubleRewardsActiveContainer.SetActive(value: true);
		Vector3 origPos = doubleRewardsTextContainer.position;
		doubleRewardsTextContainer.position = Vector3.zero;
		float time2 = 0.6f;
		Vector2 scale = Vector2.one * 2f;
		AudioMap.PlayClipAt("vipDoubleSplash", Vector3.zero, AudioMap.instance.uiMixerGroup);
		for (float t2 = 0f; t2 < time2; t2 += Time.deltaTime)
		{
			doubleRewardsTextContainer.localScale = Vector2.Lerp(Vector2.one, scale, LeanTween.easeOutCirc(0f, 1f, t2 / time2));
			yield return null;
		}
		yield return new WaitForSeconds(0.25f);
		Vector2 centerPos = doubleRewardsTextContainer.position;
		time2 = 0.4f;
		for (float t2 = 0f; t2 < time2; t2 += Time.deltaTime)
		{
			doubleRewardsTextContainer.position = Vector2.Lerp(centerPos, origPos, LeanTween.easeOutCirc(0f, 1f, t2 / time2));
			doubleRewardsTextContainer.localScale = Vector2.Lerp(scale, Vector2.one, LeanTween.easeOutCubic(0f, 1f, t2 / time2));
			yield return null;
		}
		doubleRewardsTextContainer.anchoredPosition = Vector2.zero;
		yield return new WaitForSeconds(0.25f);
		AudioMap.PlayClipAt("vipDoubleRoll", Vector3.zero, AudioMap.instance.uiMixerGroup);
		int num;
		for (int i = 0; i != elements.Count; i = num)
		{
			elements[i].SetDoubleValues(shopItems[i]);
			yield return null;
			num = i + 1;
		}
		StartCoroutine(DoubleRewardsEOFRoutine());
	}

	public void DoubleCurrentRewards()
	{
		isSubscribed = true;
		PlayerDataManager.AddChestRewards(currentChestRewards);
		MenuController.UpdateTopMenu();
		doubleRewardsActiveContainer.SetActive(value: true);
		doubleRewardsOfferButton.gameObject.SetActive(value: false);
		for (int i = 0; i != shopItems.Count; i++)
		{  
			shopItems[i].count *= 2; //Ë«±¶½±Àø
		}
		doubleRewardRoutine = StartCoroutine(DoubleRewardsRoutine());
		StartCoroutine(CurrencyCountEOFRoutine());
	}
	private IEnumerator DoubleRewardsEOFRoutine()
	{
		yield return Wait.ForEndOfFrame();
		for (int i = 0; i != elements.Count; i++)
		{
			if (shopItems[i].type == ShopMenu.ShopItemType.Coin)
			{
				AnimatedCurrencyController.AnimateCoins(shopItems[i].count / 2, MenuController.UICamera.WorldToViewportPoint(elements[i].transform.position), MenuController.TotalCoinsPositionViewport, 5, null, delegate(int ts)
				{
					MenuController.instance.topTotalCoinsText.Tick(ts);
				});
			}
			else if (shopItems[i].type == ShopMenu.ShopItemType.Gem)
			{
				AnimatedCurrencyController.AnimateGems(shopItems[i].count / 2, MenuController.UICamera.WorldToViewportPoint(elements[i].transform.position), MenuController.TotalGemsPositionViewport, 1, null, delegate(int ts)
				{
					MenuController.instance.topTotalGemsText.Tick(ts);
				});
			}
		}
		offerPressed = false;
		doubleRewardingDone = true;
		MenuController.backButtonOverrideAction = null;
	}

	public void DoubleRewardsDenied()
	{
		StartCoroutine(InputReleaseEOFRoutine());
	}

	private IEnumerator InputReleaseEOFRoutine()
	{
		yield return Wait.ForEndOfFrame();
		offerPressed = false;
	}

	private IEnumerator CurrencyCountEOFRoutine()
	{
		yield return Wait.ForEndOfFrame();
		MenuController.instance.topTotalCoinsText.Tick(-currentChestRewards.coins);
		MenuController.instance.topTotalGemsText.Tick(-currentChestRewards.gems);
	}

	private void SetNewCard(int i)
	{
		elements[i].teaserContainer.SetActive(value: true);
		elements[i].teaserContainer.GetComponent<Image>().sprite = MenuController.GetMenu<ShopMenu>().GetCardBackground(shopItems[i].rarity, small: true);
		elements[i].stackSizeText.text = ScriptLocalization.Get(shopItems[i].rarity + "Tank");
		elements[i].footerText.gameObject.SetActive(value: false);
	}

	private void ShowNewCard()
	{
		rewardingDone = false;
		KeyValuePair<Collectible, CardElement> keyValuePair = newCards.Pop();
		MenuController.ShowMenu<NewCardPopup>().Init(keyValuePair.Key);
		keyValuePair.Value.teaserContainer.SetActive(value: false);
		keyValuePair.Value.stackSizeText.text = "X" + PlayerDataManager.GetTankCardCount((Tank)keyValuePair.Key).ToString();
		keyValuePair.Value.footerText.gameObject.SetActive(value: true);
		if (newCards.Count == 0)
		{
			MenuController.backButtonOverrideAction = null;
		}
		if (isSubscribed)
		{
			foreach (CardElement element in elements)
			{
				element.doubleCardContainer.SetActive(value: false);
			}
		}
	}

	private void FinishAnimation()
	{
		if (rewardRoutine != null)
		{
			StopCoroutine(rewardRoutine);
			rewardRoutine = null;
		}
		if (elements.Count != shopItems.Count)
		{
			UnityEngine.Debug.LogError("Element count does not match ShopItem count!");
		}
		if (!rewardingDone)
		{
			for (int i = 0; i < elements.Count; i++)
			{
				if (!elements[i].gameObject.activeInHierarchy)
				{
					elements[i].gameObject.SetActive(value: true);
					elements[i].SetValues(shopItems[i], animateRoll: false, deductCount: false, isSubscribed);
					if (shopItems[i].type == ShopMenu.ShopItemType.TankCard && shopItems[i].isNew)
					{
						SetNewCard(i);
					}
				}
			}
		}
		rewardingDone = true;
		if (newCards.Count > 0)
		{
			MenuController.backButtonOverrideAction = ShowNewCard;
		}
		else if (isSubscribed && !doubleRewardingDone)
		{
			if (doubleRewardRoutine == null)
			{
				doubleRewardRoutine = StartCoroutine(DoubleRewardsRoutine());
			}
			MenuController.backButtonOverrideAction = WaitForAnimation;
		}
		else
		{
			MenuController.backButtonOverrideAction = null;
		}
		if (isSubscribed && doubleRewardRoutine == null)
		{
			doubleRewardRoutine = StartCoroutine(DoubleRewardsRoutine());
		}
	}

	private void Init(Rarity rarity)
	{
		ChestDisplayOptions[] array = this.chestDisplayOptions;
		foreach (ChestDisplayOptions chestDisplayOptions in array)
		{
			if (chestDisplayOptions.enable != null)
			{
				chestDisplayOptions.enable.SetActive(chestDisplayOptions.rarity.Equals(rarity));
			}
			chestDisplayOptions.explosion.gameObject.SetActive(chestDisplayOptions.rarity.Equals(rarity));
			if (chestDisplayOptions.rarity.Equals(rarity))
			{
				currentOptions = chestDisplayOptions;
			}
		}
		globalBackground.material = currentOptions.backgroundMaterial;
	}

	private void WaitForAnimation()
	{
	}
}
