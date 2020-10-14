using System.Collections.ObjectModel;

public static class TankLeaderboard
{
	private static readonly ReadOnlyCollection<string> leaderboardIds = new ReadOnlyCollection<string>(new string[7]
	{
		"CgkIuumNjKUDEAIQAQ",
		"CgkIuumNjKUDEAIQAg",
		"CgkIuumNjKUDEAIQAw",
		"CgkIuumNjKUDEAIQFQ",
		"CgkIuumNjKUDEAIQHA",
		"CgkIuumNjKUDEAIQHw",
		"CgkIuumNjKUDEAIQIA"
	});

	public static string GetLeaderboard(LeaderboardID id)
	{
		return leaderboardIds[(int)id];
	}
}
