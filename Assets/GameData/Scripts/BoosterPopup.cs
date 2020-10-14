using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;

public class BoosterPopup : MenuBase<BoosterPopup>
{
	public Sprite crossIcon;

	public Sprite checkmarkIcon;

	public GameObject boosterPopupContainer;

	public Button boosterBackToGarage;

	public Image boosterBackButtonIcon;

	public Button boosterUpgradeVideoAd;//免费获取

	public Button boosterUpgradeCoins;//金币获取

	public TextMeshProUGUI boosterHeaderText;

	public TextMeshProUGUI boosterUpgradeText;

	public TextMeshProUGUI boosterLevelUpText;

	public TextMeshProUGUI boosterUnlockText;

	public TextMeshProUGUI boosterFullyUpgradedText;

	public TextMeshProUGUI boosterUpgradeCoinsText;

	public TextMeshProUGUI boosterPopupBoosterName;

	public Image boosterPopupBoosterIcon;

	public BoosterItem[] boosterItems;

	private string selectedBoosterId;

	private Booster[] currentBoosters;

	private void OnEnable()
	{
		boosterBackButtonIcon.sprite = crossIcon;
		boosterBackButtonIcon.SetNativeSize();
		Tank currentTank = MenuController.GetMenu<GarageMenu>().CurrentTank;
		currentBoosters = PlayerDataManager.GetTankBoosters(currentTank);
		boosterHeaderText.text = ScriptLocalization.Get(currentTank.name + "Boosters");
		selectedBoosterId = PlayerDataManager.GetSelectedBoosterId(currentTank);
		int i = 0;
		if (currentBoosters != null && currentBoosters.Length != 0)
		{
			for (int j = 0; j < currentBoosters.Length; j++)
			{
				Booster cb = currentBoosters[j];
				if (cb.type != 0)
				{
					if (selectedBoosterId == null || selectedBoosterId.Length == 0)
					{
						SetSelectedBooster(cb.id);
					}
					SetBoosterUIData(boosterItems[i], cb, !PlayerDataManager.GetBoosterSeen(cb.id));
					if (cb.Count == 0)
					{
						boosterItems[i].GetComponent<CanvasGroup>().alpha = 0.5f;
					}
					else
					{
						boosterItems[i].GetComponent<CanvasGroup>().alpha = 1f;
					}
					boosterItems[i].button.onClick.RemoveAllListeners();
					boosterItems[i].button.onClick.AddListener(delegate
					{
						SetSelectedBooster(cb.id);
					});
					i++;
				}
			}
		}
		for (; i < boosterItems.Length; i++)
		{
			boosterItems[i].gameObject.SetActive(value: false);
		}
		if (selectedBoosterId != null && selectedBoosterId.Length > 0)
		{
			SetSelectedBooster(selectedBoosterId);
		}
	}

	private void Start()
	{
		boosterBackToGarage.onClick.AddListener(delegate
		{
			MenuController.HideMenu<BoosterPopup>();
		});
		boosterUpgradeCoins.onClick.AddListener(BuyBoosterCoins);
		boosterUpgradeVideoAd.onClick.AddListener(BuyBoosterByVAd);
	}

	private void SetSelectedBooster(string id)
	{
		selectedBoosterId = id;
		if (PlayerDataManager.SetSelectedBooster(Variables.instance.GetTank(PlayerDataManager.GetSelectedTank()), id))
		{
			boosterBackButtonIcon.sprite = checkmarkIcon;
			boosterBackButtonIcon.SetNativeSize();
		}
		Booster booster = PlayerDataManager.GetBooster(id);
		boosterPopupBoosterName.text = $"{ScriptLocalization.Get(booster.tankName)} {ScriptLocalization.Get(booster.type.ToString())}";
		boosterUpgradeCoinsText.text = Variables.instance.GetBoosterLevelUpPriceForNextLevel(booster.Level).ToString();
		boosterPopupBoosterIcon.sprite = booster.bigCard;
		for (int i = 0; i < boosterItems.Length; i++)
		{
			bool flag = boosterItems[i].id.Equals(id);
			boosterItems[i].selected.enabled = flag;
			if (flag)
			{
				SetBoosterUIData(boosterItems[i], booster);
				PlayerDataManager.SetBoosterSeen(booster.id, val: true);
			}
		}
		MenuController.GetMenu<GarageMenu>().SetTankBooster();
		TankPrefs.Save();
		PlayerDataManager.SaveToCloudOnNextInterval = true;
	}

