using UnityEngine;

public class ClearColliders : MonoBehaviour
{
	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.layer != LayerMask.NameToLayer("Bullet"))
		{
			UnityEngine.Object.Destroy(collision.gameObject);
		}
		else
		{
			TankGame.instance.DestroyBullet(collision.gameObject.GetComponent<BulletContainer>().Bullet, 0f);
		}
	}
}
