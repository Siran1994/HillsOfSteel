using UnityEngine;

public class CircleSaw : BoosterBehaviour
{
	public ParticleSystem particles;

	public Collider2D damageCollider;

	public ConstantRotation rotation;

	public AudioSource startSound;

	public AudioSource damageSound;

	public AudioSource endSound;

	private int lastFrame;

	private bool didStart;

	private bool didEnd = true;

	private RaycastHit2D[] result = new RaycastHit2D[10];

	private TankContainer thisVehicle;

	private void OnEnable()
	{
		thisVehicle = GetComponentInParent<TankContainer>();
	}

	private void FixedUpdate()
	{
		if (!thisVehicle || thisVehicle.transform == null || base.transform.parent == null)
		{
			return;
		}
		ContactFilter2D contactFilter = default(ContactFilter2D);
		int num = thisVehicle.hitMask | LayerMask.GetMask("EnemyTank", "EnemyTankMain");
		num &= ~(1 << base.transform.parent.gameObject.layer);
		contactFilter.SetLayerMask(num);
		int num2 = damageCollider.Cast(Vector2.zero, contactFilter, result, 0f);
		Vehicle vehicle = null;
		for (int i = 0; i < num2; i++)
		{
			vehicle = result[i].collider.GetComponentInParent<Vehicle>();
			if (vehicle != null)
			{
				break;
			}
		}
		if (vehicle != null)
		{
			vehicle.CurrentHealth -= base.Booster.GetValue() * Time.fixedDeltaTime;
			rotation.speed = 2f;
			particles.Emit(10);
			Transform airControlPointBack = thisVehicle.airControlPointBack;
			Rigidbody2D component = thisVehicle.transform.GetComponent<Rigidbody2D>();
			if (component != null)
			{
				component.AddForceAtPosition(airControlPointBack.right * -30f + airControlPointBack.up * 10f, airControlPointBack.position);
			}
			if (!didStart)
			{
				startSound.Play();
				endSound.Stop();
				didEnd = false;
				didStart = true;
			}
			else if (!startSound.isPlaying && !damageSound.isPlaying)
			{
				damageSound.Play();
			}
		}
		else if (!didEnd)
		{
			startSound.Stop();
			damageSound.Stop();
			endSound.Play();
			didEnd = true;
			didStart = false;
			rotation.speed = 0.5f;
		}
	}
}
