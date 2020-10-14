using I2.Loc;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class BundlePopup : MenuBase<BundlePopup>
{
	[Serializable]
	public class TankContainerParent
	{
		public GameObject parent;

		public TankContainer tankContainer;
	}

	[Header("Offer")]
	public IAPItem iapItem;

	public TMP_Text gemAmountText;

	public TMP_Text coinAmountText;

	public TMP_Text tankNameText;

	public Image gemImage;

	public Image coinImage;

	[Header("Garage")]
	public GameObject bundleTanksContainer;

	public TankContainerParent[] tanks;

	private bool bought;

	private int coins;

	private int gems;

	private string tankName;

	private int tankIndex;

	private void OnEnable()
	{
		bought = false;
		for (int i = 0; i < tanks.Length; i++)
		{
			tanks[i].parent.SetActive(value: false);
		}
		bundleTanksContainer.SetActive(value: true);
	}

	private void OnDisable()
	{
		for (int i = 0; i < tanks.Length; i++)
		{
			tanks[i].parent.SetActive(value: false);
		}
		bundleTanksContainer.SetActive(value: false);
		if (bought)
		{
			TankAnalytics.BoughtBundle(iapItem.iapId, tankName, gems, coins);
		}
		else
		{
			TankAnalytics.DeclinedBundle(iapItem.iapId, tankName);
		}
		if (bought && MenuController.GetMenu<GarageMenu>().isActiveAndEnabled)
		{
			MenuController.GetMenu<GarageMenu>().SetTank(tankIndex);
		}
	}

	public void SetIAP(Product product)
	{
		iapItem.InitIap(product.definition.id);
		string tankId = "";
		foreach (PayoutDefinition payout in product.definition.payouts)
		{
			ShopMenu.ShopItem shopItem = ShopMenu.ShopItem.FromPayoutDefinition(payout);
			switch (shopItem.type)
			{
			case ShopMenu.ShopItemType.Coin:
				coins = shopItem.count;
				coinAmountText.text = shopItem.count.ToString();
				break;
			case ShopMenu.ShopItemType.Gem:
				gems = shopItem.count;
				gemAmountText.text = shopItem.count.ToString();
				break;
			case ShopMenu.ShopItemType.TankCard:
			{
				Tank tank = Variables.instance.GetTank(shopItem.id);
				tankName = tank.name;
				int result = 1;
				int.TryParse(tank.id.Replace("tank", ""), out result);
				int num = tankIndex = Variables.instance.tankOrder[result];
				tanks[--result].parent.SetActive(value: true);
				tanks[result].tankContainer.BulletTypeIndex = num;
				tanks[result].tankContainer.BulletDef = Variables.instance.tanks[num].bullet;
				GarageMenu menu = MenuController.GetMenu<GarageMenu>();
				if (tank.bullet.type == BulletType.Missile || tank.bullet.type == BulletType.Laser)
				{
					menu.StartCoroutine(menu.MissileShoot(tanks[result].tankContainer));
				}
				else if (tank.bullet.type == BulletType.Flame)
				{
					menu.StartCoroutine(menu.FlamerShoot(tanks[result].tankContainer));
				}
				else if (tank.bullet.type == BulletType.Small)
				{
					menu.StartCoroutine(menu.BulletShoot(tanks[result].tankContainer));
				}
				else if (tank.bullet.type == BulletType.Lightning)
				{
					menu.StartCoroutine(menu.LightningShoot(tanks[result].tankContainer));
				}
				else
				{
					tanks[result].tankContainer.Shoot();
				}
				tankNameText.text = ScriptLocalization.Get(tank.name);
				tankId = tank.id;
				break;
			}
			}
		}
		iapItem.SetOnComplete(delegate
		{
			bought = true;
			MenuController.HideMenu<BundlePopup>();
			MenuController.ShowMenu<NewCardPopup>().Init(Variables.instance.GetTank(tankId));
		});
	}
}
