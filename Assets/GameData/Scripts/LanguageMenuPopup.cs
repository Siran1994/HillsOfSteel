using I2.Loc;
using UnityEngine.UI;

public class LanguageMenuPopup : MenuBase<LanguageMenuPopup>
{
	public Button languageSelectionBackButton;

	public SetLanguage[] languageSetButtons;

	private void Awake()
	{
		for (int i = 0; i < MenuBase<LanguageMenuPopup>.instance.languageSetButtons.Length; i++)
		{
			languageSetButtons[i].GetComponent<Button>().onClick.AddListener(delegate
			{
				MenuController.HideMenu<LanguageMenuPopup>();
			});
		}
	}

	private void OnEnable()
	{
		for (int i = 0; i < MenuBase<LanguageMenuPopup>.instance.languageSetButtons.Length; i++)
		{
			languageSetButtons[i].GetComponent<Button>().interactable = !MenuBase<LanguageMenuPopup>.instance.languageSetButtons[i]._Language.Equals(LocalizationManager.CurrentLanguage);
		}
	}
}
