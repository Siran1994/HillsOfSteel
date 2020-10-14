using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MenuBase<PauseMenu>
{
	public Button pauseBackToMainButton;//返回主界面

	public Button resumeButton;

	public Button pauseSoundToggleButton;

	public Image pauseSoundToggleImage;

	public Sprite soundOffImage;

	public Sprite soundOnImage;

	private void Start()
	{
		resumeButton.onClick.AddListener(delegate
		{
			MenuController.HideNewestMenu();
		});
		pauseBackToMainButton.onClick.AddListener(delegate
		{
            //SDKManager.Instance.CloseBanner();//关闭Banner
			PlayerDataManager.SelectedGameMode = GameMode.Adventure;
			TankGame.Running = false;
			MenuController.HideMenu<PauseMenu>();
			MenuController.HideMenu<GameMenu>();
			LoadingScreen.ReloadGame(delegate
			{
				MenuController.ShowMenu<MainMenu>();
				MusicManager.CrossFadeToMenu();
			});
		});
		pauseSoundToggleButton.onClick.AddListener(delegate
		{
			AudioManager.ToggleSound();
		});
	}

	private void OnEnable()
	{
		AudioManager.SetGameAudioTo(-80f);
		Time.timeScale = 0f;
	}

	private void OnDisable()
	{
		AudioManager.SetGameAudioTo(0f);
		Time.timeScale = 1f;
	}

	private void Update()
	{
		pauseSoundToggleImage.sprite = (AudioManager.IsSoundOn() ? soundOnImage : soundOffImage);
	}
}
