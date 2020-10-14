using UnityEngine;
using UnityEngine.UI;

public class QuitDialogPopup : MenuBase<QuitDialogPopup>
{
	public Button quitButton;

	public Button quitBackButton;

	private void Start()
	{
		quitButton.onClick.AddListener(delegate
		{
			Application.Quit();
		});
		quitBackButton.onClick.AddListener(delegate
		{
			MenuController.HideMenu<QuitDialogPopup>();
		});
	}
}
