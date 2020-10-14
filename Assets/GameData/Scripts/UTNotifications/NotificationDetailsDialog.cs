using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UTNotifications
{
	public class NotificationDetailsDialog : MonoBehaviour
	{
		public Text DialogTitle;

		public Text ID;

		public Text Title;

		public Text Text;

		public Text Profile;

		public Text UserData;

		public Text Badge;

		private readonly List<ReceivedNotification> received = new List<ReceivedNotification>();

		private ReceivedNotification clicked;

		public ReceivedNotification Current
		{
			get
			{
				if (clicked != null)
				{
					return clicked;
				}
				if (received.Count > 0)
				{
					return received[0];
				}
				return null;
			}
		}

		public void OnReceived(ReceivedNotification received)
		{
			this.received.Add(received);
			if (clicked == null && this.received.Count == 1)
			{
				UpdateContents();
			}
		}

		public void OnClicked(ReceivedNotification clicked)
		{
			this.clicked = clicked;
			UpdateContents();
		}

		public void Hide()
		{
			ReceivedNotification current = Current;
			UTNotificationsSample.Instance.Hide(current.id);
		}

		public void Hide(int id)
		{
			if (clicked != null && clicked.id == id)
			{
				clicked = null;
			}
			received.RemoveAll((ReceivedNotification it) => it.id == id);
			UpdateContents();
		}

		public void Cancel()
		{
			ReceivedNotification current = Current;
			UTNotificationsSample.Instance.Cancel(current.id);
		}

		public void CancelAll()
		{
			clicked = null;
			received.Clear();
			UpdateContents();
		}

		private void Start()
		{
		}

		private void UpdateContents()
		{
			ReceivedNotification current = Current;
			if (current == null)
			{
				base.gameObject.SetActive(value: false);
				return;
			}
			DialogTitle.text = ((current == clicked) ? "Notification Clicked" : "Notification Received");
			ID.text = current.id.ToString();
			Title.text = current.title;
			Text.text = current.text;
			Profile.text = (current.notificationProfile ?? "");
			UserData.text = UserDataString(current.userData);
			Badge.text = ((current.badgeNumber != -1) ? current.badgeNumber.ToString() : "");
			base.gameObject.SetActive(value: true);
		}

		private string UserDataString(IDictionary<string, string> userData)
		{
			if (userData == null)
			{
				return "{}";
			}
			return JsonUtils.ToJson(userData).ToString();
		}
	}
}
