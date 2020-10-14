using System;
using UnityEngine;
using UnityEngine.UI;

namespace UTNotifications
{
	public class CreateNotificationDialog : MonoBehaviour
	{
		public delegate void OnComplete(string title, string text, int id, string notificationProfile, int badge, bool hasImage, bool hasButtons);

		public Text DialogTitle;

		public Text Title;

		public Text Text;

		public Text ID;

		public Text NotificationProfile;

		public Text Badge;

		public Toggle HasImage;

		public Toggle HasButtons;

		private OnComplete onComplete;

		public void Show(string dialogTitle, bool showHasImage, bool showHasButtons, OnComplete onComplete)
		{
			if (base.gameObject.activeSelf)
			{
				throw new InvalidOperationException();
			}
			if (onComplete == null)
			{
				throw new ArgumentNullException("onComplete");
			}
			HasImage.gameObject.SetActive(showHasImage);
			HasButtons.gameObject.SetActive(showHasButtons);
			DialogTitle.text = dialogTitle;
			this.onComplete = onComplete;
			base.gameObject.SetActive(value: true);
		}

		public void OK()
		{
			onComplete(Title.text, Text.text, string.IsNullOrEmpty(ID.text) ? 1 : int.Parse(ID.text), NotificationProfile.text, string.IsNullOrEmpty(Badge.text) ? (-1) : int.Parse(Badge.text), HasImage.isOn, HasButtons.isOn);
			Cancel();
		}

		public void Cancel()
		{
			onComplete = null;
			base.gameObject.SetActive(value: false);
		}

		private void Start()
		{
		}
	}
}
