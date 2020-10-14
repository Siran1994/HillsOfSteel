using System.Collections;
using UnityEngine;

public class MainManager : Manager<MainManager>
{
	//public AdsManager adsManager;

	//public AnalyticsManager analyticsManager;

	public BackendManager backendManager;

	public IAPManager iapManager;

	//public PlatformManager platformManager;

	public PlayerDataManager playerDataManager;

	public NotificationManager notificationManager;

	public Variables variables;

	public AudioMap audioMap;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Startup()
	{
        Debug.Log("日志总开关");
       // PlayerPrefs.SetInt("Isbought", 0);//默认未购买
        if (Manager<MainManager>.instance == null)
		{
			UnityEngine.Debug.unityLogger.logEnabled = true;
		}
	}

	private void Awake()
	{
		Manager<MainManager>.instance = this;
		Object.DontDestroyOnLoad(base.gameObject);
		//analyticsManager.Initialize();
		StartCoroutine(LoadingScreen.AddProgress(0.15f));
		//platformManager.Initialize();
		backendManager.Initialize();
		TankPrefs.Initialize();
		playerDataManager.Initialize();
	}

	private IEnumerator Start()
	{
		notificationManager.Initialize();
		StartCoroutine(LoadingScreen.AddProgress(0.05f));
		while (MenuController.instance == null && !MenuController.GetMenu<MainMenu>().gameObject.activeInHierarchy)
		{
			yield return null;
		}
		iapManager.Initialize();
		//adsManager.Initialize();
	}

	private void OnApplicationPause(bool pause)
	{
		if (pause)
		{
			PlayerDataManager.HandleDailyChestNotification();
		}
		else if (notificationManager.IsInitialized)
		{
			NotificationManager.RemoveNotificationsOfType("chestUnopened");
		}
	}

	private void OnApplicationQuit()
	{
		PlayerDataManager.HandleDailyChestNotification();
	}
}
