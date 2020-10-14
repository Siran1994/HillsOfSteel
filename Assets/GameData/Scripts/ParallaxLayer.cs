using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
	public Vector3 moveSpeedRelativeToCamera;

	public Vector3 movePerFrame;

	public Bounds bounds;

	[ContextMenu("Use Sprite Bounds")]
	public void UseSpriteBounds()
	{
		SpriteRenderer component = GetComponent<SpriteRenderer>();
		if (component != null)
		{
			bounds = component.bounds;
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.magenta;
		Gizmos.DrawWireCube(base.transform.position, bounds.size);
	}
}
