using UnityEngine;
using UnityEngine.UI;

public class SubscriptionOfferPopup : MenuBase<SubscriptionOfferPopup>
{
	public IAPItem iapItem;

	public Button okButton;//ȷ������

	public GameObject offerContent;

	public GameObject subscribedContent;

	private string source = "OfferPopup";

	private bool bought;

	private void Start()
    {
        

        okButton.onClick.AddListener(delegate
		{
            Time.timeScale = 0;
            if (PlayerDataManager.GetGems()>500)
            {
                PlayerDataManager.AddGems(-500);

                Debug.Log("���Ʒѿ�Ǯ");
                PlayerPrefs.SetInt("Isbought", 1);//�ѹ���
                MenuController.HideMenu<SubscriptionOfferPopup>();
                MenuController.ShowMenu<PurchaseRestorePopup>();
                Debug.Log(PlayerDataManager.GetGems());
            }
            else //��ʯ����
            {
                MenuController.ShowMenu<OutOfCurrencyPopup>().SetCurrency(CurrencyType.Gems);
               // MenuController.HideMenu<SubscriptionOfferPopup>();
               // MenuController.HideMenu<PurchaseRestorePopup>();
            }

            MenuController.UpdateTopMenu();


        });
	}

	public void Init(string from)
	{
		source = from;
		iapItem.SetOnComplete(delegate
		{
			bought = true;
			offerContent.SetActive(value: false);
			subscribedContent.SetActive(value: true);
			TankAnalytics.BoughtDoubleSubscription(source);
		});
	}

	public void ShowSubscribed()
	{
		offerContent.SetActive(value: false);
		subscribedContent.SetActive(value: true);
	}

	private void OnEnable()
	{
		if (PlayerDataManager.IsSubscribed())
		{
			offerContent.SetActive(value: false);
			subscribedContent.SetActive(value: true);
		}
		TankPrefs.SetInt("subOfferSeen", 1);
		TankPrefs.Save();
	}

    private void OnDisable()
    {
        if (bought)
        {
            if (MenuController.GetMenu<ChestPopup>().isActiveAndEnabled&& PlayerPrefs.GetInt("Isbought") == 1)
            {
                MenuController.GetMenu<ChestPopup>().DoubleCurrentRewards();
            }
            if (MenuController.GetMenu<ShopMenu>().isActiveAndEnabled)
            {
                MenuController.GetMenu<ShopMenu>().HideSection(ShopMenu.Section.Subscriptions);
            }
        }
        else if (MenuController.GetMenu<ChestPopup>().isActiveAndEnabled)
        {
            MenuController.GetMenu<ChestPopup>().DoubleRewardsDenied();
        }
    }
}
