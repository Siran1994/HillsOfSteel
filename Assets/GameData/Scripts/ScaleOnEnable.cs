using UnityEngine;

public class ScaleOnEnable : MonoBehaviour
{
	public Vector3 from = Vector3.zero;

	public Vector3 to = Vector3.one;

	public float time = 1f;

	public LeanTweenType type = LeanTweenType.easeOutBack;

	private void OnEnable()
	{
		base.transform.localScale = from;
		LeanTween.scale(base.gameObject, to, time).setEase(type);
	}
}
