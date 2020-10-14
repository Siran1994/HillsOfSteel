using UnityEngine;
using UnityEngine.UI;

public class InfoButton : MonoBehaviour
{
	public Button button;

	public GameObject speechBubble;

	private bool open;

	private bool buttonClicked;

	private void Start()
	{
		button.onClick.AddListener(delegate
		{
			MenuController.ShowMenu<InfoPopup>();
		});
	}

	private void OnEnable()
	{
		MenuController.HideMenu<MainMenu>();
	}
}
