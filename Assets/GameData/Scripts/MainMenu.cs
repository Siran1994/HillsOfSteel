using I2.Loc;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MenuBase<MainMenu>
{
	private struct RectOpacityContainer
	{
		public RectTransform rect;

		public Image[] images;

		public TextMeshProUGUI text;
	}

	[Header("Play Menu Components")]
	public GameObject playMenuContainer;

	//public GameModeButton play1V1Button;

	//public GameModeButton play2V2Button;

	public GameModeButton playAdventureButton;//冒险模式开关

	public GameModeButton classicModeButton;//经典模式开关

	public GameObject[] adventureThemeLogos;

	public TextMeshProUGUI adventureStageText;//冒险模式的关卡提示

	public ProgressChestButton pvpChest;

	public ProgressChestButton adventureChest;

	private int[] starsToAddPerGameMode;

	private Coroutine adventureMapChangeRoutine;

	private Coroutine cameraShakeRoutine;

	private Vector2 cameraShake;

	private int adventureLogoLevel;

	private void Awake()
	{
		MenuBase<MainMenu>.instance = this;
		starsToAddPerGameMode = new int[5];
		InitPlayMenu();
	}

	private void OnEnable()
	{
		bool flag = HaveStarsToAdd();
		int num = (PlayerDataManager.GetSelectedLevel(GameMode.Adventure) == 0) ? (adventureThemeLogos.Length - 1) : (PlayerDataManager.GetSelectedLevel(GameMode.Adventure) - 1);
		adventureLogoLevel = (flag ? num : PlayerDataManager.GetSelectedLevel(GameMode.Adventure));
		for (int i = 0; i < adventureThemeLogos.Length; i++)
		{
			adventureThemeLogos[i].SetActive(i == adventureLogoLevel);
		}
		if (!flag)
		{
			playAdventureButton.scoreText.transform.parent.gameObject.SetActive(value: true);
			adventureStageText.gameObject.SetActive(value: true);
			adventureThemeLogos[adventureLogoLevel].SetActive(value: true);
		}
		adventureStageText.GetComponent<LocalizationParamsManager>().SetParameterValue("STAGE", (PlayerDataManager.GetCurrentStage() + 1).ToString());
        adventureStageText.text = "第" + (PlayerDataManager.GetCurrentStage() + 1).ToString() + "章";


        UpdatePlayMenu(!flag);
		StartCoroutine(AddStars());
		StartCoroutine(CameraShakeRoutine());
		MenuController.backButtonFallbackAction = (MenuController.OnBackButtonPress)Delegate.Combine(MenuController.backButtonFallbackAction, new MenuController.OnBackButtonPress(ShowQuitDialog));
		MenuController.UpdateTopMenu();
		if (PlayerDataManager.GamesThisSession > 0 && !NotificationManager.NotificationRegistrationAsked())
		{
			NotificationManager.TryFullInit();
		}
		PlayerDataManager.SaveToCloudOnNextInterval = true;
		if (!PlayerDataManager.HasActiveDailyOffer())
		{
			PlayerDataManager.SetDailyOfferTime();
			PlayerDataManager.SetDailyOffers(PlayerDataManager.GenerateDailyOffers());
		}
	}

	private void OnDisable()
	{
		GameModeButton[] array = new GameModeButton[1]
		{
			//play1V1Button,
			//play2V2Button,
			playAdventureButton
		};
		foreach (GameModeButton obj in array)
		{
			obj.glow.enabled = false;
			obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -300f);
		}
		AnimatedCurrencyController.CancelAllAnimations();
		starsToAddPerGameMode = new int[5];
		UpdatePlayMenu();
		MenuController.backButtonFallbackAction = (MenuController.OnBackButtonPress)Delegate.Remove(MenuController.backButtonFallbackAction, new MenuController.OnBackButtonPress(ShowQuitDialog));
	}

	private void Start()
	{
		MenuController.GetMenu<SettingsMenu>().InitAudio();
		MusicManager.CrossFadeToMenu();
		UpdatePlayMenu();
		VersionControl.Process();
	}

	private void InitPlayMenu()
	{
		//play1V1Button.button.onClick.AddListener(delegate
		//{
		//	NeedToBeOnlinePopup.OnlineAction(delegate
		//	{
		//		PlayerDataManager.FetchCountryCode();
		//		if (Social.localUser.authenticated)
		//		{
		//			PlayerDataManager.SelectedGameMode = GameMode.Arena;
		//			PlayerDataManager.ArenaMultiplayerAICount = 1;
		//			BackendManager.GetChallenge(PlayerDataManager.GetRating());
		//			MenuController.HideMenu<MainMenu>();
		//			MenuController.ShowMenu<GarageMenu>();
		//		}
		//		else
		//		{
		//			PlatformManager.ReconnectWithGooglePlay();
		//		}
		//	});
		//});
		//play2V2Button.button.onClick.AddListener(delegate
		//{
		//	NeedToBeOnlinePopup.OnlineAction(delegate
		//	{
		//		PlayerDataManager.FetchCountryCode();
		//		if (Social.localUser.authenticated)
		//		{
		//			PlayerDataManager.SelectedGameMode = GameMode.Arena2v2;
		//			PlayerDataManager.ArenaMultiplayerAICount = 3;
		//			BackendManager.GetChallenges(PlayerDataManager.GetRating(GameMode.Arena2v2), PlayerDataManager.ArenaMultiplayerAICount);
		//			MenuController.HideMenu<MainMenu>();
		//			MenuController.ShowMenu<GarageMenu>();
		//		}
		//		else
		//		{
		//			PlatformManager.ReconnectWithGooglePlay();
		//		}
		//	});
		//});
		classicModeButton.button.onClick.AddListener(delegate
		{
			MenuController.HideMenu<MainMenu>();
			MenuController.ShowMenu<ClassicModeMenu>();
		});
		playAdventureButton.button.onClick.AddListener(delegate
		{
            SDKManager.Instance.ShowAd(ShowAdType.ChaPing, 1, "点击冒险模式");
            PlayerDataManager.SelectedGameMode = GameMode.Adventure;
			MenuController.HideMenu<MainMenu>();
			MenuController.ShowMenu<GarageMenu>();
		});
		adventureChest.button.onClick.AddListener(delegate
		{
			if (PlayerDataManager.IsReadyToOpenChest(ChestProgressionType.Adventure))
			{
				Rarity nextChestRarity2 = PlayerDataManager.GetNextChestRarity(ChestProgressionType.Adventure);
				PlayerDataManager.ChestOpened(ChestProgressionType.Adventure);
				MenuController.ShowMenu<ChestPopup>().OpenChest(nextChestRarity2);
				UpdatePlayMenu();
			}
		});
		pvpChest.button.onClick.AddListener(delegate
		{
			if (PlayerDataManager.IsReadyToOpenChest(ChestProgressionType.Pvp))
			{
				Rarity nextChestRarity = PlayerDataManager.GetNextChestRarity(ChestProgressionType.Pvp);
				PlayerDataManager.ChestOpened(ChestProgressionType.Pvp);
				MenuController.ShowMenu<ChestPopup>().OpenChest(nextChestRarity);
				UpdatePlayMenu();
			}
		});
	}

	public void UpdatePlayMenu(bool updateChests = true)
	{
		//play1V1Button.scoreText.text = PlayerDataManager.GetRating().ToString();
		//play2V2Button.scoreText.text = PlayerDataManager.GetRating(GameMode.Arena2v2).ToString();
		playAdventureButton.scoreText.text = PlayerDataManager.GetLeaderboardScore(LeaderboardID.Adventure).ToString();
		if (!HaveStarsToAdd())
		{
			for (int i = 0; i < adventureThemeLogos.Length; i++)
			{
				adventureThemeLogos[i].SetActive(i == PlayerDataManager.GetSelectedLevel(GameMode.Adventure));
				adventureThemeLogos[i].GetComponent<RectTransform>().localScale = Vector3.one;
				adventureThemeLogos[i].GetComponent<TextMeshProUGUI>().color = Color.white;
			}
			RectOpacityContainer[] adventureButtonElements = GetAdventureButtonElements();
			for (int j = 1; j != adventureButtonElements.Length; j++)
			{
				RectOpacityContainer rectOpacityContainer = adventureButtonElements[j];
				rectOpacityContainer.rect.localScale = Vector3.one;
				if (rectOpacityContainer.text != null)
				{
					rectOpacityContainer.text.color = Color.white;
				}
				if (rectOpacityContainer.images != null && rectOpacityContainer.images.Length != 0)
				{
					Image[] images = rectOpacityContainer.images;
					for (int k = 0; k < images.Length; k++)
					{
						images[k].color = Color.white;
					}
				}
			}
		}
		if (updateChests)
		{
			pvpChest.button.image.sprite = PlayerDataManager.GetNextChest(ChestProgressionType.Pvp).sprite;
			int chestProgress = PlayerDataManager.GetChestProgress(ChestProgressionType.Pvp);
			int chestPointsNeeded = Variables.instance.GetChestPointsNeeded(ChestProgressionType.Pvp);
			pvpChest.progressText.Init(chestProgress, chestPointsNeeded);
			pvpChest.SetProgress((float)chestProgress / (float)chestPointsNeeded);
			adventureChest.button.image.sprite = PlayerDataManager.GetNextChest(ChestProgressionType.Adventure).sprite;
			chestProgress = PlayerDataManager.GetChestProgress(ChestProgressionType.Adventure);
			chestPointsNeeded = Variables.instance.GetChestPointsNeeded(ChestProgressionType.Adventure);
			adventureChest.progressText.Init(chestProgress, chestPointsNeeded);
			adventureChest.SetProgress((float)chestProgress / (float)chestPointsNeeded);
		}
	}

	public void AddStarsFromGameMode(GameMode mode, int count)
	{
		starsToAddPerGameMode[(int)mode] += count;
	}

	public bool HaveStarsToAdd()
	{
		for (int i = 0; i != starsToAddPerGameMode.Length; i++)
		{
			if (starsToAddPerGameMode[i] > 0)
			{
				return true;
			}
		}
		return false;
	}

	private RectOpacityContainer[] GetAdventureButtonElements(int level = -1)
	{
		if (level < 0)
		{
			level = PlayerDataManager.GetSelectedLevel(GameMode.Adventure);
		}
		return new RectOpacityContainer[3]
		{
			new RectOpacityContainer
			{
				rect = adventureThemeLogos[level].GetComponent<RectTransform>(),
				text = adventureThemeLogos[level].GetComponent<TextMeshProUGUI>()
			},
			new RectOpacityContainer
			{
				rect = adventureStageText.rectTransform,
				text = adventureStageText
			},
			new RectOpacityContainer
			{
				rect = playAdventureButton.scoreText.transform.parent.GetComponent<RectTransform>(),
				text = playAdventureButton.scoreText,
				images = playAdventureButton.scoreText.transform.GetComponentsInChildren<Image>()
			}
		};
	}

	private IEnumerator AddStars()
	{
		while (true)
		{
			int j;
			for (int i = 0; i < starsToAddPerGameMode.Length; i = j)
			{
				if (starsToAddPerGameMode[i] > 0)
				{
					//yield return new WaitWhile(() => AdsManager.ShowingAd);
					yield return new WaitForSeconds(1.25f);
					GameMode mode = (GameMode)i;
					GameModeButton button = null;
					Vector3 to = default(Vector3);
					switch (mode)
					{
					case GameMode.Arena:
						to = MenuController.UICamera.WorldToViewportPoint(pvpChest.starAnimationTarget.position);
					//	button = play1V1Button;
						break;
					case GameMode.Arena2v2:
						to = MenuController.UICamera.WorldToViewportPoint(pvpChest.starAnimationTarget.position);
						//button = play2V2Button;
						break;
					case GameMode.Adventure:
						to = MenuController.UICamera.WorldToViewportPoint(adventureChest.starAnimationTarget.position);
						button = playAdventureButton;
						break;
					}
					button.glow.enabled = true;
					RectTransform glowRect = button.glow.GetComponent<RectTransform>();
					Vector2 startPosition = new Vector2(0f, -300f);
					Vector2 targetPosition = new Vector2(0f, 300f);
					if (mode == GameMode.Adventure)
					{
						RectOpacityContainer[] adventureButtonElements = GetAdventureButtonElements(adventureLogoLevel);
						StartCoroutine(FadeElementOut(adventureButtonElements[2], 0.15f, 0.1f));
						StartCoroutine(FadeElementOut(adventureButtonElements[1], 0.22f, 0.1f));
						StartCoroutine(FadeElementOut(adventureButtonElements[0], 0.26f, 0.14f));
					}
					for (float t = 0f; t < 0.5f; t += Time.deltaTime)
					{
						glowRect.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t / 0.5f);
						yield return null;
					}
					button.glow.enabled = false;
					AnimateFlash[] componentsInChildren = button.GetComponentsInChildren<AnimateFlash>();
					for (j = 0; j < componentsInChildren.Length; j++)
					{
						componentsInChildren[j].Play();
					}
					button.sparkleParticles.Play();
					Vector3 fromViewportSpace = MenuController.UICamera.WorldToViewportPoint(button.starFlightStart.position);
					int startCount = 0;
					Action<int> onLeaveTick = delegate
					{
					};
					Action<int> action = (Action<int>)delegate
					{
					};
					if (mode == GameMode.Adventure)
					{
						int num = starsToAddPerGameMode[i];
						startCount = PlayerDataManager.GetChestProgress(ChestProgressionType.Adventure) - num;
						Action<int> onArriveTick = delegate(int tickCount)
						{
							float progress2 = (float)(startCount + tickCount) / (float)Variables.instance.GetChestPointsNeeded(ChestProgressionType.Adventure);
							adventureChest.SetProgress(progress2);
							adventureChest.progressText.Tick(tickCount);
							adventureChest.GetComponentInChildren<AnimateFlash>().Play();
							AudioMap.PlayClipAt("progressBarTick", Vector3.zero, AudioMap.instance.uiMixerGroup);
							MenuController.UpdateTopMenu();
							StartCoroutine(AnimateFadeMapChange());
						};
						AnimatedCurrencyController.AnimateSilverStars(num, fromViewportSpace, to, 1, onLeaveTick, onArriveTick);
					}
					else
					{
						int num = starsToAddPerGameMode[i];
						startCount = PlayerDataManager.GetChestProgress(ChestProgressionType.Pvp) - num;
						Action<int> onArriveTick = delegate(int tickCount)
						{
							float progress = (float)(startCount + tickCount) / (float)Variables.instance.GetChestPointsNeeded(ChestProgressionType.Pvp);
							pvpChest.SetProgress(progress);
							pvpChest.progressText.Tick(tickCount);
							pvpChest.GetComponentInChildren<AnimateFlash>().Play();
							AudioMap.PlayClipAt("progressBarTick", Vector3.zero, AudioMap.instance.uiMixerGroup);
							MenuController.UpdateTopMenu();
						};
						AnimatedCurrencyController.AnimateGoldenStars(num, fromViewportSpace, to, 1, onLeaveTick, onArriveTick);
					}
					starsToAddPerGameMode[i] = 0;
				}
				j = i + 1;
			}
			yield return null;
		}
	}

	private IEnumerator AnimateFadeMapChange()
	{
		RectOpacityContainer[] elements = GetAdventureButtonElements();
		float[] audioDelays = new float[3]
		{
			0.16f,
			0.2f,
			0.15f
		};
		float scale = 2f;
		for (int j = 0; j != elements.Length; j++)
		{
			RectOpacityContainer rectOpacityContainer = elements[j];
			rectOpacityContainer.rect.localScale = Vector3.one * scale;
			if (rectOpacityContainer.text != null)
			{
				rectOpacityContainer.text.color = new Color(1f, 1f, 1f, 0f);
				rectOpacityContainer.text.gameObject.SetActive(value: true);
			}
			if (rectOpacityContainer.images != null && rectOpacityContainer.images.Length != 0)
			{
				Image[] images = rectOpacityContainer.images;
				foreach (Image obj in images)
				{
					obj.color = new Color(1f, 1f, 1f, 0f);
					obj.gameObject.SetActive(value: true);
				}
			}
			rectOpacityContainer.rect.gameObject.SetActive(value: true);
		}
		int k;
		for (int i = 0; i != elements.Length; i = k)
		{
			RectOpacityContainer ele = elements[i];
			float time = 0f;
			AudioMap.PlayClipAt(AudioMap.instance.logoHit[i], Vector3.zero, AudioMap.instance.uiMixerGroup, audioDelays[i]);
			for (; time < 0.35f; time += Time.deltaTime)
			{
				float num = time / 0.35f;
				ele.rect.localScale = Vector3.Lerp(Vector3.one * scale, Vector3.one, LeanTween.easeInCubic(0f, 1f, num));
				if (ele.text != null)
				{
					ele.text.color = new Color(1f, 1f, 1f, Mathf.Lerp(0f, 1f, num));
				}
				if (ele.images != null && ele.images.Length != 0)
				{
					Image[] images = ele.images;
					for (k = 0; k < images.Length; k++)
					{
						images[k].color = new Color(1f, 1f, 1f, Mathf.Lerp(0f, 1f, num));
					}
				}
				yield return null;
			}
			ele.rect.localScale = Vector3.one;
			cameraShake += new Vector2(UnityEngine.Random.value - 0.5f, UnityEngine.Random.value - 0.5f) * 10f;
			yield return new WaitForSeconds(0.1f);
			k = i + 1;
		}
	}

	private IEnumerator CameraShakeRoutine()
	{
		while (true)
		{
			cameraShake = Vector2.Lerp(cameraShake, Vector2.zero, Variables.instance.cameraShakeDampening);
			MenuController.instance.menuContainer.GetComponent<RectTransform>().anchoredPosition = cameraShake * 3f;
			yield return new WaitForFixedUpdate();
		}
	}

	private IEnumerator FadeElementOut(RectOpacityContainer element, float delay, float duration)
	{
		yield return new WaitForSecondsRealtime(delay);
		for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
		{
			if (element.images != null && element.images.Length != 0)
			{
				Image[] images = element.images;
				for (int i = 0; i < images.Length; i++)
				{
					images[i].color = new Color(1f, 1f, 1f, Mathf.Lerp(1f, 0f, t / duration));
				}
			}
			if (element.text != null)
			{
				element.text.color = new Color(1f, 1f, 1f, Mathf.Lerp(1f, 0f, t / duration));
			}
			yield return null;
		}
		if (element.images != null && element.images.Length != 0)
		{
			Image[] images = element.images;
			for (int i = 0; i < images.Length; i++)
			{
				images[i].color = new Color(1f, 1f, 1f, 0f);
			}
		}
		if (element.text != null)
		{
			element.text.color = new Color(1f, 1f, 1f, 0f);
		}
	}

	public void ShowQuitDialog()
	{
		MenuController.ShowMenu<QuitDialogPopup>();
	}
}
