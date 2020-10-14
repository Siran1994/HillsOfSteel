using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Mask))]
public class AnimateFlash : MonoBehaviour
{
	public float duration;

	public Image flash;

	public bool showParticles;

	public ParticleSystem particles;

	public void OnDisable()
	{
		if ((bool)flash)
		{
			flash.color = new Color(1f, 1f, 1f, 0f);
		}
	}

	public void Play()
	{
		if (flash != null)
		{
			StartCoroutine(FlashAnimation());
		}
	}

	private IEnumerator FlashAnimation()
	{
		for (float t = 0f; t < duration; t += Time.deltaTime)
		{
			flash.color = new Color(1f, 1f, 1f, LeanTween.easeInCubic(0f, 1f, t / duration));
			yield return null;
		}
		flash.color = new Color(1f, 1f, 1f, 0f);
		if (showParticles && particles != null)
		{
			particles.Play();
		}
	}

	private void Reset()
	{
	}
}
