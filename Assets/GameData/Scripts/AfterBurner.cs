using System.Collections;
using UnityEngine;

public class AfterBurner : MonoBehaviour
{
	public IEnumerator Start()
	{
		LeanTween.scaleX(base.gameObject, 1.35f, 0.09f).setRepeat(-1).setLoopPingPong();
		LeanTween.scaleY(base.gameObject, 0.8f, 0.076f).setRepeat(-1).setLoopPingPong();
		SpriteRenderer sr = GetComponent<SpriteRenderer>();
		while (true)
		{
			sr.enabled = !sr.enabled;
			yield return new WaitForSeconds(0.03f);
		}
	}
}
