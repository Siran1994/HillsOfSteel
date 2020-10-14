using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class AnimatedUIElement : MonoBehaviour
{
	public List<LTDescr> tweens;

	public float delay;

	public float maxScale = 1f;

	public void CancelAllTweens()
	{
		LeanTween.cancel(base.gameObject);
	}

	public void CancellAllAndFinish()
	{
		LeanTween.cancel(base.gameObject, callOnComplete: true);
		GetComponent<CanvasGroup>().alpha = 1f;
	}
}
