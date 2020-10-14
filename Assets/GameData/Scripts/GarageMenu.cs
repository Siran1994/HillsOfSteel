using I2.Loc;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class GarageMenu : MenuBase<GarageMenu>
{
	[Header("Settings")]
	public Sprite defaultBoosterIcon;

	public RectTransform lockBolt;

	[Header("Garage Background")]
	public GameObject garageContainer;

	public Camera garageCamera;

	public GameObject[] garageBackgrounds;

	public GameObject tankSlider;

	[Header("Tank")]
	public PaintItem[] paintItems;

	public TankContainer[] tanks; //所有的坦克数组

	public CustomScrollRect tankScrollRect;

	public float tankOffset;

	public float tankCameraOffset;

	public TextMeshProUGUI tankNameText;//坦克名称

	public TextMeshProUGUI upgradePriceText;

	public TextMeshProUGUI unlockPriceText;

	public TextMeshProUGUI originalPriceText;

	public TextMeshProUGUI levelUpText;

	public CardElement tankCard;

	public Image armorProgressionSlider;

	public Image gunProgressionSlider;

	public Image engineProgressionSlider;

	public GameObject lockedContainer;

	public GameObject upgradeHelperContainer;

	public GameObject unlockHelperContainer;

	public GameObject unlockNowTextContainer;

	[Header("Boosters")]
	public GameObject garageBoosterContainer;

	public Image garageBoosterIcon;

	public Image garageBoosterUpgradeAvailableIcon;

	public Image garageBoosterNewAvailableIcon;

	[Header("Buttons")]
	public Button openBoosterPopupButton;//打开升级道具面板按钮

	public Button nextTankButton;

	public Button prevTankButton;

	public Button upgradeButton;//升级按钮

	public Button unlockButton;

	public Button playButton; //冒险模式开始按钮

	[Header("Particles")]
	public ParticleSystem sparkleParticles;

	[Header("Easter")]
	public GameObject easterEggPrefab;

	public Transform[] easterEggSpawnLocations;

	private int tankIndex;

	private TankProgression.Stats maxProgression;

	private TankProgression.Stats tankProgression;

	private Booster[] currentBoosters;

	private Coroutine garageShootRoutine;

	private Coroutine tankFlashRoutine;

	private ShopMenu.ShopItem[] dailyOffers;

	private bool popupShownOnPreviousOpen;

	public Tank CurrentTank
	{
		get;
		private set;
	}

    void TanKName(int Index)
    {
        switch (Index)
        {
            case 0:
                tankNameText.text = "眼镜蛇";
                break;
            case 1:
                tankNameText.text = "小丑";
                break;
            case 2:
                tankNameText.text = "泰坦";
                break;
            case 3:
                tankNameText.text = "凤凰";
                break;
            case 4:
                tankNameText.text = "收割者";
                break;
            case 5:
                tankNameText.text = "梭子鱼";
                break;
            case 6:
                tankNameText.text = "特斯拉";
                break;
            case 7:
                tankNameText.text = "猛犸象";
                break;
            case 8:
                tankNameText.text = "阿拉奇诺";
                break;
        }


    }

    public GameObject EasterEgg
	{
		get;
		private set;
	}

	private void OnEnable()
	{
		PlayerDataManager.SelectedArenaLevel = Random.Range(0, Variables.instance.levels.Count - 1);
		dailyOffers = new ShopMenu.ShopItem[0];
		if (PlayerDataManager.HasActiveDailyOffer())
		{
			dailyOffers = PlayerDataManager.GetActiveDailyOffers();
		}
		garageContainer.SetActive(value: true);
		for (int i = 0; i < tanks.Length; i++)
		{
			tanks[i].ResetCannon();
			int num = Variables.instance.tankOrder[i];
			tanks[i].BulletTypeIndex = num;
			tanks[i].BulletDef = Variables.instance.tanks[num].bullet;
		}
		int num2 = (PlayerDataManager.SelectedGameMode == GameMode.Adventure || PlayerDataManager.SelectedGameMode == GameMode.Classic) ? PlayerDataManager.GetSelectedLevel(PlayerDataManager.SelectedGameMode) : PlayerDataManager.SelectedArenaLevel;
		for (int j = 0; j < garageBackgrounds.Length; j++)
		{
			garageBackgrounds[j].SetActive(j == num2);
		}
		SetTank(PlayerDataManager.GetSelectedTank());
		nextTankButton.gameObject.SetActive(tankIndex != tanks.Length - 1);
		prevTankButton.gameObject.SetActive(tankIndex != 0);
		StartCoroutine(StatSlider());
		StartCoroutine(TankScrollRoutine());
		FinishTankFlash();
		MenuController.Delay(0.1f, delegate
		{
			Product bundleProduct = null;
			if (PlayerDataManager.IsTimeForDailyBonus())
			{
				popupShownOnPreviousOpen = true;
				MenuController.ShowMenu<RewardCalendarPopup>();
			}
			else if (popupShownOnPreviousOpen)
			{
				popupShownOnPreviousOpen = false;
			}
			else if (PlayerDataManager.IsTimeToShowBundle(out bundleProduct))
			{
				popupShownOnPreviousOpen = true;
				MenuController.ShowMenu<BundlePopup>().SetIAP(bundleProduct);
			}
			else if (TankPrefs.GetInt("subOfferSeen") == 0)
			{
				popupShownOnPreviousOpen = true;
				MenuController.ShowMenu<SubscriptionOfferPopup>().Init("GarageOffer");
			}
			else if (PlayerDataManager.IsTimeForFacebookPrompt())
			{
				popupShownOnPreviousOpen = true;
				//MenuController.ShowMenu<FacebookPopup>();
			}
			else if (PlayerDataManager.AskRating())
			{
				popupShownOnPreviousOpen = true;
				//MenuController.ShowMenu<RatingPopup>();
			}
		});
	}

	private void Start()
	{
        playButton.onClick.AddListener(Play);
		paintItems[0].setButton.onClick.AddListener(delegate
		{
			SetTankSkin(0);
		});
		paintItems[0].buyButton.onClick.AddListener(delegate
		{
			BuyTankSkin(0);
		});
		paintItems[1].setButton.onClick.AddListener(delegate
		{
			SetTankSkin(1);
		});
		paintItems[1].buyButton.onClick.AddListener(delegate
		{
			BuyTankSkin(1);
		});
		paintItems[2].setButton.onClick.AddListener(delegate
		{
			SetTankSkin(2);
		});
		paintItems[2].buyButton.onClick.AddListener(delegate
		{
			BuyTankSkin(2);
		});
		nextTankButton.onClick.AddListener(NextTank);
		prevTankButton.onClick.AddListener(PrevTank);
		openBoosterPopupButton.onClick.AddListener(delegate
		{
            SDKManager.Instance.ShowAd(ShowAdType.ChaPing, 1, "点击升级道具");
            MenuController.ShowMenu<BoosterPopup>();
		});
		upgradeButton.onClick.AddListener(BuyUpgrade);
		unlockButton.onClick.AddListener(BuyUpgrade);
	}

	private void OnDisable()
	{
		if (garageContainer != null)
		{
			garageContainer.SetActive(value: false);
		}
		if (garageShootRoutine != null)
		{
			StopCoroutine(garageShootRoutine);
			TankContainer tankContainer = tanks[tankIndex];
			if (tankContainer.FlameRoutine != null)
			{
				tankContainer.StopCoroutine(tankContainer.FlameRoutine);
			}
			tanks[tankIndex].FlameRoutine = null;
			if (tankContainer.BulletRoutine != null)
			{
				tankContainer.StopCoroutine(tankContainer.BulletRoutine);
			}
			tanks[tankIndex].BulletRoutine = null;
			if (tankContainer.LightningRoutine != null)
			{
				tankContainer.StopCoroutine(tankContainer.LightningRoutine);
			}
			tanks[tankIndex].LightningRoutine = null;
			for (int i = 0; i < tanks.Length; i++)
			{
				tanks[i].Shoot(val: false);
				tanks[i].StopShootSound();
			}
		}
	}

	private void Update()
	{
        UpdateUpgradeData();
        TanKName(tankIndex);

    }

    #region 坦克试玩
    public GameObject TryPanel;

    public static bool IsConfirm = false;

    public void ToTry()
    {
        IsConfirm = true;
        if (garageShootRoutine != null)
        {
            StopCoroutine(garageShootRoutine);
            for (int i = 0; i < tanks.Length; i++)
            {
                tanks[i].Shoot(val: false);
            }
        }
        AudioMap.PlayClipAt(AudioMap.instance["tankIgnition"], Vector3.zero, AudioMap.instance.uiMixerGroup).transform.parent = MenuController.instance.transform;
        FinishTankFlash();
        SetTankSkin(PlayerDataManager.GetSelectedSkin(Variables.instance.GetTank(PlayerPrefs.GetInt("TanIndex"))));
        TankAnalytics.Play(Variables.instance.levels[PlayerDataManager.GetSelectedLevel(PlayerDataManager.SelectedGameMode)].name, Variables.instance.tanks[PlayerDataManager.GetSelectedTank()].name);
        TankGame.Running = true;
        if (PlayerDataManager.SelectedGameMode == GameMode.Classic || PlayerDataManager.SelectedGameMode == GameMode.BossRush)
        {
            MenuController.HideMenu<ClassicModeMenu>();
        }
        LoadingScreen.ReloadGame(delegate
        {
            //SDKManager.Instance.RepeatShowBan(15,15);//展示Bannel
            MenuController.HideMenu<GarageMenu>();
            TankGame.Running = true;
        });

        TryPanel.SetActive(false);
    }

    public void Confirm()// 714 TankGame
    {
       // SDKManager.Instance.MakeToast("暂无广告!!!");
        SDKManager.Instance.ShowAd(ShowAdType.Reward, 1, "点击试玩坦克",(bool IsComplete)=> 
        {
            if (IsComplete)
            {
                ToTry();
            }
        });       
    }
    public void Cancel()
    {
        SDKManager.Instance.ShowAd(ShowAdType.VideoAD, 1, "冒险模式点开始");
        IsConfirm = false;
        if (garageShootRoutine != null)
        {
            StopCoroutine(garageShootRoutine);
            for (int i = 0; i < tanks.Length; i++)
            {
                tanks[i].Shoot(val: false);
            }
        }
        AudioMap.PlayClipAt(AudioMap.instance["tankIgnition"], Vector3.zero, AudioMap.instance.uiMixerGroup).transform.parent = MenuController.instance.transform;
        FinishTankFlash();
        SetTankSkin(PlayerDataManager.GetSelectedSkin(Variables.instance.GetTank(tankIndex)));
        TankAnalytics.Play(Variables.instance.levels[PlayerDataManager.GetSelectedLevel(PlayerDataManager.SelectedGameMode)].name, Variables.instance.tanks[PlayerDataManager.GetSelectedTank()].name);
        TankGame.Running = true;
        if (PlayerDataManager.SelectedGameMode == GameMode.Classic || PlayerDataManager.SelectedGameMode == GameMode.BossRush)
        {
            MenuController.HideMenu<ClassicModeMenu>();
        }
        LoadingScreen.ReloadGame(delegate
        {
            //SDKManager.Instance.RepeatShowBan(15,15);//展示Bannel
            MenuController.HideMenu<GarageMenu>();
            TankGame.Running = true;
        });
        TryPanel.SetActive(false);
    }

    private void Play()//开始游戏
    {
        TryPanel.SetActive(true);
	}
    #endregion
    private IEnumerator TankFlash(TankContainer tank)
	{
		sparkleParticles.Play();
		SpriteRenderer r = tank.GetComponent<SpriteRenderer>();
		MeshRenderer cr = null;
		if (tank.GetComponentInChildren<Chain>() != null)
		{
			cr = tank.GetComponentInChildren<Chain>().GetComponent<MeshRenderer>();
		}
		int propertyId = Shader.PropertyToID("_Amount");
		float num = 1.5f;
		float time = 0f;
		float stageTime = num / 2f;
		while (time < stageTime)
		{
			r.sharedMaterial.SetFloat(propertyId, time / stageTime);
			if (cr != null)
			{
				cr.material.SetFloat(propertyId, time / stageTime);
			}
			time += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		while (time > 0f)
		{
			r.sharedMaterial.SetFloat(propertyId, time / stageTime);
			if (cr != null)
			{
				cr.material.SetFloat(propertyId, time / stageTime);
			}
			time -= Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		FinishTankFlash(stopCoroutine: false);
	}

	private void FinishTankFlash(bool stopCoroutine = true)
	{
		if (stopCoroutine && tankFlashRoutine != null)
		{
			StopCoroutine(tankFlashRoutine);
			tankFlashRoutine = null;
		}
		TankContainer[] array = tanks;
		foreach (TankContainer tankContainer in array)
		{
			if (!(tankContainer.GetComponentInChildren<Chain>() == null))
			{
				SpriteRenderer component = tankContainer.GetComponent<SpriteRenderer>();
				MeshRenderer component2 = tankContainer.GetComponentInChildren<Chain>().GetComponent<MeshRenderer>();
				int nameID = Shader.PropertyToID("_Amount");
				component.sharedMaterial.SetFloat(nameID, 0f);
				component2.material.SetFloat(nameID, 0f);
			}
		}
	}

	private void NextTank()
	{
		SetTank(tankIndex + 1);
    }

	private void PrevTank()
	{
		SetTank(tankIndex - 1);
    }

	public void SetTank(int index)
	{
		if (index >= 0 && index < tanks.Length)
		{
			CurrentTank = Variables.instance.GetTank(index);
			bool flag = PlayerDataManager.IsTankLocked(CurrentTank);
			lockedContainer.SetActive(flag);
			if (!flag)
			{
				PlayerDataManager.SetSelectedTank(index);
			}
			tankIndex = index;
			nextTankButton.gameObject.SetActive(tankIndex != tanks.Length - 1);
			prevTankButton.gameObject.SetActive(tankIndex != 0);
			playButton.gameObject.SetActive(!flag);
			UpdateUpgradeData();
			currentBoosters = PlayerDataManager.GetTankBoosters(CurrentTank);
			SetTankBooster();
			openBoosterPopupButton.gameObject.SetActive(PlayerDataManager.GetTankCardCount(CurrentTank) > 0);
			for (int i = 0; i < paintItems.Length; i++)
			{
				bool flag2 = PlayerDataManager.SkinLocked(CurrentTank, i);
				paintItems[i].priceText.text = CurrentTank.tankSkins[i].price.ToString();
				paintItems[i].nameText.text = ScriptLocalization.Get(CurrentTank.tankSkins[i].name);
				paintItems[i].previewImage.sprite = CurrentTank.tankSkins[i].uiPreview;
				paintItems[i].buyButton.gameObject.SetActive(flag2 && !PlayerDataManager.IsTankLocked(Variables.instance.GetTank(index)));
				paintItems[i].okImage.enabled = false;
				paintItems[i].lockImage.enabled = false;
				paintItems[i].frame.enabled = false;
			}
			SetTankSkin(PlayerDataManager.GetSelectedSkin(CurrentTank));
		}
	}

	private void BuyUpgrade()
	{
		if (!PlayerDataManager.IsTankLocked(CurrentTank) && !PlayerDataManager.IsTankFullyUpgraded(CurrentTank))
		{
			int coinsNeededForLevelUp = PlayerDataManager.GetCoinsNeededForLevelUp(CurrentTank);
			if (PlayerDataManager.BuyTankUpgrade(CurrentTank))
			{
				MenuController.instance.StartCoroutine(AudioManager.AllUpgradedSound());
				tankFlashRoutine = StartCoroutine(TankFlash(tanks[tankIndex]));
				TankAnalytics.BoughtWithSoftCurrency("upgrade", "Upgrade for " + CurrentTank.name, coinsNeededForLevelUp);
				TankPrefs.CloudSyncComplete = true;
				PlayerDataManager.SaveToCloudOnNextInterval = true;
			}
			else if (PlayerDataManager.GetCoins() < coinsNeededForLevelUp)
			{
				MenuController.ShowMenu<OutOfCurrencyPopup>().SetCurrency(CurrencyType.Coins);
			}
			UpdateUpgradeData(flashUpgradeText: true);
		}
		else
		{
			if (PlayerDataManager.IsTankFullyUpgraded(CurrentTank))
			{
				return;
			}
			int num = Variables.instance.GetTankGemValue(CurrentTank);
			if (PlayerDataManager.HasActiveDailyOffer())
			{
				for (int i = 0; i != dailyOffers.Length; i++)
				{
					if (dailyOffers[i] != null && dailyOffers[i].id.Equals(CurrentTank.id) && dailyOffers[i].discount > 0 && dailyOffers[i].currency == CurrencyType.Gems)
					{
						num = dailyOffers[i].price;
						break;
					}
				}
			}
			if (PlayerDataManager.TakeGems(num))
			{
				PlayerDataManager.AddTankCards(CurrentTank, 1);
				StartCoroutine(UnlockRoutine());
				tankFlashRoutine = StartCoroutine(TankFlash(tanks[tankIndex]));
				TankAnalytics.BoughtWithPremiumCurrency("tank", CurrentTank.id, num);
				TankPrefs.SaveAndSendToCloud(forced: true);
			}
			else
			{
				MenuController.ShowMenu<OutOfCurrencyPopup>().SetCurrency(CurrencyType.Gems);
			}
			UpdateUpgradeData(flashUpgradeText: true);
		}
	}

	private void UpdateUpgradeData(bool flashUpgradeText = false)
	{
		tankNameText.text = ScriptLocalization.Get(CurrentTank.name);
      //  Debug.Log("当前坦克是:"+ScriptLocalization.Get(CurrentTank.name));
        bool flag = PlayerDataManager.IsTankLocked(CurrentTank);
		bool flag2 = PlayerDataManager.GetTankCardCount(CurrentTank) >= PlayerDataManager.GetCardsNeededForLevelUpCumulative(CurrentTank) && PlayerDataManager.GetTankUpgradeLevel(CurrentTank) < Variables.instance.tankLevelMinMax.max;
		int coinsNeededForLevelUp = PlayerDataManager.GetCoinsNeededForLevelUp(CurrentTank);
		levelUpText.text = ScriptLocalization.Get("LevelUp");
		if (flag)
		{
			int num = Variables.instance.GetTankGemValue(CurrentTank);
			if (PlayerDataManager.HasActiveDailyOffer())
			{
				for (int i = 0; i != dailyOffers.Length; i++)
				{
					if (dailyOffers[i] != null && dailyOffers[i].id.Equals(CurrentTank.id) && dailyOffers[i].discount > 0 && dailyOffers[i].currency == CurrencyType.Gems)
					{
						num = dailyOffers[i].price;
						break;
					}
				}
			}
			unlockPriceText.text = num.ToString();
		}
		upgradeButton.interactable = flag2;
		upgradeButton.gameObject.SetActive(!flag);
		upgradeHelperContainer.SetActive(!flag2 && !flag && PlayerDataManager.GetTankUpgradeLevel(CurrentTank) < Variables.instance.tankLevelMinMax.max);
		levelUpText.gameObject.SetActive(!flag);
		unlockButton.gameObject.SetActive(flag);
		unlockHelperContainer.SetActive(flag);
		upgradePriceText.text = ((coinsNeededForLevelUp > 0) ? "<size=50%>"+coinsNeededForLevelUp.ToString() : ScriptLocalization.Max);
		tankCard.SetValues(CurrentTank, PlayerDataManager.GetTankCardCount(CurrentTank), useNew: false, useStackSize: false, deductCount: false, useTankName: false);
		maxProgression = Variables.instance.GetMaxProgression().maxStep;
		tankProgression = CurrentTank.GetProgression(PlayerDataManager.GetTankUpgradeLevel(CurrentTank));
		if (!MenuController.GetMenu<RewardCalendarPopup>().isActiveAndEnabled)
		{
			MenuController.UpdateTopMenu();
		}
	}

	public void SetTankBooster()
	{
		Tank tank = Variables.instance.GetTank(tankIndex);
		Booster selectedBooster = PlayerDataManager.GetSelectedBooster(Variables.instance.GetTank(tankIndex));
		if (selectedBooster.Count == 0 || selectedBooster.type == BoosterGameplayType.None)
		{
			bool flag = false;
			for (int i = 0; i < currentBoosters.Length; i++)
			{
				if (currentBoosters[i].Count > 0)
				{
					PlayerDataManager.SetSelectedBooster(tank, currentBoosters[i].id);
					garageBoosterIcon.sprite = currentBoosters[i].card;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				garageBoosterIcon.sprite = defaultBoosterIcon;
			}
		}
		else
		{
			garageBoosterIcon.sprite = selectedBooster.card;
		}
		garageBoosterUpgradeAvailableIcon.enabled = false;
		if (!PlayerDataManager.IsTankLocked(tank))
		{
			for (int j = 0; j < currentBoosters.Length; j++)
			{
				if (Variables.instance.CanUpgradeBooster(currentBoosters[j]))
				{
					garageBoosterUpgradeAvailableIcon.enabled = true;
					break;
				}
			}
		}
		garageBoosterNewAvailableIcon.gameObject.SetActive(value: false);
		if (!PlayerDataManager.IsTankLocked(tank))
		{
			for (int k = 0; k < currentBoosters.Length; k++)
			{
				if (!PlayerDataManager.GetBoosterSeen(currentBoosters[k].id))
				{
					garageBoosterNewAvailableIcon.gameObject.SetActive(value: true);
					break;
				}
			}
		}
		tanks[tankIndex].SetTankBooster(selectedBooster);
	}

	private void SetTankSkin(int skin)
	{
		if (paintItems[skin].lockImage.enabled)
		{
			BuyTankSkin(skin);
		}
		else
		{
			SetSkinInfo(skin);
		}
	}

	private void BuyTankSkin(int skin)
	{
		if (!PlayerDataManager.IsTankLocked(Variables.instance.GetTank(tankIndex)))
		{
			if (PlayerDataManager.BuySkin(Variables.instance.GetTank(tankIndex), skin))
			{
				MenuController.instance.StartCoroutine(AudioManager.AllUpgradedSound());
				PlayerDataManager.SetSelectedSkin(Variables.instance.GetTank(tankIndex), skin);
				SetSkinInfo(skin);
				TankPrefs.CloudSyncComplete = true;
				MenuController.UpdateTopMenu();
			}
			else
			{
				MenuController.ShowMenu<OutOfCurrencyPopup>().SetCurrency(CurrencyType.Coins);
			}
		}
	}

	private void SetSkinInfo(int skin)
	{
		tanks[tankIndex].SetSkin(Variables.instance.GetTank(tankIndex).tankSkins[skin]);
		bool flag = false;
		for (int i = 0; i < paintItems.Length; i++)
		{
			flag = PlayerDataManager.SkinLocked(Variables.instance.GetTank(tankIndex), i);
			paintItems[i].frame.enabled = false;
			paintItems[i].lockImage.enabled = false;
			paintItems[i].okImage.enabled = false;
			paintItems[i].frame.enabled = false;
			paintItems[i].buyButton.gameObject.SetActive(flag && !PlayerDataManager.IsTankLocked(Variables.instance.GetTank(tankIndex)));
		}
		paintItems[skin].frame.enabled = true;
		flag = PlayerDataManager.SkinLocked(Variables.instance.GetTank(tankIndex), skin);
		paintItems[skin].okImage.enabled = !flag;
		paintItems[skin].lockImage.enabled = flag;
		paintItems[skin].buyButton.gameObject.SetActive(flag && !PlayerDataManager.IsTankLocked(Variables.instance.GetTank(tankIndex)));
		if (!flag)
		{
			PlayerDataManager.SetSelectedSkin(Variables.instance.GetTank(tankIndex), skin);
		}
	}

	private IEnumerator StatSlider()
	{
		while (maxProgression.dps == 0f)
		{
			yield return null;
		}
		float currentArmor = tankProgression.health / maxProgression.health;
		float currentGun = tankProgression.dps / maxProgression.dps;
		if (CurrentTank.bullet.type == BulletType.Missile)
		{
			currentGun = tankProgression.damage / CurrentTank.bullet.perMissileTime / (1f + tankProgression.reloadTime) / maxProgression.dps;
		}
		float currentEngine = (tankProgression.acceleration / maxProgression.acceleration + tankProgression.maxSpeed / maxProgression.maxSpeed) / 2f;
		float armorVel = 0f;
		float gunVel = 0f;
		float engineVel = 0f;
		while (true)
		{
			float target = tankProgression.health / maxProgression.health;
			float target2 = tankProgression.dps / maxProgression.dps;
			if (CurrentTank.bullet.type == BulletType.Missile)
			{
				target2 = tankProgression.damage / CurrentTank.bullet.perMissileTime / (1f + tankProgression.reloadTime) / maxProgression.dps;
			}
			float num = 1f;
			if (CurrentTank.type == PlayerTankType.Arachno)
			{
				num = 200f;
			}
			float target3 = num * (tankProgression.acceleration / maxProgression.acceleration + tankProgression.maxSpeed / maxProgression.maxSpeed) / 2f;
			currentArmor = Mathf.SmoothDamp(currentArmor, target, ref armorVel, 0.25f);
			currentGun = Mathf.SmoothDamp(currentGun, target2, ref gunVel, 0.25f);
			currentEngine = Mathf.SmoothDamp(currentEngine, target3, ref engineVel, 0.25f);
			float width = armorProgressionSlider.rectTransform.parent.GetComponent<RectTransform>().rect.width;
			Vector2 a = new Vector2(width, 0f);
			armorProgressionSlider.rectTransform.anchoredPosition = a * currentArmor;
			gunProgressionSlider.rectTransform.anchoredPosition = a * currentGun;
			engineProgressionSlider.rectTransform.anchoredPosition = a * currentEngine;
			yield return new WaitForEndOfFrame();
		}
	}

	private IEnumerator TankScrollRoutine()
	{
		float velocity = 0f;
		float multiple = 1f / (float)(tanks.Length - 1);
		int lastIndex = -1;
		float timeSinceLast = 0f;
		CustomScrollRect sr = tankScrollRect;
		sr.horizontalNormalizedPosition = (float)tankIndex * multiple;
		while (true)
		{
			float target = (float)tankIndex * multiple;
			int curr = tankIndex;
			if (!tankScrollRect.IsDragging)
			{
				sr.horizontalNormalizedPosition = Mathf.SmoothDamp(sr.horizontalNormalizedPosition, target, ref velocity, 0.1f);
			}
			else
			{
				SetTank(Mathf.RoundToInt(Mathf.Clamp01(sr.horizontalNormalizedPosition + Mathf.Sign(sr.velocity.x) * (0f - multiple) * 0.4f) / multiple));
			}
			Vector3 localPosition = tankSlider.transform.localPosition;
			localPosition.x = sr.horizontalNormalizedPosition * tankOffset * (float)(tanks.Length - 1) + tankCameraOffset;
			tankSlider.transform.localPosition = localPosition;
			if (curr != lastIndex && !MenuController.GetMenu<BundlePopup>().isActiveAndEnabled)
			{
				if (garageShootRoutine != null)
				{
					StopCoroutine(garageShootRoutine);
					for (int i = 0; i < tanks.Length; i++)
					{
						tanks[i].Shoot(val: false);
					}
				}
				if (timeSinceLast > 0.75f)
				{
					Tank tank = Variables.instance.GetTank(curr);
					if (tank.bullet.type == BulletType.Missile || tank.bullet.type == BulletType.Laser)
					{
						garageShootRoutine = StartCoroutine(MissileShoot(tanks[curr]));
					}
					else if (tank.bullet.type == BulletType.Flame)
					{
						garageShootRoutine = StartCoroutine(FlamerShoot(tanks[curr]));
					}
					else if (tank.bullet.type == BulletType.Small)
					{
						garageShootRoutine = StartCoroutine(BulletShoot(tanks[curr]));
					}
					else if (tank.bullet.type == BulletType.Lightning)
					{
						garageShootRoutine = StartCoroutine(LightningShoot(tanks[curr]));
					}
					else
					{
						tanks[curr].Shoot();
					}
					lastIndex = tankIndex;
				}
				else
				{
					timeSinceLast += Time.deltaTime;
				}
			}
			yield return null;
			if (curr != tankIndex)
			{
				timeSinceLast = 0f;
				lastIndex = -1;
			}
		}
	}

	public IEnumerator MissileShoot(TankContainer tank)
	{
		Tank tank2 = Variables.instance.GetTank(tankIndex);
		tank.Shoot();
		yield return new WaitForSeconds(tank2.bullet.maxTime);
		tank.Shoot(val: false);
	}

	public IEnumerator LightningShoot(TankContainer tank)
	{
		tank.ReloadTime = 0f;
		tank.Shoot();
		yield return new WaitForSeconds(2f);
		tank.Shoot(val: false);
	}

	public IEnumerator FlamerShoot(TankContainer tank)
	{
		tank.ReloadTime = 0f;
		tank.Shoot();
		yield return new WaitForSeconds(0.75f);
		tank.Shoot(val: false);
	}

	public IEnumerator BulletShoot(TankContainer tank)
	{
		tank.Shoot();
		yield return new WaitForSeconds(1.25f);
		tank.Shoot(val: false);
	}

	private IEnumerator UnlockRoutine()
	{
		int boughtIndex = tankIndex;
		Tank boughtTank = CurrentTank;
		float time = 0.2f;
		Vector2 boltPos = lockBolt.anchoredPosition;
		for (float t = 0f; t < time; t += Time.deltaTime)
		{
			lockBolt.anchoredPosition = Vector2.Lerp(boltPos, boltPos + Vector2.up * 40f, LeanTween.easeInExpo(0f, 1f, t / time));
			yield return null;
		}
		yield return new WaitForSeconds(0.5f);
		SetTank(boughtIndex);
		MenuController.ShowMenu<NewCardPopup>().Init(boughtTank);
		lockBolt.anchoredPosition = boltPos;
	}
}
