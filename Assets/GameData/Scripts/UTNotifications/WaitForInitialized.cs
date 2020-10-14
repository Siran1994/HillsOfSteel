using UnityEngine;

namespace UTNotifications
{
	public class WaitForInitialized : MonoBehaviour
	{
		private void Start()
		{
			if (!Manager.Instance.Initialized)
			{
				Manager.Instance.OnInitialized += OnInitialized;
				base.gameObject.SetActive(value: false);
			}
		}

		private void OnDestroy()
		{
			if (Manager.Instance != null)
			{
				Manager.Instance.OnInitialized -= OnInitialized;
			}
		}

		private void OnInitialized()
		{
			base.gameObject.SetActive(value: true);
			Manager.Instance.OnInitialized -= OnInitialized;
		}
	}
}
