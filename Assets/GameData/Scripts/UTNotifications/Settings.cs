using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

#pragma warning disable 0649
#pragma warning disable 0414
namespace UTNotifications
{
	public class Settings : ScriptableObject
	{
		[Serializable]
		public struct NotificationProfile
		{
			public string profileName;

			public string iosSound;

			public string androidChannelName;

			public string androidChannelDescription;

			public string androidIcon;

			public string androidLargeIcon;

			public string androidIcon5Plus;

			[FormerlySerializedAs("androidIconBGColorSpecified")]
			public bool colorSpecified;

			[FormerlySerializedAs("androidIconBGColor")]
			public Color androidColor;

			public string androidSound;

			public bool androidHighPriority;
		}

		public enum ShowNotifications
		{
			WHEN_CLOSED_OR_IN_BACKGROUND,
			WHEN_CLOSED,
			ALWAYS
		}

		public enum NotificationsGroupingMode
		{
			NONE,
			BY_NOTIFICATION_PROFILES,
			FROM_USER_DATA,
			ALL_IN_A_SINGLE_GROUP
		}

		public enum GooglePlayUpdatingIfRequiredMode
		{
			DISABLED,
			ONCE,
			EVERY_INITIALIZE
		}

		private class UpdateMessage
		{
			public readonly string version;

			public readonly string text;

			public UpdateMessage(string version, string text)
			{
				this.version = version;
				this.text = text;
			}
		}

		public const string Version = "1.7.3";

		public const string DEFAULT_PROFILE_NAME = "default";

		public const string DEFAULT_PROFILE_NAME_INTERNAL = "__default_profile";

		[SerializeField]
		private List<NotificationProfile> m_notificationProfiles = new List<NotificationProfile>();

		[SerializeField]
		private string m_pushPayloadTitleFieldName = "title";

		[SerializeField]
		private string m_pushPayloadTextFieldName = "text";

		[SerializeField]
		private string m_pushPayloadIdFieldName = "id";

		[SerializeField]
		private string m_pushPayloadUserDataParentFieldName = "";

		[SerializeField]
		private string m_pushPayloadNotificationProfileFieldName = "notification_profile";

		[SerializeField]
		private string m_pushPayloadBadgeFieldName = "badge_number";

		[SerializeField]
		private string m_pushPayloadButtonsParentName = "buttons";

		[SerializeField]
		private string m_googlePlayServicesLibVersion = "11.8.0+";

		[SerializeField]
		private string m_androidSupportLibVersion = "25.3.1+";

		[SerializeField]
		private ShowNotifications m_androidShowNotificationsMode;

		[SerializeField]
		private bool m_android4CompatibilityMode;

		[SerializeField]
		private bool m_androidRestoreScheduledNotificationsAfterReboot = true;

		[SerializeField]
		private NotificationsGroupingMode m_androidNotificationsGrouping;

		[SerializeField]
		private bool m_androidShowLatestNotificationOnly;

		[SerializeField]
		private bool m_pushNotificationsEnabledIOS;

		[SerializeField]
		private bool m_pushNotificationsEnabledFirebase;

		[SerializeField]
		private bool m_pushNotificationsEnabledAmazon;

		[SerializeField]
		private bool m_pushNotificationsEnabledWindows;

		[SerializeField]
		private GooglePlayUpdatingIfRequiredMode m_allowUpdatingGooglePlayIfRequired = GooglePlayUpdatingIfRequiredMode.ONCE;

		[SerializeField]
		private string m_firebaseSenderID = "";

		[SerializeField]
		private string m_assetVersionSaved = "";

		[SerializeField]
		private bool m_windowsDontShowWhenRunning = true;

		private const string m_assetName = "UTNotificationsSettings";

		private const string m_settingsMenuItem = "Edit/Project Settings/UTNotifications";

		private static Settings m_instance;

		public static Settings Instance
		{
			get
			{
				if (m_instance == null)
				{
					m_instance = (Resources.Load("UTNotificationsSettings") as Settings);
					if (m_instance == null)
					{
						m_instance = ScriptableObject.CreateInstance<Settings>();
					}
				}
				return m_instance;
			}
		}

		public List<NotificationProfile> NotificationProfiles
		{
			get
			{
				if (m_notificationProfiles.Count == 0 || m_notificationProfiles[0].profileName != "default")
				{
					m_notificationProfiles.Insert(0, new NotificationProfile
					{
						profileName = "default"
					});
				}
				return m_notificationProfiles;
			}
		}

		public string PushPayloadTitleFieldName
		{
			get
			{
				return m_pushPayloadTitleFieldName;
			}
			set
			{
				if (m_pushPayloadTitleFieldName != value)
				{
					m_pushPayloadTitleFieldName = value;
				}
			}
		}

