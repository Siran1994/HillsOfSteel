using UnityEngine;
using UnityEngine.UI;

namespace UTNotifications
{
	[RequireComponent(typeof(Toggle))]
	public class RememberValueToggle : MonoBehaviour
	{
		private Toggle toggle;

		private string uniqueName;

		private void Awake()
		{
			toggle = GetComponent<Toggle>();
			uniqueName = SampleUtils.UniqueName(base.transform);
			toggle.isOn = ((PlayerPrefs.GetInt(uniqueName, toggle.isOn ? 1 : 0) != 0) ? true : false);
			toggle.onValueChanged.AddListener(OnValueChanged);
		}

		private void OnValueChanged(bool value)
		{
			PlayerPrefs.SetInt(uniqueName, value ? 1 : 0);
		}
	}
}
