using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgressChestButton : MonoBehaviour
{
	public float sheenAnimationInterval = 5f;

	public float shakeAnimationTime = 1f;

	public float shakeAnimationInterval = 5f;

	public bool openable;

	public ChestProgressionType type;

	public Button button;

	public Image progressImage;

	public GameObject progressFull;

	public CountText progressText;

	public Transform starAnimationTarget;

	public GameObject openableSheen;

	public GameObject openableIcon;

	public TextMeshProUGUI openableText;

	private Vector2 sheenOriginalPosition = new Vector2(-200f, 25f);

	private void OnEnable()
	{
		openableSheen.transform.localPosition = sheenOriginalPosition;
		StartCoroutine(OpenableSheenAnimation());
		StartCoroutine(OpenableShakeAnimation());
	}

	public void SetProgress(float fill)
	{
		progressImage.fillAmount = fill;
		progressImage.gameObject.SetActive(fill < 1f);
		SetOpenable(fill >= 1f);
	}

	private void SetOpenable(bool value)
	{
		progressFull.SetActive(value);
		openableIcon.SetActive(value);
		openableText.text = Mathf.FloorToInt(PlayerDataManager.GetChestProgressPercentage(type)).ToString();
		if (value != openable)
		{
			openable = value;
			if (openable && base.gameObject.activeInHierarchy)
			{
				StartCoroutine(OpenableSheenAnimation());
				StartCoroutine(OpenableShakeAnimation());
			}
		}
	}

	private IEnumerator OpenableSheenAnimation()
	{
		while (openable)
		{
			Vector2 start = sheenOriginalPosition;
			Vector2 end = start + Vector2.right * 400f;
			float time = 0.6f;
			for (float t = 0f; t < time; t += Time.deltaTime)
			{
				openableSheen.transform.localPosition = Vector3.Lerp(start, end, t / time);
				yield return null;
			}
			openableSheen.transform.localPosition = start;
			yield return new WaitForSeconds(sheenAnimationInterval);
		}
	}

	private IEnumerator OpenableShakeAnimation()
	{
		while (openable)
		{
			float time = 0f;
			for (float t = 0f; t < shakeAnimationTime; t += Time.deltaTime)
			{
				button.transform.rotation = Quaternion.Euler(0f, 0f, 35f * (Mathf.PingPong(t / 0.2f, 0.3f) - 0.2f));
				yield return null;
				time += Time.deltaTime;
			}
			button.transform.rotation = Quaternion.identity;
			yield return new WaitForSeconds(shakeAnimationInterval);
		}
	}
}
