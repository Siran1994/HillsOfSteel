using UnityEngine;

public class Coin : MonoBehaviour
{
	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.layer == LayerMask.NameToLayer("PlayerTank"))
		{
			MenuController.GetMenu<GameMenu>().AnimateGameCoins(1, base.transform);
			TankGame.instance.AddCoins(1, sync: false);
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
