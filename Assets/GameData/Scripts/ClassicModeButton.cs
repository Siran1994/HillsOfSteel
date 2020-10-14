using I2.Loc;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClassicModeButton : MonoBehaviour
{
	public LeaderboardID leaderboard;

	public Button button;

	public TextMeshProUGUI scoreText;

	public Button unlockButton;

	public TextMeshProUGUI unlockPriceText;

	public TextMeshProUGUI coinMultiplierText;//金币奖励
    
	private void Start()
	{
		int unlockPrice = Variables.instance.levels[(int)leaderboard].price;
		Action tryUnlock = delegate
		{
			PlayerDataManager.OnTransaction(CurrencyType.Coins, unlockPrice, delegate
			{
				PlayerDataManager.UnlockLevel((int)leaderboard);
				TankAnalytics.BoughtWithSoftCurrency("level", "level" + (int)leaderboard, unlockPrice);
				MenuController.UpdateTopMenu();
				StartCoroutine(AnimateUnlock());
			});
		};
		unlockButton.onClick.AddListener(delegate
		{
			tryUnlock();
		});
		button.onClick.AddListener(delegate
		{
            SDKManager.Instance.ShowAd(ShowAdType.VideoAD, 2, "点击经典模式的关卡");
            if (PlayerDataManager.LevelLocked((int)leaderboard))
			{
				tryUnlock();
			}
			else
			{
				PlayerDataManager.SelectedGameMode = GameMode.Classic;
				PlayerDataManager.SetSelectedLevel((int)leaderboard);
				MenuController.HideMenu<ClassicModeMenu>();
				MenuController.ShowMenu<GarageMenu>();
			}
		});
		coinMultiplierText.GetComponent<LocalizationParamsManager>().SetParameterValue("AMOUNT", Variables.instance.levels[(int)leaderboard].coinsPerScore.ToString() + "X");
        coinMultiplierText.text = "<color=#9F00F3><size=50%>" + Variables.instance.levels[(int) leaderboard].coinsPerScore.ToString() + "X" + "金币奖励";

    }

	private void OnEnable()
	{
		bool flag = PlayerDataManager.LevelLocked((int)leaderboard);
		unlockButton.gameObject.SetActive(flag);
		unlockPriceText.text = Variables.instance.levels[(int)leaderboard].price.ToString();
		scoreText.gameObject.SetActive(!flag);
		scoreText.text = PlayerDataManager.GetLeaderboardScore(leaderboard).ToString();
		if (flag)
		{
			return;
		}
		CanvasGroup component = scoreText.GetComponent<CanvasGroup>();
		CanvasGroup[] componentsInChildren = scoreText.GetComponentsInChildren<CanvasGroup>();
		CanvasGroup canvasGroup = component;
		CanvasGroup[] array = componentsInChildren;
		foreach (CanvasGroup canvasGroup2 in array)
		{
			if (canvasGroup2 != component)
			{
				canvasGroup = canvasGroup2;
			}
		}
		component.alpha = 1f;
		canvasGroup.alpha = 1f;
	}

	private IEnumerator AnimateUnlock()
	{
		AudioMap.PlayClipAt(AudioMap.instance["levelUnlock"], Vector3.zero, AudioMap.instance.uiMixerGroup);
		CanvasGroup buttonCanvasGroup = unlockButton.GetComponent<CanvasGroup>();
		float time = 0.4f;
		for (float t2 = 0f; t2 <= time; t2 += Time.deltaTime)
		{
			buttonCanvasGroup.alpha = LeanTween.easeOutExpo(1f, 0f, t2 / time);
			yield return null;
		}
		unlockButton.gameObject.SetActive(value: false);
		CanvasGroup scoreCanvasGroup = scoreText.GetComponent<CanvasGroup>();
		CanvasGroup[] componentsInChildren = scoreText.GetComponentsInChildren<CanvasGroup>();
		CanvasGroup scoreBgCanvasGroup = scoreCanvasGroup;
		CanvasGroup[] array = componentsInChildren;
		foreach (CanvasGroup canvasGroup in array)
		{
			if (canvasGroup != scoreCanvasGroup)
			{
				scoreBgCanvasGroup = canvasGroup;
			}
		}
		scoreCanvasGroup.alpha = 0f;
		scoreBgCanvasGroup.alpha = 0f;
		scoreText.gameObject.SetActive(value: true);
		for (float t2 = 0f; t2 <= time; t2 += Time.deltaTime)
		{
			float val = t2 / time;
			scoreCanvasGroup.alpha = LeanTween.easeInExpo(0f, 1f, val);
			scoreBgCanvasGroup.alpha = LeanTween.easeInExpo(0f, 1f, val);
			yield return null;
		}
		scoreCanvasGroup.alpha = 1f;
		scoreBgCanvasGroup.alpha = 1f;
	}
}
