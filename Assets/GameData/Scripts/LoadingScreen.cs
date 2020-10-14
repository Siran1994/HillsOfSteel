using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
	public static LoadingScreen instance;

	public CanvasGroup canvasGroup;

	public Image[] logos;

	public Image[] gameLogoParts;

	public Image loadingProgressImage;

	public Button cancelWaitingForOpponentButton;

	public TextMeshProUGUI loadingText;

	public TextMeshProUGUI waitingForOpponentText;

	public TextMeshProUGUI waitingForPlayersText;

	private Coroutine loadingRoutine;

	private float loadingProgress;

	private static Action OnFinish;

	public static bool IsLoading
	{
		get;
		private set;
	}

	public static bool MenuLoaded
	{
		get;
		private set;
	}

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		loadingProgress = 0f;
	}

	private IEnumerator Start()
	{
		yield return null;
		loadingRoutine = StartCoroutine(StartRoutine());
	}

	private void Update()
	{
		if (IsLoading)
		{
			loadingProgressImage.fillAmount = Mathf.Lerp(loadingProgressImage.fillAmount, loadingProgress, 1f);
		}
	}

	public static IEnumerator AddProgress(float value)
	{
		while (instance == null)
		{
			yield return null;
		}
		instance.loadingProgress = Mathf.Clamp01(instance.loadingProgress + value);
	}

	public static IEnumerator FadeIn()
	{
		instance.loadingText.gameObject.SetActive(!PlayerDataManager.IsSelectedGameModePvP);
		instance.waitingForOpponentText.gameObject.SetActive(PlayerDataManager.SelectedGameMode == GameMode.Arena);
		instance.waitingForPlayersText.gameObject.SetActive(PlayerDataManager.SelectedGameMode == GameMode.Arena2v2);
		instance.cancelWaitingForOpponentButton.gameObject.SetActive(PlayerDataManager.IsSelectedGameModePvP);
		instance.canvasGroup.gameObject.SetActive(value: true);
		for (float t = 0f; t < 0.25f; t += Time.unscaledDeltaTime)
		{
			instance.canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / 0.25f);
			yield return null;
		}
		instance.canvasGroup.alpha = 1f;
	}

	public static IEnumerator FadeOut()
	{
		for (float t = 0f; t < 0.25f; t += Time.unscaledDeltaTime)
		{
			instance.canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / 0.25f);
			yield return null;
		}
		instance.canvasGroup.alpha = 0f;
		instance.canvasGroup.gameObject.SetActive(value: false);
	}

	public static void ReloadGame(Action onFinish)
	{
		if (!IsLoading)
		{
			OnFinish = onFinish;
			instance.loadingProgress = 0f;
			instance.loadingRoutine = instance.StartCoroutine(instance.StartRoutine());
		}
	}

	public static void LoadIfNotLoaded()
	{
		if (!SceneManager.GetSceneByName("Loading").isLoaded)
		{
			try
			{
				SceneManager.LoadScene("Loading", LoadSceneMode.Additive);
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.Log(ex.Message);
			}
		}
	}

	public static void LoadMenu()
	{
		if (!SceneManager.GetSceneByName("Menu").isLoaded)
		{
			MenuLoaded = false;
			try
			{
				SceneManager.LoadScene("Menu", LoadSceneMode.Additive);
				MenuLoaded = true;
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.Log(ex.Message);
			}
		}
	}

	public static void OpenShopOnLoad(ShopMenu.Section section = ShopMenu.Section.Chests)
	{
		if (MenuLoaded)
		{
			MenuController.ShowMenu<ShopMenu>().ScrollToSection(section);
		}
		else
		{
			OnFinish = delegate
			{
				MenuController.ShowMenu<ShopMenu>().ScrollToSection(section);
			};
		}
	}

	private IEnumerator StartRoutine()
	{
		float startTime = Time.time;
		GameMode selectedGameMode = PlayerDataManager.SelectedGameMode;
		IsLoading = true;
		instance.loadingText.gameObject.SetActive(selectedGameMode == GameMode.Adventure);
		instance.waitingForOpponentText.gameObject.SetActive(selectedGameMode == GameMode.Arena);
		instance.waitingForPlayersText.gameObject.SetActive(selectedGameMode == GameMode.Arena2v2);
		for (int i = 0; i < logos.Length; i++)
		{
			logos[i].enabled = (selectedGameMode == GameMode.Adventure);
		}
		for (int j = 0; j < gameLogoParts.Length; j++)
		{
			gameLogoParts[j].SetNativeSize();
		}
		cancelWaitingForOpponentButton.gameObject.SetActive(PlayerDataManager.IsSelectedGameModePvP);
		if (PlayerDataManager.IsSelectedGameModePvP)
		{
			cancelWaitingForOpponentButton.onClick.RemoveAllListeners();
			cancelWaitingForOpponentButton.onClick.AddListener(delegate
			{
				StopCoroutine(loadingRoutine);
				IsLoading = false;
				TankGame.Running = false;
				PlayerDataManager.SelectedGameMode = GameMode.Adventure;
				ReloadGame(delegate
				{
					PlayerDataManager.SelectedGameMode = selectedGameMode;
					MenuController.HideMenu<GarageMenu>();
					MenuController.ShowMenu<MainMenu>();
				});
			});
		}
		canvasGroup.gameObject.SetActive(value: true);
		canvasGroup.alpha = 1f;
		while (!PlayerDataManager.IsInitialized)
		{
			yield return null;
		}
		yield return null;
		StartCoroutine(AddProgress(0.1f));
		if (TankPrefs.IsInitialized)
		{
			TankPrefs.Save();
		}
		while (selectedGameMode == GameMode.Arena && PlayerDataManager.ArenaMatchData == null)
		{
			if (!BackendManager.IsRequestingChallenge)
			{
				BackendManager.GetChallenge(PlayerDataManager.GetRating());
			}
			yield return null;
		}
		while (selectedGameMode == GameMode.Arena2v2 && PlayerDataManager.ArenaMultiMatchData == null)
		{
			if (!BackendManager.IsRequestingChallenge)
			{
				BackendManager.GetChallenges(PlayerDataManager.GetRating(selectedGameMode), PlayerDataManager.ArenaMultiplayerAICount);
			}
			yield return null;
		}
		StartCoroutine(AddProgress(0.1f));
		cancelWaitingForOpponentButton.onClick.RemoveAllListeners();
		cancelWaitingForOpponentButton.gameObject.SetActive(value: false);
		if (SceneManager.GetSceneByName("Game").isLoaded)
		{
			AsyncOperation unload2 = SceneManager.UnloadSceneAsync("Game");
			while (!unload2.isDone)
			{
				yield return null;
			}
		}
		StartCoroutine(AddProgress(0.1f));
		if (TankGame.Running)
		{
			AsyncOperation unload2 = SceneManager.LoadSceneAsync("Game", LoadSceneMode.Additive);
			while (!unload2.isDone)
			{
				yield return null;
			}
			StartCoroutine(AddProgress(0.2f));
		}
		Action newReloadAction = null;
		if (OnFinish != null)
		{
			OnFinish();
		}
		else if (PlayerDataManager.AppJustStarted && !PlayerDataManager.BeenInAppBefore)
		{
			PlayerDataManager.AppJustStarted = false;
			PlayerDataManager.SelectedGameMode = GameMode.Adventure;
			TankGame.Running = true;
			newReloadAction = delegate
			{
				MenuController.HideMenu<MainMenu>();
				//MenuController.ShowMenu<PrivacyPolicyPopup>();
			};
		}
		if (Time.time - startTime < 1f)
		{
			yield return new WaitForSecondsRealtime(1f - (Time.time - startTime));
		}
		Time.timeScale = 1f;
		StartCoroutine(AddProgress(1f));
		while (loadingProgressImage.fillAmount < 0.99f)
		{
			yield return null;
		}
		IsLoading = false;
		if (newReloadAction != null)
		{
			ReloadGame(delegate
			{
				newReloadAction();
			});
		}
		else
		{
			StartCoroutine(FadeOut());
		}
	}
}
