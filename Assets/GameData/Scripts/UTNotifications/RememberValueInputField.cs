using UnityEngine;
using UnityEngine.UI;

namespace UTNotifications
{
	[RequireComponent(typeof(InputField))]
	public class RememberValueInputField : MonoBehaviour
	{
		private InputField inputField;

		private string uniqueName;

		private void Awake()
		{
			inputField = GetComponent<InputField>();
			uniqueName = SampleUtils.UniqueName(base.transform);
			inputField.text = PlayerPrefs.GetString(uniqueName, inputField.text);
			inputField.onEndEdit.AddListener(OnEndEdit);
		}

		private void OnEndEdit(string value)
		{
			PlayerPrefs.SetString(uniqueName, value);
		}
	}
}
