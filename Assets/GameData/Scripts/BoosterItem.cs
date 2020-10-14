using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoosterItem : MonoBehaviour
{
	public Image icon;

	public Image selected;

	public Image countProgress;

	public Image levelUpReadyProgress;

	public Image maxProgress;

	public Button button;

	public TextMeshProUGUI nameText;

	public TextMeshProUGUI shortNameText;

	public TextMeshProUGUI countText;

	public TextMeshProUGUI countTextWithLevelUp;

	public TextMeshProUGUI levelText;

	public TextMeshProUGUI givenCountText;

	public GameObject givenCountContainer;

	public GameObject newContainer;

	public string id;

	private Sprite originalSprite;

	private void Awake()
	{
		originalSprite = icon.sprite;
	}

	public void SetData(Booster b, bool isSelected, int givenCount = 0, bool isNew = false)
	{
		id = b.id;
		selected.enabled = isSelected;
		if (b.type == BoosterGameplayType.None)
		{
			icon.sprite = originalSprite;
		}
		else
		{
			icon.sprite = b.card;
		}
		icon.SetNativeSize();
		bool num = PlayerDataManager.CanLevelUpBooster(b);
		maxProgress.gameObject.SetActive(b.MaxLevel);
		countProgress.gameObject.SetActive(!b.MaxLevel);
		levelUpReadyProgress.gameObject.SetActive(!b.MaxLevel);
		TextMeshProUGUI textMeshProUGUI = num ? countTextWithLevelUp : countText;
		if (!b.MaxLevel)
		{
			textMeshProUGUI.text = b.Count - b.ThisLevelCount + "/" + (b.NextLevelCount - b.ThisLevelCount);
		}
		float num2 = (float)(b.Count - b.ThisLevelCount) / (float)(b.NextLevelCount - b.ThisLevelCount);
		if (num2 >= 1f)
		{
			countProgress.gameObject.SetActive(value: false);
			levelUpReadyProgress.gameObject.SetActive(value: true);
		}
		else
		{
			countProgress.gameObject.SetActive(value: true);
			levelUpReadyProgress.gameObject.SetActive(value: false);
			countProgress.fillAmount = Mathf.Clamp01(num2);
		}
		levelText.text = ScriptLocalization.Get("Level") + " " + (b.Level + 1);
		if (nameText != null)
		{
			nameText.text = ScriptLocalization.Get(b.tankName) + "\n" + ScriptLocalization.Get(b.type.ToString());
		}
		if (shortNameText != null)
		{
			shortNameText.text = ScriptLocalization.Get(b.type.ToString());
		}
		if (givenCountContainer != null)
		{
			givenCountContainer.SetActive(givenCount > 0);
			if (givenCount > 0)
			{
				givenCountText.text = givenCount.ToString() + "x";
			}
		}
		if (newContainer != null)
		{
			newContainer.SetActive(isNew);
		}
	}
}
