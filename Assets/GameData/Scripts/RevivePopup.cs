using TMPro;
using UnityEngine.UI;

public class RevivePopup : MenuBase<RevivePopup>
{
	public Button reviveCoinsButton;

	public TextMeshProUGUI reviveCoinsText;

	public Button reviveAdsButton;

	public Button reviveBackButton;

	public TextMeshProUGUI reviveSecondsText;

	private void OnEnable()
	{
		MenuBase<RevivePopup>.instance = this;
		reviveCoinsText.text = Variables.instance.reviveCost.ToString();
		//reviveAdsButton.gameObject.SetActive(AdsManager.VideoAdAvailable(VideoAdPlacement.Revive));
	}

	private void OnDisable()
	{
		if (TankGame.instance != null)
		{
			TankGame.instance.ReviveTried = true;
		}
	}
}
