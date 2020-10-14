using UnityEngine;
using UnityEngine.UI;

public class EasterEggPopup : MenuBase<EasterEggPopup>
{
	public Button okButton;

	private void Awake()
	{
		okButton.onClick.AddListener(delegate
		{
			MenuController.HideMenu<EasterEggPopup>();
		});
	}

	private void OnEnable()
	{
		UnityEngine.Object.Destroy(MenuController.GetMenu<GarageMenu>().EasterEgg);
	}

	private void OnDisable()
	{
		AnimatedCurrencyController.AnimateCoins(Variables.instance.easterEggCoins, MenuController.UICamera.WorldToViewportPoint(base.transform.position), MenuController.TotalCoinsPositionViewport, 5);
		PlayerDataManager.CollectEasterEgg();
		TankPrefs.Save();
	}
}
