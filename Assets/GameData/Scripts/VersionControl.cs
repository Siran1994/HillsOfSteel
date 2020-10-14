public static class VersionControl
{
	public static readonly int CurrentVersionNumber = 220;

	public static void Process()
	{
		int version = GetVersion();
		if (version == 0)
		{
			NotificationManager.RemoveNotificationsOfType("dailyReward");
			NotificationManager.ResetNotificationBadge();
		}
		if (version < 203)
		{
			for (int i = 0; i != Variables.instance.levels.Count; i++)
			{
				if (PlayerDataManager.GetLeaderboardScore(Variables.instance.levels[i].leaderboard) > 0)
				{
					TankPrefs.SetInt("level" + i + "Unlocked", 1);
				}
			}
		}
		if (version < 210)
		{
			Variables.Bundle[] bundles = Variables.instance.bundles;
			int num = bundles.Length - 1;
			TankPrefs.SetInt("bundleStopIndex", TankPrefs.GetInt("bundleIndex", -1));
			TankPrefs.SetInt("bundleIndex", num);
			TankPrefs.SetInt("bundleCounter", bundles[num].showAfterTimesTried + 3);
			PlayerDataManager.SetCurrentAdventureLevel(PlayerDataManager.GetSelectedLevel());
		}
		if (version > 0 && version < CurrentVersionNumber && PlayerDataManager.BeenInAppBefore)
		{
			MenuController.ShowMenu<WhatsNewPopup>();
		}
		SetVersion(CurrentVersionNumber);
	}

	public static int GetVersion()
	{
		return TankPrefs.GetInt("version");
	}

	public static void SetVersion(int version)
	{
		TankPrefs.SetInt("version", version);
		TankPrefs.SaveAtEndOfFrame();
	}
}
