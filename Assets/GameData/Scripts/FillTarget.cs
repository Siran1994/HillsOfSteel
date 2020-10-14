using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class FillTarget : MonoBehaviour
{
	public float smoothTime = 0.25f;

	public float fillTarget;

	public Image[] moveImages;

	private Image image;

	private float velocity;

	public void Reset(float fill)
	{
		fillTarget = fill;
		image.fillAmount = fill;
		MoveMoveImages();
	}

	private void OnEnable()
	{
		image = GetComponent<Image>();
		fillTarget = image.fillAmount;
		MoveMoveImages();
	}

	private void MoveMoveImages()
	{
		Image[] array = moveImages;
		foreach (Image obj in array)
		{
			Vector2 anchoredPosition = obj.rectTransform.anchoredPosition;
			anchoredPosition.x = image.rectTransform.rect.width * image.fillAmount;
			obj.rectTransform.anchoredPosition = anchoredPosition;
		}
	}

	private void Update()
	{
		image.fillAmount = Mathf.SmoothDamp(image.fillAmount, fillTarget, ref velocity, smoothTime);
		MoveMoveImages();
	}
}
