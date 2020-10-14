using I2.Loc;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameEndMenu : MenuBase<GameEndMenu>
{
	[Serializable]
	public struct GameOverPlayerInfo
	{
		public RawImage profileImage;

		public Image rankImage;

		public Image flagImage;

		public TextMeshProUGUI nameText;

		public TextMeshProUGUI ratingText;
	}

	public class ArenaGameOverStats
	{
		public Texture profileTexture;

		public Sprite flagSprite;

		public Sprite rankSprite;

		public string name;

		public string rating;
	}

	[Serializable]
	public class ArenaResultScreen
	{
		public GameObject container;

		public TextMeshProUGUI winStreakCountText;

		public TextMeshProUGUI winStreakBonusText;

		public TextMeshProUGUI ratingChangeText;

		public TextMeshProUGUI ratingText;

		public Image prevRank;

		public Image nextRank;

		public GameObject barContainer;

		public Image barFill;

		public Image barFillIndicator;
	}

	public Button gameOverBackToMainButton;

	//public Button shareButton;

	[Header("Adventure")]
	public GameObject gameOverAdventureContainer;

	public TextMeshProUGUI oldScoreText;  //最高得分

	public TextMeshProUGUI newScoreText;

	public TextMeshProUGUI coinsText;

	public Image gameOverShine;

	public Image newHighscoreAnnouncement;

	public Image gameOverPrevRank;

	public Image gameOverNextRank;

	public Image gameOverXpBarFill;

	public Image gameOverXpIndicator;

	public GameObject gameOverXpContainer;

	public GameObject gameOverLastRankImage;

	public GameObject gameOverBossDefeatedContainer;

	public GameObject gameOverDiedContainer;

	[Header("Arena")]
	public GameObject gameOverArenaContainer;

	public TextMeshProUGUI[] arenaGameOverTexts;

	public ArenaResultScreen arenaWinResult;

	public ArenaResultScreen arenaOtherResult;

	public GameOverPlayerInfo[] gameOverPlayerInfos;

	[Header("2v2")]
	public GameObject gameOverArena2v2Container;

	public TextMeshProUGUI[] arena2v2GameOverTexts;

	public ArenaResultScreen arena2v2WinResult;

	public ArenaResultScreen arena2v2OtherResult;

	public GameOverPlayerInfo[] gameOver2v2PlayerInfos;

	[Header("Promotion")]   //晋升
	public GameObject promotionContainer;

	public TextMeshProUGUI promotionRankText;//晋升头衔

	public Image promotionPrevRank;

	public Image promotionNextRank;

	public Image promotionXpBarFill;

	public Image promotionXpIndicator;

	public Image newRank;

	public Image newRankShine;

	public GameObject promotionXpContainer;

	private bool didArenaLevelUp;

	private Ranks ranks;

	private Rank prevRank;//当前军衔

	private Rank nextRank;//下一个军衔

	private int oldXp;

	private int oldHighscore;

	private int scoreGotten;

	private int coinsGotten;

	private Coroutine scoreRollRoutine;

	private Coroutine highscoreRoutine;

	private void OnEnable()
	{
		coinsText.text = "";
        SDKManager.Instance.ShowAd(ShowAdType.VideoAD, 1, "游戏通关和失败");
    }

	private void OnDisable()
	{
	}

	private void Start()
	{
		gameOverBackToMainButton.onClick.AddListener(delegate
		{
			if (scoreRollRoutine != null)
			{
				StopCoroutine(MenuBase<GameEndMenu>.instance.scoreRollRoutine);
				FinishScoreRoll();
				scoreRollRoutine = null;
			}
			else
			{
				if (highscoreRoutine != null)
				{
					StopCoroutine(MenuBase<GameEndMenu>.instance.highscoreRoutine);
					scoreRollRoutine = null;
				}
				bool num = gameOverAdventureContainer.activeInHierarchy && ranks.DidLevelUp;
				bool flag = (gameOverArenaContainer.activeInHierarchy || gameOverArena2v2Container.activeInHierarchy) && didArenaLevelUp;
				if (num | flag)
				{
					InitPromotion();
				}
				else
				{
					PlayerDataManager.GamesThisSession++;
					PlayerDataManager.SelectedGameMode = GameMode.Adventure;
					TankPrefs.Save();
					LoadingScreen.ReloadGame(delegate
					{
						TankPrefs.CheckAndCreateLongtermBackup();
						MenuController.HideMenu<GameEndMenu>();
						//AdsManager.ShowInterstitial();
					});
				}
			}
		});
		//shareButton.onClick.AddListener(Share);
	}

	private void InitPromotion()
	{
		gameOverAdventureContainer.SetActive(value: false);
		gameOverArenaContainer.SetActive(value: false);
		gameOverArena2v2Container.SetActive(value: false);
		//shareButton.gameObject.SetActive(PlayerDataManager.SelectedGameMode == GameMode.Adventure);
		promotionContainer.SetActive(value: true);
		if (!PlayerDataManager.IsSelectedGameModePvP)
		{
			if (MenuBase<GameEndMenu>.instance.ranks.DidLevelUp)
			{
				TankAnalytics.PlayerPromotion(ranks.ranks[ranks.ranks.Count - 1]);
			}
			int currentXp = oldXp + scoreGotten + 1;
			int index = (nextRank != null) ? (ranks.ranks.Count - 2) : (ranks.ranks.Count - 1);
           // Debug.Log("当前军衔是:"+ranks.ranks[index].name);
			//promotionRankText.text = ScriptLocalization.Get(ranks.ranks[index].name);
            promotionRankText.text ="<color=black>"+ ranks.ranks[index].name;
            newRank.sprite = ranks.ranks[index].sprite;
			newRank.SetNativeSize();
			SetXpBar(ranks, currentXp, promotionXpBarFill, promotionXpIndicator, promotionPrevRank, promotionNextRank, promotionXpContainer, null);
			if (MenuBase<GameEndMenu>.instance.nextRank != null)
			{
				float num = oldXp + scoreGotten;
				float num2 = (prevRank != null) ? (num - (float)prevRank.xp) : num;
				float num3 = (prevRank != null) ? (nextRank.xp - prevRank.xp) : nextRank.xp;
				float fillAmount = num2 / num3;
				promotionXpBarFill.fillAmount = fillAmount;
				Vector2 anchoredPosition = promotionXpIndicator.rectTransform.anchoredPosition;
				anchoredPosition.x = promotionXpBarFill.rectTransform.rect.width * promotionXpBarFill.fillAmount;
				promotionXpIndicator.rectTransform.anchoredPosition = anchoredPosition;
			}
		}
		else
		{
			int rating = PlayerDataManager.GetRating(PlayerDataManager.SelectedGameMode);
			RankStats prevRankStats = Variables.instance.GetPrevRankStats(rating);
			RankStats rankStats = Variables.instance.GetRankStats(rating);
			RankStats nextRankStats = Variables.instance.GetNextRankStats(rating);
			float num4 = (float)(rating - prevRankStats.maxTrophies) / (float)(rankStats.maxTrophies - prevRankStats.maxTrophies);
			promotionXpBarFill.fillAmount = num4;
			Vector2 anchoredPosition2 = promotionXpIndicator.rectTransform.anchoredPosition;
			anchoredPosition2.x = promotionXpBarFill.rectTransform.rect.width * num4;
			promotionXpIndicator.rectTransform.anchoredPosition = anchoredPosition2;
			promotionRankText.text = ScriptLocalization.Get(rankStats.name);
			newRank.sprite = ((PlayerDataManager.SelectedGameMode == GameMode.Arena2v2) ? rankStats.sprite2v2 : rankStats.sprite);
			newRank.SetNativeSize();
			promotionPrevRank.sprite = rankStats.smallSprite;
			promotionPrevRank.SetNativeSize();
			promotionNextRank.sprite = nextRankStats.smallSprite;
			promotionNextRank.SetNativeSize();
		}
		StartCoroutine(PromotionAnimation());
		MenuController.instance.StartCoroutine(AudioManager.FadeMusicForSound("promotion"));
	}

	private IEnumerator PromotionSound()
	{
		yield return AudioManager.FadeMusicTo(-80f, 0.5f);
		AudioSource audioSource = AudioMap.PlayClipAt("promotion", Vector3.zero, AudioMap.instance.uiMixerGroup);
		yield return new WaitForSeconds(audioSource.clip.length);
		yield return AudioManager.FadeMusicTo(0f, 0.5f);
	}

	private IEnumerator PromotionAnimation()
	{
		while (true)
		{
			Vector3 eulerAngles = newRankShine.transform.rotation.eulerAngles;
			eulerAngles.z += Time.deltaTime * 45f;
			newRankShine.transform.rotation = Quaternion.Euler(eulerAngles);
			yield return new WaitForEndOfFrame();
		}
	}

	private void Init()
	{
		gameOverAdventureContainer.SetActive(!PlayerDataManager.IsSelectedGameModePvP);
		gameOverArenaContainer.SetActive(PlayerDataManager.SelectedGameMode == GameMode.Arena);
		gameOverArena2v2Container.SetActive(PlayerDataManager.SelectedGameMode == GameMode.Arena2v2);
		//shareButton.gameObject.SetActive(PlayerDataManager.SelectedGameMode == GameMode.Adventure);
		promotionContainer.SetActive(value: false);
		MusicManager.CrossFadeToMenu();
	}

	public void Init(int highscore, int oldXp, int score, int coins, bool bossDefeated)
	{
		Init();
		gameOverDiedContainer.SetActive(!bossDefeated);
		gameOverBossDefeatedContainer.SetActive(bossDefeated);
		newHighscoreAnnouncement.gameObject.SetActive(value: false);
		gameOverShine.gameObject.SetActive(value: false);
		ranks = Variables.instance.GetRanks(oldXp, score);
		this.oldXp = oldXp;
		oldHighscore = highscore;
		scoreGotten = score;
		coinsGotten = coins;
		newScoreText.text = score.ToString();
		oldScoreText.GetComponent<LocalizationParamsManager>().SetParameterValue("SCORE", highscore.ToString());
        if (score> highscore)
        {
            oldScoreText.text = "最高得分:" + score.ToString();
        }
        else
        {
            oldScoreText.text = "最高得分:" + highscore.ToString();
        }

       

        SetXpBar(ranks, oldXp, gameOverXpBarFill, gameOverXpIndicator, gameOverPrevRank, gameOverNextRank, gameOverXpContainer, gameOverLastRankImage);
		float num = (prevRank != null) ? (oldXp - prevRank.xp) : oldXp;
		if (nextRank != null)
		{
			gameOverXpBarFill.fillAmount = num / (float)nextRank.xp;
			Vector2 anchoredPosition = gameOverXpIndicator.rectTransform.anchoredPosition;
			anchoredPosition.x = gameOverXpBarFill.rectTransform.rect.width * gameOverXpBarFill.fillAmount;
			gameOverXpIndicator.rectTransform.anchoredPosition = anchoredPosition;
		}
		scoreRollRoutine = StartCoroutine(ScoreRoll(highscore, score, coins));
	}

	public void Init(int ratingBefore, int ratingDifference, List<ArenaGameOverStats> playerStats = null, int winStreakCount = 0, int winStreakBonus = 0, bool showWinStreak = true)
	{
		Init();
		StartCoroutine(RatingScroll(ratingBefore, ratingDifference, winStreakCount, winStreakBonus, showWinStreak));
		if (PlayerDataManager.SelectedGameMode != GameMode.Arena && PlayerDataManager.SelectedGameMode != GameMode.Arena2v2)
		{
			return;
		}
		GameOverPlayerInfo[] array = (PlayerDataManager.SelectedGameMode == GameMode.Arena) ? gameOverPlayerInfos : gameOver2v2PlayerInfos;
		for (int i = 0; i != playerStats.Count; i++)
		{
			ArenaGameOverStats arenaGameOverStats = playerStats[i];
			array[i].profileImage.gameObject.SetActive(arenaGameOverStats.profileTexture != null);
			if (arenaGameOverStats.profileTexture != null)
			{
				array[i].profileImage.texture = arenaGameOverStats.profileTexture;
			}
			array[i].flagImage.sprite = arenaGameOverStats.flagSprite;
			array[i].rankImage.sprite = arenaGameOverStats.rankSprite;
			array[i].nameText.text = arenaGameOverStats.name;
			if (PlayerDataManager.SelectedGameMode == GameMode.Arena2v2)
			{
				array[i].ratingText.text = arenaGameOverStats.rating.ToString();
			}
		}
	}

	private IEnumerator RatingScroll(int ratingBefore, int difference, int winStreakCount = 0, int winStreakBonus = 0, bool showWinStreak = true)
	{
		arenaWinResult.container.SetActive(value: false);
		arena2v2WinResult.container.SetActive(value: false);
		arenaOtherResult.container.SetActive(value: false);
		arena2v2OtherResult.container.SetActive(value: false);
		ArenaResultScreen result = (PlayerDataManager.SelectedGameMode != GameMode.Arena) ? ((difference > 0) ? arena2v2WinResult : arena2v2OtherResult) : ((difference > 0) ? arenaWinResult : arenaOtherResult);
		if (!showWinStreak)
		{
			result = ((PlayerDataManager.SelectedGameMode == GameMode.Arena) ? arenaOtherResult : arena2v2OtherResult);
		}
		result.container.SetActive(value: true);
		result.ratingText.text = ratingBefore.ToString();
		result.ratingChangeText.gameObject.SetActive(value: true);
		result.winStreakCountText.gameObject.SetActive(showWinStreak);
		if (difference < 0)
		{
			result.ratingChangeText.text = difference.ToString();
		}
		else if (difference > 0)
		{
			if (showWinStreak)
			{
				result.winStreakCountText.GetComponent<LocalizationParamsManager>().SetParameterValue("AMOUNT", winStreakCount.ToString());
				result.winStreakBonusText.text = $"+{winStreakBonus}";
			}
			result.ratingChangeText.text = $"+{difference - winStreakBonus}";
		}
		else
		{
			result.ratingChangeText.gameObject.SetActive(value: false);
		}
		RankStats prevRankStats = Variables.instance.GetPrevRankStats(ratingBefore);
		RankStats rankStats = Variables.instance.GetRankStats(ratingBefore);
		RankStats nextRankStats = Variables.instance.GetNextRankStats(ratingBefore);
		RankStats rankStats2 = Variables.instance.GetRankStats(ratingBefore + difference);
		bool flag = rankStats.maxTrophies == Variables.instance.rankStats[Variables.instance.rankStats.Length - 1].maxTrophies;
		didArenaLevelUp = false;
		if (rankStats2.maxTrophies > rankStats.maxTrophies)
		{
			didArenaLevelUp = true;
		}
		result.prevRank.sprite = rankStats.smallSprite;
		result.prevRank.SetNativeSize();
		if (nextRankStats.sprite != null)
		{
			result.nextRank.gameObject.SetActive(value: true);
			result.nextRank.sprite = nextRankStats.smallSprite;
			result.nextRank.SetNativeSize();
		}
		else
		{
			result.nextRank.gameObject.SetActive(value: false);
		}
		result.barFill.fillAmount = (float)(ratingBefore - prevRankStats.maxTrophies) / (float)(rankStats.maxTrophies - prevRankStats.maxTrophies);
		Vector2 anchoredPosition = result.barFillIndicator.rectTransform.anchoredPosition;
		anchoredPosition.x = result.barFill.rectTransform.rect.width * result.barFill.fillAmount;
		result.barFillIndicator.rectTransform.anchoredPosition = anchoredPosition;
		result.barContainer.SetActive(!flag);
		yield return new WaitForSecondsRealtime(1f);
		int num2;
		for (int i = 1; i <= Mathf.Abs(difference); i = num2)
		{
			int num = ratingBefore + i * (int)Mathf.Sign(difference);
			prevRankStats = Variables.instance.GetPrevRankStats(num);
			rankStats = Variables.instance.GetRankStats(num);
			nextRankStats = Variables.instance.GetNextRankStats(num);
			result.prevRank.sprite = rankStats.smallSprite;
			result.prevRank.SetNativeSize();
			if (nextRankStats.sprite != null)
			{
				result.nextRank.gameObject.SetActive(value: true);
				result.nextRank.sprite = nextRankStats.smallSprite;
				result.nextRank.SetNativeSize();
			}
			else
			{
				result.nextRank.gameObject.SetActive(value: false);
			}
			result.ratingText.text = num.ToString();
			flag = (rankStats.maxTrophies == Variables.instance.rankStats[Variables.instance.rankStats.Length - 1].maxTrophies);
			result.barContainer.SetActive(!flag);
			result.barFill.fillAmount = (float)(num - prevRankStats.maxTrophies) / (float)(rankStats.maxTrophies - prevRankStats.maxTrophies);
			anchoredPosition = result.barFillIndicator.rectTransform.anchoredPosition;
			anchoredPosition.x = result.barFill.rectTransform.rect.width * result.barFill.fillAmount;
			result.barFillIndicator.rectTransform.anchoredPosition = anchoredPosition;
			yield return new WaitForSecondsRealtime(0.1f);
			num2 = i + 1;
		}
	}

	private void SetXpBar(Ranks ranks, int currentXp, Image xpBarFill, Image xpIndicator, Image prevRankImage, Image nextRankImage, GameObject xpBarContainer, GameObject lastRankImage)
	{
		prevRank = null;
		nextRank = null;
		if (ranks.ranks != null)
		{
			for (int i = 0; i < ranks.ranks.Count; i++)
			{
				if (currentXp >= ranks.ranks[i].xp)
				{
					prevRank = ranks.ranks[i];
				}
				else if (currentXp < ranks.ranks[i].xp)
				{
					nextRank = ranks.ranks[i];
					break;
				}
			}
		}
		if (nextRank != null)
		{
			if (xpBarContainer != null)
			{
				xpBarContainer.SetActive(value: true);
			}
			if (lastRankImage != null)
			{
				lastRankImage.SetActive(value: false);
			}
			prevRankImage.sprite = ((prevRank != null) ? prevRank.smallSprite : null);
			prevRankImage.enabled = (prevRank != null);
			nextRankImage.sprite = nextRank.smallSprite;
			prevRankImage.SetNativeSize();
			nextRankImage.SetNativeSize();
		}
		else
		{
			if (xpBarContainer != null)
			{
				xpBarContainer.SetActive(value: false);
			}
			if (lastRankImage != null)
			{
				lastRankImage.SetActive(value: true);
			}
		}
	}

	private IEnumerator ScoreRoll(int highscore, int score, int coins)
	{
		int num = 0;
		bool waitedFirst = false;
		float maxTime = 2f;
		float minTick = 0.1f;
		float scoreRollTime = Mathf.Min(minTick * (float)scoreGotten, maxTime);
		FillTarget imageFillTarget = gameOverXpBarFill.GetComponent<FillTarget>();
		imageFillTarget.smoothTime = 0.066f;
		for (float t2 = 0f; t2 < scoreRollTime; t2 += Time.deltaTime)
		{
			SetXpBar(ranks, oldXp + num, gameOverXpBarFill, gameOverXpIndicator, gameOverPrevRank, gameOverNextRank, gameOverXpContainer, gameOverLastRankImage);
			newScoreText.text = num.ToString();
			AudioMap.PlayClipAt("scoreroll", Vector3.zero, AudioMap.instance.uiMixerGroup);
			if (MenuBase<GameEndMenu>.instance.nextRank != null)
			{
				float num2 = oldXp + num;
				float fillAmount = gameOverXpBarFill.fillAmount;
				float num3 = (prevRank != null) ? (num2 - (float)prevRank.xp) : num2;
				float num4 = (prevRank != null) ? (nextRank.xp - prevRank.xp) : nextRank.xp;
				float num5 = num3 / num4;
				if (num5 < imageFillTarget.fillTarget)
				{
					imageFillTarget.Reset(num5);
				}
				else
				{
					imageFillTarget.fillTarget = num5;
				}
			}
			yield return null;
			if (!waitedFirst)
			{
				yield return new WaitForSeconds(1f);
				waitedFirst = true;
			}
			num = Mathf.RoundToInt(Mathf.Lerp(0f, scoreGotten, t2 / scoreRollTime));
		}
		num = scoreGotten;
		newScoreText.text = num.ToString();
		if (MenuBase<GameEndMenu>.instance.nextRank != null)
		{
			float num6 = oldXp + num;
			float fillAmount2 = gameOverXpBarFill.fillAmount;
			float num7 = (prevRank != null) ? (num6 - (float)prevRank.xp) : num6;
			float num8 = (prevRank != null) ? (nextRank.xp - prevRank.xp) : nextRank.xp;
			float num5 = imageFillTarget.fillTarget = num7 / num8;
		}
		int currentCoins = 0;
		float coinRollTime = Mathf.Min((float)coinsGotten * minTick, maxTime);
		for (float t2 = 0f; t2 < coinRollTime; t2 += Time.deltaTime)
		{
			coinsText.text = currentCoins.ToString();
			AudioMap.PlayClipAt("coinroll", Vector3.zero, AudioMap.instance.uiMixerGroup);
			currentCoins = Mathf.RoundToInt(Mathf.Lerp(0f, coinsGotten, t2 / coinRollTime));
			yield return null;
		}
		scoreRollRoutine = null;
		FinishScoreRoll();
	}

	private void FinishScoreRoll()
	{
		coinsText.text = coinsGotten.ToString();
		newScoreText.text = scoreGotten.ToString();
		if (scoreGotten > oldHighscore)
		{
			StartCoroutine(HighScoreIntro());
			highscoreRoutine = StartCoroutine(HighScoreRoutine());
			newHighscoreAnnouncement.gameObject.SetActive(value: true);
			oldScoreText.GetComponent<LocalizationParamsManager>().SetParameterValue("SCORE", scoreGotten.ToString());
			gameOverShine.gameObject.SetActive(value: true);
		}
		SetXpBar(ranks, oldXp + scoreGotten, gameOverXpBarFill, gameOverXpIndicator, gameOverPrevRank, gameOverNextRank, gameOverXpContainer, gameOverLastRankImage);
	}

	private IEnumerator HighScoreIntro()
	{
		for (float t = 0f; t < 0.5f; t += Time.deltaTime)
		{
			Vector3 localScale = Vector3.Lerp(new Vector3(0f, 0f, 1f), new Vector3(1f, 1f, 1f), t / 0.5f);
			newHighscoreAnnouncement.transform.localScale = localScale;
			gameOverShine.transform.localScale = localScale;
			yield return null;
		}
		newHighscoreAnnouncement.transform.localScale = new Vector3(1f, 1f, 1f);
		gameOverShine.transform.localScale = new Vector3(1f, 1f, 1f);
	}

	private IEnumerator HighScoreRoutine()
	{
		float rotation = 0f;
		float scale = 0f;
		float time = 0f;
		while (true)
		{
			rotation += 90f * Time.deltaTime;
			float z = Mathf.PingPong(rotation, 90f) - 45f;
			newHighscoreAnnouncement.transform.rotation = Quaternion.Euler(0f, 0f, z);
			gameOverShine.transform.rotation *= Quaternion.Euler(0f, 0f, 45f * Time.deltaTime);
			if (time > 0.5f)
			{
				scale += Time.deltaTime * 1f;
				float num = Mathf.PingPong(scale, 0.5f) + 1f;
				newHighscoreAnnouncement.transform.localScale = new Vector3(num, num, 1f);
			}
			yield return null;
			time += Time.deltaTime;
		}
	}

	private void Share()
	{
		string shareMessage = ScriptLocalization.ShareMessage;
		shareMessage = shareMessage.Replace("{[SCORE]}", scoreGotten.ToString());
		GetComponent<NativeShareTool>().ShareScreenshotWithText(shareMessage + "\n\nGoogle Play: https://goo.gl/7bMa57\nApp Store: https://itunes.apple.com/ie/app/id1304887309");
		TankAnalytics.ShareButtonClicked();
	}
}
