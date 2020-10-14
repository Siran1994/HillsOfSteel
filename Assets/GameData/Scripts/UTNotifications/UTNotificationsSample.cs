using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0618
namespace UTNotifications
{
	public class UTNotificationsSample : MonoBehaviour
	{
		public ValidatedInputField DemoServerURLInputField;

		public Text NotifyAllText;

		public Text InitializeText;

		public Toggle NotificationsEnabledToggle;

		public CreateNotificationDialog CreateNotificationDialog;

		public NotificationDetailsDialog NotificationDetailsDialog;

		private static UTNotificationsSample instance;

		private string notifyAllTextOriginal;

		private string initializeTextOriginal;

		public static UTNotificationsSample Instance => instance;

		public void Initialize()
		{
			bool flag = Manager.Instance.Initialize(willHandleReceivedNotifications: true);
			UnityEngine.Debug.Log("UTNotifications Initialize: " + flag.ToString());
			if (flag)
			{
				Manager.Instance.SetBadge(0);
			}
		}

		public void NotifyAll()
		{
			CreateNotificationDialog.Show("Push Notify All Registered Devices", showHasImage: false, showHasButtons: false, delegate(string title, string text, int id, string notificationProfile, int badge, bool hasImage, bool hasButtons)
			{
				StartCoroutine(NotifyAll(title, text, id, notificationProfile, badge));
			});
		}

		public IEnumerator NotifyAll(string title, string text, int id, string notificationProfile, int badgeNumber)
		{
			title = WWW.EscapeURL(title);
			text = WWW.EscapeURL(text);
			string text2 = "&_NO_CACHE=" + UnityEngine.Random.value;
			WWW www = new WWW(string.Format("{0}/notify?title={1}&text={2}&id={3}&badge={4}{5}{6}", DemoServerURLInputField.text, title, text, id, badgeNumber, string.IsNullOrEmpty(notificationProfile) ? "" : ("&notification_profile=" + notificationProfile), text2));
			int dots = 0;
			do
			{
				string text3 = notifyAllTextOriginal + "\nSending";
				dots = (dots + 1) % 4;
				for (int i = 0; i < dots; i++)
				{
					text3 += ".";
				}
				NotifyAllText.text = text3;
				yield return new WaitForSeconds(0.15f);
			}
			while (!www.isDone && string.IsNullOrEmpty(www.error));
			if (www.error != null)
			{
				NotifyAllText.text = notifyAllTextOriginal + "\n" + www.error + " " + www.text;
			}
			else
			{
				NotifyAllText.text = notifyAllTextOriginal + "\n" + www.text;
			}
		}

		public void CreateLocalNotification()
		{
			CreateNotificationDialog.Show("Create Local Notification", showHasImage: true, showHasButtons: true, delegate(string title, string text, int id, string notificationProfile, int badge, bool hasImage, bool hasButtons)
			{
				Manager.Instance.PostLocalNotification(title, text, id, UserData(hasImage), notificationProfile, badge, Buttons(hasButtons));
			});
		}

		public void ScheduleLocalNotification()
		{
			CreateNotificationDialog.Show("Schedule Local Notification", showHasImage: true, showHasButtons: true, delegate(string title, string text, int id, string notificationProfile, int badge, bool hasImage, bool hasButtons)
			{
				Manager.Instance.ScheduleNotification(30, title, text, id, UserData(hasImage), notificationProfile, badge, Buttons(hasButtons));
			});
		}

		public void ScheduleRepeatingLocalNotification()
		{
			CreateNotificationDialog.Show("Schedule Local Notification", showHasImage: true, showHasButtons: true, delegate(string title, string text, int id, string notificationProfile, int badge, bool hasImage, bool hasButtons)
			{
				Manager.Instance.ScheduleNotificationRepeating(DateTime.Now.AddSeconds(10.0), 25, title, text, id, UserData(hasImage), notificationProfile, badge, Buttons(hasButtons));
			});
		}

		public void Hide(int id)
		{
			Manager.Instance.HideNotification(id);
			NotificationDetailsDialog.Hide(id);
		}

		public void Cancel(int id)
		{
			Manager.Instance.CancelNotification(id);
			NotificationDetailsDialog.Hide(id);
		}

		public void CancelAll()
		{
			Manager.Instance.CancelAllNotifications();
			Manager.Instance.SetBadge(0);
			NotificationDetailsDialog.CancelAll();
		}

