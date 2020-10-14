using I2.Loc;
using System;
using System.Collections;
using UnityEngine;
using UTNotifications;

public class NotificationManager : Manager<NotificationManager>
{
	public bool IsInitialized
	{
		get;
		private set;
	}

	public void Initialize()
	{
		Manager<NotificationManager>.instance = this;
		IsInitialized = false;
		TryFullInit();
		IsInitialized = true;
		ResetNotificationBadgeWhenReady();
	}

	public static void TryFullInit()
	{
		Manager.Instance.OnNotificationClicked += Manager<NotificationManager>.instance.OnNotificationClicked;
		Manager.Instance.Initialize(willHandleReceivedNotifications: true);
		Manager.Instance.SetPushNotificationsEnabled(enable: true);
		SetWeMissYou();
	}

	public static void SetWeMissYou()
	{
		string[] array = new string[3]
		{
			"weMissYouTraditional",
			"weMissYouIncentive",
			"weMissYouCompetitive"
		};
		string[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			RemoveNotificationsOfType(array2[i]);
		}
		string text = array[UnityEngine.Random.Range(0, array.Length)];
		ScheduleNotification(ScriptLocalization.Get(text), text, DateTime.Now + TimeSpan.FromDays(7.0));
	}

	public static void RegisterForNotifications(bool val)
	{
		Manager.Instance.SetPushNotificationsEnabled(val);
		PlayerPrefs.SetInt("notificationsAsked", 1);
	}

	public static bool NotificationRegistrationAsked()
	{
		return PlayerPrefs.GetInt("notificationsAsked") == 1;
	}

	public static void RemoveNotificationsOfType(string type)
	{
		if (Manager.Instance.Initialized)
		{
			Manager.Instance.CancelNotification(type.GetHashCode());
		}
	}

	public static void ResetNotificationBadge()
	{
		if (Manager.Instance.Initialized)
		{
			Manager.Instance.SetBadge(0);
		}
	}

	public static void ResetNotificationBadgeWhenReady()
	{
		Manager<NotificationManager>.instance.StartCoroutine(WaitForInit(ResetNotificationBadge));
	}

	private static IEnumerator WaitForInit(Action after)
	{
		while (!Manager.Instance.Initialized)
		{
			yield return null;
		}
		after();
	}

	public static void ScheduleNotification(string text, string type, DateTime when)
	{
		if (Manager.Instance.Initialized)
		{
			string title = "Hills of Steel";
			Manager.Instance.ScheduleNotification(when, title, text, type.GetHashCode(), null, null, 1);
		}
	}

	private void OnNotificationClicked(ReceivedNotification notification)
	{
		if (notification.id == "dailyReward".GetHashCode())
		{
			TankAnalytics.AppStartReason("dailyReward");
		}
		else if (notification.id == "freeUpgrade".GetHashCode())
		{
			TankAnalytics.AppStartReason("freeUpgrade");
		}
		else if (notification.id == "weMissYouTraditional".GetHashCode())
		{
			TankAnalytics.AppStartReason("weMissYouTraditional");
		}
		else if (notification.id == "weMissYouIncentive".GetHashCode())
		{
			TankAnalytics.AppStartReason("weMissYouIncentive");
		}
		else if (notification.id == "weMissYouCompetitive".GetHashCode())
		{
			TankAnalytics.AppStartReason("weMissCompetitive");
		}
		else if (notification.id == "coinBonus".GetHashCode())
		{
			TankAnalytics.AppStartReason("coinBonus");
			LoadingScreen.OpenShopOnLoad();
		}
		else if (notification.id == "chestUnopened".GetHashCode())
		{
			TankAnalytics.AppStartReason("chestUnopened");
			LoadingScreen.OpenShopOnLoad();
		}
		else if (notification.id == "chestReady".GetHashCode())
		{
			TankAnalytics.AppStartReason("chestReady");
			LoadingScreen.OpenShopOnLoad();
		}
		else if (notification.id == "xmasPresent".GetHashCode())
		{
			TankAnalytics.AppStartReason("xmasPresent");
		}
	}
}
