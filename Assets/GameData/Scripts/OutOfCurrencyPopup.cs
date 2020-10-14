using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OutOfCurrencyPopup : MenuBase<OutOfCurrencyPopup>
{
	[Header("Out of Coins Popup")]
	public Button okButton;

	public TextMeshProUGUI outOfCoinsHeader;

	public TextMeshProUGUI outOfCoinsText;

	public TextMeshProUGUI outOfGemsHeader;

	public TextMeshProUGUI outOfGemsText;

	private CurrencyType currency;

	private void OnEnable()
	{
	}

	private void Start()
	{
		okButton.onClick.AddListener(delegate
		{
			MenuController.HideMenu<OutOfCurrencyPopup>();
			MenuController.ShowMenu<ShopMenu>().ScrollToCurrency(currency);
		});
	}

	public void SetCurrency(CurrencyType currency)
	{
		this.currency = currency;
		outOfCoinsHeader.gameObject.SetActive(currency == CurrencyType.Coins);
		outOfCoinsText.gameObject.SetActive(currency == CurrencyType.Coins);
		outOfGemsHeader.gameObject.SetActive(currency == CurrencyType.Gems);
		outOfGemsText.gameObject.SetActive(currency == CurrencyType.Gems);
	}
}
