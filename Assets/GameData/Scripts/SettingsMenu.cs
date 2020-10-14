using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MenuBase<SettingsMenu>
{
	public Sprite audioOnIcon;

	public Sprite audioOffIcon;

	public Button soundToggleButton;

	public Button languageButton;

	public Button restorePurchasesButton;

	public ToggleButton googlePlayButton;

	public ToggleButton facebookButton;

	public Button getCloudSaveButton;

	public Button nameChangeButton;

	public Image soundToggleImage;

	private void OnEnable()
	{
		googlePlayButton.SetToggled(Social.localUser.authenticated);
		facebookButton.SetToggled(BackendManager.ConnectedWithFacebook);
	}

	private void Start()
	{
		InitAudio();
		restorePurchasesButton.onClick.AddListener(delegate
		{
			IAPManager.RestorePurchases();
		});
		soundToggleButton.onClick.AddListener(SoundToggle);
		facebookButton.SetOnClick(ButtonState.Default, delegate
		{
			NeedToBeOnlinePopup.OnlineAction(ToggleFacebook);
		});
		facebookButton.SetOnClick(ButtonState.Toggled, delegate
		{
			NeedToBeOnlinePopup.OnlineAction(ToggleFacebook);
		});
		nameChangeButton.onClick.AddListener(delegate
		{
			MenuController.ShowMenu<NameChangePopup>();
		});
		getCloudSaveButton.onClick.AddListener(delegate
		{
			NeedToBeOnlinePopup.OnlineAction(delegate
			{
				MenuController.ShowMenu<CloudSavePopup>();
			});
		});
		googlePlayButton.SetOnClick(ButtonState.Default, delegate
		{
			NeedToBeOnlinePopup.OnlineAction(HandleGooglePlaySigning);
		});
		googlePlayButton.SetOnClick(ButtonState.Toggled, delegate
		{
			NeedToBeOnlinePopup.OnlineAction(HandleGooglePlaySigning);
		});
		languageButton.onClick.AddListener(delegate
		{
			MenuController.ShowMenu<LanguageMenuPopup>();
		});
	}

	private void HandleGooglePlaySigning()
	{
		if (Social.localUser.authenticated)
		{
			PlayerDataManager.DeauthenticateGooglePlay();
			googlePlayButton.SetToggled(toggled: false);
		}
		else
		{
			googlePlayButton.SetDisabled();
			PlatformManager.ReconnectWithGooglePlay(delegate
			{
				googlePlayButton.SetToggled(toggled: true);
			}, delegate
			{
				googlePlayButton.SetToggled(toggled: false);
			});
			MenuController.GetMenu<ShopMenu>().ResetOfferItems();
		}
	}

	private void ToggleFacebook()
	{
		if (BackendManager.ConnectedWithFacebook)
		{
			BackendManager.DisconnectWithFacebook();
			List<ArenaPlayerHUD> list = new List<ArenaPlayerHUD>();
			ArenaPlayerHUD[] arenaPlayerHUDs = MenuController.GetMenu<GameMenu>().arenaPlayerHUDs;
			if (arenaPlayerHUDs != null && arenaPlayerHUDs.Length != 0)
			{
				list.AddRange(arenaPlayerHUDs);
			}
			arenaPlayerHUDs = MenuController.GetMenu<GameMenu>().arenaMultiplayerHUDs;
			if (arenaPlayerHUDs != null && arenaPlayerHUDs.Length != 0)
			{
				list.AddRange(arenaPlayerHUDs);
			}
			foreach (ArenaPlayerHUD item in list)
			{
				if ((bool)item.profilePicture)
				{
					item.profilePicture.texture = null;
				}
			}
			facebookButton.SetToggled(toggled: false);
		}
		else
		{
			facebookButton.SetDisabled();
			BackendManager.ConnectWithFacebook(delegate
			{
				facebookButton.SetToggled(toggled: true);
			}, delegate
			{
				facebookButton.SetToggled(toggled: false);
			});
		}
	}

	public void InitAudio()
	{
		if (PlayerPrefs.GetInt("audioOn") == 1)
		{
			SetSoundOn();
		}
		else
		{
			SetSoundOff();
		}
	}

	private void SetSoundOn()
	{
		AudioManager.SetSoundOn();
		soundToggleImage.sprite = audioOnIcon;
	}

	private void SetSoundOff()
	{
		AudioManager.SetSoundOff();
		soundToggleImage.sprite = audioOffIcon;
	}

	private void SoundToggle()
	{
		if (AudioListener.volume == 0f)
		{
			SetSoundOn();
		}
		else
		{
			SetSoundOff();
		}
	}
}
