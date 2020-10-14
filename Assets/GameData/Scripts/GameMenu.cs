using I2.Loc;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;

public class GameMenu : MenuBase<GameMenu>
{
	public TouchScreenController touchScreenController;

	public Button pauseButton;//暂停

	public GameObject arenaContainer;

	public GameObject adventureContainer;

	[Header("Adventure & Boss Rush")]
	public GameObject bossContainer;

	public Transform bossHealthbar;

	public Transform bossHealthbarBase;

	public TextMeshProUGUI bossName;

	public GameObject progressBar;

	public GameObject progressStarIcon;

	public GameObject progressSkullIcon;

	public Image progressBarFill;

	public Image progressBarFillHighlight;

	public GameObject otherThanBossContainer;

	public Image reloadFillBar;

	public TextMeshProUGUI scoreText;

	public TextMeshProUGUI highscoreText;

	public Image coinImage;

	public Transform coinAnimationTarget;

	public CountText coinsText;

	[Header("Classic")]
	public RectTransform classicStageClearedContainer;

	[Header("Arena")]
	public Transform[] arenaPlayerHealthbar;

	public Transform[] arenaPlayerHealthbarBase;

	public GameObject arenaMultiplayerContainer;

	public GameObject hideDuringArenaMultiplayer;

	public ArenaPlayerHUD[] arenaPlayerHUDs;

	public ArenaPlayerHUD[] arenaMultiplayerHUDs;

	public TextMeshProUGUI[] arenaAllyNames;

	public TextMeshProUGUI[] arenaOpponentNames;

	public TextMeshProUGUI arenaVSText;

	public TextMeshProUGUI arenaTimeText;

	public TextMeshProUGUI suddenDeathText;

	public Button arenaSurrenderButton;

	[Header("Stage Cleared")]
	public RectTransform stageClearedContainer;

	public Transform stageClearedShine;

	public RectTransform stageClearedRect;

	public RectTransform stageClearedText;

	private float progressBarVel;

	private int score;

	private int scoreToAdd;

	private bool pauseBlocked;

	private Vector3 scoreScaleOriginal;

	public Vector3 CoinPositionViewport => MenuController.UICamera.WorldToViewportPoint(MenuBase<GameMenu>.instance.coinAnimationTarget.transform.position);

	private Vector3 BossNameOriginalPosition
	{
		get;
		set;
	}

	private Vector3 BossNameOriginalScale
	{
		get;
		set;
	}

	private void Awake()
	{
		scoreScaleOriginal = scoreText.transform.localScale;
	}

