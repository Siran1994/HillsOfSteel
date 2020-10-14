using UnityEngine;
using UnityEngine.UI;

public class TouchScreenController : BaseController
{
	public TouchButton buttonLeft;

	public TouchButton buttonRight;

	public TouchButton buttonShoot;

	public TouchButton buttonBooster;

	public Image boosterButtonIcon;

	public Button boosterButton;

	public override void SetBooster(Booster booster)
	{
		boosterButton.gameObject.SetActive(value: false);
		if (booster != null && booster.Count > 0 && booster.activationType == BoosterGameplayActivation.Active)
		{
			boosterButton.gameObject.SetActive(value: true);
			boosterButtonIcon.sprite = booster.inGameIcon;
			boosterButtonIcon.SetNativeSize();
			boosterButton.transform.localScale = Vector3.zero;
			LeanTween.scale(boosterButton.gameObject, Vector3.one, 1f).setEase(LeanTweenType.easeOutBounce);
		}
	}

	private void Update()
	{
		float v = 0f;
		if (buttonLeft.IsDown())
		{
			v = 1f;
		}
		else if (buttonRight.IsDown())
		{
			v = -1f;
		}
		DoMove(v);
		if (buttonBooster.IsDown())
		{
			DoBooster();
			SetBooster(null);
		}
		DoShoot(buttonShoot.IsDown());
	}
}
