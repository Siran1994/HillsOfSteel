using UnityEngine;
using UnityEngine.UI;

namespace UTNotifications
{
	[RequireComponent(typeof(UnityEngine.UI.Button))]
	public class ButtonHelper : MonoBehaviour
	{
		public Color DisabledColor = new Color(0.462f, 0.482f, 0.494f);

		private UnityEngine.UI.Button button;

		private Text text;

		private bool lastInteractable = true;

		private Color initialColor;

		private void Start()
		{
			button = GetComponent<UnityEngine.UI.Button>();
			text = base.transform.GetComponentInChildren<Text>();
			initialColor = text.color;
		}

		private void Update()
		{
			if (lastInteractable != button.interactable)
			{
				lastInteractable = button.interactable;
				text.color = (lastInteractable ? initialColor : DisabledColor);
			}
		}
	}
}