	private void OnEnable()
	{
		MenuBase<GameMenu>.instance = this;
		TankGame.AddPlayerController(touchScreenController);
		TankGame.OnSetHighscore = (TankGame.SetHighscoreDelegate)Delegate.Combine(TankGame.OnSetHighscore, new TankGame.SetHighscoreDelegate(OnSetHighscore));
		TankGame.OnPlayerShot = (TankGame.PlayerShotDelegate)Delegate.Combine(TankGame.OnPlayerShot, new TankGame.PlayerShotDelegate(OnPlayerShot));
		TankGame.OnBoosterUsed = (TankGame.BoosterUsedDelegate)Delegate.Combine(TankGame.OnBoosterUsed, new TankGame.BoosterUsedDelegate(OnBoosterUsed));
		TankGame.OnScoreAdded = (TankGame.ScoreAddedDelegate)Delegate.Combine(TankGame.OnScoreAdded, new TankGame.ScoreAddedDelegate(OnScoreAdded));
		TankGame.OnSetBossHealth = (TankGame.SetBossHealthDelegate)Delegate.Combine(TankGame.OnSetBossHealth, new TankGame.SetBossHealthDelegate(OnSetBossHealth));
		TankGame.OnSetStageProgress = (TankGame.SetStageProgressDelegate)Delegate.Combine(TankGame.OnSetStageProgress, new TankGame.SetStageProgressDelegate(OnSetStageProgress));
		TankGame.OnBossIntro = (TankGame.BossIntroDelegate)Delegate.Combine(TankGame.OnBossIntro, new TankGame.BossIntroDelegate(OnBossIntro));
		TankGame.OnArenaIntro = (TankGame.ArenaIntroDelegate)Delegate.Combine(TankGame.OnArenaIntro, new TankGame.ArenaIntroDelegate(OnArenaIntro));
		TankGame.OnGameBegin = (TankGame.GameBeginDelegate)Delegate.Combine(TankGame.OnGameBegin, new TankGame.GameBeginDelegate(OnGameBegin));
		TankGame.OnGameEnd = (TankGame.GameEndDelegate)Delegate.Combine(TankGame.OnGameEnd, new TankGame.GameEndDelegate(OnGameEnd));
		TankGame.OnGameTime = (TankGame.GameTimeDelegate)Delegate.Combine(TankGame.OnGameTime, new TankGame.GameTimeDelegate(OnGameTime));
		TankGame.OnSuddenDeath = (TankGame.SuddenDeathDelegate)Delegate.Combine(TankGame.OnSuddenDeath, new TankGame.SuddenDeathDelegate(OnSuddenDeath));
		TankGame.OnPlayerHealth = (TankGame.PlayerHealthDelegate)Delegate.Combine(TankGame.OnPlayerHealth, new TankGame.PlayerHealthDelegate(OnPlayerHealth));
		BossNameOriginalPosition = bossName.rectTransform.anchoredPosition;
		BossNameOriginalScale = bossName.transform.localScale;
		progressStarIcon.SetActive(PlayerDataManager.SelectedGameMode == GameMode.Adventure);
		progressSkullIcon.SetActive(PlayerDataManager.SelectedGameMode == GameMode.Classic);
		bossContainer.SetActive(value: false);
		scoreText.transform.localScale = scoreScaleOriginal;
		StartCoroutine(ScoreUpdater());
		score = 0;
		scoreToAdd = 0;
		arenaSurrenderButton.onClick.RemoveAllListeners();
		arenaSurrenderButton.onClick.AddListener(delegate
		{
			arenaSurrenderButton.gameObject.SetActive(value: false);
			TankGame.instance.PlayerSurrendered = true;
			MenuController.ShowMenu<GameEndMenu>();
		});
		pauseBlocked = false;
		MenuController.backButtonFallbackAction = (MenuController.OnBackButtonPress)Delegate.Combine(MenuController.backButtonFallbackAction, new MenuController.OnBackButtonPress(Pause));
	}

