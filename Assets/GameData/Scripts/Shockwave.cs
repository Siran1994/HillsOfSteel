using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shockwave : MonoBehaviour
{
	public float progress;

	public float time = 2f;

	public float radius = 15f;

	public float minDamage = 5f;

	public float maxDamage = 10f;

	public float enterMinForce = 10f;

	public float enterMaxForce = 20f;

	public float enterMinTorque = 2f;

	public float enterMaxTorque = 5f;

	public float stayMinForce = 10f;

	public float stayMaxForce = 20f;

	public float stayMinTorque = 2f;

	public float stayMaxTorque = 5f;

	private HashSet<Vehicle> vehiclesHit;

	public Vehicle Owner
	{
		get;
		set;
	}

	private void OnEnable()
	{
		vehiclesHit = new HashSet<Vehicle>();
	}

	private IEnumerator Start()
	{
		SpriteRenderer sr = GetComponent<SpriteRenderer>();
		for (float t = 0f; t < time; t += Time.fixedDeltaTime)
		{
			float t2 = LeanTween.easeOutQuart(0f, 1f, t / time);
			float num = Mathf.Lerp(1f, radius, t2);
			base.transform.localScale = new Vector3(num, num, 1f);
			if (t > time - 0.25f)
			{
				sr.color = Color.Lerp(Color.white, new Color(1f, 1f, 1f, 0f), (t - time + 0.25f) / 0.25f);
			}
			yield return new WaitForFixedUpdate();
		}
		sr.color = new Color(0f, 0f, 0f, 0f);
		GetComponent<Collider2D>().enabled = false;
		while (GetComponent<AudioSource>().isPlaying)
		{
			yield return null;
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		Vehicle vehicle = collision.GetComponent<Vehicle>();
		if (vehicle == null)
		{
			vehicle = collision.GetComponentInParent<Vehicle>();
		}
		if (!(vehicle != null) || !(vehicle != Owner) || vehiclesHit.Contains(vehicle))
		{
			return;
		}
		vehicle.CurrentHealth -= Mathf.Lerp(minDamage, maxDamage, progress);
		vehiclesHit.Add(vehicle);
		Vector3 normalized = (collision.transform.position - base.transform.position).normalized;
		float num = Mathf.Lerp(enterMinForce, enterMaxForce, progress);
		float num2 = Mathf.Lerp(enterMinTorque, enterMaxTorque, progress);
		if (PlayerDataManager.SelectedGameMode == GameMode.Arena2v2)
		{
			num *= 0.5f;
			num2 *= 0.5f;
		}
		Rigidbody2D component = collision.GetComponent<Rigidbody2D>();
		if (!(component != null))
		{
			return;
		}
		if (component.bodyType == RigidbodyType2D.Dynamic)
		{
			Vector2 force = new Vector2(num * normalized.x, num);
			component.AddForce(force, ForceMode2D.Impulse);
			component.AddTorque(Mathf.Sign(normalized.x) * num2, ForceMode2D.Impulse);
		}
		else if (component.bodyType == RigidbodyType2D.Kinematic)
		{
			Helicopter component2 = component.GetComponent<Helicopter>();
			if (component2 != null)
			{
				component2.ExtraMove += (Vector2)normalized * num * 0.2f;
			}
		}
	}

	private void OnTriggerStay2D(Collider2D collision)
	{
		Vector3 normalized = (collision.transform.position - base.transform.position).normalized;
		float num = Mathf.Lerp(stayMinForce, stayMaxForce, progress);
		float num2 = Mathf.Lerp(stayMinTorque, stayMaxTorque, progress);
		if (PlayerDataManager.SelectedGameMode == GameMode.Arena2v2)
		{
			num *= 0.5f;
			num2 *= 0.5f;
		}
		Vehicle component = collision.GetComponent<Vehicle>();
		if (component != null && component != Owner && component.rigidbodies[0].bodyType == RigidbodyType2D.Dynamic)
		{
			Vector2 force = new Vector2(num * normalized.x, 0f);
			component.rigidbodies[0].AddForce(force, ForceMode2D.Force);
			component.rigidbodies[0].AddTorque(Mathf.Sign(normalized.x) * num2, ForceMode2D.Force);
		}
	}
}