	private void SetBoosterUIData(BoosterItem item, Booster b, bool isNew = false)
	{
		item.gameObject.SetActive(value: true);
		item.SetData(b, b.id.Equals(selectedBoosterId), 0, isNew);
		bool flag = PlayerDataManager.CanLevelUpBooster(b.id);
		boosterLevelUpText.gameObject.SetActive(flag);
		boosterUpgradeVideoAd.gameObject.SetActive(flag && b.Level < 12);
		boosterUpgradeCoins.gameObject.SetActive(flag);
		boosterUnlockText.gameObject.SetActive(b.Count == 0);
		boosterUpgradeText.gameObject.SetActive(b.Count != 0 && !flag && !b.MaxLevel);
		boosterFullyUpgradedText.gameObject.SetActive(b.MaxLevel);
	}

	private void BuyBoosterCoins()
	{
		if (PlayerDataManager.BuyBooster(selectedBoosterId))
		{
			AudioMap.PlayClipAt("upgradeBooster", Vector3.zero, AudioMap.instance.uiMixerGroup);
			AudioMap.PlayClipAt("purchase", Vector3.zero, AudioMap.instance.uiMixerGroup);
			currentBoosters = PlayerDataManager.GetTankBoosters(Variables.instance.GetTank(PlayerDataManager.GetSelectedTank()));
			SetSelectedBooster(selectedBoosterId);
			TankPrefs.Save();
			TankPrefs.CloudSyncComplete = true;
			PlayerDataManager.SaveToCloudOnNextInterval = true;
		}
		else
		{
			MenuController.ShowMenu<OutOfCurrencyPopup>().SetCurrency(CurrencyType.Coins);
		}
	}
    

    #region
    bool isCanShow = false;
    int index = 1;
    int time = 0;
    public void CanShow()
    {
        isCanShow = true;
    }
    void Timer()
    {
        time++;
        if (time == 60)
        {
            time = 0;
        }
        else
        {
            Invoke("Timer", 1.0f);
        }
    }
    #endregion
    public void BuyBoosterByVAd()
    {
        if (isCanShow || index == 1)
        {
            index++;
            isCanShow = false;
            SDKManager.Instance.ShowAd(ShowAdType.Reward, 1, "免费升级",(bool IsComp)=> 
            {
                if (IsComp)
                {
                    BuyBoosterVideoAd();
                }
            });           
            Invoke("CanShow", 60);
            Invoke("Timer", 1.0f);
        }
        else
        {
            Debug.Log("广告请求过于频繁,请在" + (60 - time) + "秒后再试!");
            if (60 - time == 0)
            {
                CancelInvoke("Timer");
            }
            SDKManager.Instance.MakeToast("广告请求过于频繁, 请在" + (60 - time) + "秒后再试!");
        }
    }

    public void BuyBoosterVideoAd()//购买观看视频的提升道具
	{
        AudioMap.PlayClipAt("upgradeBooster", Vector3.zero, AudioMap.instance.uiMixerGroup);
        TankAnalytics.BoughtWithVideoAd("Upgrade " + selectedBoosterId);
        Booster booster = PlayerDataManager.GetBooster(selectedBoosterId);
        int cardsUsed = booster.NextLevelCount - booster.ThisLevelCount;
        TankAnalytics.BoosterUpgraded(booster.id, cardsUsed);
        PlayerDataManager.AddBoosterLevel(selectedBoosterId);
        currentBoosters = PlayerDataManager.GetTankBoosters(Variables.instance.GetTank(PlayerDataManager.GetSelectedTank()));
        SetSelectedBooster(selectedBoosterId);
    }
}

