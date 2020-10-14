using UnityEngine;

public class ConstantRotation : MonoBehaviour
{
	public float speed = 1f;

	public Vector3 rotation;

	public bool useUnscaled = true;

	private void Update()
	{
		float d = useUnscaled ? Time.unscaledDeltaTime : Time.deltaTime;
		base.transform.rotation *= Quaternion.Euler(rotation * d * speed);
	}
}
