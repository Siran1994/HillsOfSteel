using UnityEngine;

public class RigidbodyRotationConstraint : MonoBehaviour
{
	public float angle = 65f;

	private void FixedUpdate()
	{
		Rigidbody2D component = GetComponent<Rigidbody2D>();
		component.rotation = Mathf.Clamp(component.rotation, 0f - angle, angle);
	}
}
