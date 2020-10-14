using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ClassicModeMenu : MenuBase<ClassicModeMenu>
{
	public Button bossRushButton;

	public Button bossRushBuyButton;

	public GameObject bossRushFreeContainer;

	public GameObject bossRushPayContainer;

	private void Start()
	{
		Action startBossRush = delegate
		{
			PlayerDataManager.SetNextBossRushFree(value: true);
			bossRushFreeContainer.SetActive(value: true);
			bossRushPayContainer.SetActive(value: false);
			PlayerDataManager.SelectedGameMode = GameMode.BossRush;
			MenuController.HideMenu<ClassicModeMenu>();
			MenuController.ShowMenu<GarageMenu>();
		};
		UnityAction call = delegate
		{
			if (PlayerDataManager.IsNextBossRushFree())
			{
				startBossRush();
			}
			else
			{
				PlayerDataManager.OnTransaction(Variables.instance.bossRushCost, startBossRush);
			}
		};
		bossRushButton.onClick.AddListener(call);
		bossRushBuyButton.onClick.AddListener(call);
	}

	private void OnEnable()
	{
		bool flag = PlayerDataManager.IsNextBossRushFree();
		bossRushFreeContainer.SetActive(flag);
		bossRushPayContainer.SetActive(!flag);
	}
}
