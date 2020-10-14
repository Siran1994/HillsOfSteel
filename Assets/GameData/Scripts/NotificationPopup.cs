using UnityEngine.UI;

public class NotificationPopup : MenuBase<NotificationPopup>
{
	public Button registerForNotificationsButton;

	public Button dontRegisterForNotificationsButton;

	private void Start()
	{
		registerForNotificationsButton.onClick.AddListener(delegate
		{
			MenuController.HideMenu<NotificationPopup>();
			NotificationManager.RegisterForNotifications(val: true);
		});
		dontRegisterForNotificationsButton.onClick.AddListener(delegate
		{
			MenuController.HideMenu<NotificationPopup>();
			NotificationManager.RegisterForNotifications(val: false);
		});
	}
}
