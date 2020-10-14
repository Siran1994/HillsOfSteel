using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceField : BoosterBehaviour
{
	public ParticleSystem[] particles;

	public Vehicle attachedVehicle;

	public float minHealth = 0.1f;

	public float maxHealth = 0.2f;

	public float minPushForce = 100f;

	public float maxPushForce = 200f;

	private float fullHealth;

	private float health;

	private float pushForce;

	private HashSet<Vehicle> vehiclesDamaged;

	private IEnumerator Start()
	{
		if (attachedVehicle.gameObject.layer == LayerMask.NameToLayer("PlayerTank"))
		{
			base.gameObject.layer = LayerMask.NameToLayer("ForceField");
		}
		else
		{
			base.gameObject.layer = LayerMask.NameToLayer("ForceFieldRight");
		}
		SpriteRenderer sr = GetComponent<SpriteRenderer>();
		sr.color = new Color(0f, 0f, 0f, 0f);
		vehiclesDamaged = new HashSet<Vehicle>();
		float value = base.Booster.GetValue();
		float num = Mathf.Lerp(minHealth, maxHealth, value);
		pushForce = Mathf.Lerp(minPushForce, maxPushForce, value);
		fullHealth = (health = attachedVehicle.Stats.health * num);
		Vector3 position = attachedVehicle.transform.position;
		position.z = base.transform.position.z;
		base.transform.position = position;
		base.transform.localScale = Vector3.zero;
		Bounds bounds = default(Bounds);
		for (int i = 0; i < attachedVehicle.spriteRenderers.Length; i++)
		{
			if (attachedVehicle.spriteRenderers[i] != null)
			{
				bounds.Encapsulate(attachedVehicle.spriteRenderers[i].transform.localPosition);
			}
		}
		AudioMap.PlayClipAt("forceFieldAppear", base.transform.position, AudioMap.instance.effectsMixerGroup);
		float forceFieldScale = attachedVehicle.forceFieldScale;
		Vector3 targetScale = new Vector3(forceFieldScale, forceFieldScale, 1f);
		for (float t = 0f; t < 0.5f; t += Time.deltaTime)
		{
			if (base.transform == null || sr == null)
			{
				yield break;
			}
			float t2 = LeanTween.easeInOutCirc(0f, 1f, t / 0.5f);
			base.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, t2);
			if (t > 0.3f)
			{
				float t3 = LeanTween.easeInOutCirc(0f, 1f, (t - 0.3f) / 0.2f);
				sr.color = Color.Lerp(new Color(1f, 1f, 1f, 0f), Color.white, t3);
			}
			yield return null;
		}
		base.transform.localScale = targetScale;
		sr.color = Color.white;
	}

	private void Update()
	{
		if (attachedVehicle != null)
		{
			float num = (attachedVehicle.gameObject.layer == LayerMask.NameToLayer("PlayerTank")) ? 1f : (-1f);
			Vector3 forceFieldOffset = attachedVehicle.forceFieldOffset;
			forceFieldOffset.x *= num;
			Vector3 position = attachedVehicle.transform.position + forceFieldOffset;
			position.z = base.transform.position.z;
			base.transform.position = position;
		}
		if (health <= 0f)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		vehiclesDamaged.Clear();
	}

	public void Absorb(Vector2 position, float damage)
	{
		health -= damage;
		float a = LeanTween.easeOutExpo(0f, 1f, health / fullHealth);
		GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, a);
		for (int i = 0; i < particles.Length; i++)
		{
			particles[i].transform.position = position;
			particles[i].Emit(Random.Range(10, 20));
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		Vehicle componentInParent = collision.gameObject.GetComponentInParent<Vehicle>();
		if (componentInParent != null && componentInParent != attachedVehicle && !vehiclesDamaged.Contains(componentInParent))
		{
			componentInParent.rigidbodies[0].AddForce((componentInParent.transform.position - base.transform.position).normalized * pushForce, ForceMode2D.Impulse);
			vehiclesDamaged.Add(componentInParent);
		}
		BulletContainer componentInParent2 = collision.gameObject.GetComponentInParent<BulletContainer>();
		if (componentInParent2 != null && componentInParent2.Bullet.owner != attachedVehicle.transform)
		{
			Absorb(componentInParent2.transform.position, componentInParent2.Bullet.damage);
			componentInParent2.Bullet.explodeDelayed = false;
			TankGame.instance.DestroyBullet(componentInParent2.Bullet, 0f);
			AudioMap.PlayClipAt("forceFieldAbsorbBig", base.transform.position, TankGame.instance.audioMap.effectsMixerGroup);
		}
	}
}
