using UnityEngine;
using UnityEngine.UI;

public class SideMenu : MenuBase<SideMenu> //��߰�ť�˵�
{
	public GameObject shopMenuAlert;//�̵����ϽǸ�̾��

	public Button shopMenuButton; //�̵�

	//public Button achievementsButton;//�ɾ�

	//public Button leaderboardsButton;//���а�

	public Button settingsButton; //����

	private void Start()
	{
		shopMenuButton.onClick.AddListener(delegate  //�̵갴ť�ص�
		{
            SDKManager.Instance.ShowAd(ShowAdType.ChaPing,1,"������ﳵ");
			MenuController.ShowMenu<ShopMenu>();
		});
		//achievementsButton.onClick.AddListener(delegate //�ɾͻص�
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
		//leaderboardsButton.onClick.AddListener(delegate //���а�ص�
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
		settingsButton.onClick.AddListener(delegate  //���ð�ť�ص�
		{
            SDKManager.Instance.ShowAd(ShowAdType.VideoAD, 1, "�������");
            MenuController.ShowMenu<SettingsMenu>();
		});
	}

	private void OnEnable()
	{
		shopMenuAlert.SetActive(PlayerDataManager.IsTimeForDailyChestAlert());//��ȡ�´�����ʱ��
	}
}
