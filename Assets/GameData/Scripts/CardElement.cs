using I2.Loc;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardElement : MonoBehaviour
{
	public RectTransform container;

	public GameObject newTextContainer;

	public GameObject newBorderContainer;

	public Image image;//道具图标

	public Image background;

	public TextMeshProUGUI levelText;

	public TextMeshProUGUI bottomBarText;

	public GameObject countGreenContainer;

	public TextMeshProUGUI countGreenText;//升级材料数量

	public GameObject countBlueContainer;

	public TextMeshProUGUI countBlueText;

	public Image countBlueImage;

	public GameObject countRedContainer;

	public GameObject stackSizeContainer;

	public TextMeshProUGUI stackSizeText;

	public GameObject newIconContainer;

	public TextMeshProUGUI footerText;

	public GameObject teaserContainer;

	public GameObject doubleCardContainer;

	public void SetValues(ShopMenu.ShopItem item, bool animateRoll = true, bool deductCount = false, bool vip = false)
	{
		if (item.type == ShopMenu.ShopItemType.TankCard)
		{
			Tank tank = Manager<PlayerDataManager>.instance.variables.GetTank(item.id);
			SetValues(tank, vip ? (item.count / 2) : item.count, useNew: true, useStackSize: true, deductCount, useTankName: true, item.isNew);
			if (item.count > 0 && animateRoll)
			{
				AnimateTankCardCountRoll(item, deductCount, vip);
			}
		}
		else if (item.type == ShopMenu.ShopItemType.BoosterCard)
		{
			Booster booster = PlayerDataManager.GetBooster(item.id);
            image.sprite = booster.bigCard;
			if (deductCount)
			{
				booster.Count -= item.count;
			}
			levelText.text = string.Format("{0} {1}", ScriptLocalization.Get("Level"), booster.Level + 1);
			countBlueContainer.SetActive(booster.Count < booster.NextLevelCount);
			countGreenContainer.SetActive(booster.Count >= booster.NextLevelCount && !booster.MaxLevel);
			countRedContainer.SetActive(booster.MaxLevel);
			string text = $"{booster.Count - booster.ThisLevelCount}/{booster.NextLevelCount - booster.ThisLevelCount}";
			countBlueText.text = text;
			countGreenText.text = text;
			if (booster.Count < booster.NextLevelCount && booster.NextLevelCount > booster.ThisLevelCount)
			{
				countBlueImage.fillAmount = (float)(booster.Count - booster.ThisLevelCount) / (float)(booster.NextLevelCount - booster.ThisLevelCount);
			}
			stackSizeText.text = "X" + (vip ? (item.count / 2) : item.count).ToString();
			newBorderContainer.SetActive(booster.Count == 0);
			newIconContainer.SetActive(booster.Count == 0);
			newTextContainer.SetActive(booster.Count == 0);
			footerText.gameObject.SetActive(value: true);
			footerText.text = $"{ScriptLocalization.Get(booster.tankName)}\n{ScriptLocalization.Get(booster.type.ToString())}";
			if (item.count > 0 && animateRoll)
			{
				StartCoroutine(CardCountRollRoutine(vip ? (item.count / 2) : item.count, booster.Count, booster.ThisLevelCount, booster.NextLevelCount, booster.MaxLevel));
			}
		}
		else if (item.type == ShopMenu.ShopItemType.Coin)
		{
			image.sprite = ((item.rarity == Rarity.Epic) ? MenuController.GetMenu<ShopMenu>().epicCoinsSprite : ((item.rarity == Rarity.Rare) ? MenuController.GetMenu<ShopMenu>().rareCoinsSprite : MenuController.GetMenu<ShopMenu>().commonCoinsSprite));
			countBlueContainer.transform.parent.gameObject.SetActive(value: false);
			stackSizeText.text = (vip ? (item.count / 2) : item.count).ToString();
			newIconContainer.SetActive(value: false);
			newTextContainer.SetActive(value: false);
			bottomBarText.gameObject.SetActive(value: true);
			bottomBarText.text = ScriptLocalization.Get("Coins");
			levelText.gameObject.SetActive(value: false);
			footerText.gameObject.SetActive(value: false);
		}
		else if (item.type == ShopMenu.ShopItemType.Gem)
		{
			image.sprite = ((item.count <= 2) ? MenuController.GetMenu<ShopMenu>().commonGemsSprite : ((item.count <= 3) ? MenuController.GetMenu<ShopMenu>().rareGemsSprite : MenuController.GetMenu<ShopMenu>().epicGemsSprite));
			countBlueContainer.transform.parent.gameObject.SetActive(value: false);
			stackSizeText.text = (vip ? (item.count / 2) : item.count).ToString();
			newIconContainer.SetActive(value: false);
			newTextContainer.SetActive(value: false);
			bottomBarText.gameObject.SetActive(value: true);
			bottomBarText.text = ScriptLocalization.Get("Gems");
			levelText.gameObject.SetActive(value: false);
			footerText.gameObject.SetActive(value: false);
		}
    }

	public void SetValues(Tank tank, int count, bool useNew = true, bool useStackSize = true, bool deductCount = false, bool useTankName = true, bool isNew = false)
	{
		image.sprite = tank.card;
		background.sprite = MenuController.GetMenu<ShopMenu>().GetCardBackground(tank.rarity, small: true);
		int tankUpgradeLevel = PlayerDataManager.GetTankUpgradeLevel(tank);
		int max = Manager<PlayerDataManager>.instance.variables.tankLevelMinMax.max;
		int num = PlayerDataManager.GetTankCardCount(tank) - (deductCount ? count : 0);
		int tankLevelUpCardsCumulative = Manager<PlayerDataManager>.instance.variables.GetTankLevelUpCardsCumulative(tank.rarity, tankUpgradeLevel - 1);
		int tankLevelUpCardsCumulative2 = Manager<PlayerDataManager>.instance.variables.GetTankLevelUpCardsCumulative(tank.rarity, tankUpgradeLevel);
		levelText.text = string.Format("{0} {1}", ScriptLocalization.Get("Level"), tankUpgradeLevel + 1);
		countBlueContainer.SetActive(num < tankLevelUpCardsCumulative2);
		countGreenContainer.SetActive(num >= tankLevelUpCardsCumulative2 && tankUpgradeLevel < max);
		countRedContainer.SetActive(tankUpgradeLevel == max);
		string text = $"{num - tankLevelUpCardsCumulative}/{tankLevelUpCardsCumulative2 - tankLevelUpCardsCumulative}";
		countBlueText.text = text;
		countGreenText.text = text;
		if (num < tankLevelUpCardsCumulative2 && tankLevelUpCardsCumulative2 > tankLevelUpCardsCumulative)
		{
			countBlueImage.fillAmount = (float)(num - tankLevelUpCardsCumulative) / (float)(tankLevelUpCardsCumulative2 - tankLevelUpCardsCumulative);
		}
		footerText.gameObject.SetActive(useTankName);
		footerText.text = ScriptLocalization.Get(tank.name);
		stackSizeText.text = "X" + count.ToString();
		stackSizeContainer.gameObject.SetActive(useStackSize);
		newBorderContainer.SetActive(useNew && isNew);
		newIconContainer.SetActive(useNew && isNew);
		newTextContainer.SetActive(useNew && isNew);
	}

	public void AnimateTankCardCountRoll(ShopMenu.ShopItem item, bool deductCount = false, bool doubleCards = false)
	{
		if (item.count != 0)
		{
			Tank tank = Manager<PlayerDataManager>.instance.variables.GetTank(item.id);
			int tankUpgradeLevel = PlayerDataManager.GetTankUpgradeLevel(tank);
			StartCoroutine(CardCountRollRoutine(doubleCards ? (item.count / 2) : item.count, PlayerDataManager.GetTankCardCount(tank) - (deductCount ? item.count : 0), Manager<PlayerDataManager>.instance.variables.GetTankLevelUpCardsCumulative(tank.rarity, tankUpgradeLevel - 1), Manager<PlayerDataManager>.instance.variables.GetTankLevelUpCardsCumulative(tank.rarity, tankUpgradeLevel), tankUpgradeLevel + 1 == Manager<PlayerDataManager>.instance.variables.tankLevelMinMax.max));
		}
	}

	private IEnumerator CardCountRollRoutine(int amountToAdd, int currentCount, int prevLevelMax, int nextLevelMin, bool nextIsMax)
	{
		float num = 0.4f;
		float waitForStep = num / (float)amountToAdd;
		int num4;
		for (int i = 0; i <= amountToAdd; i = num4)
		{
			int num2 = currentCount + i - prevLevelMax;
			int num3 = nextLevelMin - prevLevelMax;
			string text = $"{num2}/{num3}";
			countBlueContainer.SetActive(currentCount + i < nextLevelMin);
			countGreenContainer.SetActive(currentCount + i >= nextLevelMin);
			if (currentCount + i < nextLevelMin && num3 > 0)
			{
				AudioMap.PlayClipAt("progressBarTick", Vector3.zero, AudioMap.instance.uiMixerGroup);
				countBlueImage.fillAmount = (float)num2 / (float)num3;
				countBlueText.text = text;
			}
			else if (currentCount + i >= nextLevelMin)
			{
				countGreenText.text = text;
				if (nextIsMax)
				{
					break;
				}
			}
			yield return new WaitForSecondsRealtime(waitForStep);
			num4 = i + 1;
		}
	}

	public void SetDoubleValues(ShopMenu.ShopItem item)
	{
		string format = "X{0}";
		if (item.type == ShopMenu.ShopItemType.Coin || item.type == ShopMenu.ShopItemType.Gem)
		{
			format = "{0}";
		}
		doubleCardContainer.SetActive(value: true);
		if (item.type == ShopMenu.ShopItemType.TankCard)
		{
			item.count /= 2;
			AnimateTankCardCountRoll(item, deductCount: true);
			item.count *= 2;
		}
		else if (item.type == ShopMenu.ShopItemType.BoosterCard)
		{
			Booster booster = PlayerDataManager.GetBooster(item.id);
			StartCoroutine(CardCountRollRoutine(item.count / 2, booster.Count - item.count / 2, booster.ThisLevelCount, booster.NextLevelCount, booster.MaxLevel));
		}
		StartCoroutine(DoubleTextCountRoutine(item.count, format));
		StartCoroutine(DoubleTextScaleRoutine());
	}

	private IEnumerator DoubleTextCountRoutine(int doubleCount, string format)
	{
		int num = doubleCount / 2;
		stackSizeText.text = string.Format(format, doubleCount.ToString());
		yield return null;
	}

	private IEnumerator DoubleTextScaleRoutine()
	{
		float time = 0.4f;
		Vector2 scale = new Vector2(1.4f, 1.4f);
		for (float t = 0f; t < time; t += Time.deltaTime)
		{
			stackSizeText.rectTransform.localScale = Vector2.Lerp(scale, Vector2.one, LeanTween.easeInBack(0f, 1f, t / time));
			yield return null;
		}
		stackSizeText.rectTransform.localScale = Vector2.one;
	}
}