		public string PushPayloadTextFieldName
		{
			get
			{
				return m_pushPayloadTextFieldName;
			}
			set
			{
				if (m_pushPayloadTextFieldName != value)
				{
					m_pushPayloadTextFieldName = value;
				}
			}
		}

		public string PushPayloadIdFieldName
		{
			get
			{
				return m_pushPayloadIdFieldName;
			}
			set
			{
				if (m_pushPayloadIdFieldName != value)
				{
					m_pushPayloadIdFieldName = value;
				}
			}
		}

		public string PushPayloadUserDataParentFieldName
		{
			get
			{
				return m_pushPayloadUserDataParentFieldName;
			}
			set
			{
				if (m_pushPayloadUserDataParentFieldName != value)
				{
					m_pushPayloadUserDataParentFieldName = value;
				}
			}
		}

		public string PushPayloadNotificationProfileFieldName
		{
			get
			{
				return m_pushPayloadNotificationProfileFieldName;
			}
			set
			{
				if (m_pushPayloadNotificationProfileFieldName != value)
				{
					m_pushPayloadNotificationProfileFieldName = value;
				}
			}
		}

		public string PushPayloadBadgeFieldName
		{
			get
			{
				return m_pushPayloadBadgeFieldName;
			}
			set
			{
				if (m_pushPayloadBadgeFieldName != value)
				{
					m_pushPayloadBadgeFieldName = value;
				}
			}
		}

		public string PushPayloadButtonsParentName
		{
			get
			{
				return m_pushPayloadButtonsParentName;
			}
			set
			{
				if (m_pushPayloadButtonsParentName != value)
				{
					m_pushPayloadButtonsParentName = value;
				}
			}
		}

		public string GooglePlayServicesLibVersion
		{
			get
			{
				return m_googlePlayServicesLibVersion;
			}
			set
			{
				if (m_googlePlayServicesLibVersion != value)
				{
					m_googlePlayServicesLibVersion = value;
				}
			}
		}

		public string AndroidSupportLibVersion
		{
			get
			{
				return m_androidSupportLibVersion;
			}
			set
			{
				if (m_androidSupportLibVersion != value)
				{
					m_androidSupportLibVersion = value;
				}
			}
		}

		public bool PushNotificationsEnabledIOS
		{
			get
			{
				return m_pushNotificationsEnabledIOS;
			}
			set
			{
				if (m_pushNotificationsEnabledIOS != value)
				{
					m_pushNotificationsEnabledIOS = value;
				}
			}
		}

		public bool PushNotificationsEnabledFirebase
		{
			get
			{
				return m_pushNotificationsEnabledFirebase;
			}
			set
			{
				if (m_pushNotificationsEnabledFirebase != value)
				{
					m_pushNotificationsEnabledFirebase = value;
				}
			}
		}

		public bool PushNotificationsEnabledAmazon
		{
			get
			{
				return m_pushNotificationsEnabledAmazon;
			}
			set
			{
				if (m_pushNotificationsEnabledAmazon != value)
				{
					m_pushNotificationsEnabledAmazon = value;
				}
			}
		}

		public bool PushNotificationsEnabledWindows
		{
			get
			{
				return m_pushNotificationsEnabledWindows;
			}
			set
			{
				if (m_pushNotificationsEnabledWindows != value)
				{
					m_pushNotificationsEnabledWindows = value;
				}
			}
		}

		public ShowNotifications AndroidShowNotificationsMode
		{
			get
			{
				return m_androidShowNotificationsMode;
			}
			set
			{
				if (m_androidShowNotificationsMode != value)
				{
					m_androidShowNotificationsMode = value;
				}
			}
		}

		public bool AndroidRestoreScheduledNotificationsAfterReboot => m_androidRestoreScheduledNotificationsAfterReboot;

		public NotificationsGroupingMode AndroidNotificationsGrouping
		{
			get
			{
				return m_androidNotificationsGrouping;
			}
			set
			{
				if (m_androidNotificationsGrouping != value)
				{
					m_androidNotificationsGrouping = value;
				}
			}
		}

		public GooglePlayUpdatingIfRequiredMode AllowUpdatingGooglePlayIfRequired
		{
			get
			{
				return m_allowUpdatingGooglePlayIfRequired;
			}
			set
			{
				if (m_allowUpdatingGooglePlayIfRequired != value)
				{
					m_allowUpdatingGooglePlayIfRequired = value;
				}
			}
		}

		public bool AndroidShowLatestNotificationOnly => m_androidShowLatestNotificationOnly;

		public string FirebaseSenderID => m_firebaseSenderID;

		public bool WindowsDontShowWhenRunning => m_windowsDontShowWhenRunning;
	}
}
