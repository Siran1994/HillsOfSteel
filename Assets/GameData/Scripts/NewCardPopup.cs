using I2.Loc;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewCardPopup : MenuBase<NewCardPopup>
{
	[Serializable]
	public class BackgroundOptions
	{
		public Rarity rarity;

		public Material backgroundMaterial;

		public Material shineMaterial;
	}

	public Image cardImage;

	public TextMeshProUGUI headerText;

	public TextMeshProUGUI footerText;

	public Image backgroundImage;

	public Material defaultBackgroundMaterial;

	public Image shine;

	public ParticleSystem shineParticles;

	[ArrayElementTitle("rarity")]
	public BackgroundOptions[] backgroundOptions;

	public GameObject[] tankImageContainers;

	private bool animationDone;

	private Vector2 headerOriginalPos;

	private Vector2 footerOriginalPos;

	public void Init(Collectible card)
	{
		if (card.cardType == CardType.TankCard)
		{
			int result = 0;
			int.TryParse(card.id.Replace("tank", ""), out result);
			for (int i = 0; i != tankImageContainers.Length; i++)
			{
				tankImageContainers[i].SetActive(i == result);
			}
			cardImage = tankImageContainers[result].GetComponent<Image>();
		}
		animationDone = false;
		cardImage.GetComponent<RectTransform>().localScale = Vector3.zero;
		headerOriginalPos = headerText.GetComponent<RectTransform>().anchoredPosition;
		headerText.GetComponent<RectTransform>().localScale = Vector3.zero;
		headerText.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
		headerText.text = ScriptLocalization.Get(card.name);
		footerOriginalPos = footerText.GetComponent<RectTransform>().anchoredPosition;
		footerText.GetComponent<RectTransform>().localScale = Vector3.zero;
		footerText.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
		footerText.text = ScriptLocalization.Get("UnlockedNewTank");
		for (int j = 0; j != backgroundOptions.Length; j++)
		{
			if (backgroundOptions[j].rarity.Equals(card.rarity))
			{
				backgroundImage.material = backgroundOptions[j].backgroundMaterial;
				shine.gameObject.SetActive(value: true);
				shine.material = backgroundOptions[j].shineMaterial;
				break;
			}
		}
		if (shineParticles != null)
		{
			shineParticles.Play();
		}
		StartCoroutine(AnimationRoutine());
	}

	private void Update()
	{
		if (Input.GetMouseButtonUp(0))
		{
			if (!animationDone)
			{
				FinishAnimation();
			}
			else
			{
				MenuController.HideMenu<NewCardPopup>();
			}
		}
	}

	private void OnDisable()
	{
		if (!animationDone)
		{
			FinishAnimation();
		}
		backgroundImage.material = defaultBackgroundMaterial;
		shine.gameObject.SetActive(value: false);
		MenuController.GetMenu<ChestPopup>().OnNewCardRewarded();
	}

	private void FinishAnimation()
	{
		StopAllCoroutines();
		Color color = new Color(1f, 1f, 1f, 1f);
		cardImage.GetComponent<RectTransform>().localScale = Vector3.one;
		cardImage.color = color;
		headerText.GetComponent<RectTransform>().anchoredPosition = headerOriginalPos;
		headerText.GetComponent<RectTransform>().localScale = Vector3.one;
		headerText.color = color;
		footerText.GetComponent<RectTransform>().anchoredPosition = footerOriginalPos;
		footerText.GetComponent<RectTransform>().localScale = Vector3.one;
		footerText.color = color;
		animationDone = true;
	}

	private IEnumerator ScaleElementUp(RectTransform rt, float time)
	{
		for (float t = 0f; t < time; t += Time.deltaTime)
		{
			rt.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, LeanTween.easeInExpo(0f, 1f, t / time));
			yield return null;
		}
		rt.localScale = Vector3.one;
	}

	private IEnumerator FadeElementIn(Image img, TextMeshProUGUI text, float time)
	{
		Color c3 = new Color(1f, 1f, 1f, 0f);
		Color c2 = new Color(1f, 1f, 1f, 1f);
		for (float t = 0f; t < time; t += Time.deltaTime)
		{
			float t2 = LeanTween.easeInExpo(0f, 1f, t / time);
			if (img != null)
			{
				img.color = Color.Lerp(c3, c2, t2);
			}
			if (text != null)
			{
				text.color = Color.Lerp(c3, c2, t2);
			}
			yield return null;
		}
		if (img != null)
		{
			img.color = c2;
		}
		if (text != null)
		{
			text.color = c2;
		}
	}

	private IEnumerator MoveElement(RectTransform rt, Vector2 newPos, float time)
	{
		Vector2 origPos = rt.anchoredPosition;
		for (float t = 0f; t < time; t += Time.deltaTime)
		{
			rt.anchoredPosition = Vector2.Lerp(origPos, newPos, LeanTween.easeInExpo(0f, 1f, t / time));
			yield return null;
		}
		rt.anchoredPosition = newPos;
	}

	private IEnumerator AnimationRoutine()
	{
		float time = 0.33f;
		MenuController.instance.StartCoroutine(AudioManager.FadeMusicForSound("newTankReveal"));
		StartCoroutine(FadeElementIn(cardImage, null, 0.1f));
		StartCoroutine(ScaleElementUp(cardImage.GetComponent<RectTransform>(), time));
		yield return new WaitForSeconds(0.1f);
		StartCoroutine(FadeElementIn(null, headerText, 0.1f));
		StartCoroutine(ScaleElementUp(headerText.GetComponent<RectTransform>(), time));
		StartCoroutine(MoveElement(headerText.GetComponent<RectTransform>(), headerOriginalPos, time));
		yield return new WaitForSeconds(0.1f);
		StartCoroutine(FadeElementIn(null, footerText, 0.1f));
		StartCoroutine(ScaleElementUp(footerText.GetComponent<RectTransform>(), time));
		StartCoroutine(MoveElement(footerText.GetComponent<RectTransform>(), footerOriginalPos, time));
		yield return new WaitForSeconds(time);
		FinishAnimation();
	}
}
