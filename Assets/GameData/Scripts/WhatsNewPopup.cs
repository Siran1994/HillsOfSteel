using UnityEngine.UI;

public class WhatsNewPopup : MenuBase<WhatsNewPopup>
{
	public Button okButton;

	private void Start()
	{
		okButton.onClick.AddListener(delegate
		{
			MenuController.HideMenu<WhatsNewPopup>();
		});
		if (VersionControl.CurrentVersionNumber == 220)
		{
			okButton.onClick.AddListener(delegate
			{
				MenuController.ShowMenu<SubscriptionOfferPopup>().Init("WhatsNewPopup");
			});
		}
	}
}
