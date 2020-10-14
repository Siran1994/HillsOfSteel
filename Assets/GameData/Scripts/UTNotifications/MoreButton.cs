using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UTNotifications
{
	[RequireComponent(typeof(UnityEngine.UI.Button))]
	public class MoreButton : MonoBehaviour
	{
		public struct PopupMenuItem
		{
			public readonly string label;

			public readonly UnityAction action;

			public PopupMenuItem(string label, UnityAction action)
			{
				this.label = label;
				this.action = action;
			}
		}

		public GameObject PopupPrefab;

		public PopupMenuItem[] MenuItems;

		private GameObject popup;

		public static MoreButton FindInstance()
		{
			return UnityEngine.Object.FindObjectOfType<MoreButton>();
		}

		private void Start()
		{
			GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnClick);
		}

		private void OnClick()
		{
			if (popup != null)
			{
				UnityEngine.Object.Destroy(popup);
				popup = null;
				return;
			}
			popup = UnityEngine.Object.Instantiate(PopupPrefab, GetComponentInParent<Canvas>().transform, worldPositionStays: false);
			Popup component = popup.GetComponent<Popup>();
			PopupMenuItem[] menuItems = MenuItems;
			foreach (PopupMenuItem popupMenuItem in menuItems)
			{
				component.AddItem(popupMenuItem.label, popupMenuItem.action);
			}
		}
	}
}
