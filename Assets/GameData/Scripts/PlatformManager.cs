//using GooglePlayGames;
//using GooglePlayGames.BasicApi;
using System;
using UnityEngine;

public class PlatformManager : Manager<PlatformManager>
{
	private bool localUserWasAuthenticated;

	public static bool ReconnectingWithGooglePlay
	{
		get;
		set;
	}

	private void Update()
	{
		if (localUserWasAuthenticated && !Social.localUser.authenticated)
		{
			PlayerPrefs.SetInt("noSocialPlatform", 1);
		}
		if (Social.localUser.authenticated)
		{
			PlayerPrefs.SetInt("noSocialPlatform", 0);
			localUserWasAuthenticated = true;
		}
	}

	public void Initialize()
	{
		Manager<PlatformManager>.instance = this;
//		PlayGamesPlatform.InitializeInstance(new PlayGamesClientConfiguration.Builder().RequestServerAuthCode(forceRefresh: false).Build());
//		PlayGamesPlatform.Activate();
		if (PlayerPrefs.GetInt("explicitSignOut") == 0 && PlayerPrefs.GetInt("noSocialPlatform") == 0)
		{
			AuthenticateInternal(null, afterNeedsSuccess: false);
		}
	}

	public static void ReconnectWithGooglePlay(Action onSuccess = null, Action onFailure = null)
	{
		ReconnectingWithGooglePlay = true;
		DoAuthenticate(delegate
		{
			if (Social.localUser.authenticated)
			{
				PlayerPrefs.SetInt("explicitSignOut", 0);
				BackendManager.ConnectWithPlayGamesServices();
				if (onSuccess != null)
				{
					onSuccess();
				}
			}
			else if (onFailure != null)
			{
				onFailure();
			}
		}, onFailure == null, silent: false);
	}

	public static void AuthenticateExplicit()
	{
		DoAuthenticate(delegate
		{
			if (Social.localUser.authenticated)
			{
				PlayerPrefs.SetInt("explicitSignOut", 0);
			}
		}, afterNeedsSuccess: false, silent: false);
	}

	private static void AuthenticateInternal(Action after, bool afterNeedsSuccess)
	{
		if (Social.localUser.authenticated)
		{
			after?.Invoke();
		}
		else
		{
			DoAuthenticate(after, afterNeedsSuccess, silent: true);
		}
	}

	private static void DoAuthenticate(Action after, bool afterNeedsSuccess, bool silent)
	{
		Action<bool, string> callback = delegate(bool success, string s)
		{
			if (success)
			{
				if (after != null)
				{
					after();
				}
			}
			else if (!Social.localUser.authenticated)
			{
				PlayerPrefs.SetInt("explicitSignOut", 1);
				if (!afterNeedsSuccess && after != null)
				{
					after();
				}
			}
			else if (after != null)
			{
				after();
			}
		};
//		PlayGamesPlatform.Instance.Authenticate(callback, silent);
	}

	public static void ReportHighscore(int score, string id)
	{
		//Social.ReportScore(score, id, delegate
		//{
		//});
	}

	public static void ReportAchievement(AchievementID id)
	{
		if (Social.localUser.authenticated && id != 0)
		{
			Social.ReportProgress(TankAchievement.GetAchievement(id), 100.0, delegate
			{
			});
		}
	}
}
