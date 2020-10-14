using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NameChangePopup : MenuBase<NameChangePopup>
{
	public TMP_InputField nameChangeField;

	public TextMeshProUGUI nameChangePlaceholder;

	public Button nameChangeOkButton;

	public Button nameChangeOkButtonDisabled;

	public Button nameChangeBackButton;

	public int nameChangeLengthMin = 3;

	public int nameChangeLengthMax = 16;

	private void OnEnable()
	{
		string @string = TankPrefs.GetString("challengeName", Social.localUser.userName);
		if (@string != "")
		{
			nameChangePlaceholder.text = @string;
			nameChangeField.text = @string;
		}
		nameChangeOkButton.onClick.RemoveAllListeners();
		nameChangeOkButton.onClick.AddListener(delegate
		{
			string text = Utilities.SanitizeInput(nameChangeField.text);
			if (text != "" && text.Length >= nameChangeLengthMin && text.Length <= nameChangeLengthMax)
			{
				TankPrefs.SetString("challengeName", text);
			}
			MenuController.HideMenu<NameChangePopup>();
			TankPrefs.Save();
			PlayerDataManager.SaveToCloudOnNextInterval = true;
		});
	}

	private void Start()
	{
		nameChangeField.onValueChanged.AddListener(delegate(string input)
		{
			OnNameChangeInputChanged(input);
		});
	}

	private void OnNameChangeInputChanged(string input)
	{
		string text = Utilities.SanitizeInput(input);
		MenuBase<NameChangePopup>.instance.nameChangeField.text = text;
		bool flag = text.Length >= nameChangeLengthMin && text.Length <= nameChangeLengthMax;
		nameChangeOkButton.gameObject.SetActive(flag);
		nameChangeOkButtonDisabled.gameObject.SetActive(!flag);
	}
}
