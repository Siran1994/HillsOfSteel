using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CloudSavePopup : MenuBase<CloudSavePopup>
{
	public GameObject cloudSaveLoadingContainer;

	public GameObject cloudSaveLoadedContainer;

	public GameObject cloudSaveLoadedSuccessContainer;

	public TextMeshProUGUI localSaveDateText;

	public TextMeshProUGUI cloudSaveDateText;

	public Button cancelCloudSaveButton;

	public Button useLocalSaveButton;

	public Button useCloudSaveButton;

	public Button cloudSaveLoadedOkButton;

	private void OnEnable()
	{
		cancelCloudSaveButton.onClick.AddListener(delegate
		{
			MenuController.HideMenu<CloudSavePopup>();
		});
		if (BackendManager.IsAuthenticated)
		{
			cloudSaveLoadingContainer.SetActive(value: true);
			cloudSaveLoadedContainer.SetActive(value: false);
			cloudSaveLoadedSuccessContainer.SetActive(value: false);
			TankPrefs.GetCloudSave(delegate(bool result)
			{
				if (result)
				{
					SetActiveCloudSavePopup(TankPrefs.LocalFileTime.ToString(), TankPrefs.CloudFileTime.ToString(), null, TankPrefs.OverrideLocalWithCloudData);
				}
			});
		}
		else
		{
			cloudSaveLoadingContainer.SetActive(value: true);
			PlatformManager.ReconnectWithGooglePlay(delegate
			{
				cloudSaveLoadingContainer.SetActive(value: false);
				MenuController.HideMenu<CloudSavePopup>();
			});
		}
	}

	private void SetActiveCloudSavePopup(string localDate, string cloudDate, Action useLocalSaveCallback, Action useCloudSaveCallback)
	{
		cloudSaveLoadingContainer.SetActive(value: false);
		cloudSaveLoadedContainer.SetActive(value: true);
		cloudSaveLoadedSuccessContainer.SetActive(value: false);
		localSaveDateText.text = localDate;
		cloudSaveDateText.text = cloudDate;
		useLocalSaveButton.onClick.RemoveAllListeners();
		useLocalSaveButton.onClick.AddListener(delegate
		{
			if (useLocalSaveCallback != null)
			{
				useLocalSaveCallback();
			}
			MenuController.HideMenu<CloudSavePopup>();
		});
		useCloudSaveButton.onClick.RemoveAllListeners();
		useCloudSaveButton.onClick.AddListener(delegate
		{
			useCloudSaveCallback();
			SetActiveCloudSaveLoadedSuccessPopup();
			MenuController.UpdateTopMenu();
			MenuController.GetMenu<MainMenu>().UpdatePlayMenu();
		});
	}

	public static void SetActiveCloudSaveLoadedSuccessPopup()
	{
		MenuBase<CloudSavePopup>.instance.cloudSaveLoadedContainer.SetActive(value: false);
		MenuBase<CloudSavePopup>.instance.cloudSaveLoadingContainer.SetActive(value: false);
		MenuBase<CloudSavePopup>.instance.cloudSaveLoadedSuccessContainer.SetActive(value: true);
		MenuBase<CloudSavePopup>.instance.cloudSaveLoadedOkButton.onClick.RemoveAllListeners();
		MenuBase<CloudSavePopup>.instance.cloudSaveLoadedOkButton.onClick.AddListener(delegate
		{
			MenuController.HideMenu<CloudSavePopup>();
		});
	}
}
