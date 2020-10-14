using UnityEngine;

public class ConstantScale : MonoBehaviour
{
	public Vector3 min;

	public Vector3 max;

	public float speed = 1f;

	private float t;

	private void Update()
	{
		t += Time.unscaledDeltaTime * speed;
		if (t > 2f)
		{
			t -= 2f;
		}
		float val = Mathf.PingPong(t, 1f);
		float num = LeanTween.easeInOutCirc(0f, 1f, val);
		base.transform.localScale = Vector3.Lerp(min, max, num);
	}
}
