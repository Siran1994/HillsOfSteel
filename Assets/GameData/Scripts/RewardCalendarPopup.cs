using I2.Loc;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RewardCalendarPopup : MenuBase<RewardCalendarPopup>
{
	public Button dailyRewardOkButton;//µã»÷»ñÈ¡

	public RewardDay[] rewardDays;

	private void Start()
	{
		dailyRewardOkButton.onClick.AddListener(delegate
		{
			dailyRewardOkButton.interactable = false;
			StartCoroutine(RewardAnimationRoutine());
		});
	}

	private void OnEnable()
	{
		dailyRewardOkButton.interactable = true;
		int dailyBonusIndex = PlayerDataManager.GetDailyBonusIndex();
		for (int i = 0; i < rewardDays.Length; i++)
		{
			rewardDays[i].rewardAmount.text = Variables.instance.dailyBonusAmounts[i].ToString();
			rewardDays[i].dayText.text = ScriptLocalization.Day;
			rewardDays[i].dayText.GetComponent<LocalizationParamsManager>().SetParameterValue("DAY_NUMBER", (i + 1).ToString());
		}
		rewardDays[dailyBonusIndex].dayText.text = ScriptLocalization.Today;
		for (int j = 0; j < dailyBonusIndex - 1; j++)
		{
			rewardDays[j].checkmark.enabled = true;
		}
		for (int k = dailyBonusIndex; k < MenuBase<RewardCalendarPopup>.instance.rewardDays.Length; k++)
		{
			rewardDays[k].checkmark.enabled = false;
		}
	}

	private void OnDisable()
	{
		PlayerDataManager.GetDailyBonusAndSetNext();
	}

	private IEnumerator RewardAnimationRoutine()
	{
		int dailyBonusIndex = PlayerDataManager.GetDailyBonusIndex();
		AnimatedCurrencyController.AnimateGems(Variables.instance.dailyBonusAmounts[dailyBonusIndex], MenuController.UICamera.WorldToViewportPoint(rewardDays[dailyBonusIndex].transform.position), MenuController.TotalGemsPositionViewport, 1, null, delegate(int tc)
		{
			//*MenuController.instance.topTotalGemsText.Tick(tc);
		});
		yield return StartCoroutine(CheckMarkAnimation(dailyBonusIndex));
		yield return new WaitForSeconds(1f);
		MenuController.HideMenu<RewardCalendarPopup>();
	}

	private IEnumerator CheckMarkAnimation(int index)
	{
		yield return new WaitForSeconds(1f);
		Image image = MenuBase<RewardCalendarPopup>.instance.rewardDays[index].checkmark;
		image.enabled = true;
		Vector3 scale = image.rectTransform.localScale;
		for (float t = 0f; t < 0.25f; t += Time.deltaTime)
		{
			image.rectTransform.localScale = Vector3.Lerp(Vector3.zero, scale, t / 0.25f);
			yield return null;
		}
		image.rectTransform.localScale = scale;
	}
}
