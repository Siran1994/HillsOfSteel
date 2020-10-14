using UnityEngine.UI;

public class FacebookPopup : MenuBase<FacebookPopup>
{
	public ToggleButton button;

	public Button proceedButton;

	private void Start()
	{
		TankPrefs.SetInt("facebookAsked", 1);
		TankPrefs.Save();
		button.SetToggled(BackendManager.ConnectedWithFacebook);
		button.SetOnClick(ButtonState.Default, delegate
		{
			button.SetDisabled();
			BackendManager.ConnectWithFacebook(delegate
			{
				button.SetToggled(toggled: true);
			}, delegate
			{
				button.SetToggled(toggled: false);
			});
		});
		proceedButton.onClick.AddListener(delegate
		{
			MenuController.HideMenu<FacebookPopup>();
		});
	}
}
