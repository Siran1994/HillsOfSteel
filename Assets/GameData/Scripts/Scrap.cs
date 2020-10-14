using System.Collections;
using UnityEngine;

public class Scrap : MonoBehaviour
{
	public Rigidbody2D body;

	public Collider2D collider2d;

	public SpriteRenderer sprite;

	public bool IsActive
	{
		get;
		private set;
	}

	public bool IsFadingOut
	{
		get;
		private set;
	}

	private void OnDisable()
	{
		if (IsActive)
		{
			TankGame.instance.AddScrap(-1);
		}
	}

	public void SetActive(ScrapMode mode)
	{
		IsActive = true;
		FadeOut(mode);
	}

	public void FadeOut(ScrapMode mode)
	{
		if (!IsFadingOut && IsActive)
		{
			switch (mode)
			{
			case ScrapMode.FadeOnCollision:
				break;
			case ScrapMode.Normal:
				StartCoroutine(FadeOutRoutine(immediate: false));
				break;
			case ScrapMode.FadeImmediate:
				StartCoroutine(FadeOutRoutine());
				break;
			}
		}
	}

	private IEnumerator FadeOutRoutine(bool immediate = true)
	{
		IsFadingOut = true;
		if (!immediate)
		{
			yield return new WaitForSeconds(Variables.instance.enemyScrapDuration);
		}
		for (float t = 0f; t < Variables.instance.enemyScrapFadeOutTime; t += Time.deltaTime)
		{
			if (!sprite)
			{
				break;
			}
			float t2 = t / Variables.instance.enemyScrapFadeOutTime;
			sprite.color = Color.Lerp(Color.white, new Color(1f, 1f, 1f, 0f), t2);
			yield return null;
		}
	}

	private void OnCollisionEnter2D(Collision2D col)
	{
		FadeOut(ScrapMode.FadeImmediate);
	}
}