	private void OnDisable()
	{
		TankGame.OnSetHighscore = (TankGame.SetHighscoreDelegate)Delegate.Remove(TankGame.OnSetHighscore, new TankGame.SetHighscoreDelegate(OnSetHighscore));
		TankGame.OnPlayerShot = (TankGame.PlayerShotDelegate)Delegate.Remove(TankGame.OnPlayerShot, new TankGame.PlayerShotDelegate(OnPlayerShot));
		TankGame.OnBoosterUsed = (TankGame.BoosterUsedDelegate)Delegate.Remove(TankGame.OnBoosterUsed, new TankGame.BoosterUsedDelegate(OnBoosterUsed));
		TankGame.OnScoreAdded = (TankGame.ScoreAddedDelegate)Delegate.Remove(TankGame.OnScoreAdded, new TankGame.ScoreAddedDelegate(OnScoreAdded));
		TankGame.OnSetBossHealth = (TankGame.SetBossHealthDelegate)Delegate.Remove(TankGame.OnSetBossHealth, new TankGame.SetBossHealthDelegate(OnSetBossHealth));
		TankGame.OnSetStageProgress = (TankGame.SetStageProgressDelegate)Delegate.Remove(TankGame.OnSetStageProgress, new TankGame.SetStageProgressDelegate(OnSetStageProgress));
		TankGame.OnBossIntro = (TankGame.BossIntroDelegate)Delegate.Remove(TankGame.OnBossIntro, new TankGame.BossIntroDelegate(OnBossIntro));
		TankGame.OnArenaIntro = (TankGame.ArenaIntroDelegate)Delegate.Remove(TankGame.OnArenaIntro, new TankGame.ArenaIntroDelegate(OnArenaIntro));
		TankGame.OnGameBegin = (TankGame.GameBeginDelegate)Delegate.Remove(TankGame.OnGameBegin, new TankGame.GameBeginDelegate(OnGameBegin));
		TankGame.OnGameEnd = (TankGame.GameEndDelegate)Delegate.Remove(TankGame.OnGameEnd, new TankGame.GameEndDelegate(OnGameEnd));
		TankGame.OnGameTime = (TankGame.GameTimeDelegate)Delegate.Remove(TankGame.OnGameTime, new TankGame.GameTimeDelegate(OnGameTime));
		TankGame.OnSuddenDeath = (TankGame.SuddenDeathDelegate)Delegate.Remove(TankGame.OnSuddenDeath, new TankGame.SuddenDeathDelegate(OnSuddenDeath));
		TankGame.OnPlayerHealth = (TankGame.PlayerHealthDelegate)Delegate.Remove(TankGame.OnPlayerHealth, new TankGame.PlayerHealthDelegate(OnPlayerHealth));
		if (arenaAllyNames != null && arenaAllyNames.Length != 0)
		{
			TextMeshProUGUI[] array = arenaAllyNames;
			foreach (TextMeshProUGUI obj in array)
			{
				obj.text = "";
				obj.gameObject.SetActive(value: false);
			}
		}
		if (arenaOpponentNames != null && arenaOpponentNames.Length != 0)
		{
			TextMeshProUGUI[] array = arenaOpponentNames;
			foreach (TextMeshProUGUI obj2 in array)
			{
				obj2.text = "";
				obj2.gameObject.SetActive(value: false);
			}
		}
		classicStageClearedContainer.gameObject.SetActive(value: false);
		MenuController.backButtonFallbackAction = (MenuController.OnBackButtonPress)Delegate.Remove(MenuController.backButtonFallbackAction, new MenuController.OnBackButtonPress(Pause));
	}

	private void Start()
	{
		pauseButton.onClick.AddListener(Pause);
	}

	public void AnimateGameCoins(int count, Transform from)
	{
		AnimatedCurrencyController.AnimateCoins(count, TankGame.instance.ingameCameras[0].WorldToViewportPoint(from.position), CoinPositionViewport, 1, delegate
		{
		}, delegate(int value)
		{
			coinsText.Tick(value);
		}, departureSound: false);
	}

	private IEnumerator ScoreUpdater()
	{
		scoreText.text = "0";
		while (true)
		{
			if (scoreToAdd > 0)
			{
				Vector3 fontScale = scoreText.transform.localScale;
				Vector3 targetScale = fontScale;
				targetScale.Scale(new Vector3(1.5f, 1.5f, 1f));
				float time2 = 0f;
				while (time2 < 0.25f)
				{
					scoreText.transform.localScale = Vector3.Lerp(fontScale, targetScale, time2 / 0.25f);
					time2 += Time.deltaTime;
					yield return new WaitForEndOfFrame();
				}
				scoreText.transform.localScale = targetScale;
				int step = Mathf.RoundToInt(Mathf.Max(1f, (float)scoreToAdd * 0.25f));
				while (scoreToAdd > 0)
				{
					scoreToAdd = Mathf.Max(0, scoreToAdd - step);
					scoreText.text = (score - scoreToAdd).ToString();
					yield return new WaitForSeconds(0.1f);
				}
				scoreText.text = score.ToString();
				time2 = 0f;
				while (time2 < 0.25f)
				{
					scoreText.transform.localScale = Vector3.Lerp(targetScale, fontScale, time2 / 0.25f);
					time2 += Time.deltaTime;
					yield return new WaitForEndOfFrame();
				}
				scoreText.transform.localScale = fontScale;
			}
			else
			{
				yield return new WaitForEndOfFrame();
			}
		}
	}

