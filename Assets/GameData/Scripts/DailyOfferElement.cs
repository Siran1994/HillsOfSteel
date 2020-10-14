using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyOfferElement : MonoBehaviour
{
	[HideInInspector]
	public int index;

	[Header("General")]
	public Image cardBackgroundImage;

	public TextMeshProUGUI headerText;

	[Header("Chest")]
	public GameObject chestContainer;

	public GameObject commonChest;

	public GameObject rareChest;

	public GameObject epicChest;

	[Header("Currency")]
	public GameObject currencyContainer;

	public Image currencyImage;

	[Header("Card")]
	public GameObject cardContainer;

	public CardElement cardElement;

	public GameObject premiumCardContainer;

	public Image premiumCardImage;

	[Header("Purchasing")]
	public Button mainButton;//每日奖励按钮

	public Button graphicButton;

	public GameObject priceContainer;

	public TextMeshProUGUI priceText;

	public GameObject premiumDiscountContainer;

	public TextMeshProUGUI originalPriceText;

	public TextMeshProUGUI newPriceText;

	public TextMeshProUGUI discountText;

	public GameObject purchaseWithGems;

	public GameObject purchaseWithCoins;

	public GameObject purchased;

	public void SetValues(int i, ShopMenu.ShopItem item)
	{
		index = i;
		chestContainer.SetActive(item.type == ShopMenu.ShopItemType.Chest);
		cardContainer.SetActive(item.type == ShopMenu.ShopItemType.TankCard || item.type == ShopMenu.ShopItemType.BoosterCard);
		currencyContainer.SetActive(item.type == ShopMenu.ShopItemType.Coin || item.type == ShopMenu.ShopItemType.Gem);
		premiumCardContainer.SetActive(item.type == ShopMenu.ShopItemType.PremiumCard);
		mainButton.interactable = !item.bought;
		graphicButton.image.sprite = (item.bought ? MenuBase<ShopMenu>.instance.buttonDisabledSprite : MenuBase<ShopMenu>.instance.buttonEnabledSprite);
		priceText.text = item.price.ToString();
		priceText.gameObject.SetActive(!item.bought);
		purchased.SetActive(item.bought);
		ShopMenu.ShopItemType type = item.type;
		if (type == ShopMenu.ShopItemType.TankCard || type == ShopMenu.ShopItemType.PremiumCard)
		{
			Tank tank = Manager<PlayerDataManager>.instance.variables.GetTank(item.id);
			headerText.text = ScriptLocalization.Get(tank.name);
			cardBackgroundImage.sprite = MenuController.GetMenu<ShopMenu>().GetCardBackground(item.rarity);
			premiumCardImage.sprite = tank.bigCard;
			cardElement.SetValues(tank, item.count, useNew: false);
			cardElement.background.sprite = MenuController.GetMenu<ShopMenu>().GetCardBackground(item.rarity, small: true);
			purchaseWithGems.SetActive(item.currency == CurrencyType.Gems);
			purchaseWithCoins.SetActive(item.currency == CurrencyType.Coins);
			if (item.discount > 0 && item.type == ShopMenu.ShopItemType.PremiumCard && !item.bought)
			{
				priceContainer.SetActive(value: false);
				premiumDiscountContainer.SetActive(value: true);
				originalPriceText.text = Variables.instance.GetTankGemValue(tank).ToString();
				newPriceText.text = item.price.ToString();
				discountText.text = $"-{item.discount.ToString()}%";
			}
			if (item.bought)
			{
				purchaseWithCoins.SetActive(value: false);
				purchaseWithGems.SetActive(value: false);
			}
			mainButton.onClick.RemoveAllListeners();
			mainButton.onClick.AddListener(delegate
			{
                SDKManager.Instance.ShowAd(ShowAdType.ChaPing, 1, "点击每日奖励");
                ShopMenu.TryPurchase(item.price, item.currency, delegate
				{
					PlayerDataManager.BuyDailyOffer(index);
					mainButton.interactable = false;
					graphicButton.enabled = true;
					graphicButton.interactable = false;
					priceText.gameObject.SetActive(value: false);
					purchased.SetActive(value: true);
					purchaseWithGems.SetActive(value: false);
					purchaseWithCoins.SetActive(value: false);
					premiumDiscountContainer.SetActive(value: false);
					cardElement.newTextContainer.SetActive(value: false);
					CurrencyType currency = item.currency;
					int price = item.price;
					if (currency == CurrencyType.Coins)
					{
						MenuController.instance.topTotalCoinsText.Tick(-price);
						cardElement.AnimateTankCardCountRoll(item);
					}
					else
					{
						MenuController.instance.topTotalGemsText.Tick(-price);
						cardElement.newTextContainer.SetActive(value: false);
					}
					AudioMap.PlayClipAt(AudioMap.instance[(item.currency == CurrencyType.Coins) ? "coinroll" : "gemCollect"], Vector3.zero, AudioMap.instance.uiMixerGroup);
					PlayerDataManager.AddTankCards(tank, item.count, updateDailies: false);
					TankPrefs.SaveAndSendToCloud(forced: true);
					TankAnalytics.BoughtDailyOffer(tank, item.count, price, PlayerDataManager.GetTankUpgradeLevel(tank), currency);
					if (currency == CurrencyType.Gems)
					{
						MenuController.ShowMenu<NewCardPopup>().Init(tank);
					}
				});
			});
		}
	}
}
