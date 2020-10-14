using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : BoosterBehaviour
{
	public ParticleSystem particles;

	public Rigidbody2D body;

	public LayerMask layerMask;

	public SpriteRenderer mainSprite;

	public SpriteRenderer glowSprite;

	[Header("Stats")]
	public float timeAtMinLevel = 2.5f;

	public float timeAtMaxLevel = 1.5f;

	public float damageAtMinLevel = 10f;

	public float damageAtMaxLevel = 10f;

	public float damageRangeAtMinLevel = 2.5f;

	public float damageRangeAtMaxLevel = 3f;

	public float forceAtMinLevel = 10f;

	public float forceAtMaxLevel = 20f;

	private Coroutine explosionRoutine;

	public bool Detonated
	{
		get;
		set;
	}

	private IEnumerator Start()
	{
		GetComponent<Collider2D>().enabled = false;
		AudioMap audioMap = TankGame.instance.audioMap;
		AudioMap.PlayClipAt(audioMap, "mineSpawn", base.transform.position, audioMap.effectsMixerGroup);
		for (float t = 0f; t < 0.75f; t += Time.deltaTime)
		{
			float t2 = LeanTween.easeOutElastic(0f, 1f, t / 0.75f);
			base.transform.localScale = Vector3.Lerp(Vector3.zero, new Vector3(2f, 2f, 2f), t2);
			yield return null;
		}
		GetComponent<Collider2D>().enabled = true;
		Detonated = false;
		yield return new WaitForSeconds(2f);
		body.simulated = false;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (explosionRoutine == null && ((1 << collision.gameObject.layer) & (int)layerMask) != 0)
		{
			mainSprite.enabled = true;
			GetComponent<Collider2D>().enabled = false;
			explosionRoutine = StartCoroutine(Explosion());
		}
	}

	public void IgnoreLayer(GameObject go)
	{
		layerMask = ((int)layerMask & ~(1 << go.layer));
	}

	private IEnumerator Explosion()
	{
		AudioMap audioMap = TankGame.instance.audioMap;
		float time = Mathf.Lerp(timeAtMinLevel, timeAtMaxLevel, base.Booster.GetValue());
		int num;
		for (int i = 0; i < 5; i = num)
		{
			glowSprite.enabled = !glowSprite.enabled;
			if (glowSprite.enabled)
			{
				AudioMap.PlayClipAt(audioMap, "mineBeep", base.transform.position, audioMap.effectsMixerGroup);
			}
			yield return new WaitForSeconds(time / 5f);
			num = i + 1;
		}
		AudioMap.PlayClipAt(audioMap, "mineExplode", base.transform.position, audioMap.effectsMixerGroup);
		glowSprite.enabled = false;
		mainSprite.enabled = false;
		float radius = Mathf.Lerp(damageRangeAtMinLevel, damageRangeAtMaxLevel, base.Booster.GetValue());
		RaycastHit2D[] array = Physics2D.CircleCastAll(base.transform.position, radius, Vector2.zero, 0f, layerMask);
		HashSet<Vehicle> hashSet = new HashSet<Vehicle>();
		for (int j = 0; j < array.Length; j++)
		{
			Vehicle componentInParent = array[j].collider.GetComponentInParent<Vehicle>();
			if (componentInParent != null && !hashSet.Contains(componentInParent))
			{
				hashSet.Add(componentInParent);
				componentInParent.CurrentHealth -= Mathf.Lerp(damageAtMinLevel, damageAtMaxLevel, base.Booster.GetValue());
				componentInParent.rigidbodies[0].AddForce((componentInParent.transform.position - base.transform.position).normalized * Mathf.Lerp(forceAtMinLevel, forceAtMaxLevel, base.Booster.GetValue()), ForceMode2D.Impulse);
			}
		}
		particles.Play();
		Detonated = true;
		UnityEngine.Object.Destroy(base.gameObject, particles.main.duration);
	}
}
