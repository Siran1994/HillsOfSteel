using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SubscriptionManagePopup : MenuBase<SubscriptionManagePopup>
{
	public Button linkButton;

	public string googlePlayLink = "https://play.google.com/store/account/subscriptions";

	public string appStoreLink = "https://buy.itunes.apple.com/WebObjects/MZFinance.woa/wa/manageSubscriptions";

	public TextMeshProUGUI statusText;

	private void Start()
	{
		linkButton.onClick.AddListener(delegate
		{
			Application.OpenURL(googlePlayLink);
		});
	}

	private void OnEnable()
	{
        //statusText.GetComponent<LocalizationParamsManager>().SetParameterValue("STATUS", ScriptLocalization.Get("Active"));
        statusText.text = "¹ºÂò×´Ì¬:<color=yellow>ÒÑ¹ºÂò";

    }
}
