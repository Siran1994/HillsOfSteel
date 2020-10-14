using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UTNotifications
{
	public class Popup : MonoBehaviour
	{
		public GameObject ItemPrefab;

		public void AddItem(string label, UnityAction action)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(ItemPrefab, base.transform, worldPositionStays: false);
			gameObject.GetComponentInChildren<Text>().text = label;
			gameObject.GetComponentInChildren<UnityEngine.UI.Button>().onClick.AddListener(action);
		}
	}
}
