//using GameAnalyticsSDK;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class TankAnalytics : MonoBehaviour
{
	public enum AttributionEvent
	{
		FirstOpen,
		CoinsSpent,
		GemsSpent,
		TankUpgraded,
		ShopOpened,
		RatingReached,
		AdsWatched,
		PlatformConnected
	}

	public static void NewAttributionEvent(AttributionEvent e, int? intArg = default(int?), string strArg = null)
	{
		string text = EventToString(e, intArg, strArg);
		if (!string.IsNullOrEmpty(text))
		{
//			AppsFlyer.trackRichEvent(text, new Dictionary<string, string>());
		}
	}

	private static string EventToString(AttributionEvent e, int? intArg = default(int?), string strArg = null)
	{
		switch (e)
		{
		case AttributionEvent.AdsWatched:
			return $"{intArg}{strArg}AdsWatched";
		case AttributionEvent.CoinsSpent:
			return $"{intArg}CoinsSpent";
		case AttributionEvent.GemsSpent:
			return $"{intArg}GemsSpent";
		case AttributionEvent.FirstOpen:
			return "FirstOpen";
		case AttributionEvent.PlatformConnected:
			return $"{strArg}Connected";
		case AttributionEvent.RatingReached:
			return $"{intArg}RatingReached{strArg}";
		case AttributionEvent.ShopOpened:
			return "ShopOpened";
		case AttributionEvent.TankUpgraded:
			return $"TankUpgraded{intArg}Times";
		default:
			return "";
		}
	}

	public static void StageCleared(string level, int stage, int score, string tank)
	{
//		Analytics.CustomEvent("StageCleared", new Dictionary<string, object>
//		{
//			{
//				"Level",
//				level
//			},
//			{
//				"Stage",
//				stage.ToString()
//			},
//			{
//				"Score",
//				score
//			},
//			{
//				"Tank",
//				tank
//			}
//		});
//		GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, level, stage.ToString(), score);
	}

	public static void CrossPromoDownloadPressed(string appName)
	{
//		Analytics.CustomEvent("CrossPromoDownloadPressed", new Dictionary<string, object>
//		{
//			{
//				"Name",
//				appName
//			}
//		});
//		GameAnalytics.NewDesignEvent("MenuAction:CrossPromoDownload:" + appName);
	}

	public static void CrossPromoTrailerPressed(string appName)
	{
//		Analytics.CustomEvent("CrossPromoTrailerPressed", new Dictionary<string, object>
//		{
//			{
//				"Name",
//				appName
//			}
//		});
//		GameAnalytics.NewDesignEvent("MenuAction:CrossPromoTrailer:" + appName);
	}

	public static void ReviveDeclined()
	{
//		Analytics.CustomEvent("ReviveDeclined");
//		GameAnalytics.NewDesignEvent("MenuAction:CustomAction:ReviveDeclined");
	}

	public static void DailyReward(int day, int amount)
	{
//		Analytics.CustomEvent("DailyReward", new Dictionary<string, object>
//		{
//			{
//				"Day",
//				day
//			},
//			{
//				"Gems",
//				amount
//			}
//		});
//		GameAnalytics.NewResourceEvent(GAResourceFlowType.Source, "gems", amount, "dailyReward", "day" + day);
	}

	public static void BoosterUsed(string booster)
	{
//		Analytics.CustomEvent("BoosterUsed", new Dictionary<string, object>
//		{
//			{
//				"Booster",
//				booster
//			}
//		});
//		GameAnalytics.NewDesignEvent("Game:BoosterUsed:" + booster);
	}

	public static void BoosterCardsRewarded(string booster, int count)
	{
//		Analytics.CustomEvent("BoosterCardsRewarded", new Dictionary<string, object>
//		{
//			{
//				"Booster",
//				booster
//			},
//			{
//				"Count",
//				count
//			}
//		});
//		GameAnalytics.NewResourceEvent(GAResourceFlowType.Source, "cards", count, "booster", booster);
	}

	public static void BoosterUpgraded(string booster, int cardsUsed)
	{
		//GameAnalytics.NewResourceEvent(GAResourceFlowType.Sink, "cards", cardsUsed, "booster", booster);
	}

	public static void TankCardsRewarded(string tankId, int count)
	{
//		Analytics.CustomEvent("TankCardsRewarded", new Dictionary<string, object>
//		{
//			{
//				"Tank",
//				tankId
//			},
//			{
//				"Count",
//				count
//			}
//		});
//		GameAnalytics.NewResourceEvent(GAResourceFlowType.Source, "cards", count, "tank", tankId);
	}

	public static void BoughtWithSoftCurrency(string productType, string product, int price)
	{
//		Analytics.CustomEvent("BoughtWithSoftCurrency", new Dictionary<string, object>
//		{
//			{
//				"ProductType",
//				productType
//			},
//			{
//				"Product",
//				product
//			},
//			{
//				"Price",
//				price
//			},
//			{
//				"RemainingCoins",
//				PlayerDataManager.GetCoins()
//			}
//		});
//		GameAnalytics.NewResourceEvent(GAResourceFlowType.Sink, "coins", price, productType, product);
	}

	public static void BoughtWithPremiumCurrency(string productType, string product, int price)
	{
//		Analytics.CustomEvent("BoughtWithPremiumCurrency", new Dictionary<string, object>
//		{
//			{
//				"ProductType",
//				productType
//			},
//			{
//				"Product",
//				product
//			},
//			{
//				"Price",
//				price
//			},
//			{
//				"RemainingGems",
//				PlayerDataManager.GetGems()
//			}
//		});
//		GameAnalytics.NewResourceEvent(GAResourceFlowType.Sink, "gems", price, productType, product);
	}

	public static void BoughtWithVideoAd(string product)
	{
//		Analytics.CustomEvent("BoughtWithVideoAd", new Dictionary<string, object>
//		{
//			{
//				"Product",
//				product
//			},
//			{
//				"RemainingCoins",
//				PlayerDataManager.GetCoins()
//			}
//		});
//		GameAnalytics.NewDesignEvent("Ad:Bought:" + product);
	}

	public static void CrossPromoShown()
	{
//		Analytics.CustomEvent("CrossPromoShown");
//		GameAnalytics.NewDesignEvent("Ad:CrossPromo:Shown");
	}

	public static void CrossPromoClicked()
	{
//		Analytics.CustomEvent("CrossPromoClicked");
//		GameAnalytics.NewDesignEvent("Ad:CrossPromo:Clicked");
	}

	public static void InterstitialClicked()
	{
//		Analytics.CustomEvent("InterstitialClicked");
//		GameAnalytics.NewDesignEvent("Ad:Interstitial:Clicked");
	}

	public static void InterstitialShown()
	{
//		Analytics.CustomEvent("InterstitialShown");
//		GameAnalytics.NewDesignEvent("Ad:Interstitial:Shown");
	}

	public static void BoughtCoins(string product, int coins)
	{
//		Analytics.CustomEvent("BoughtCoins", new Dictionary<string, object>
//		{
//			{
//				"Product",
//				product
//			},
//			{
//				"Coins",
//				coins
//			}
//		});
//		GameAnalytics.NewResourceEvent(GAResourceFlowType.Source, "coins", coins, "iap", product);
	}

	public static void BoughtGems(string product, int amount)
	{
//		Analytics.CustomEvent("BoughtGems", new Dictionary<string, object>
//		{
//			{
//				"Product",
//				product
//			},
//			{
//				"Gems",
//				amount
//			}
//		});
//		GameAnalytics.NewResourceEvent(GAResourceFlowType.Source, "gems", amount, "iap", product);
	}

	public static void BoughtNoAdsOffer(bool firstPurchase)
	{
//		Analytics.CustomEvent("BoughtNoAds", new Dictionary<string, object>
//		{
//			{
//				"FirstPurchase",
//				firstPurchase
//			}
//		});
//		GameAnalytics.NewDesignEvent("Purchasing:NoAdsBought:FirstPurchase:" + firstPurchase.ToString());
		BoughtGems("noAds", 20);
	}

	public static void BoughtBundle(string iapId, string tank, int gems, int coins)
	{
//		Analytics.CustomEvent("BoughtBundle", new Dictionary<string, object>
//		{
//			{
//				"Product",
//				iapId
//			},
//			{
//				"Tank",
//				tank
//			},
//			{
//				"Gems",
//				gems
//			},
//			{
//				"Coins",
//				coins
//			}
//		});
//		GameAnalytics.NewDesignEvent("Purchasing:Bundle:" + iapId + ":Bought");
		BoughtGems(iapId, gems);
		BoughtCoins(iapId, coins);
	}

	public static void DeclinedBundle(string iapId, string tank)
	{
//		Analytics.CustomEvent("DeclinedBundle", new Dictionary<string, object>
//		{
//			{
//				"Product",
//				iapId
//			},
//			{
//				"Tank",
//				tank
//			}
//		});
//		GameAnalytics.NewDesignEvent("Purchasing:Bundle:" + iapId + ":Declined");
	}

	public static void BoughtChest(Rarity rarity)
	{
//		Analytics.CustomEvent("BoughtChest", new Dictionary<string, object>
//		{
//			{
//				"Chest",
//				rarity
//			}
//		});
//		GameAnalytics.NewDesignEvent("Shop:Chest:" + rarity.ToString());
	}

	public static void BoughtDailyOffer(Tank tank, int amount, int price, int currentLevel, CurrencyType currency)
	{
//		Analytics.CustomEvent("BoughtDailyOffer", new Dictionary<string, object>
//		{
//			{
//				"Product",
//				"tankCards"
//			},
//			{
//				"Tank",
//				tank.name
//			},
//			{
//				"Amount",
//				amount
//			},
//			{
//				"Price",
//				price
//			},
//			{
//				"Currency",
//				currency.ToString()
//			},
//			{
//				"Level",
//				currentLevel
//			}
//		});
//		GameAnalytics.NewResourceEvent(GAResourceFlowType.Sink, currency.ToString().ToLower(), price, "tank", tank.id);
		TankCardsRewarded(tank.id, amount);
	}

	public static void BoughtStarterPack(int coins)
	{
//		Analytics.CustomEvent("BoughtStarterPack", new Dictionary<string, object>
//		{
//			{
//				"Coins",
//				coins
//			}
//		});
//		GameAnalytics.NewResourceEvent(GAResourceFlowType.Source, "coins", coins, "iap", "starterPack");
	}

	public static void BoughtDoubleSubscription(string from)
	{
//		Analytics.CustomEvent("BoughtSubscription", new Dictionary<string, object>
//		{
//			{
//				"Product",
//				"DoubleChest30"
//			},
//			{
//				"From",
//				from
//			}
//		});
//		GameAnalytics.NewDesignEvent("Purchasing:Subscription:DoubleChest30:" + from);
	}

	public static void InvalidPurchase()
	{
//		Analytics.CustomEvent("InvalidPurchase");
//		GameAnalytics.NewErrorEvent(GAErrorSeverity.Info, "User tried to make an invalid purchase.");
	}

	public static void LeveledUpTank(Tank tank, int level)
	{
//		Analytics.CustomEvent("LeveledUpTank", new Dictionary<string, object>
//		{
//			{
//				"Tank",
//				tank.name
//			},
//			{
//				"Level",
//				level
//			}
//		});
//		GameAnalytics.NewDesignEvent("LevelUp:" + tank.name, level);
	}

	public static void PlayerPromotion(Rank rank)
	{
//		Analytics.CustomEvent("Promotion", new Dictionary<string, object>
//		{
//			{
//				"Rank",
//				rank.name
//			}
//		});
//		GameAnalytics.NewDesignEvent("Player:Promotion:" + rank.name);
	}

	public static void PlayerGotKilled(string killerName, string tankUsed, string level, int stage, int score, int coins)
	{
//		Analytics.CustomEvent("PlayerGotKilled", new Dictionary<string, object>
//		{
//			{
//				"Killer",
//				killerName
//			},
//			{
//				"Score",
//				score
//			},
//			{
//				"Coins",
//				coins
//			},
//			{
//				"TankUsed",
//				tankUsed
//			},
//			{
//				"Level",
//				level
//			},
//			{
//				"Stage",
//				stage.ToString()
//			}
//		});
//		GameAnalytics.NewDesignEvent("Game:PlayerKiller:" + killerName);
//		GameAnalytics.NewDesignEvent("Game:PlayerKilledTankUsed:" + tankUsed);
//		GameAnalytics.NewResourceEvent(GAResourceFlowType.Source, "coins", coins, "game", "gameEnd");
//		GameAnalytics.NewProgressionEvent(GAProgressionStatus.Fail, level, stage.ToString(), score);
	}

	public static void PvpRewardedCoins(int coins)
	{
//		GameAnalytics.NewResourceEvent(GAResourceFlowType.Source, "coins", coins, "game", "1vs1");
	}

	public static void PvpMatchEnded(GameMode mode, int result)
	{
		string arg = (result < 0) ? "loss" : ((result > 0) ? "win" : "tie");
//		GameAnalytics.NewDesignEvent($"Game:Pvp{mode.ToString()}:{arg}");
	}

	public static void SetHighestStagePlayed(string level, int stage)
	{
//		GameAnalytics.SetCustomDimension01(level + " " + Mathf.Min(stage, 4));
	}

	public static void SetSelectedTank(string tank)
	{
//		GameAnalytics.SetCustomDimension02(tank);
	}

	public static void SetMidsummerHighestStage(int stage)
	{
//		GameAnalytics.SetCustomDimension03("Midsummer Siege " + Mathf.Min(stage, 19));
	}

	public static void Play(string level, string selectedTank)
	{
//		Analytics.CustomEvent("Play", new Dictionary<string, object>
//		{
//			{
//				"Level",
//				level
//			},
//			{
//				"SelectedTank",
//				selectedTank
//			}
//		});
//		GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, level, "0", 0);
		SetSelectedTank(selectedTank);
	}

	public static void ReportGameSessionLength(float length)
	{
//		Analytics.CustomEvent("Game Session Length", new Dictionary<string, object>
//		{
//			{
//				"Length",
//				length
//			}
//		});
//		GameAnalytics.NewDesignEvent("Game:Session:Length", length);
	}

	public static void AppStartReason(string reason)
	{
//		Analytics.CustomEvent("AppStartReason", new Dictionary<string, object>
//		{
//			{
//				"Reason",
//				reason
//			}
//		});
//		GameAnalytics.NewDesignEvent("App:StartReason:" + reason);
	}

	public static void ShareButtonClicked()
	{
//		Analytics.CustomEvent("ShareButtonClicked");
//		GameAnalytics.NewDesignEvent("MenuAction:ShareButton:Clicked");
	}
}
