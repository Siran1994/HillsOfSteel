using I2.Loc;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class NoAdsPopup : MenuBase<NoAdsPopup>
{
	public string iapId;

	public TextMeshProUGUI gemAmountText;

	public IAPItem iapItem;

	private void OnEnable()
	{
		StartCoroutine(InitIAP());
	}

	private IEnumerator InitIAP()
	{
		while (!iapItem.IsInitialized)
		{
			yield return null;
		}
		gemAmountText.GetComponent<LocalizationParamsManager>().SetParameterValue("AMOUNT", iapItem.product.definition.payout.quantity.ToString());
		bool someIAPbought = IAPManager.GetSomeIAPBought();
		iapItem.SetOnComplete(delegate
		{
			TankAnalytics.BoughtNoAdsOffer(!someIAPbought);
			LeanTween.delayedCall(1.25f, (Action)delegate
			{
				AnimatedCurrencyController.AnimateGems((int)iapItem.product.definition.payout.quantity, MenuController.UICamera.WorldToViewportPoint(iapItem.transform.position), MenuController.TotalGemsPositionViewport, 1, null, delegate(int tc)
				{
					//MenuController.instance.topTotalGemsText.Tick(tc);
				});
				AudioMap.PlayClipAt(AudioMap.instance["gemCollect"], Vector3.zero, AudioMap.instance.uiMixerGroup);
				TankPrefs.SaveAndSendToCloud(forced: true);
				MenuController.HideMenu<NoAdsPopup>();
			});
		});
	}

	private void OnDisable()
	{
		PlayerDataManager.SetNoAdsPopupShown();
	}
}