	private void OnSetHighscore(int score)
	{
		highscoreText.text = score.ToString();
	}

	private IEnumerator OnGameBegin(TankGame game, GameMode mode)
	{
		coinsText.Init(PlayerDataManager.GetCoins());
		bossContainer.SetActive(value: false);
		switch (mode)
		{
		case GameMode.Adventure:
		case GameMode.Classic:
			adventureContainer.SetActive(value: true);
			arenaContainer.SetActive(value: false);
			arenaMultiplayerContainer.SetActive(value: false);
			progressBar.SetActive(value: true);
			otherThanBossContainer.SetActive(value: true);
			break;
		case GameMode.BossRush:
			adventureContainer.SetActive(value: true);
			arenaContainer.SetActive(value: false);
			arenaMultiplayerContainer.SetActive(value: false);
			progressBar.SetActive(value: false);
			otherThanBossContainer.SetActive(value: true);
			break;
		case GameMode.Arena:
			arenaContainer.SetActive(value: true);
			arenaMultiplayerContainer.SetActive(value: false);
			hideDuringArenaMultiplayer.SetActive(value: true);
			otherThanBossContainer.SetActive(value: true);
			adventureContainer.SetActive(value: false);
			break;
		case GameMode.Arena2v2:
		{
			arenaContainer.SetActive(value: true);
			arenaMultiplayerContainer.SetActive(value: true);
			hideDuringArenaMultiplayer.SetActive(value: false);
			otherThanBossContainer.SetActive(value: true);
			adventureContainer.SetActive(value: false);
			arenaSurrenderButton.gameObject.SetActive(value: false);
			ArenaPlayerHUD[] array = arenaMultiplayerHUDs;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].killedIcon.SetActive(value: false);
			}
			break;
		}
		}
		yield return null;
	}

    

    public void FH()
    {
        TankGame.instance.ReviveUsed = true;
        TankGame.instance.ReviveTried = true;
        MenuController.HideMenu<RevivePopup>();
    }

    private IEnumerator OnGameEnd(TankGame game, GameMode mode)
	{
		if (game.playerTankContainer.CurrentHealth <= 0f)
		{
			GetComponent<CanvasGroup>().alpha = 0f;
			if (!TankGame.instance.ReviveTried)
			{
				RevivePopup revive = MenuController.ShowMenu<RevivePopup>();
				revive.reviveCoinsButton.onClick.RemoveAllListeners();
				revive.reviveCoinsButton.onClick.AddListener(delegate
				{
					if (PlayerDataManager.TakeGems(Variables.instance.reviveCost))
					{
						TankGame.instance.ReviveTried = true;
						TankGame.instance.ReviveUsed = true;
						MenuController.HideMenu<RevivePopup>();
						TankAnalytics.BoughtWithPremiumCurrency("revive", "revive", Variables.instance.reviveCost);
					}
					else
					{
						MenuController.ShowMenu<OutOfCurrencyPopup>().SetCurrency(CurrencyType.Gems);
					}
				});
				revive.reviveAdsButton.onClick.RemoveAllListeners();
				revive.reviveAdsButton.onClick.AddListener(delegate
				{
                   // SDKManager.Instance.MakeToast("暂无广告!!!");
                    SDKManager.Instance.ShowAd(ShowAdType.Reward,1,"点击免费复活",(bool IsComp)=> 
                    {
                        if (IsComp)
                        {
                            FH();
                        }
                    });                    		
                });
				bool blink = false;
				float blinkTime = 0f;
				float blinkInterval = 0.1f;
				float time = 0f;
				while (time <= Variables.instance.reviveTime && !TankGame.instance.ReviveTried)
				{
					revive.reviveSecondsText.text = Mathf.FloorToInt(Variables.instance.reviveTime - time).ToString();
					if (MenuController.GetMenu<ShopMenu>().gameObject.activeInHierarchy || MenuController.GetMenu<OutOfCurrencyPopup>().gameObject.activeInHierarchy)
					{
						time = 0f;
						yield return null;
						continue;
					}
					AudioMap.PlayClipAt("countdown", Vector3.zero, AudioMap.instance.uiMixerGroup);
					for (float secondTime = 0f; secondTime <= 1f; secondTime += Time.deltaTime)
					{
						if (Variables.instance.reviveTime - time < 4f)
						{
							if (blinkTime > Mathf.Max(0.1f, blinkInterval * (Variables.instance.reviveTime - time)))
							{
								revive.reviveSecondsText.enabled = blink;
								blink = !blink;
								blinkTime = 0f;
							}
							blinkTime += Time.deltaTime;
						}
						time += Time.deltaTime;
						yield return null;
					}
				}
				MenuController.HideMenu<RevivePopup>();
			}
			if (TankGame.instance.ReviveTried)
            {
                yield return null; //new WaitWhile(() => AdsManager.ShowingAd);
            }
			if (TankGame.instance.AllLivesUsed)
			{
				yield return new WaitForSecondsRealtime(1.5f);
			}
			GetComponent<CanvasGroup>().alpha = 1f;
		}
		else
		{
			MusicManager.SetToBossEndMusic();
		}
	}

	private IEnumerator OnGameTime(TankGame game, GameMode mode, float t)
	{
		yield return null;
	}

	private IEnumerator OnSuddenDeath(TankGame game, GameMode mode)
	{
		yield return null;
	}

	private void Update()
	{
		if (!(TankGame.instance.playerTankContainer == null))
		{
			if (TankGame.instance.playerTank.bullet.type == BulletType.Cannon)
			{
				reloadFillBar.fillAmount = Mathf.Lerp(reloadFillBar.fillAmount, 1f - TankGame.instance.playerTankContainer.ReloadTime / TankGame.instance.playerTankContainer.Stats.reloadTime, 0.3f);
			}
			else if (TankGame.instance.playerTank.bullet.type == BulletType.Missile || TankGame.instance.playerTank.bullet.type == BulletType.Laser)
			{
				reloadFillBar.fillAmount = Mathf.Lerp(0f, 1f, 1f - TankGame.instance.playerTankContainer.ReloadTime / TankGame.instance.playerTank.bullet.maxTime);
			}
			else if (TankGame.instance.playerTank.bullet.type == BulletType.Flame || TankGame.instance.playerTank.bullet.type == BulletType.Lightning)
			{
				reloadFillBar.fillAmount = Mathf.Lerp(0f, 1f, 1f - TankGame.instance.playerTankContainer.ReloadTime / TankGame.instance.playerTank.bullet.maxTime);
			}
			else if (TankGame.instance.playerTank.bullet.type == BulletType.Small)
			{
				reloadFillBar.fillAmount = Mathf.Lerp(0f, 1f, 1f - TankGame.instance.playerTankContainer.ShootTimeTotal / TankGame.instance.playerTank.bullet.maxTime);
			}
		}
	}

	private void OnPlayerShot(BulletDefinition bulletDef)
	{
	}

	private void OnBoosterUsed()
	{
	}

	private void OnScoreAdded(int addedScore)
	{
		score += addedScore;
		scoreToAdd += addedScore;
	}

	private void OnSetBossHealth(float v)
	{
	}

	private void OnSetStageProgress(float t)
	{
		progressBarFill.fillAmount = Mathf.SmoothDamp(progressBarFill.fillAmount, t, ref progressBarVel, 0.5f);
		Vector2 anchoredPosition = progressBarFill.rectTransform.anchoredPosition;
		anchoredPosition.x += progressBarFill.fillAmount * progressBarFill.rectTransform.rect.width;
		progressBarFillHighlight.rectTransform.anchoredPosition = anchoredPosition;
		Color color = progressBarFillHighlight.color;
		color.a = Mathf.Abs(progressBarVel * 15f);
		progressBarFillHighlight.color = color;
	}

	private IEnumerator OnBossIntro(TankGame game, Enemy boss)
	{
		pauseBlocked = true;
		progressBar.gameObject.SetActive(value: false);
		otherThanBossContainer.SetActive(value: false);
		bossContainer.SetActive(value: true);
		boss.vehicleContainer.SetHealthBar(bossHealthbar, bossHealthbarBase);
		bossName.text = ScriptLocalization.Get(boss.settings.name);
		bossName.SetNativeSize();
		Vector2 origPos = BossNameOriginalPosition;
		Vector3 origScale = BossNameOriginalScale;
		Vector3 largeScale = origScale * 1.2f;
		Vector2 startPos = MenuBase<GameMenu>.instance.bossName.rectTransform.anchoredPosition + new Vector2(Screen.width * 2, -250f);
		Vector2 endPos = MenuBase<GameMenu>.instance.bossName.rectTransform.anchoredPosition + new Vector2(-250f, -250f);
		bossName.rectTransform.anchoredPosition = startPos;
		bossName.transform.localScale = largeScale;
		bossHealthbarBase.gameObject.SetActive(value: false);
		for (float t6 = 0f; t6 < 1f; t6 += Time.deltaTime)
		{
			yield return null;
		}
		otherThanBossContainer.gameObject.SetActive(value: false);
		TankGame.instance.playerTankContainer.SetSpeed(0f, 0f);
		game.cameraFollowTransform = boss.vehicleContainer.cameraFollowTransform;
		for (float t6 = 0f; t6 < 1.5f; t6 += Time.deltaTime)
		{
			yield return null;
		}
		AudioManager.SetGameAudioTo(-80f);
		Time.timeScale = 0f;
		for (float t6 = 0f; t6 < 0.5f; t6 += Time.unscaledDeltaTime)
		{
			float t7 = Mathf.Clamp01(LeanTween.easeInOutCirc(0f, 1f, Mathf.Clamp01(t6 / 0.5f)));
			bossName.rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t7);
			yield return null;
		}
		for (float t6 = 0f; t6 < 1.5f; t6 += Time.unscaledDeltaTime)
		{
			yield return null;
		}
		AudioManager.SetGameAudioTo(0f);
		Time.timeScale = 1f;
		for (float t6 = 0f; t6 < 0.5f; t6 += Time.deltaTime)
		{
			float t8 = Mathf.Clamp01(LeanTween.easeInOutCirc(0f, 1f, Mathf.Clamp01(t6 / 0.5f)));
			bossName.rectTransform.anchoredPosition = Vector3.Lerp(endPos, origPos, t8);
			bossName.transform.localScale = Vector3.Lerp(largeScale, origScale, t8);
			yield return null;
		}
		for (float t6 = 0f; t6 < 0.5f; t6 += Time.deltaTime)
		{
			yield return null;
		}
		otherThanBossContainer.gameObject.SetActive(value: true);
		bossHealthbarBase.gameObject.SetActive(value: true);
		bossHealthbar.GetComponent<Image>().fillAmount = 0f;
		bossName.rectTransform.anchoredPosition = origPos;
		bossName.transform.localScale = origScale;
		game.cameraFollowTransform = (game.playerTankContainer.cameraFollowTransform ? game.playerTankContainer.cameraFollowTransform : game.playerTankContainer.transform);
		pauseBlocked = false;
	}

	private void OnPlayerHealth()
	{
	}

	private IEnumerator OnArenaIntro(TankGame game)
	{
		pauseBlocked = true;
		otherThanBossContainer.SetActive(value: false);
		arenaContainer.GetComponent<CanvasGroup>().alpha = 0f;
		float cameraSize = game.ingameCameras[0].orthographicSize;
		float time = 0.5f;
		float vel = 0f;
		float vel2 = 0f;
		float cameraXVelocity = 0f;
		float cameraYVelocity = 0f;
		game.cameraFollowTransform = game.arenaEnemies[0].transform;
		yield return null;
		Vector3 position = game.arenaEnemies[0].transform.position - Vector3.right * 7f;
		position.z = game.cameraContainer.position.z;
		game.cameraContainer.transform.position = position;
		game.backgroundCamera.orthographicSize = cameraSize * 0.9f;
		game.ingameCameras[0].orthographicSize = cameraSize * 0.9f;
		yield return null;
		yield return null;
		yield return null;
		string[] allyNames = new string[(PlayerDataManager.SelectedGameMode == GameMode.Arena) ? 1 : 2];
		string[] array = new string[(PlayerDataManager.SelectedGameMode == GameMode.Arena) ? 1 : 2];
		allyNames[0] = TankPrefs.GetString("challengeName", Social.localUser.userName);
		if (PlayerDataManager.SelectedGameMode == GameMode.Arena)
		{
			array[0] = PlayerDataManager.ArenaMatchData.arenaPayload.name;
		}
		else
		{
			int num = 1;
			int num2 = 0;
			for (int i = 0; i != PlayerDataManager.ArenaMultiMatchData.arenaPayload.Length; i++)
			{
				if (i > 0)
				{
					array[num2++] = PlayerDataManager.ArenaMultiMatchData.arenaPayload[i].name;
				}
				else
				{
					allyNames[num++] = PlayerDataManager.ArenaMultiMatchData.arenaPayload[i].name;
				}
			}
		}
		yield return AnimateTextRoutine(array, arenaOpponentNames);
		if (PlayerDataManager.SelectedGameMode == GameMode.Arena)
		{
			arenaPlayerHUDs[0].ratingText.text = PlayerDataManager.GetRating().ToString();
			arenaPlayerHUDs[1].ratingText.text = ((PlayerDataManager.ArenaMatchData.arenaPayload.actualRating == 0) ? PlayerDataManager.ArenaMatchData.arenaPayload.rating.ToString() : PlayerDataManager.ArenaMatchData.arenaPayload.actualRating.ToString());
		}
		game.cameraFollowTransform = game.playerTankContainer.transform;
		for (float t2 = 0f; t2 < time; t2 += Time.unscaledDeltaTime)
		{
			game.backgroundCamera.orthographicSize = Mathf.SmoothDamp(game.backgroundCamera.orthographicSize, cameraSize * 1f, ref vel, time, float.PositiveInfinity, Time.unscaledDeltaTime);
			game.ingameCameras[0].orthographicSize = Mathf.SmoothDamp(game.ingameCameras[0].orthographicSize, cameraSize * 1f, ref vel2, time, float.PositiveInfinity, Time.unscaledDeltaTime);
			game.cameraContainer.position = new Vector3(Mathf.SmoothDamp(game.cameraContainer.position.x, game.cameraFollowTransform.position.x, ref cameraXVelocity, time, float.PositiveInfinity, Time.unscaledDeltaTime), Mathf.SmoothDamp(game.cameraContainer.position.y, game.cameraFollowTransform.position.y + 2f, ref cameraYVelocity, time, float.PositiveInfinity, Time.unscaledDeltaTime), game.cameraContainer.position.z);
			yield return null;
		}
		Coroutine allyNameRoutine = StartCoroutine(AnimateTextRoutine(allyNames, arenaAllyNames));
		for (float t2 = 0f; t2 < time + 0.5f; t2 += Time.unscaledDeltaTime)
		{
			game.backgroundCamera.orthographicSize = Mathf.SmoothDamp(game.backgroundCamera.orthographicSize, cameraSize, ref vel, time, float.PositiveInfinity, Time.unscaledDeltaTime);
			game.ingameCameras[0].orthographicSize = Mathf.SmoothDamp(game.ingameCameras[0].orthographicSize, cameraSize, ref vel2, time, float.PositiveInfinity, Time.unscaledDeltaTime);
			game.cameraContainer.position = new Vector3(Mathf.SmoothDamp(game.cameraContainer.position.x, game.cameraFollowTransform.position.x + 3f, ref cameraXVelocity, time, float.PositiveInfinity, Time.unscaledDeltaTime), Mathf.SmoothDamp(game.cameraContainer.position.y, game.cameraFollowTransform.position.y + 2f, ref cameraYVelocity, time, float.PositiveInfinity, Time.unscaledDeltaTime), game.cameraContainer.position.z);
			yield return null;
		}
		yield return new WaitForSecondsRealtime(0.6f);
		yield return allyNameRoutine;
		otherThanBossContainer.SetActive(value: true);
		arenaContainer.GetComponent<CanvasGroup>().alpha = 1f;
	}

	private IEnumerator AnimateTextRoutine(string[] names, TextMeshProUGUI[] uiTexts)
	{
		Vector2[] resetPositions = new Vector2[names.Length];
		Coroutine[] textInAnimations = new Coroutine[names.Length];
		int num;
		for (int j = 0; j != names.Length; j = num)
		{
			resetPositions[j] = uiTexts[j].rectTransform.anchoredPosition;
			uiTexts[j].text = names[j];
			textInAnimations[j] = StartCoroutine(AnimateTextIn(uiTexts[j]));
			yield return new WaitForSecondsRealtime(0.4f);
			num = j + 1;
		}
		yield return new WaitForSecondsRealtime(0.6f);
		for (int j = 0; j != names.Length; j = num)
		{
			StartCoroutine(AnimateTextOut(uiTexts[j]));
			yield return new WaitForSecondsRealtime(0.4f);
			num = j + 1;
		}
		yield return new WaitForSecondsRealtime(0.6f);
		for (int k = 0; k != names.Length; k++)
		{
			uiTexts[k].rectTransform.anchoredPosition = resetPositions[k];
		}
	}

	private IEnumerator AnimateTextIn(TextMeshProUGUI text)
	{
		RectTransform rect = text.rectTransform;
		Vector2 originalPos = rect.anchoredPosition;
		Vector2 startPos = rect.anchoredPosition = originalPos + Vector2.right * Screen.width;
		text.gameObject.SetActive(value: true);
		for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime)
		{
			float t2 = LeanTween.easeInOutCirc(0f, 1f, t);
			rect.anchoredPosition = Vector2.Lerp(startPos, originalPos, t2);
			yield return null;
		}
	}

	private IEnumerator AnimateTextOut(TextMeshProUGUI text)
	{
		RectTransform rect = text.rectTransform;
		Vector2 originalPos = rect.anchoredPosition;
		Vector2 endPos = originalPos + Vector2.left * Screen.width;
		for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime)
		{
			float t2 = LeanTween.easeInExpo(0f, 1f, t);
			rect.anchoredPosition = Vector2.Lerp(originalPos, endPos, t2);
			yield return null;
		}
		text.gameObject.SetActive(value: false);
		rect.anchoredPosition = originalPos;
	}

	private IEnumerator ResizeElement(RectTransform element, float scaleFrom, float scaleTo, float time)
	{
		for (float t = 0f; t < time; t += Time.unscaledDeltaTime)
		{
			float t2 = LeanTween.easeInExpo(0f, 1f, t / time);
			element.localScale = Vector3.Lerp(Vector3.one * scaleFrom, Vector3.one * scaleTo, t2);
			yield return null;
		}
		element.localScale = Vector3.one * scaleTo;
	}

	public IEnumerator ToggleStageClearedText()
	{
		if (classicStageClearedContainer.gameObject.activeInHierarchy)
		{
			yield return ResizeElement(classicStageClearedContainer, 1f, 0f, 0.2f);
			classicStageClearedContainer.gameObject.SetActive(value: false);
		}
		else
		{
			classicStageClearedContainer.gameObject.SetActive(value: true);
			yield return ResizeElement(classicStageClearedContainer, 0f, 1f, 0.4f);
		}
	}

	public void Pause()
	{
        SDKManager.Instance.ShowAd(ShowAdType.VideoAD, 1, "点击暂停");
        if (!pauseBlocked)
		{
			MenuController.ShowMenu<PauseMenu>();
		}
	}
}
