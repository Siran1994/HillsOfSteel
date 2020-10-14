using UnityEngine;
using UnityEngine.UI;

namespace UTNotifications
{
	[RequireComponent(typeof(UnityEngine.UI.Button))]
	public class ValidatedInputDependent : MonoBehaviour
	{
		public bool AllowWhenPushDisabled;

		public ValidatedInputField[] ValidatedInputFields;

		private UnityEngine.UI.Button button;

		private void Start()
		{
			if (AllowWhenPushDisabled && !PushNotificationsEnabled())
			{
				base.enabled = false;
			}
			else
			{
				button = GetComponent<UnityEngine.UI.Button>();
			}
		}

		private void Update()
		{
			bool interactable = true;
			ValidatedInputField[] validatedInputFields = ValidatedInputFields;
			for (int i = 0; i < validatedInputFields.Length; i++)
			{
				if (!validatedInputFields[i].IsValid())
				{
					interactable = false;
					break;
				}
			}
			button.interactable = interactable;
		}

		private bool PushNotificationsEnabled()
		{
			if (!Settings.Instance.PushNotificationsEnabledFirebase)
			{
				return Settings.Instance.PushNotificationsEnabledAmazon;
			}
			return true;
		}
	}
}