		public void IncrementBadge()
		{
			Manager.Instance.SetBadge(Manager.Instance.GetBadge() + 1);
		}

		public void OnNotificationsEnabledToggleValueChanged(bool value)
		{
			if (value != Manager.Instance.NotificationsEnabled())
			{
				Manager.Instance.SetNotificationsEnabled(value);
			}
		}

		protected Dictionary<string, string> UserData(bool hasImage)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("user", "data");
			if (hasImage)
			{
				dictionary.Add("image_url", "http://thecatapi.com/api/images/get?format=src&type=png&size=med");
			}
			return dictionary;
		}

		protected List<Button> Buttons(bool hasButtons)
		{
			if (!hasButtons)
			{
				return null;
			}
			return new List<Button>
			{
				new Button("Open App", new Dictionary<string, string>
				{
					{
						"button",
						"first"
					}
				}),
				new Button("Open URL", new Dictionary<string, string>
				{
					{
						"open_url",
						"https://assetstore.unity.com/packages/tools/utnotifications-professional-local-push-notification-plugin-37767"
					},
					{
						"button",
						"second"
					}
				})
			};
		}

		protected void SendRegistrationId(string providerName, string registrationId)
		{
			string userId = SampleUtils.GenerateDeviceUniqueIdentifier();
			StartCoroutine(SendRegistrationId(userId, providerName, registrationId));
		}

		protected IEnumerator SendRegistrationId(string userId, string providerName, string registrationId)
		{
			WWWForm wWWForm = new WWWForm();
			wWWForm.AddField("uid", userId);
			wWWForm.AddField("provider", providerName);
			wWWForm.AddField("id", registrationId);
			WWW www = new WWW(DemoServerURLInputField.text + "/register", wWWForm);
			int dots = 0;
			do
			{
				string text = initializeTextOriginal + "\nSending registrationId";
				dots = (dots + 1) % 4;
				for (int i = 0; i < dots; i++)
				{
					text += ".";
				}
				InitializeText.text = text;
				yield return new WaitForSeconds(0.15f);
			}
			while (!www.isDone && string.IsNullOrEmpty(www.error));
			if (!string.IsNullOrEmpty(www.error))
			{
				InitializeText.text = initializeTextOriginal + "\n" + www.error + " " + www.text;
			}
			else
			{
				InitializeText.text = initializeTextOriginal + "\n" + www.text;
			}
		}

		protected void OnNotificationClicked(ReceivedNotification notification)
		{
			NotificationDetailsDialog.OnClicked(notification);
		}

		protected void OnNotificationsReceived(IList<ReceivedNotification> receivedNotifications)
		{
			foreach (ReceivedNotification receivedNotification in receivedNotifications)
			{
				NotificationDetailsDialog.OnReceived(receivedNotification);
			}
		}

		private void Awake()
		{
			if (instance != null)
			{
				throw new UnityException("Creating the second instance of UTNotificationsSample...");
			}
			instance = this;
		}

		private void Start()
		{
			MoreButton moreButton = MoreButton.FindInstance();
			if (moreButton != null)
			{
				moreButton.MenuItems = new MoreButton.PopupMenuItem[1]
				{
					new MoreButton.PopupMenuItem("EXIT", delegate
					{
						Application.Quit();
					})
				};
			}
			notifyAllTextOriginal = NotifyAllText.text;
			initializeTextOriginal = InitializeText.text;
			NotificationsEnabledToggle.onValueChanged.AddListener(OnNotificationsEnabledToggleValueChanged);
			Manager manager = Manager.Instance;
			manager.OnSendRegistrationId += SendRegistrationId;
			manager.OnNotificationClicked += OnNotificationClicked;
			manager.OnNotificationsReceived += OnNotificationsReceived;
			if (DemoServerURLInputField.IsValid())
			{
				Initialize();
			}
		}

		private void Update()
		{
			NotificationsEnabledToggle.isOn = Manager.Instance.NotificationsEnabled();
			if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
			{
				Application.Quit();
			}
		}

		private void OnDestroy()
		{
			Manager manager = Manager.Instance;
			if (manager != null)
			{
				manager.OnSendRegistrationId -= SendRegistrationId;
				manager.OnNotificationClicked -= OnNotificationClicked;
				manager.OnNotificationsReceived -= OnNotificationsReceived;
			}
			if (instance == this)
			{
				instance = null;
			}
		}
	}
}
