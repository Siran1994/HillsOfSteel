using I2.Loc;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
	public delegate void OnBackButtonPress();

	[Serializable]
	private struct VisibleMenu
	{
		public GameObject gameObject;

		public MenuSettings menuSettings;
	}

	public static MenuController instance;

	public Variables variables;

	public Camera uiCamera;

	public GameObject menuContainer;

	public GameObject popupDimmer;

	public GameObject bg;

	public GameObject sideMenu;

	[Header("Top Menu Components")]
	public GameObject topMenuContainer; //¶¥²¿²Ëµ¥À¸

	public GameObject topMenuBg;

	public CountText topTotalGemsText;

	public Image topTotalGemsImage;

	public Transform topTotalGemsTarget;

	public Button topBuyGemsButton; //±¦Ê¯+

	public CountText topTotalCoinsText;

	public Image topTotalCoinsImage;

	public Transform topTotalCoinsTarget;

	public Button topBuyCoinsButton;//½ð±Ò+

	public GameObject topTotalChestsContainer;

	public CountText topTotalChestsText;

	public Button topBuyChestsButton;//×°¼×+

	public Button exitButton;

	public GameObject topSubInfoContainer;

	public TextMeshProUGUI topSubStatusText;

	public Button topSubManageButton;//VIP °´Å¥

	public static OnBackButtonPress backButtonFallbackAction;

	public static OnBackButtonPress backButtonOverrideAction;

	private List<VisibleMenu> visibleMenus;

	public static Camera UICamera => instance.uiCamera;

	public static Vector3 TotalCoinsPositionViewport => instance.uiCamera.WorldToViewportPoint(instance.topTotalCoinsTarget.transform.position);

	public static Vector3 TotalGemsPositionViewport => instance.uiCamera.WorldToViewportPoint(instance.topTotalGemsTarget.transform.position);

	private void Awake()
	{
		instance = this;
		visibleMenus = new List<VisibleMenu>();
		InitTopMenu();
	}

	private void Start()
	{


		exitButton.onClick.AddListener(HideNewestMenu);
		topSubManageButton.onClick.AddListener(delegate  //VIP°´Å¥¼àÌý
		{
            //if (PlayerDataManager.IsSubscribed()) //ÊÇ·ñ¹ºÂò
            //{
            //	ShowMenu<SubscriptionManagePopup>();//ÒÑ¹ºÂò
            //}
            //else
            //{
            //	ShowMenu<SubscriptionOfferPopup>().Init("MainMenu");//Î´¹ºÂò
            //}
            SDKManager.Instance.ShowAd(ShowAdType.ChaPing, 1, "µã»÷VIP");

            if (PlayerPrefs.GetInt("Isbought") ==1)
            {
                ShowMenu<SubscriptionManagePopup>();//ÒÑ¹ºÂò
            }
            else
            {
                ShowMenu<SubscriptionOfferPopup>().Init("MainMenu");//Î´¹ºÂò
            }
        });
	}

	private void Update()
	{
		if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
		{
			if (backButtonOverrideAction != null)
			{
				backButtonOverrideAction();
			}
			else if (visibleMenus.Count == 1 && backButtonFallbackAction != null)
			{
				backButtonFallbackAction();
			}
			else
			{
				HideNewestMenu();
			}
		}
		if (visibleMenus.Count == 0)
		{
			ShowMenu<MainMenu>();
		}
	}

	private void InitTopMenu()
	{
		topBuyGemsButton.onClick.AddListener(delegate
		{
            SDKManager.Instance.ShowAd(ShowAdType.ChaPing, 1, "µã»÷+±¦Ê¯");
            ShowMenu<ShopMenu>().ScrollToSection(ShopMenu.Section.Gems);
		});
		topBuyCoinsButton.onClick.AddListener(delegate
		{
            SDKManager.Instance.ShowAd(ShowAdType.ChaPing, 1, "µã»÷+½ð±Ò");
            ShowMenu<ShopMenu>().ScrollToSection(ShopMenu.Section.Coins);
		});
		topBuyChestsButton.onClick.AddListener(delegate
		{
            SDKManager.Instance.ShowAd(ShowAdType.ChaPing, 1, "µã»÷+×°¼×");
            ShowMenu<ShopMenu>().ScrollToSection(ShopMenu.Section.Chests);
		});
	}

	public static T GetMenu<T>() where T : MenuBase<T>
	{
		T val = MenuBase<T>.instance;
		if ((UnityEngine.Object)val == (UnityEngine.Object)null)
		{
			val = (MenuBase<T>.instance = instance.menuContainer.GetComponentInChildren<T>(includeInactive: true));
		}
		return val;
	}

	public static T ShowMenu<T>() where T : MenuBase<T>
	{
		T menu = GetMenu<T>();
		if (menu.gameObject.activeInHierarchy)
		{
			return menu;
		}
		menu.gameObject.SetActive(value: true);
		instance.visibleMenus.Add(new VisibleMenu
		{
			gameObject = menu.gameObject,
			menuSettings = menu.settings
		});
		ApplyNewestMenuSettings();
		return menu;
	}

	public static T HideMenu<T>() where T : MenuBase<T>
	{
		T menu = GetMenu<T>();
		menu.gameObject.SetActive(value: false);
		for (int num = instance.visibleMenus.Count - 1; num >= 0; num--)
		{
			if (instance.visibleMenus[num].gameObject == menu.gameObject)
			{
				instance.visibleMenus.RemoveAt(num);
				break;
			}
		}
		ApplyNewestMenuSettings();
		return menu;
	}

	public static int GetHighestSortingOrder()
	{
		int num = 0;
		foreach (VisibleMenu visibleMenu in instance.visibleMenus)
		{
			if (num < visibleMenu.gameObject.GetComponent<Canvas>().sortingOrder)
			{
				num = visibleMenu.gameObject.GetComponent<Canvas>().sortingOrder;
			}
		}
		return num + 2;
	}

	public static void HideNewestMenu()
	{
		if (instance.visibleMenus.Count > 0)
		{
			instance.visibleMenus[instance.visibleMenus.Count - 1].gameObject.SetActive(value: false);
			instance.visibleMenus.RemoveAt(instance.visibleMenus.Count - 1);
			AnimatedCurrencyController.CancelAllAnimations();
			ApplyNewestMenuSettings();
		}
	}

	public static void ApplyNewestMenuSettings()
	{
		if (instance.visibleMenus.Count <= 0)
		{
			return;
		}
		VisibleMenu visibleMenu = instance.visibleMenus[instance.visibleMenus.Count - 1];
		Canvas component = visibleMenu.gameObject.GetComponent<Canvas>();
		component.overrideSorting = true;
		component.sortingOrder = GetHighestSortingOrder();
		instance.bg.SetActive(visibleMenu.menuSettings.useBg);
		instance.sideMenu.SetActive(visibleMenu.menuSettings.showSideMenu);
		CanvasGroup component2 = instance.sideMenu.gameObject.GetComponent<CanvasGroup>();
		component2.interactable = true;
		if (visibleMenu.menuSettings.dimSideMenu)
		{
			component2.interactable = false;
		}
		instance.popupDimmer.SetActive(visibleMenu.menuSettings.useDimmer);
		instance.popupDimmer.GetComponent<Canvas>().sortingOrder = component.sortingOrder - 1;
		SetTopMenu(visibleMenu.menuSettings.showTopCurrencies, visibleMenu.menuSettings.showTopCurrencyButtons, visibleMenu.menuSettings.showTopExitButton, visibleMenu.menuSettings.showTopSubInfo, visibleMenu.menuSettings.showTopBackground, visibleMenu.menuSettings.showTopChests);
		for (int i = 0; i < instance.visibleMenus.Count - 1; i++)
		{
			CanvasGroup component3 = instance.visibleMenus[i].gameObject.GetComponent<CanvasGroup>();
			component3.interactable = false;
			if (visibleMenu.menuSettings.hidePrev)
			{
				component3.alpha = 0f;
			}
		}
		if (visibleMenu.menuSettings.hideOnlyPrev)
		{
			int num = instance.visibleMenus.Count - 2;
			if (num >= 0)
			{
				instance.visibleMenus[num].gameObject.GetComponent<CanvasGroup>().alpha = 0f;
			}
		}
		CanvasGroup component4 = instance.visibleMenus[instance.visibleMenus.Count - 1].gameObject.GetComponent<CanvasGroup>();
		component4.interactable = true;
		component4.alpha = 1f;
		MainMenu menu = GetMenu<MainMenu>();
		if (menu.isActiveAndEnabled)
		{
			menu.UpdatePlayMenu(!menu.HaveStarsToAdd());
		}
	}

	public static void UpdateTopMenu()
	{
		instance.topTotalCoinsText.Init(PlayerDataManager.GetCoins());
		instance.topTotalGemsText.Init(PlayerDataManager.GetGems());
		int count = Mathf.FloorToInt(PlayerDataManager.GetChestProgressPercentage(ChestProgressionType.Pvp)) + Mathf.FloorToInt(PlayerDataManager.GetChestProgressPercentage(ChestProgressionType.Adventure));
		instance.topTotalChestsText.Init(count);
	}

	public static void SetTopMenu(bool currenciesEnabled, bool currencyButtonsEnabled, bool exitButtonEnabled, bool subContainerEnabled, bool bgEnabled, bool chestsEnabled)
	{
		if (currenciesEnabled)
		{
			UpdateTopMenu();
		}
		instance.topTotalCoinsText.gameObject.SetActive(currenciesEnabled);
		instance.topTotalCoinsImage.gameObject.SetActive(currenciesEnabled);
		instance.topTotalGemsText.gameObject.SetActive(currenciesEnabled);
		instance.topTotalGemsImage.gameObject.SetActive(currenciesEnabled);
		instance.exitButton.gameObject.SetActive(exitButtonEnabled);
		instance.topSubInfoContainer.gameObject.SetActive(subContainerEnabled);
		UpdateSubscriptionStatus();
		SetActiveCurrencyButtons(currencyButtonsEnabled);
		instance.topTotalChestsContainer.SetActive(chestsEnabled);
		instance.topMenuBg.SetActive(bgEnabled);
	}

	public static void UpdateSubscriptionStatus()
	{
		instance.topSubStatusText.text = ScriptLocalization.Get(PlayerDataManager.IsSubscribed() ? "Active" : "Inactive");
	}

	public static void SetActiveCurrencyButtons(bool active)
	{
		instance.topBuyGemsButton.gameObject.SetActive(active);
		instance.topBuyCoinsButton.gameObject.SetActive(active);
	}

	public static void Delay(float seconds, Action action)
	{
		instance.StartCoroutine(DelayRoutine(seconds, action));
	}

	private static IEnumerator DelayRoutine(float t, Action action)
	{
		yield return new WaitForSeconds(t);
		action();
	}
}
