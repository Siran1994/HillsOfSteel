using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UTNotifications
{
	public class ManagerImpl : Manager
	{
		private bool m_willHandleReceivedNotifications;

		private const float m_timeBetweenCheckingForIncomingNotifications = 0.5f;

		private float m_timeToCheckForIncomingNotifications;

		public override bool Initialize(bool willHandleReceivedNotifications, int startId = 0, bool incrementalId = false)
		{
			m_willHandleReceivedNotifications = willHandleReceivedNotifications;
			bool flag = false;
			switch (Settings.Instance.AllowUpdatingGooglePlayIfRequired)
			{
			case Settings.GooglePlayUpdatingIfRequiredMode.DISABLED:
				flag = false;
				break;
			case Settings.GooglePlayUpdatingIfRequiredMode.EVERY_INITIALIZE:
				flag = true;
				break;
			case Settings.GooglePlayUpdatingIfRequiredMode.ONCE:
				flag = (PlayerPrefs.GetInt("_UT_NOTIFICATIONS_GP_UPDATING_WAS_ALLOWED", 0) == 0);
				if (flag)
				{
					PlayerPrefs.SetInt("_UT_NOTIFICATIONS_GP_UPDATING_WAS_ALLOWED", 1);
				}
				break;
			}
			try
			{
				using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("universal.tools.notifications.Manager"))
				{
					return base.Initialized = androidJavaClass.CallStatic<bool>("initialize", new object[19]
					{
						Settings.Instance.PushNotificationsEnabledFirebase,
						Settings.Instance.PushNotificationsEnabledAmazon,
						Settings.Instance.FirebaseSenderID,
						willHandleReceivedNotifications,
						startId,
						incrementalId,
						(int)Settings.Instance.AndroidShowNotificationsMode,
						Settings.Instance.AndroidRestoreScheduledNotificationsAfterReboot,
						(int)Settings.Instance.AndroidNotificationsGrouping,
						Settings.Instance.AndroidShowLatestNotificationOnly,
						Settings.Instance.PushPayloadTitleFieldName,
						Settings.Instance.PushPayloadTextFieldName,
						Settings.Instance.PushPayloadUserDataParentFieldName,
						Settings.Instance.PushPayloadNotificationProfileFieldName,
						Settings.Instance.PushPayloadIdFieldName,
						Settings.Instance.PushPayloadBadgeFieldName,
						Settings.Instance.PushPayloadButtonsParentName,
						ProfilesSettingsJson(),
						flag
					});
				}
			}
			catch (AndroidJavaException exception)
			{
				UnityEngine.Debug.LogException(exception);
				return false;
			}
		}

		public override void PostLocalNotification(string title, string text, int id, IDictionary<string, string> userData, string notificationProfile, int badgeNumber, ICollection<Button> buttons)
		{
			try
			{
				using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("universal.tools.notifications.Manager"))
				{
					androidJavaClass.CallStatic("postNotification", ToBase64(title), ToBase64(text), id, ToBase64(ToString(JsonUtils.ToJson(userData))), notificationProfile, badgeNumber, ToBase64(ToString(JsonUtils.ToJson(buttons))));
				}
			}
			catch (AndroidJavaException exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public override void ScheduleNotification(int triggerInSeconds, string title, string text, int id, IDictionary<string, string> userData, string notificationProfile, int badgeNumber, ICollection<Button> buttons)
		{
			try
			{
				using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("universal.tools.notifications.Manager"))
				{
					androidJavaClass.CallStatic("scheduleNotification", triggerInSeconds, ToBase64(title), ToBase64(text), id, ToBase64(ToString(JsonUtils.ToJson(userData))), notificationProfile, badgeNumber, ToBase64(ToString(JsonUtils.ToJson(buttons))));
				}
			}
			catch (AndroidJavaException exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public override void ScheduleNotificationRepeating(int firstTriggerInSeconds, int intervalSeconds, string title, string text, int id, IDictionary<string, string> userData, string notificationProfile, int badgeNumber, ICollection<Button> buttons)
		{
			try
			{
				using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("universal.tools.notifications.Manager"))
				{
					androidJavaClass.CallStatic("scheduleNotificationRepeating", firstTriggerInSeconds, intervalSeconds, ToBase64(title), ToBase64(text), id, ToBase64(ToString(JsonUtils.ToJson(userData))), notificationProfile, badgeNumber, ToBase64(ToString(JsonUtils.ToJson(buttons))));
				}
			}
			catch (AndroidJavaException exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public override bool NotificationsEnabled()
		{
			try
			{
				using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("universal.tools.notifications.Manager"))
				{
					return androidJavaClass.CallStatic<bool>("notificationsEnabled", Array.Empty<object>());
				}
			}
			catch (AndroidJavaException exception)
			{
				UnityEngine.Debug.LogException(exception);
				return false;
			}
		}

		public override bool NotificationsAllowed()
		{
			try
			{
				using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("universal.tools.notifications.Manager"))
				{
					return androidJavaClass.CallStatic<bool>("notificationsAllowed", Array.Empty<object>());
				}
			}
			catch (AndroidJavaException exception)
			{
				UnityEngine.Debug.LogException(exception);
				return true;
			}
		}

		public override void SetNotificationsEnabled(bool enabled)
		{
			try
			{
				using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("universal.tools.notifications.Manager"))
				{
					androidJavaClass.CallStatic("setNotificationsEnabled", enabled);
				}
			}
			catch (AndroidJavaException exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public override bool PushNotificationsEnabled()
		{
			try
			{
				using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("universal.tools.notifications.Manager"))
				{
					return androidJavaClass.CallStatic<bool>("pushNotificationsEnabled", Array.Empty<object>()) && ((Settings.Instance.PushNotificationsEnabledFirebase && androidJavaClass.CallStatic<bool>("fcmProviderAvailable", new object[1]
					{
						false
					})) || (Settings.Instance.PushNotificationsEnabledAmazon && androidJavaClass.CallStatic<bool>("admProviderAvailable", Array.Empty<object>())));
				}
			}
			catch (AndroidJavaException exception)
			{
				UnityEngine.Debug.LogException(exception);
				return false;
			}
		}

		public override bool SetPushNotificationsEnabled(bool enabled)
		{
			try
			{
				using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("universal.tools.notifications.Manager"))
				{
					androidJavaClass.CallStatic("setPushNotificationsEnabled", enabled);
					return PushNotificationsEnabled();
				}
			}
			catch (AndroidJavaException exception)
			{
				UnityEngine.Debug.LogException(exception);
				return false;
			}
		}

		public override void CancelNotification(int id)
		{
			try
			{
				using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("universal.tools.notifications.Manager"))
				{
					androidJavaClass.CallStatic("cancelNotification", id);
				}
			}
			catch (AndroidJavaException exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
			HideNotification(id);
		}

		public override void HideNotification(int id)
		{
			try
			{
				using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("universal.tools.notifications.Manager"))
				{
					androidJavaClass.CallStatic("hideNotification", id);
				}
			}
			catch (AndroidJavaException exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public override void HideAllNotifications()
		{
			try
			{
				using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("universal.tools.notifications.Manager"))
				{
					androidJavaClass.CallStatic("hideAllNotifications");
				}
			}
			catch (AndroidJavaException exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public override void CancelAllNotifications()
		{
			try
			{
				using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("universal.tools.notifications.Manager"))
				{
					androidJavaClass.CallStatic("cancelAllNotifications");
				}
			}
			catch (AndroidJavaException exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public override int GetBadge()
		{
			try
			{
				using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("universal.tools.notifications.Manager"))
				{
					return androidJavaClass.CallStatic<int>("getBadge", Array.Empty<object>());
				}
			}
			catch (AndroidJavaException exception)
			{
				UnityEngine.Debug.LogException(exception);
				return 0;
			}
		}

		public override void SetBadge(int bandgeNumber)
		{
			try
			{
				using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("universal.tools.notifications.Manager"))
				{
					androidJavaClass.CallStatic("setBadge", bandgeNumber);
				}
			}
			catch (AndroidJavaException exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public void _OnAndroidIdReceived(string providerAndId)
		{
			JSONNode jSONNode = JSON.Parse(providerAndId);
			if (OnSendRegistrationIdHasSubscribers())
			{
				_OnSendRegistrationId(jSONNode[0], jSONNode[1]);
			}
		}

		protected void LateUpdate()
		{
			m_timeToCheckForIncomingNotifications -= Time.unscaledDeltaTime;
			if (!(m_timeToCheckForIncomingNotifications > 0f))
			{
				m_timeToCheckForIncomingNotifications = 0.5f;
				if (OnNotificationClickedHasSubscribers())
				{
					try
					{
						using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("universal.tools.notifications.Manager"))
						{
							HandleClickedNotification(androidJavaClass.CallStatic<string>("getClickedNotificationPacked", Array.Empty<object>()));
						}
					}
					catch (AndroidJavaException exception)
					{
						UnityEngine.Debug.LogException(exception);
					}
				}
				if (m_willHandleReceivedNotifications && OnNotificationsReceivedHasSubscribers())
				{
					try
					{
						using (AndroidJavaClass androidJavaClass2 = new AndroidJavaClass("universal.tools.notifications.Manager"))
						{
							HandleReceivedNotifications(androidJavaClass2.CallStatic<string>("getReceivedNotificationsPacked", Array.Empty<object>()));
						}
					}
					catch (AndroidJavaException exception2)
					{
						UnityEngine.Debug.LogException(exception2);
					}
				}
			}
		}

		protected void OnApplicationPause(bool paused)
		{
			try
			{
				using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("universal.tools.notifications.Manager"))
				{
					androidJavaClass.CallStatic("setBackgroundMode", paused);
				}
			}
			catch (AndroidJavaException exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		private void HandleClickedNotification(string receivedNotificationPacked)
		{
			if (!string.IsNullOrEmpty(receivedNotificationPacked))
			{
				_OnNotificationClicked(ParseReceivedNotification(JSON.Parse(receivedNotificationPacked)));
			}
		}

		private void HandleReceivedNotifications(string receivedNotificationsPacked)
		{
			if (string.IsNullOrEmpty(receivedNotificationsPacked) || receivedNotificationsPacked == "[]")
			{
				return;
			}
			List<ReceivedNotification> list = new List<ReceivedNotification>();
			JSONNode jSONNode = JSON.Parse(receivedNotificationsPacked);
			for (int i = 0; i < jSONNode.Count; i++)
			{
				ReceivedNotification receivedNotification = ParseReceivedNotification(jSONNode[i]);
				bool flag = false;
				for (int j = 0; j < list.Count; j++)
				{
					if (list[j].id == receivedNotification.id)
					{
						list[j] = receivedNotification;
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					list.Add(receivedNotification);
				}
			}
			_OnNotificationsReceived(list);
		}

		private static ReceivedNotification ParseReceivedNotification(JSONNode json)
		{
			string value = json["title"].Value;
			string value2 = json["text"].Value;
			int asInt = json["id"].AsInt;
			string value3 = json["notificationProfile"].Value;
			int asInt2 = json["badgeNumber"].AsInt;
			JSONNode jSONNode = json["userData"];
			if (!(json["buttonIndex"] is JSONLazyCreator) && !(json["buttons"] is JSONLazyCreator))
			{
				JSONArray asArray = json["buttons"].AsArray;
				int asInt3 = json["buttonIndex"].AsInt;
				if (asArray != null && asInt3 >= 0 && asInt3 < asArray.Count)
				{
					jSONNode = asArray[asInt3]["userData"];
				}
			}
			Dictionary<string, string> dictionary;
			if (jSONNode != null && jSONNode.Count > 0)
			{
				dictionary = new Dictionary<string, string>();
				foreach (KeyValuePair<string, JSONNode> item in (JSONClass)jSONNode)
				{
					dictionary.Add(item.Key, item.Value.Value);
				}
			}
			else
			{
				dictionary = null;
			}
			return new ReceivedNotification(value, value2, asInt, dictionary, value3, asInt2);
		}

		private static string ProfilesSettingsJson()
		{
			JSONArray jSONArray = new JSONArray();
			foreach (Settings.NotificationProfile notificationProfile in Settings.Instance.NotificationProfiles)
			{
				JSONClass jSONClass = new JSONClass();
				jSONClass.Add("id", new JSONData((notificationProfile.profileName != "default") ? notificationProfile.profileName : "__default_profile"));
				jSONClass.Add("name", (!string.IsNullOrEmpty(notificationProfile.androidChannelName)) ? notificationProfile.androidChannelName : notificationProfile.profileName);
				jSONClass.Add("description", notificationProfile.androidChannelDescription ?? "");
				jSONClass.Add("high_priority", new JSONData(notificationProfile.androidHighPriority));
				if (notificationProfile.colorSpecified)
				{
					Color32 color = notificationProfile.androidColor;
					int aData = (color.a << 24) | (color.r << 16) | (color.g << 8) | color.b;
					jSONClass.Add("color", new JSONData(aData));
				}
				jSONArray.Add(jSONClass);
			}
			return jSONArray.ToString();
		}

		private static string ToString(object o)
		{
			return o?.ToString();
		}

		private static string ToBase64(string str)
		{
			if (string.IsNullOrEmpty(str))
			{
				return str;
			}
			return Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
		}
	}
}
