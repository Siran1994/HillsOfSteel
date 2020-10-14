using UnityEngine;
using UnityEngine.UI;

public class SideMenu : MenuBase<SideMenu> //侧边按钮菜单
{
	public GameObject shopMenuAlert;//商店右上角感叹号

	public Button shopMenuButton; //商店

	//public Button achievementsButton;//成就

	//public Button leaderboardsButton;//排行榜

	public Button settingsButton; //设置

	private void Start()
	{
		shopMenuButton.onClick.AddListener(delegate  //商店按钮回调
		{
            SDKManager.Instance.ShowAd(ShowAdType.ChaPing,1,"点击购物车");
			MenuController.ShowMenu<ShopMenu>();
		});
		//achievementsButton.onClick.AddListener(delegate //成就回调
		//{
		//	if (Social.localUser.authenticated)
		//	{
		//		Social.ShowAchievementsUI();
		//	}
		//	else
		//	{
		//		NeedToBeOnlinePopup.OnlineAction(delegate
		//		{
		//			PlatformManager.ReconnectWithGooglePlay();
		//		});
		//	}
		//});
		//leaderboardsButton.onClick.AddListener(delegate //排行榜回调
		//{
		//	if (Social.localUser.authenticated)
		//	{
		//		Social.ShowLeaderboardUI();
		//	}
		//	else
		//	{
		//		NeedToBeOnlinePopup.OnlineAction(delegate 
		//		{
		//			PlatformManager.ReconnectWithGooglePlay();
		//		});
		//	}
		//});
		settingsButton.onClick.AddListener(delegate  //设置按钮回调
		{
            SDKManager.Instance.ShowAd(ShowAdType.VideoAD, 1, "点击设置");
            MenuController.ShowMenu<SettingsMenu>();
		});
	}

	private void OnEnable()
	{
		shopMenuAlert.SetActive(PlayerDataManager.IsTimeForDailyChestAlert());//获取下次闹钟时间
	}
}
