using UnityEngine;

public class PingPongRotation : MonoBehaviour
{
	public bool lockWorld;

	public Vector3 from;

	public Vector3 to;

	private Quaternion world;

	private float time;

	private void Awake()
	{
		world = base.transform.rotation;
	}

	private void Update()
	{
		Quaternion identity = Quaternion.identity;
		identity = ((!lockWorld) ? base.transform.rotation : world);
		time += Time.deltaTime;
		float val = Mathf.PingPong(time, 1f);
		if (time > 2f)
		{
			time -= 2f;
		}
		identity *= Quaternion.Euler(Vector3.Lerp(from, to, LeanTween.easeInOutSine(0f, 1f, val)));
		base.transform.rotation = identity;
	}
}
