namespace I2.Loc
{
	public static class ScriptLocalization
	{
		public static string Accept => Get("Accept");

		public static string Archive_SDF_large => Get("Archive SDF_large");

		public static string Archive_SDF_medium => Get("Archive SDF_medium");

		public static string Archive_SDF_small => Get("Archive SDF_small");

		public static string Armor => Get("Armor");

		public static string ArmorUpgradeDescription => Get("ArmorUpgradeDescription");

		public static string CancelRatingMenuContent => Get("CancelRatingMenuContent");

		public static string CancelRatingMenuHeader => Get("CancelRatingMenuHeader");

		public static string CoinShopHeader => Get("CoinShopHeader");

		public static string CollectButtonText => Get("CollectButtonText");

		public static string CommercialBreak => Get("CommercialBreak");

		public static string DailyRewardHeader => Get("DailyRewardHeader");

		public static string Day => Get("Day");

		public static string Engine => Get("Engine");

		public static string EngineUpgrade => Get("EngineUpgrade");

		public static string EngineUpgradeDescription => Get("EngineUpgradeDescription");

		public static string Free => Get("Free");

		public static string GameOver => Get("GameOver");

		public static string Gun => Get("Gun");

		public static string GunUpgrade => Get("GunUpgrade");

		public static string Highscore => Get("Highscore");

		public static string InGameRatingMenuContent => Get("InGameRatingMenuContent");

		public static string InGameRatingMenuHeader => Get("InGameRatingMenuHeader");

		public static string Increase_the_damage_and_speed_of_your_gun_to_level => Get("Increase the damage and speed of your gun to level");

		public static string Later => Get("Later");

		public static string Max => Get("Max");

		public static string MaxAll => Get("MaxAll");

		public static string More => Get("More");

		public static string No => Get("No");

		public static string NoThanks => Get("NoThanks");

		public static string NotificationMenuHeader => Get("NotificationMenuHeader");

		public static string NotificationPopupContent => Get("NotificationPopupContent");

		public static string OneTimeOffer => Get("OneTimeOffer");

		public static string OpenCoinShopButtonText => Get("OpenCoinShopButtonText");

		public static string OutOfCoinsPopupDescription => Get("OutOfCoinsPopupDescription");

		public static string OutOfCoinsPopupHeader => Get("OutOfCoinsPopupHeader");

		public static string Paused => Get("Paused");

		public static string PaymentDeclined => Get("PaymentDeclined");

		public static string PleaseEnableSocialPlatform => Get("PleaseEnableSocialPlatform");

		public static string Promotion => Get("Promotion");

		public static string PurchaseFailedTransactionInProgress => Get("PurchaseFailedTransactionInProgress");

		public static string PurchaseFailedTryAgain => Get("PurchaseFailedTryAgain");

		public static string PurchaseWillRemoveAdsDescription => Get("PurchaseWillRemoveAdsDescription");

		public static string PurchasingUnavailable => Get("PurchasingUnavailable");

		public static string QuitPopupHeader => Get("QuitPopupHeader");

		public static string Rate => Get("Rate");

		public static string RateMenuContent => Get("RateMenuContent");

		public static string RateMenuHeader => Get("RateMenuHeader");

		public static string ReviveDescription => Get("ReviveDescription");

		public static string RevivePopupHeader => Get("RevivePopupHeader");

		public static string StageCleared => Get("StageCleared");

		public static string TankBundle => Get("TankBundle");

		public static string Yes => Get("Yes");

		public static string SocialPlatform => Get("SocialPlatform");

		public static string Stage => Get("Stage");

		public static string ArmorUpgrade => Get("ArmorUpgrade");

		public static string Cleared => Get("Cleared");

		public static string DiscountOff => Get("DiscountOff");

		public static string UnlockPreviousTanksFirst => Get("UnlockPreviousTanksFirst");

		public static string Today => Get("Today");

		public static string PurchaseRestorationFailed => Get("PurchaseRestorationFailed");

		public static string PurchaseRestorationSuccess => Get("PurchaseRestorationSuccess");

		public static string PurchaseInProgressPleaseWait => Get("PurchaseInProgressPleaseWait");

		public static string FreeUpgradeNotification => Get("FreeUpgradeNotification");

		public static string DailyRewardNotification => Get("DailyRewardNotification");

		public static string ShareMessage => Get("ShareMessage");

		public static string Get(string Term)
		{
			return LocalizationManager.GetTermTranslation(Term, LocalizationManager.IsRight2Left, 0, ignoreRTLnumbers: false);
		}

		public static string Get(string Term, bool FixForRTL)
		{
			return LocalizationManager.GetTermTranslation(Term, FixForRTL, 0, ignoreRTLnumbers: false);
		}

		public static string Get(string Term, bool FixForRTL, int maxLineLengthForRTL)
		{
			return LocalizationManager.GetTermTranslation(Term, FixForRTL, maxLineLengthForRTL, ignoreRTLnumbers: false);
		}

		public static string Get(string Term, bool FixForRTL, int maxLineLengthForRTL, bool ignoreNumbers)
		{
			return LocalizationManager.GetTermTranslation(Term, FixForRTL, maxLineLengthForRTL, ignoreNumbers);
		}
	}
}
