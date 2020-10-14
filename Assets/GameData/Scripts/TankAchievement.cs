using System.Collections.ObjectModel;

public static class TankAchievement
{
	private static readonly ReadOnlyCollection<string> achievementIds = new ReadOnlyCollection<string>(new string[34]
	{
		"",
		"CgkIuumNjKUDEAIQBA",
		"CgkIuumNjKUDEAIQBQ",
		"CgkIuumNjKUDEAIQBg",
		"CgkIuumNjKUDEAIQBw",
		"CgkIuumNjKUDEAIQCA",
		"CgkIuumNjKUDEAIQCQ",
		"CgkIuumNjKUDEAIQCg",
		"CgkIuumNjKUDEAIQCw",
		"CgkIuumNjKUDEAIQDA",
		"CgkIuumNjKUDEAIQDQ",
		"CgkIuumNjKUDEAIQDg",
		"CgkIuumNjKUDEAIQDw",
		"AllArmor",
		"AllGun",
		"AllEngine",
		"AllOneTank",
		"CgkIuumNjKUDEAIQEA",
		"CgkIuumNjKUDEAIQEQ",
		"CgkIuumNjKUDEAIQEg",
		"CgkIuumNjKUDEAIQEw",
		"CgkIuumNjKUDEAIQFg",
		"CgkIuumNjKUDEAIQFw",
		"CgkIuumNjKUDEAIQGA",
		"CgkIuumNjKUDEAIQGQ",
		"CgkIuumNjKUDEAIQGg",
		"CgkIuumNjKUDEAIQGw",
		"CgkIuumNjKUDEAIQHQ",
		"CgkIuumNjKUDEAIQIQ",
		"CgkIuumNjKUDEAIQIg",
		"CgkIuumNjKUDEAIQIw",
		"CgkIuumNjKUDEAIQJA",
		"CgkIuumNjKUDEAIQJQ",
		"CgkIuumNjKUDEAIQJg"
	});

	public static string GetAchievement(AchievementID id)
	{
		return achievementIds[(int)id];
	}
}
