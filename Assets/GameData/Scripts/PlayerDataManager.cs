using GooglePlayGames;
using I2.Loc;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class PlayerDataManager : Manager<PlayerDataManager>
{
	public Variables variables;

	public float mainMenuCloudSaveInterval = 15f;

	private int saveFrame;

	private bool saveFrameSet;

	public static bool IsInitialized
	{
		get;
		private set;
	}

	public static bool AppJustStarted
	{
		get;
		set;
	}

	public static int GamesThisSession
	{
		get;
		set;
	}

	public static bool RatingAskedThisSession
	{
		get;
		set;
	}

	public static DateTime FirstOnline
	{
		get;
		set;
	}

	public static DateTime ApplicationPausedAt
	{
		get;
		private set;
	}

	public static GameMode SelectedGameMode
	{
		get;
		set;
	}

	public static bool IsSelectedGameModePvP
	{
		get
		{
			if (SelectedGameMode != GameMode.Arena)
			{
				return SelectedGameMode == GameMode.Arena2v2;
			}
			return true;
		}
	}

	public static ArenaMatchData ArenaMatchData
	{
		get;
		set;
	}

	public static ArenaMultiMatchData ArenaMultiMatchData
	{
		get;
		set;
	}

	public static int ArenaMultiplayerAICount
	{
		get;
		set;
	}

	public static int SelectedArenaLevel
	{
		get;
		set;
	}

	public static bool BeenInAppBefore
	{
		get;
		set;
	}

	public static float MenuSaveTimer
	{
		get;
		private set;
	}

	public static bool SaveToCloudOnNextInterval
	{
		get;
		set;
	}

	public static bool AskRating()
	{
		if (!GetRatingAsked())
		{
			return TankPrefs.GetInt("playedDays") > 2;
		}
		return false;
	}

	public static void SetRatingAsked()
	{
		TankPrefs.SetInt("ratingAsked", 1);
		TankPrefs.Save();
	}

	public static bool GetRatingAsked()
	{
		return TankPrefs.GetInt("ratingAsked") == 1;
	}

	public static void SetSessionCount()
	{
		TankPrefs.SetInt("sessionCount", GetSessionCount() + 1);
	}

	public static int GetSessionCount()
	{
		return TankPrefs.GetInt("sessionCount");
	}

	public static int GetSelectedLevel(GameMode mode = GameMode.Classic)
	{
		if (mode == GameMode.Adventure)
		{
			return GetCurrentAdventureLevel();
		}
		return Mathf.Clamp(TankPrefs.GetInt("selectedLevel"), 0, Variables.instance.levels.Count - 1);
	}

	public static void SetSelectedLevel(int level, GameMode mode = GameMode.Classic)
	{
		if (mode == GameMode.Adventure)
		{
			SetCurrentAdventureLevel(level);
		}
		else
		{
			TankPrefs.SetInt("selectedLevel", Mathf.Clamp(level, 0, Variables.instance.levels.Count - 1));
		}
	}

	public static void SetCurrentStage(int stage)
	{
		TankPrefs.SetInt("currentStage", stage);
	}

	public static int GetCurrentStage()
	{
		return TankPrefs.GetInt("currentStage");
	}

	public static int GetCurrentAdventureLevel()
	{
		return Mathf.Clamp(TankPrefs.GetInt("currentLevel"), 0, Variables.instance.levels.Count - 1);
	}

	public static void SetCurrentAdventureLevel(int level)
	{
		TankPrefs.SetInt("currentLevel", Mathf.Clamp(level, 0, Variables.instance.levels.Count - 1));
	}

	public static bool LevelLocked(int level)
	{
		return TankPrefs.GetInt("level" + level + "Unlocked") == 0;
	}

	public static void UnlockLevel(int level)
	{
		TankPrefs.SetInt("level" + level + "Unlocked", 1);
		TankPrefs.Save();
	}

	public static int GetLeaderboardScore(LeaderboardID lid)
	{
		return TankPrefs.GetInt(TankLeaderboard.GetLeaderboard(lid));
	}

	public static void SetLeaderboardScore(LeaderboardID lid, int highscore)
	{
		string leaderboard = TankLeaderboard.GetLeaderboard(lid);
		int leaderboardScore = GetLeaderboardScore(lid);
		if (highscore > leaderboardScore)
		{
			TankPrefs.SetInt(leaderboard, highscore);
		}
		foreach (ScoreAchievement scoreAchievement in Variables.instance.GetScoreAchievements(Mathf.Max(highscore, leaderboardScore)))
		{
			PlatformManager.ReportAchievement(scoreAchievement.achievement);
		}
		PlatformManager.ReportHighscore(Mathf.Max(highscore, leaderboardScore), leaderboard);
	}

	public static int GetSelectedSkin(Tank tank)
	{
		return TankPrefs.GetInt(tank.id + "_selectedSkin");
	}

	public static void SetSelectedSkin(Tank tank, int skin)
	{
		TankPrefs.SetInt(tank.id + "_selectedSkin", skin);
	}

	public static bool SkinLocked(Tank tank, int skin)
	{
		return TankPrefs.GetInt(tank.id + "_skin" + skin + "_unlocked") == 0;
	}

	public static void UnlockSkin(Tank tank, int skin, bool save = true)
	{
		TankPrefs.SetInt(tank.id + "_skin" + skin + "_unlocked", 1);
		if (save)
		{
			TankPrefs.Save();
		}
	}

	public static bool BuySkin(Tank tank, int skinIndex)
	{
		TankSkin tankSkin = tank.tankSkins[skinIndex];
		int price = tankSkin.price;
		if (TakeCoins(price))
		{
			TankAnalytics.BoughtWithSoftCurrency("skin", tankSkin.name, price);
			UnlockSkin(tank, skinIndex);
			return true;
		}
		return false;
	}

	public static int GetSelectedTank()
	{
		return TankPrefs.GetInt("selectedTank");
	}

	public static void SetSelectedTank(int index)
	{
		TankPrefs.SetInt("selectedTank", index);
	}

	public static int GetRating(GameMode mode = GameMode.Arena)
	{
		if (mode == GameMode.Arena2v2)
		{
			return TankPrefs.GetInt("rating2v2");
		}
		return TankPrefs.GetInt("rating");
	}

	public static void AddWinRating(GameMode mode = GameMode.Arena)
	{
		int rating = GetRating(mode);
		int winTrophies = Manager<PlayerDataManager>.instance.variables.GetRankStats(rating).winTrophies;
		TankPrefs.SetInt((mode == GameMode.Arena) ? "rating" : "rating2v2", Mathf.Min(Manager<PlayerDataManager>.instance.variables.GetMaxTrophies(), rating + winTrophies));
	}

	public static void AddLoseRating(GameMode mode = GameMode.Arena)
	{
		int rating = GetRating(mode);
		int loseTrophies = Manager<PlayerDataManager>.instance.variables.GetRankStats(rating).loseTrophies;
		TankPrefs.SetInt((mode == GameMode.Arena) ? "rating" : "rating2v2", Mathf.Max(0, rating - loseTrophies));
	}

	public static List<Booster> GetBoostersForOwnedTanks()
	{
		List<Booster> list = new List<Booster>();
		Tank[] tanks = Manager<PlayerDataManager>.instance.variables.tanks;
		foreach (Tank tank in tanks)
		{
			if (GetTankCardCount(tank) <= 0)
			{
				continue;
			}
			for (int j = 0; j != tank.boosters.Length; j++)
			{
				Booster booster = GetBooster(tank.boosters[j]);
				if (!booster.HasMaxCount)
				{
					list.Add(booster);
				}
			}
		}
		return list;
	}

	public static bool BuyBooster(string id)
	{
		int boosterLevel = GetBoosterLevel(id);
		if (boosterLevel + 1 < Manager<PlayerDataManager>.instance.variables.boosterLevelUpCosts.Length && GetBoosterCardCount(id) >= Manager<PlayerDataManager>.instance.variables.GetBoosterCardCountNextLevel(boosterLevel))
		{
			int boosterLevelUpPriceForNextLevel = Manager<PlayerDataManager>.instance.variables.GetBoosterLevelUpPriceForNextLevel(GetBoosterLevel(id));
			if (TakeCoins(boosterLevelUpPriceForNextLevel))
			{
				AddBoosterLevel(id);
				TankPrefs.Save();
				int cardsUsed = Manager<PlayerDataManager>.instance.variables.GetBoosterCardCountNextLevel(boosterLevel) - Manager<PlayerDataManager>.instance.variables.GetBoosterCardCountNextLevel(boosterLevel - 1);
				TankAnalytics.BoughtWithSoftCurrency("booster", id, boosterLevelUpPriceForNextLevel);
				TankAnalytics.BoosterUpgraded(id, cardsUsed);
				return true;
			}
		}
		return false;
	}

	public static bool CanLevelUpBooster(string id)
	{
		int boosterLevel = GetBoosterLevel(id);
		if (boosterLevel + 1 < Manager<PlayerDataManager>.instance.variables.boosterLevelUpCosts.Length && GetBoosterCardCount(id) >= Manager<PlayerDataManager>.instance.variables.GetBoosterCardCountNextLevel(boosterLevel))
		{
			return true;
		}
		return false;
	}

	public static bool CanLevelUpBooster(Booster b)
	{
		if (b.Level + 1 < Manager<PlayerDataManager>.instance.variables.boosterLevelUpCosts.Length && b.Count >= b.NextLevelCount)
		{
			return true;
		}
		return false;
	}

	public static Booster GetBooster(string id, int level, int count)
	{
		Booster booster = Manager<PlayerDataManager>.instance.variables.GetBooster(id);
		booster.Level = level;
		booster.Count = count;
		booster.NextLevelCount = Manager<PlayerDataManager>.instance.variables.GetBoosterCardCountNextLevel(booster.Level);
		booster.ThisLevelCount = Manager<PlayerDataManager>.instance.variables.GetBoosterCardCountNextLevel(booster.Level - 1);
		booster.MaxLevel = (booster.NextLevelCount == -1);
		booster.MaxCount = Manager<PlayerDataManager>.instance.variables.GetBoosterCardCountMax();
		return booster;
	}

	public static Booster GetBooster(string id)
	{
		return GetBooster(id, GetBoosterLevel(id), GetBoosterCardCount(id));
	}

	public static int GetBoosterLevel(string id)
	{
		return TankPrefs.GetInt(id + "Level");
	}

	public static void AddBoosterLevel(string id)
	{
		int boosterLevel = GetBoosterLevel(id);
		int b = Manager<PlayerDataManager>.instance.variables.boosterCardsPerLevelUp.Length - 1;
		TankPrefs.SetInt(id + "Level", Mathf.Min(boosterLevel + 1, b));
	}

	public static int GetBoosterCardCount(string id)
	{
		return TankPrefs.GetInt(id + "Count");
	}

	public static void AddBoosterCards(string id, int count)
	{
		int boosterCardCount = GetBoosterCardCount(id);
		if (boosterCardCount == 0)
		{
			SetBoosterSeen(id, val: false);
		}
		int b = Variables.instance.boosterCardsPerLevelUp[Variables.instance.boosterCardsPerLevelUp.Length - 1];
		TankPrefs.SetInt(id + "Count", Mathf.Min(boosterCardCount + count, b));
		TankAnalytics.BoosterCardsRewarded(id, count);
	}

	public static void SetBoosterSeen(string id, bool val)
	{
		TankPrefs.SetInt(id + "Seen", val ? 1 : 0);
	}

	public static bool GetBoosterSeen(string id)
	{
		return TankPrefs.GetInt(id + "Seen", 1) == 1;
	}

	public static Booster[] GetTankBoosters(Tank tank)
	{
		Booster[] array = new Booster[tank.boosters.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = GetBooster(tank.boosters[i]);
		}
		return array;
	}

	public static Booster GetSelectedBooster(Tank tank)
	{
		return GetBooster(GetSelectedBoosterId(tank));
	}

	public static string GetSelectedBoosterId(Tank tank)
	{
		return TankPrefs.GetString(tank.id + "selectedBooster");
	}

	public static bool SetSelectedBooster(Tank tank, string id)
	{
		Booster booster = GetBooster(id);
		Booster selectedBooster = GetSelectedBooster(tank);
		if (IsTankLocked(tank) || booster.Count == 0 || (selectedBooster.type != 0 && selectedBooster.id.Equals(id)))
		{
			return false;
		}
		TankPrefs.SetString(tank.id + "selectedBooster", id);
		return true;
	}

	public static int GetDailyBonusIndex()
	{
		if (NextDailyBonusBeginTime() <= DateTime.Now && NextDailyBonusEndTime() >= DateTime.Now)
		{
			return TankPrefs.GetInt("dailyBonusIndex");
		}
		if (NextDailyBonusEndTime() <= DateTime.Now)
		{
			return 0;
		}
		return -1;
	}

	public static void GetDailyBonusAndSetNext()
	{
		int dailyBonusIndex = GetDailyBonusIndex();
		if (dailyBonusIndex > -1)
		{
			int num = (dailyBonusIndex + 1) % 7;
//			NotificationManager.ResetNotificationBadge();
//			NotificationManager.RemoveNotificationsOfType("dailyReward");
//			NotificationManager.ScheduleNotification(ScriptLocalization.DailyRewardNotification.Replace("{[DAY]}", (num + 1).ToString()), "dailyReward", DateTime.Now + TimeSpan.FromDays(1.0));
			TankPrefs.SetString("nextDailyBonusBeginTime", (DateTime.Today + TimeSpan.FromDays(1.0)).ToString());
			TankPrefs.SetString("nextDailyBonusEndTime", (DateTime.Today + TimeSpan.FromDays(2.0)).ToString());
			TankPrefs.SetInt("dailyBonusIndex", num);
			AddGems(Manager<PlayerDataManager>.instance.variables.dailyBonusAmounts[dailyBonusIndex]);
			TankAnalytics.DailyReward(dailyBonusIndex + 1, Manager<PlayerDataManager>.instance.variables.dailyBonusAmounts[dailyBonusIndex]);
		}
	}

	public static bool IsTimeToShowBundle(out Product bundleProduct)
	{
		bundleProduct = null;
		if (!IAPManager.PurchasingInitialized || Application.internetReachability == NetworkReachability.NotReachable)
		{
			return false;
		}
		Variables.Bundle[] bundles = Variables.instance.bundles;
		int @int = TankPrefs.GetInt("bundleIndex");
		bool flag = @int <= TankPrefs.GetInt("bundleStopIndex");
		if (@int < bundles.Length && @int >= 0 && !flag)
		{
			int num = TankPrefs.GetInt("bundleCounter") - 1;
			TankPrefs.SetInt("bundleCounter", num);
			if (num <= bundles[@int].showAfterTimesTried)
			{
				TankPrefs.SetInt("bundleIndex", @int - 1);
				TankPrefs.Save();
				bundleProduct = IAPManager.GetIAPInfo(bundles[@int].iapId);
				if (IsTankLocked(Variables.instance.FindTankFromProduct(bundleProduct)))
				{
					return true;
				}
				if (@int - 1 > 0)
				{
					TankPrefs.SetInt("bundleCounter", bundles[@int - 1].showAfterTimesTried + 1);
					TankPrefs.SetInt("bundleIndex", @int - 1);
					TankPrefs.Save();
					return IsTimeToShowBundle(out bundleProduct);
				}
				return false;
			}
			TankPrefs.Save();
		}
		return false;
	}

	public static DateTime NextDailyBonusBeginTime()
	{
		return DateTime.Parse(TankPrefs.GetString("nextDailyBonusBeginTime", (DateTime.Now - TimeSpan.FromMinutes(1.0)).ToString()));
	}

	public static DateTime NextDailyBonusEndTime()
	{
		return DateTime.Parse(TankPrefs.GetString("nextDailyBonusEndTime", (DateTime.Now + TimeSpan.FromMinutes(1.0)).ToString()));
	}

	public static bool IsTimeForDailyBonus()
	{
		if (string.IsNullOrEmpty(TankPrefs.GetString("nextDailyBonusBeginTime")) || string.IsNullOrEmpty(TankPrefs.GetString("nextDailyBonusEndTime")))
		{
			return true;
		}
		return DateTime.Now > NextDailyBonusBeginTime();
	}

	public static bool TimeForEasterEgg()
	{
		return DateTime.Now > DateTime.Parse(TankPrefs.GetString("nextEasterEggTime", (DateTime.Today + TimeSpan.FromMinutes(1.0)).ToString()));
	}

	public static bool TimeForGarageCoinBonus()
	{
		return DateTime.Now > DateTime.Parse(TankPrefs.GetString("nextGarageCoinBonus", (DateTime.Now - TimeSpan.FromMinutes(1.0)).ToString()));
	}

	public static DateTime NextGarageCoinBonusTime()
	{
		return DateTime.Parse(TankPrefs.GetString("nextGarageCoinBonus", (DateTime.Now - TimeSpan.FromMinutes(1.0)).ToString()));
	}

	public static void SetNextGarageCoinBonus()
	{
		TankPrefs.SetString("nextGarageCoinBonus", (DateTime.Now + TimeSpan.FromHours(8.0)).ToString());
		NotificationManager.RemoveNotificationsOfType("coinBonus");
		NotificationManager.ScheduleNotification(ScriptLocalization.Get("CoinAdAvailableNotification"), "coinBonus", DateTime.Now + TimeSpan.FromHours(8.0));
	}

	public static void CollectEasterEgg()
	{
		TankPrefs.SetString("nextEasterEggTime", (DateTime.Today + TimeSpan.FromHours(24.0)).ToString());
		AddCoins(Manager<PlayerDataManager>.instance.variables.easterEggCoins);
	}

	public static void SetFreeUpgradeUsed()
	{
		DateTime when = DateTime.Now + TimeSpan.FromMinutes(60.0);
		NotificationManager.RemoveNotificationsOfType("freeUpgrade");
		NotificationManager.ResetNotificationBadge();
		NotificationManager.ScheduleNotification(ScriptLocalization.FreeUpgradeNotification, "freeUpgrade", when);
		TankPrefs.SetString("nextFreeUpgrade", when.ToString());
		TankPrefs.Save();
	}

	public static bool IsTankLockedMenuIndex(int index)
	{
		return IsTankLocked(Manager<PlayerDataManager>.instance.variables.GetTank(index));
	}

	public static bool IsTankLocked(Tank tank)
	{
		return GetTankCardCount(tank) <= 0;
	}

	public static bool IsTankBuyableWithCoins(Tank tank)
	{
		for (int i = 0; i < Manager<PlayerDataManager>.instance.variables.tankOrder.Length; i++)
		{
			Tank tank2 = Manager<PlayerDataManager>.instance.variables.GetTank(i);
			if (IsTankLocked(tank2))
			{
				if (tank2 == tank)
				{
					return true;
				}
				return false;
			}
		}
		return false;
	}

	public static void AddXP(int add)
	{
		int num = GetXP() + add;
		TankPrefs.SetInt("xp", num);
		TankPrefs.SaveAtEndOfFrame();
		foreach (Rank item in Manager<PlayerDataManager>.instance.variables.GetRanksUntil(num))
		{
			PlatformManager.ReportAchievement(item.achievement);
		}
	}

	public static int GetXP()
	{
		return TankPrefs.GetInt("xp");
	}

	public static int GetCoins()
	{
		return TankPrefs.GetInt("coins");
	}

	public static int AddCoins(int coins, bool sync = true, bool cloudSync = false)
	{
		int num = GetCoins() + coins;
		TankPrefs.SetInt("coins", num);
		if (cloudSync)
		{
			TankPrefs.SaveAndSendToCloud();
		}
		else if (sync)
		{
			TankPrefs.Save();
		}
		return num;
	}

	public static bool TakeCoins(int coins)
	{
		int coins2 = GetCoins();
		if (coins2 - coins >= 0)
		{
			coins2 -= coins;
			TankPrefs.SetInt("coins", coins2);
			int @int = TankPrefs.GetInt("coinsSpent");
			int num = @int + coins;
			int[] array = new int[3]
			{
				1000,
				5000,
				10000
			};
			foreach (int num2 in array)
			{
				if (@int < num2 && num >= num2)
				{
					TankAnalytics.NewAttributionEvent(TankAnalytics.AttributionEvent.CoinsSpent, num2);
				}
			}
			TankPrefs.SetInt("coinsSpent", num);
			TankPrefs.Save();
			return true;
		}
		return false;
	}

	public void Initialize()
	{
		if (Manager<PlayerDataManager>.instance == null)
		{
			Manager<PlayerDataManager>.instance = this;
			AppJustStarted = true;
			StartCoroutine(InitializationRoutine());
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public static void DeauthenticateGooglePlay()
	{
		PlayerPrefs.SetInt("explicitSignOut", 1);
//		PlayGamesPlatform.Instance.SignOut();
		TankPrefs.HasCloudBeenFetched = false;
		TankPrefs.CloudSyncComplete = false;
	}

	private IEnumerator InitializationRoutine()
	{
		while (!TankPrefs.IsInitialized)
		{
			yield return null;
		}
		BeenInAppBefore = (TankPrefs.GetInt("BeenInAppBefore") == 1);
		TankPrefs.SetInt("BeenInAppBefore", 1);
		if (!BeenInAppBefore)
		{
			TankAnalytics.NewAttributionEvent(TankAnalytics.AttributionEvent.FirstOpen);
			SetTankCardCount(Manager<PlayerDataManager>.instance.variables.GetTank(0), 1);
			SetTankUpgradeLevel(Manager<PlayerDataManager>.instance.variables.GetTank(0), 0);
			SetChestProgress(ChestProgressionType.Adventure, Variables.instance.GetChestPointsNeeded(ChestProgressionType.Adventure));
			TankPrefs.SetInt("saveFileConverted", 1);
			TankPrefs.SetInt("whatsNewSeen", 1);
			AudioManager.SetSoundOn();
			TankPrefs.Save();
		}
		string @string = TankPrefs.GetString("firstOnline");
		if (@string.Length == 0)
		{
			TankPrefs.SetString("firstOnline", DateTime.Today.ToString());
			FirstOnline = DateTime.Today;
		}
		else
		{
			FirstOnline = DateTime.Parse(@string);
		}
		TankPrefs.SetInt("tank0_unlocked", 1);
		UnlockSkin(Manager<PlayerDataManager>.instance.variables.GetTank(0), 0, save: false);
		UnlockSkin(Manager<PlayerDataManager>.instance.variables.GetTank(1), 0, save: false);
		UnlockSkin(Manager<PlayerDataManager>.instance.variables.GetTank(2), 0, save: false);
		UnlockSkin(Manager<PlayerDataManager>.instance.variables.GetTank(3), 0, save: false);
		UnlockSkin(Manager<PlayerDataManager>.instance.variables.GetTank(4), 0, save: false);
		UnlockSkin(Manager<PlayerDataManager>.instance.variables.GetTank(5), 0, save: false);
		UnlockSkin(Manager<PlayerDataManager>.instance.variables.GetTank(6), 0, save: false);
		UnlockSkin(Manager<PlayerDataManager>.instance.variables.GetTank(7), 0, save: false);
		UnlockSkin(Manager<PlayerDataManager>.instance.variables.GetTank(8), 0, save: false);
		UnlockLevel(0);
		LoadingScreen.LoadMenu();
		IsInitialized = true;
		while (LoadingScreen.IsLoading)
		{
			yield return null;
		}
		CheckAndSetPlayedDays();
		SetSessionCount();
		LoadingScreen.AddProgress(0.1f);
	}

	public static void SetCountryCode(string code)
	{
		TankPrefs.SetString("countryCode", code);
	}

	public static string GetCountryCode()
	{
		return TankPrefs.GetString("countryCode");
	}

	public static void FetchCountryCode()
	{
		if (string.IsNullOrEmpty(GetCountryCode()))
		{
			BackendManager.FetchCountryCode(delegate(FetchCountryCodeResponse response)
			{
				if (response.error == BackendError.Ok)
				{
					SetCountryCode(response.countryCode);
					TankPrefs.SaveAtEndOfFrame();
				}
			});
		}
	}

	private void CheckAndSetPlayedDays()
	{
		if (DateTime.Parse(TankPrefs.GetString("lastDayPlayed", (DateTime.Today - TimeSpan.FromTicks(864000000000L)).ToString())) != DateTime.Today)
		{
			TankPrefs.SetInt("playedDays", TankPrefs.GetInt("playedDays") + 1);
		}
		TankPrefs.SetString("lastDayPlayed", DateTime.Today.ToString());
	}

	private void Update()
	{
		if (!(MenuController.instance == null) && SaveToCloudOnNextInterval && !(MenuController.GetMenu<MainMenu>() == null) && (MenuController.GetMenu<MainMenu>().gameObject.activeInHierarchy || MenuController.GetMenu<GarageMenu>().gameObject.activeInHierarchy))
		{
			if (MenuSaveTimer >= mainMenuCloudSaveInterval)
			{
				MenuSaveTimer = 0f;
				TankPrefs.SaveAndSendToCloud();
				SaveToCloudOnNextInterval = false;
			}
			MenuSaveTimer += Time.deltaTime;
		}
	}

	private void LateUpdate()
	{
		if (TankPrefs.SavingEOF && !saveFrameSet)
		{
			saveFrame = Time.frameCount + 3;
			saveFrameSet = true;
		}
		if (Time.frameCount == saveFrame && saveFrame > 0)
		{
			TankPrefs.SavingEOF = false;
			TankPrefs.Save();
		}
	}

	private void OnApplicationPause(bool pause)
	{
		if (IsInitialized && !pause)
		{
			CheckAndSetPlayedDays();
		}
	}

	public static bool HasCurrency(CurrencyType currency, int amount)
	{
		if (currency.Equals(CurrencyType.Gems))
		{
			return GetGems() >= amount;
		}
		if (currency.Equals(CurrencyType.Coins))
		{
			return GetCoins() >= amount;
		}
		return false;
	}

	public static int GetCurrency(CurrencyType type)
	{
		if (type.Equals(CurrencyType.Gems))
		{
			return GetGems();
		}
		if (type.Equals(CurrencyType.Coins))
		{
			return GetCoins();
		}
		return 0;
	}

	public static void TakeCurrency(CurrencyType type, int amount)
	{
		if (type.Equals(CurrencyType.Gems))
		{
			TakeGems(amount);
		}
		else if (type.Equals(CurrencyType.Coins))
		{
			TakeCoins(amount);
		}
	}

	public static void OnTransaction(CurrencyType currency, int amount, Action onSuccess = null, Action onFailure = null)
	{
		if (!HasCurrency(currency, amount))
		{
			if (onFailure != null)
			{
				onFailure();
			}
			else
			{
				MenuController.ShowMenu<OutOfCurrencyPopup>().SetCurrency(currency);
			}
		}
		else
		{
			TakeCurrency(currency, amount);
			onSuccess?.Invoke();
		}
	}

	public static void OnTransaction(TransactionCost cost, Action onSuccess = null, Action onFailure = null)
	{
		OnTransaction(cost.currency, cost.amount, onSuccess, onFailure);
	}

	public static int GetGems()
	{
		return TankPrefs.GetInt("gems");
	}

	public static void AddGems(int amount, bool cloudSync = false)
	{
		TankPrefs.SetInt("gems", GetGems() + amount);
		if (cloudSync)
		{
			TankPrefs.SaveAndSendToCloud();
		}
	}

	public static bool TakeGems(int amount)
	{
		if (amount <= GetGems())
		{
			TankPrefs.SetInt("gems", Mathf.Max(0, GetGems() - amount));
			int @int = TankPrefs.GetInt("gemsSpent");
			int num = @int + amount;
			int[] array = new int[3]
			{
				50,
				250,
				1000
			};
			foreach (int num2 in array)
			{
				if (@int < num2 && num >= num2)
				{
					TankAnalytics.NewAttributionEvent(TankAnalytics.AttributionEvent.GemsSpent, num2);
				}
			}
			TankPrefs.SetInt("gemsSpent", num);
			TankPrefs.SaveAndSendToCloud();
			return true;
		}
		return false;
	}

	public static int GetCoinsNeededForLevelUp(Tank tank)
	{
		return Manager<PlayerDataManager>.instance.variables.GetTankLevelUpCoins(tank.rarity, GetTankUpgradeLevel(tank));
	}

	public static int GetCardsNeededForLevelUp(Tank tank)
	{
		return Manager<PlayerDataManager>.instance.variables.GetTankLevelUpCards(tank.rarity, GetTankUpgradeLevel(tank));
	}

	public static int GetCardsNeededForLevelUp(Tank tank, int level)
	{
		return Manager<PlayerDataManager>.instance.variables.GetTankLevelUpCards(tank.rarity, level);
	}

	public static int GetCardsNeededForLevelUpCumulative(Tank tank)
	{
		return Manager<PlayerDataManager>.instance.variables.GetTankLevelUpCardsCumulative(tank.rarity, GetTankUpgradeLevel(tank));
	}

	public static int GetTankUpgradeLevel(Tank tank)
	{
		return TankPrefs.GetInt(tank.id + "Level");
	}

	public static void SetTankUpgradeLevel(Tank tank, int level)
	{
		TankPrefs.SetInt(tank.id + "Level", level);
		TankPrefs.Save();
	}

	public static bool HasAllTankCards(Tank tank)
	{
		return GetTankCardCount(tank) >= Manager<PlayerDataManager>.instance.variables.GetMaxTankCardCount(tank.rarity);
	}

	public static int GetTankCardCount(Tank tank)
	{
		return TankPrefs.GetInt(tank.id + "Cards");
	}

	public static void SetTankCardCount(Tank tank, int count)
	{
		TankPrefs.SetInt(tank.id + "Cards", Mathf.Clamp(count, 0, Variables.instance.GetMaxTankCardCount(tank)));
	}

	public static void AddTankCards(string id, int count)
	{
		AddTankCards(Manager<PlayerDataManager>.instance.variables.GetTank(id), count);
	}

	public static void AddTankCards(Tank tank, int count, bool updateDailies = true)
	{
		int tankCardCount = GetTankCardCount(tank);
		if (tankCardCount == 0 && count > 0)
		{
			TankPrefs.SetInt(tank.id + "_unlocked", 1);
			UnlockSkin(tank, 0, save: false);
		}
		int num = tankCardCount + count;
		if (updateDailies && HasActiveDailyOffer() && (tankCardCount == 0 || num >= Variables.instance.GetMaxTankCardCount(tank)))
		{
			List<ShopMenu.ShopItem> list = GetActiveDailyOffers().ToList();
			for (int i = 0; i != list.Count; i++)
			{
				if (list[i].id.Equals(tank.id))
				{
					if (tankCardCount == 0)
					{
						list[i].bought = true;
					}
					else
					{
						list.RemoveAt(i);
					}
					break;
				}
			}
			SetDailyOffers(list.ToArray());
		}
		SetTankCardCount(tank, num);
	}

	public static bool BuyTankUpgrade(Tank tank)
	{
		if (!IsTankFullyUpgraded(tank))
		{
			int cardsNeededForLevelUpCumulative = GetCardsNeededForLevelUpCumulative(tank);
			int coinsNeededForLevelUp = GetCoinsNeededForLevelUp(tank);
			if (GetCoins() >= coinsNeededForLevelUp && GetTankCardCount(tank) >= cardsNeededForLevelUpCumulative)
			{
				TakeCoins(coinsNeededForLevelUp);
				int tankUpgradeLevel = GetTankUpgradeLevel(tank);
				SetTankUpgradeLevel(tank, tankUpgradeLevel + 1);
				TankAnalytics.LeveledUpTank(tank, tankUpgradeLevel + 1);
				int @int = TankPrefs.GetInt("levelUpCount");
				if (@int < 15)
				{
					@int++;
					TankPrefs.SetInt("levelUpCount", @int);
					if (@int >= 15)
					{
						TankAnalytics.NewAttributionEvent(TankAnalytics.AttributionEvent.TankUpgraded, 15);
					}
				}
				return true;
			}
		}
		return false;
	}

	public static bool IsTankFullyUpgraded(Tank tank)
	{
		return GetTankUpgradeLevel(tank) >= Manager<PlayerDataManager>.instance.variables.tankLevelMinMax.max;
	}

	[Obsolete("Use AddTankCards(Tank) instead.")]
	public static void UnlockTank(Tank tank)
	{
		TankPrefs.SetInt(tank.id + "_unlocked", 1);
		TankPrefs.Save();
	}

	[Obsolete("Use AddTankCards(Tank) instead.")]
	public static bool BuyTank(Tank tank)
	{
		if (CanLevelUpTank(tank) && BuyTankUpgrade(tank))
		{
			UnlockTank(tank);
			return true;
		}
		return false;
	}

	public static bool CanLevelUpTank(Tank tank)
	{
		int tankUpgradeLevel = GetTankUpgradeLevel(tank);
		int tankLevelUpCoins = Manager<PlayerDataManager>.instance.variables.GetTankLevelUpCoins(tank.rarity, tankUpgradeLevel);
		int tankLevelUpCardsCumulative = Manager<PlayerDataManager>.instance.variables.GetTankLevelUpCardsCumulative(tank.rarity, tankUpgradeLevel);
		if (GetCoins() >= tankLevelUpCoins)
		{
			return GetTankCardCount(tank) >= tankLevelUpCardsCumulative;
		}
		return false;
	}

	public static Dictionary<Rarity, List<Tank>> GetNonMaxedTanks()
	{
		Dictionary<Rarity, List<Tank>> dictionary = new Dictionary<Rarity, List<Tank>>
		{
			{
				Rarity.Common,
				new List<Tank>()
			},
			{
				Rarity.Rare,
				new List<Tank>()
			},
			{
				Rarity.Epic,
				new List<Tank>()
			}
		};
		Tank[] tanks = Manager<PlayerDataManager>.instance.variables.tanks;
		for (int i = 0; i != tanks.Length; i++)
		{
			if (!HasAllTankCards(tanks[i]))
			{
				dictionary[tanks[i].rarity].Add(tanks[i]);
			}
		}
		return dictionary;
	}

	public static Dictionary<Rarity, List<Tank>> GetTanksWithOwnedCards()
	{
		Dictionary<Rarity, List<Tank>> nonMaxedTanks = GetNonMaxedTanks();
		Tank[] tanks = Manager<PlayerDataManager>.instance.variables.tanks;
		for (int i = 0; i != tanks.Length; i++)
		{
			if (GetTankCardCount(tanks[i]) == 0)
			{
				nonMaxedTanks[tanks[i].rarity].Remove(tanks[i]);
			}
		}
		return nonMaxedTanks;
	}

	public static Dictionary<Rarity, List<Tank>> GetTanksWithNoCards()
	{
		Dictionary<Rarity, List<Tank>> nonMaxedTanks = GetNonMaxedTanks();
		Tank[] tanks = Manager<PlayerDataManager>.instance.variables.tanks;
		for (int i = 0; i != tanks.Length; i++)
		{
			if (GetTankCardCount(tanks[i]) > 0)
			{
				nonMaxedTanks[tanks[i].rarity].Remove(tanks[i]);
			}
		}
		return nonMaxedTanks;
	}

	public static List<Tank> GetTanksListWithNoCards()
	{
		List<Tank> list = new List<Tank>();
		Tank[] tanks = Manager<PlayerDataManager>.instance.variables.tanks;
		for (int i = 0; i != tanks.Length; i++)
		{
			if (GetTankCardCount(tanks[i]) != 0)
			{
				list.Add(tanks[i]);
			}
		}
		return list;
	}

	public static int GetChestProgress(ChestProgressionType type)
	{
		return TankPrefs.GetInt("chestProgress" + type.ToString());
	}

	public static float GetChestProgressPercentage(ChestProgressionType type)
	{
		return (float)GetChestProgress(type) / (float)Manager<PlayerDataManager>.instance.variables.GetChestPointsNeeded(type);
	}

	public static void SetChestProgress(ChestProgressionType type, int value)
	{
		TankPrefs.SetInt("chestProgress" + type.ToString(), value);
	}

	public static void AddToChestProgress(ChestProgressionType type, int amount)
	{
		SetChestProgress(type, GetChestProgress(type) + amount);
	}

	public static bool IsReadyToOpenChest(ChestProgressionType type)
	{
		return GetChestProgress(type) >= Manager<PlayerDataManager>.instance.variables.GetChestPointsNeeded(type);
	}

	public static void ChestOpened(ChestProgressionType type)
	{
		AddToChestProgress(type, -Manager<PlayerDataManager>.instance.variables.GetChestPointsNeeded(type));
		SetNextChestRarity(type);
	}

	public static Rarity GetNextChestRarity(ChestProgressionType type)
	{
		return (Rarity)TankPrefs.GetInt("chestRarity" + type.ToString());
	}

	public static Variables.ChestStruct GetNextChest(ChestProgressionType type)
	{
		return Variables.instance.GetChest(GetNextChestRarity(type));
	}

	public static void SetNextChestRarity(ChestProgressionType type)
	{
		float num = 0f;
		Variables.ChestStruct[] chests = Variables.instance.chests;
		foreach (Variables.ChestStruct chestStruct in chests)
		{
			num += chestStruct.dropProbability;
		}
		float num2 = UnityEngine.Random.value * num;
		int num3 = 0;
		for (num3 = 0; num3 < Variables.instance.chests.Length - 1 && !(num2 < Variables.instance.chests[num3].dropProbability); num3++)
		{
			num2 -= Variables.instance.chests[num3].dropProbability;
		}
		TankPrefs.SetInt("chestRarity" + type.ToString(), (int)Variables.instance.chests[num3].rarity);
	}

	public static void AddChestRewards(ChestRewards rewards)//×°¼×¿¨Ìí¼Ó
	{
		for (int i = 0; i != rewards.cards.Count; i++)
		{
			Card card = rewards.cards[i];
			if (card.type == CardType.TankCard)
			{
               // Debug.Log("BT°æ");
				AddTankCards(card.id, card.count);
			}
			else if (card.type == CardType.BoosterCard)
            {
               // Debug.Log("BT°æ");
                AddBoosterCards(card.id, card.count);
			}
		}
		bool flag = rewards.gems > 0;
		AddCoins(rewards.coins, !flag, !flag);
		if (flag)
		{
			AddGems(rewards.gems);
		}
	}

	public static void HandleDailyChestNotification()
	{
		if (IsDailyChestReady() /*&& AdsManager.VideoAdAvailable(VideoAdPlacement.DailyChest)*/)
		{
			NotificationManager.ScheduleNotification(ScriptLocalization.Get("ChestReadyNotification"), "chestUnopened", DateTime.Now + TimeSpan.FromMinutes(10.0));
			return;
		}
		NotificationManager.RemoveNotificationsOfType("chestUnopened");
		NotificationManager.ResetNotificationBadge();
	}

	public static DateTime GetDailyChestResetTime()
	{
		if (string.IsNullOrEmpty(TankPrefs.GetString("dailyChestResetTime")))
		{
			return DateTime.Today;
		}
		DateTime result = DateTime.Now;
		if (!DateTime.TryParse(TankPrefs.GetString("dailyChestResetTime"), out result))
		{
			return DateTime.Today;
		}
		return result;
	}

	public static bool IsDailyChestReady()
	{
		return DateTime.Now > GetDailyChestResetTime();
	}

	public static void DailyChestOpened()
	{
		NotificationManager.ResetNotificationBadge();
		NotificationManager.RemoveNotificationsOfType("chestReady");
		NotificationManager.ScheduleNotification(ScriptLocalization.Get("ChestReadyNotification"), "chestReady", DateTime.Now + TimeSpan.FromDays(1.0));
		SetNextDailyChestAlert(DateTime.Today + TimeSpan.FromDays(1.0));
		TankPrefs.SetString("dailyChestResetTime", (DateTime.Today + TimeSpan.FromDays(1.0)).ToString());
		TankPrefs.SaveAndSendToCloud();
	}

	public static void SetNextDailyChestAlert(DateTime date)
	{
		TankPrefs.SetString("nextDailyChestAlert", date.ToString());
	}

	public static DateTime GetNextDailyChestAlertTime()
	{
		if (!DateTime.TryParse(TankPrefs.GetString("nextDailyChestAlert"), out DateTime result))
		{
			return GetDailyChestResetTime();
		}
		return result;
	}

	public static bool IsTimeForDailyChestAlert()
	{
		return DateTime.Now > GetNextDailyChestAlertTime();
	}

	public static bool CanBuyChest(Rarity rarity)
	{
		return GetGems() >= Manager<PlayerDataManager>.instance.variables.GetChest(rarity).gemValue;
	}

	public static bool HasActiveDailyOffer()
	{
		if (string.IsNullOrEmpty(TankPrefs.GetString("dailyOfferResetTime")))
		{
			return false;
		}
		DateTime result = DateTime.Now;
		if (!DateTime.TryParse(TankPrefs.GetString("dailyOfferResetTime"), out result))
		{
			return false;
		}
		return DateTime.Now < result;
	}

	public static void SetDailyOfferTime()
	{
		TankPrefs.SetString("dailyOfferResetTime", (DateTime.Today + TimeSpan.FromDays(1.0)).ToString());
	}

	public static void SetDailyOffers(ShopMenu.ShopItem[] items)
	{
		string text = "";
		try
		{
			ShopMenu.ShopItemSerialized[] array = new ShopMenu.ShopItemSerialized[items.Length];
			for (int i = 0; i != items.Length; i++)
			{
				array[i] = items[i].Serialize();
			}
			text = JsonConvert.SerializeObject(array);
			TankPrefs.SetString("currentDailyOffers", text);
		}
		catch (Exception)
		{
		}
		TankPrefs.Save();
	}

	public static ShopMenu.ShopItem[] GetActiveDailyOffers()
	{
		ShopMenu.ShopItem[] array = new ShopMenu.ShopItem[0];
		try
		{
			ShopMenu.ShopItemSerialized[] array2 = JsonConvert.DeserializeObject<ShopMenu.ShopItemSerialized[]>(TankPrefs.GetString("currentDailyOffers"));
			array = new ShopMenu.ShopItem[array2.Length];
			for (int i = 0; i != array2.Length; i++)
			{
				array[i] = array2[i].Deserialize();
			}
			return array;
		}
		catch (Exception)
		{
			return array;
		}
	}

	public static void ResetDailyOffers()
	{
		TankPrefs.SetString("dailyOfferResetTime", (DateTime.Today - TimeSpan.FromDays(1.0)).ToString());
	}

	public static bool BuyDailyOffer(int index)
	{
		ShopMenu.ShopItem[] activeDailyOffers = GetActiveDailyOffers();
		if (activeDailyOffers != null && activeDailyOffers.Length > index && !activeDailyOffers[index].bought)
		{
			DailyOfferBought(index);
			return true;
		}
		return false;
	}

	public static void DailyOfferBought(int index)
	{
		ShopMenu.ShopItem[] activeDailyOffers = GetActiveDailyOffers();
		if (activeDailyOffers != null && activeDailyOffers.Length > index)
		{
			activeDailyOffers[index].bought = true;
			SetDailyOffers(activeDailyOffers);
		}
	}

	public static ShopMenu.ShopItem[] GenerateDailyOffers()
	{
		Dictionary<Rarity, List<Tank>> newTanksPool = GetTanksWithNoCards();
		int count = newTanksPool.Count;
		Dictionary<Rarity, List<Tank>> ownedTanksPool = GetTanksWithOwnedCards();
		bool flag = FirstOnline >= DateTime.Today;
		int num = Mathf.Clamp(ownedTanksPool.Count + 1, 1, 4);
		List<ShopMenu.ShopItem> list = new List<ShopMenu.ShopItem>(num);
		Action<Tank> removeFromPools = delegate(Tank tank)
		{
			if (newTanksPool[tank.rarity].Contains(tank))
			{
				newTanksPool[tank.rarity].Remove(tank);
			}
			if (ownedTanksPool[tank.rarity].Contains(tank))
			{
				ownedTanksPool[tank.rarity].Remove(tank);
			}
		};
		Func<Dictionary<Rarity, List<Tank>>, Rarity, float, CurrencyType, ShopMenu.ShopItem> func = delegate(Dictionary<Rarity, List<Tank>> pool, Rarity rarity, float amountMultiplier, CurrencyType currency)
		{
			Tank tank3 = pool[rarity].Random();
			int cardsNeededForLevelUp = GetCardsNeededForLevelUp(tank3, GetTankPossibleLevel(tank3));
			removeFromPools(tank3);
			return GenerateDailyOffer(tank3, Mathf.CeilToInt((float)cardsNeededForLevelUp * amountMultiplier), currency);
		};
		Rarity[] array = new Rarity[3];
		float value = UnityEngine.Random.value;
		float num2 = 0f;
		Variables.ChestGenerationSettings.TankCardDrop[] premiumOfferProbabilities = Variables.instance.premiumOfferProbabilities;
		foreach (Variables.ChestGenerationSettings.TankCardDrop tankCardDrop in premiumOfferProbabilities)
		{
			num2 += tankCardDrop.probability;
			if (value < num2)
			{
				array[0] = tankCardDrop.rarity;
				array[1] = (Rarity)((int)(tankCardDrop.rarity + 1) % 3);
				array[2] = (Rarity)((int)(tankCardDrop.rarity + 2) % 3);
				break;
			}
		}
		for (int j = 0; j != array.Length; j++)
		{
			if (newTanksPool[array[j]].Count > 0)
			{
				list.Add(func(newTanksPool, array[j], 0.01f, CurrencyType.Gems));
				break;
			}
		}
		Tank tank2 = Manager<PlayerDataManager>.instance.variables.GetTank(0);
		if (flag && GetTankCardCount(tank2) < 12 && list.Count < num)
		{
			list.Add(GenerateDailyOffer(tank2, UnityEngine.Random.Range(4, 7), CurrencyType.Coins));
		}
		else
		{
			Variables.ChestGenerationSettings chestSettings = Manager<PlayerDataManager>.instance.variables.GetChestSettings(Rarity.Common);
			Rarity[] array2 = new Rarity[3]
			{
				Rarity.Common,
				Rarity.Rare,
				Rarity.Epic
			};
			while (list.Count < num)
			{
				bool flag2 = true;
				bool num3 = UnityEngine.Random.value < chestSettings.newTankCardProbability;
				Rarity tankRarity = chestSettings.GetTankRarity(UnityEngine.Random.value);
				if (num3 && newTanksPool[tankRarity].Count > 0)
				{
					flag2 = false;
					list.Add(func(newTanksPool, tankRarity, 0.01f, CurrencyType.Gems));
				}
				else if (ownedTanksPool[tankRarity].Count > 0)
				{
					flag2 = false;
					list.Add(func(ownedTanksPool, tankRarity, Variables.instance.dailyOfferStackSize.Random(), CurrencyType.Coins));
				}
				else
				{
					for (int k = 0; k != array2.Length; k++)
					{
						if (ownedTanksPool[array2[k]].Count > 0)
						{
							flag2 = false;
							list.Add(func(ownedTanksPool, array2[k], Variables.instance.dailyOfferStackSize.Random(), CurrencyType.Coins));
							break;
						}
					}
				}
				if ((list.Count >= num) | flag2)
				{
					break;
				}
			}
		}
		list.Sort(delegate(ShopMenu.ShopItem item, ShopMenu.ShopItem other)
		{
			if (item.currency == CurrencyType.Gems)
			{
				return -1;
			}
			return (other.currency == CurrencyType.Gems) ? 1 : 0;
		});
		for (int l = 0; l != list.Count; l++)
		{
			list[l].index = l;
		}
		return list.ToArray();
	}

	public static ShopMenu.ShopItem GenerateDailyOffer(Tank tank, int count, CurrencyType currency)
	{
		count = ((currency == CurrencyType.Gems) ? 1 : Mathf.Max(3, count));
		float cardGemValue = Variables.instance.GetTankLevelUpRequirements(tank.rarity).cardGemValue;
		float num = 0f;
		if (currency == CurrencyType.Coins)
		{
			cardGemValue *= Variables.instance.gemCoinValue / (float)Variables.instance.tankLevelMinMax.max;
			num = Variables.instance.dailyOfferDiscount.Random();
		}
		else
		{
			num = Variables.instance.premiumDailyOfferDiscounts.Random();
			cardGemValue *= Variables.instance.dollarGemValue * Variables.instance.premiumOfferMultiplier;
		}
		int price = Mathf.Max(1, Mathf.CeilToInt(cardGemValue * (float)count * (1f - num)));
		return new ShopMenu.ShopItem
		{
			type = ((currency == CurrencyType.Gems) ? ShopMenu.ShopItemType.PremiumCard : ShopMenu.ShopItemType.TankCard),
			currency = currency,
			id = tank.id,
			rarity = tank.rarity,
			price = price,
			discount = Mathf.RoundToInt(num * 100f),
			count = count,
			bought = false
		};
	}

	public static void SetNextBossRushFree(bool value)
	{
		TankPrefs.SetInt("isNextBossRushFree", value ? 1 : 0);
		TankPrefs.Save();
	}

	public static bool IsNextBossRushFree()
	{
		return TankPrefs.GetInt("isNextBossRushFree") == 1;
	}

	public static bool HasNoAdsPopupBeenShown()
	{
		return TankPrefs.GetInt("noAdsOfferShown") == 1;
	}

	public static bool IsTimeForNoAdsPopup()
	{
		return GetTotalInterstitialsShown() >= Variables.instance.showAdsBeforeOffer;
	}

	public static void SetNoAdsPopupShown()
	{
		TankPrefs.SetInt("noAdsOfferShown", 1);
		TankPrefs.Save();
	}

	public static int GetTotalInterstitialsShown()
	{
		return TankPrefs.GetInt("totalInterstitialsShown");
	}

	public static void InterstitialShown()
	{
		TankPrefs.SetInt("totalInterstitialsShown", GetTotalInterstitialsShown() + 1);
		TankPrefs.Save();
	}

	public static int GetTankPossibleLevel(Tank tank)
	{
		return Variables.instance.GetTankPossibleLevel(tank, GetTankCardCount(tank));
	}

	public static int GetWinStreak(GameMode mode)
	{
		switch (mode)
		{
		case GameMode.Arena:
			return TankPrefs.GetInt("winStreak1v1");
		case GameMode.Arena2v2:
			return TankPrefs.GetInt("winStreak2v2");
		default:
			return 0;
		}
	}

	public static void SetWinStreak(GameMode mode, int streak)
	{
		streak = Mathf.Max(0, streak);
		switch (mode)
		{
		case GameMode.Arena:
			TankPrefs.SetInt("winStreak1v1", streak);
			break;
		case GameMode.Arena2v2:
			TankPrefs.SetInt("winStreak2v2", streak);
			break;
		}
	}

	public static void ResetWinStreak(GameMode mode)
	{
		SetWinStreak(mode, 0);
	}

	public static void AdvanceWinStreak(GameMode mode)
	{
		int num = GetWinStreak(mode) + 1;
		SetWinStreak(mode, num);
		int bestWinStreak = GetBestWinStreak(mode);
		if (num > bestWinStreak)
		{
			SetBestWinStreak(mode, num);
		}
	}

	public static int GetBestWinStreak(GameMode mode)
	{
		switch (mode)
		{
		case GameMode.Arena:
			return TankPrefs.GetInt("winStreakBest1v1");
		case GameMode.Arena2v2:
			return TankPrefs.GetInt("winStreakBest2v2");
		default:
			return 0;
		}
	}

	public static void SetBestWinStreak(GameMode mode, int streak)
	{
		streak = Mathf.Max(0, streak);
		switch (mode)
		{
		case GameMode.Arena:
			TankPrefs.SetInt("winStreakBest1v1", streak);
			break;
		case GameMode.Arena2v2:
			TankPrefs.SetInt("winStreakBest2v2", streak);
			break;
		}
	}

	public static void AddWinStreakRating(GameMode mode)
	{
		int b = GetRating(mode) + Variables.instance.GetWinStreakBonus(GetWinStreak(mode));
		TankPrefs.SetInt((mode == GameMode.Arena) ? "rating" : "rating2v2", Mathf.Min(Manager<PlayerDataManager>.instance.variables.GetMaxTrophies(), b));
	}

	public static bool IsSubscribed(SubscriptionType type = SubscriptionType.DoubleChestRewards)
	{
		if (Application.internetReachability == NetworkReachability.NotReachable || !IAPManager.PurchasingInitialized)
		{
			return GetSubscriptionExpirationDate(type) > DateTime.Now;
		}
		return IAPManager.IsActiveSubscription(type);
	}

	public static bool IsCancelled(SubscriptionType type)
	{
		SubscriptionInfo subscriptionInfo = IAPManager.GetSubscriptionInfo(type);
		if (subscriptionInfo == null)
		{
			return false;
		}
		return subscriptionInfo?.isCancelled().Equals(Result.True) ?? false;
	}

	public static bool RecentlyExpired(SubscriptionType type)
	{
		if (NeverSubscribed(type))
		{
			return false;
		}
		return GetSubscriptionExpirationDate(type).AddDays(7.0) > DateTime.Today;
	}

	public static bool NeverSubscribed(SubscriptionType type)
	{
		return GetSubscriptionExpirationDate(type).Equals(DateTime.MinValue);
	}

	public static void SetSubscriptionExpirationDate(SubscriptionType type, DateTime date)
	{
		string subExpirationKey = GetSubExpirationKey(type);
		if (!string.IsNullOrEmpty(subExpirationKey))
		{
			TankPrefs.SetString(subExpirationKey, date.ToString());
		}
	}

	public static DateTime GetSubscriptionExpirationDate(SubscriptionType type)
	{
		string subExpirationKey = GetSubExpirationKey(type);
		DateTime result = DateTime.MinValue;
		if (!string.IsNullOrEmpty(subExpirationKey))
		{
			DateTime.TryParse(TankPrefs.GetString(subExpirationKey), out result);
		}
		return result;
	}

	private static string GetSubExpirationKey(SubscriptionType type)
	{
		if (type == SubscriptionType.DoubleChestRewards)
		{
			return "subDblChestExpire";
		}
		return null;
	}

	private static string GetSubReceiptKey(SubscriptionType type)
	{
		if (type == SubscriptionType.DoubleChestRewards)
		{
			return "subDblChest";
		}
		return null;
	}

	public static void SetSubscriptionReceipt(SubscriptionType type, string receipt)
	{
		string subReceiptKey = GetSubReceiptKey(type);
		if (!string.IsNullOrEmpty(subReceiptKey))
		{
			TankPrefs.SetString(subReceiptKey, receipt);
		}
	}

	public static string GetSubscriptionReceipt(SubscriptionType type)
	{
		string subReceiptKey = GetSubReceiptKey(type);
		if (!string.IsNullOrEmpty(subReceiptKey))
		{
			return TankPrefs.GetString(subReceiptKey);
		}
		return string.Empty;
	}

	public static int GetPvpGamesPlayedCount()
	{
		return TankPrefs.GetInt("pvpGameCount");
	}

	public static void PvpGamePlayed()
	{
		TankPrefs.SetInt("pvpGameCount", GetPvpGamesPlayedCount() + 1);
	}

	public static bool HasFacebookBeenAsked()
	{
		return TankPrefs.GetInt("facebookAsked") == 1;
	}

	public static bool IsTimeForFacebookPrompt()
	{
		if (!BackendManager.ConnectedWithFacebook && !HasFacebookBeenAsked())
		{
			return GetPvpGamesPlayedCount() >= 2;
		}
		return false;
	}
}
