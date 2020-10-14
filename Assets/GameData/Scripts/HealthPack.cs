using System.Collections;
using UnityEngine;

public class HealthPack : MonoBehaviour
{
	public float percentage = 0.2f;

	public GameObject wrench;

	private TankContainer target;

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (((1 << collision.collider.gameObject.layer) & LayerMask.GetMask("PlayerTank", "PlayerTankRight")) != 0)
		{
			target = collision.transform.GetComponentInParent<TankContainer>();
			TankGame.GivePlayerHealth(target, percentage);
			wrench.transform.SetParent(TankGame.instance.transform);
			wrench.transform.rotation = Quaternion.identity;
			Vector3 localScale = wrench.transform.localScale;
			localScale.Scale(new Vector3(2f, 2f, 1f));
			LeanTween.scale(wrench, localScale, 1f).setEaseInOutExpo();
			TankGame.instance.Delay(0.5f, delegate
			{
				LeanTween.rotate(wrench, new Vector3(0f, 0f, -45f), 0.5f).setEaseInOutExpo();
			});
			TankGame.instance.Delay(1f, delegate
			{
				LeanTween.rotate(wrench, new Vector3(0f, 0f, 0f), 0.5f).setEaseInOutExpo();
			});
			TankGame.instance.Delay(1.5f, delegate
			{
				LeanTween.rotate(wrench, new Vector3(0f, 0f, -45f), 0.5f).setEaseInOutExpo();
			});
			TankGame.instance.Delay(2f, delegate
			{
				LeanTween.rotate(wrench, new Vector3(0f, 0f, 0f), 0.5f).setEaseInOutExpo();
			});
			TankGame.instance.Delay(2f, delegate
			{
				LeanTween.alpha(wrench, 0f, 0.5f);
				UnityEngine.Object.Destroy(wrench, 3f);
				UnityEngine.Object.Destroy(base.gameObject, 3.5f);
			});
			GetComponent<Collider2D>().enabled = false;
			if (GetComponent<SpriteRenderer>() != null)
			{
				GetComponent<SpriteRenderer>().enabled = false;
			}
			StartCoroutine(Move());
		}
	}

	private IEnumerator Move()
	{
		for (float t = 0f; t < 1f; t += Time.deltaTime)
		{
			if (target == null || !target.transform)
			{
				LeanTween.cancelAll();
				break;
			}
			wrench.transform.position = target.transform.position + Vector3.up * LeanTween.easeInOutExpo(0f, 3f, t);
			yield return null;
		}
		while (!(target == null) && !(wrench == null) && !(base.gameObject == null) && !(target.transform == null))
		{
			wrench.transform.position = target.transform.position + Vector3.up * 3f;
			yield return null;
		}
		LeanTween.cancelAll();
	}
}
