using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace UTNotifications
{
	[RequireComponent(typeof(InputField))]
	public class ValidatedInputField : MonoBehaviour
	{
		public string RequiredPattern;

		private Regex regex;

		private InputField inputField;

		private GameObject incorrect;

		public string text
		{
			get
			{
				return inputField.text;
			}
			set
			{
				inputField.text = value;
			}
		}

		public bool IsValid()
		{
			return regex.IsMatch(inputField.text);
		}

		private void Awake()
		{
			regex = new Regex(RequiredPattern);
			inputField = GetComponent<InputField>();
			incorrect = base.transform.Find("Incorrect").gameObject;
			inputField.onValueChanged.AddListener(OnValueChanged);
		}

		private void Start()
		{
			OnValueChanged(inputField.text);
		}

		private void OnValueChanged(string value)
		{
			incorrect.SetActive(!IsValid());
		}
	}
}
