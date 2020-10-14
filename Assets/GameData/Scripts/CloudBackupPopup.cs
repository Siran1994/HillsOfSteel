using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CloudBackupPopup : MenuBase<CloudBackupPopup>
{
	public GameObject cloudBackupMainContainer;

	public GameObject cloudBackupMainHeader;

	public GameObject cloudBackupConfirmationContainer;

	public GameObject cloudBackupConfirmationHeader;

	public GameObject cloudBackupLoadedContainer;

	public GameObject cloudBackupLoadedHeader;

	public TextMeshProUGUI cloudBackupDateText;

	public Button cloudBackupNoButton;

	public Button cloudBackupYesButton;

	public Button cloudBackupConfirmNoButton;

	public Button cloudBackupConfirmYesButton;

	public Button cloudBackupLoadedOkButton;

	private void OnEnable()
	{
		MenuController.HideMenu<CloudSavePopup>();
	}

	private void OnDisable()
	{
		Time.timeScale = 1f;
		TankPrefs.CloudSyncComplete = true;
	}

	public void Init(string cloudDate, Action useLocalSaveCallback, Action useCloudSaveCallback)
	{
		if (TankGame.instance != null && TankGame.Running)
		{
			Time.timeScale = 0f;
		}
		cloudBackupConfirmationHeader.SetActive(value: false);
		cloudBackupConfirmationContainer.SetActive(value: false);
		cloudBackupLoadedHeader.SetActive(value: false);
		cloudBackupLoadedContainer.SetActive(value: false);
		cloudBackupDateText.text = cloudDate;
		cloudBackupNoButton.onClick.RemoveAllListeners();
		cloudBackupNoButton.onClick.AddListener(delegate
		{
			SetActiveCloudBackupConfirmation(cloudDate, useLocalSaveCallback, useCloudSaveCallback);
		});
		cloudBackupYesButton.onClick.RemoveAllListeners();
		cloudBackupYesButton.onClick.AddListener(delegate
		{
			SetActiveCloudBackupLoaded(useCloudSaveCallback);
		});
		cloudBackupMainHeader.SetActive(value: true);
		cloudBackupMainContainer.SetActive(value: true);
	}

	private void SetActiveCloudBackupConfirmation(string cloudDate, Action useLocalSaveCallback, Action useCloudSaveCallback)
	{
		cloudBackupMainHeader.SetActive(value: false);
		cloudBackupMainContainer.SetActive(value: false);
		cloudBackupLoadedHeader.SetActive(value: false);
		cloudBackupLoadedContainer.SetActive(value: false);
		cloudBackupConfirmNoButton.onClick.RemoveAllListeners();
		cloudBackupConfirmNoButton.onClick.AddListener(delegate
		{
			Init(cloudDate, useLocalSaveCallback, useCloudSaveCallback);
		});
		cloudBackupConfirmYesButton.onClick.RemoveAllListeners();
		cloudBackupConfirmYesButton.onClick.AddListener(delegate
		{
			MenuController.HideMenu<CloudBackupPopup>();
			if (useLocalSaveCallback != null)
			{
				useLocalSaveCallback();
			}
		});
		cloudBackupConfirmationHeader.SetActive(value: true);
		cloudBackupConfirmationContainer.SetActive(value: true);
	}

	private void SetActiveCloudBackupLoaded(Action after)
	{
		cloudBackupMainHeader.SetActive(value: false);
		cloudBackupMainContainer.SetActive(value: false);
		cloudBackupConfirmationHeader.SetActive(value: false);
		cloudBackupConfirmationContainer.SetActive(value: false);
		after = (Action)Delegate.Combine(after, (Action)delegate
		{
			TankGame.Running = false;
			MenuController.HideMenu<CloudBackupPopup>();
			MenuController.HideMenu<PauseMenu>();
			MenuController.HideMenu<GameMenu>();
			MusicManager.CrossFadeToMenu();
		});
		cloudBackupLoadedOkButton.onClick.RemoveAllListeners();
		cloudBackupLoadedOkButton.onClick.AddListener(delegate
		{
			after();
		});
		TankPrefs.OverrideLocalWithCloudData();
		cloudBackupLoadedHeader.SetActive(value: true);
		cloudBackupLoadedContainer.SetActive(value: true);
	}
}
