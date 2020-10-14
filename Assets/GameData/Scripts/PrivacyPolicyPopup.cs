using UnityEngine;
using UnityEngine.UI;

public class PrivacyPolicyPopup : MenuBase<PrivacyPolicyPopup>
{
	//public Button privacyPolicyLinkButton;

	//public Button privacyPolicyOkButton;

	private void Start()
	{
		//privacyPolicyLinkButton.onClick.AddListener(delegate
		//{
		//	Application.OpenURL("https://round-zero.com/privacy_policy.html");
		//});
		//privacyPolicyOkButton.onClick.AddListener(delegate
		//{
		//	MenuController.HideMenu<PrivacyPolicyPopup>();
		//});
	}

	private void OnEnable()
	{
		Time.timeScale = 0f;
	}

	private void OnDisable()
	{
		TankPrefs.SetInt("privacyPolicyAgreed", 1);
		Time.timeScale = 1f;
	}

	private void Update()
	{
		Time.timeScale = 0f;
	}
}
