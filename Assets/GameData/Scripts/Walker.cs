using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walker : TankContainer
{
	private struct CollisionMatrix
	{
		public bool immovableLeft;

		public bool immovableRight;

		public bool movableLeft;

		public bool movableRight;
	}

	[Space(20f)]
	[Header("Kinematic tanks")]
	public float animationSpeedMultiplier = 1.1f;

	public float collisionSpeedMultiplier = 0.2f;

	public float engineMaxPitch = 1.075f;

	public float collisionRadius = 2f;

	public Animator animator;

	public Transform pivotBack;

	public Transform pivotFront;

	public Transform chassis;

	public SpringJoint2D[] springJoints;

	private float animationSpeed;

	private float chassisRotationTarget;

	private Vector2 centerOffset;

	private Vector2 back;

	private Vector2 center;

	private Vector2 front;

	private CollisionMatrix collisionMatrix;

	private Dictionary<TankContainer, float> activeCollisions;

	private MinMaxFloat moveBoundaries;

	private bool destroyed;

	private bool inGarage;

	protected new void Awake()
	{
		base.Awake();
		centerOffset.x = base.Rigidbody.position.x - pivotBack.position.x;
		centerOffset.y = base.Rigidbody.position.y - pivotFront.position.y - 0.25f;
		animator = GetComponent<Animator>();
	}

	protected new void Start()
	{
		base.Start();
		inGarage = (base.gameObject.layer == LayerMask.NameToLayer("GarageTank") || !TankGame.instance);
		if (!inGarage)
		{
			animator.SetFloat("Speed", 0f);
			base.Rigidbody.position = new Vector2(base.Rigidbody.position.x, TankGame.instance.GetGroundHeight(base.Rigidbody.position.x) + centerOffset.y);
			moveBoundaries = new MinMaxFloat(TankGame.instance.backBlockerContainer.position.x, PlayerDataManager.IsSelectedGameModePvP ? TankGame.instance.frontBlockerContainer.position.x : float.PositiveInfinity);
			activeCollisions = new Dictionary<TankContainer, float>();
			FixedUpdate();
			chassis.rotation = Quaternion.Euler(0f, chassis.rotation.eulerAngles.y, chassisRotationTarget);
			StartCoroutine(ChassisRotationRoutine());
		}
	}

	protected void FixedUpdate()
	{
		if (inGarage || destroyed)
		{
			return;
		}
		back = pivotBack.position;
		front = pivotFront.position;
		back.y = TankGame.instance.GetGroundHeight(back.x, approximate: false);
		front.y = TankGame.instance.GetGroundHeight(front.x, approximate: false);
		Vector2 vector = front - back;
		Vector2 normalized = Vector2.Perpendicular(vector).normalized;
		normalized.y = Mathf.Abs(normalized.y);
		Vector2 a = back + vector.normalized * centerOffset.x;
		center = (a + normalized * centerOffset.y).Replace(base.Rigidbody.position.x);
		chassisRotationTarget = Mathf.Atan(vector.y / vector.x) * 57.29578f;
		animationSpeed = (0f - base.Movement) * animationSpeedMultiplier;
		if ((base.Movement < 0f && !base.Flipped) || (base.Movement > 0f && base.Flipped))
		{
			if (collisionMatrix.immovableRight)
			{
				animationSpeed *= collisionSpeedMultiplier;
				base.Movement = 0f;
			}
			else if (collisionMatrix.movableRight)
			{
				animationSpeed *= collisionSpeedMultiplier;
				base.Movement *= collisionSpeedMultiplier;
			}
		}
		else if ((base.Movement > 0f && !base.Flipped) || (base.Movement < 0f && base.Flipped))
		{
			if (collisionMatrix.immovableLeft)
			{
				animationSpeed *= collisionSpeedMultiplier;
				base.Movement = 0f;
			}
			else if (collisionMatrix.movableLeft)
			{
				animationSpeed *= collisionSpeedMultiplier;
				base.Movement *= collisionSpeedMultiplier;
			}
		}
		base.Rigidbody.MoveRotation(chassisRotationTarget);
		base.Rigidbody.MovePosition(center + Vector2.right * (0f - base.Movement) * base.Stats.maxSpeed * Time.fixedDeltaTime);
	}

	protected new void Update()
	{
		base.Update();
		if (!inGarage && !destroyed)
		{
			if ((bool)TankGame.instance)
			{
				moveBoundaries.min = TankGame.instance.backBlockerContainer.position.x;
			}
			UpdateCollisions();
			animator.SetBool("IsWalking", base.Movement != 0f);
			animator.SetFloat("Speed", animationSpeed * (float)((!base.Flipped) ? 1 : (-1)));
		}
	}

	private void UpdateCollisions()
	{
		List<TankContainer> list = new List<TankContainer>();
		CollisionMatrix collisionMatrix = default(CollisionMatrix);
		Vector3 position = base.transform.position;
		foreach (KeyValuePair<TankContainer, float> activeCollision in activeCollisions)
		{
			TankContainer key = activeCollision.Key;
			if (key == null || key.transform == null || key.CurrentHealth <= 0f)
			{
				list.Add(key);
			}
			else
			{
				Vector3 position2 = key.transform.position;
				if ((position - position2).sqrMagnitude > activeCollision.Value + collisionRadius)
				{
					list.Add(key);
				}
				else
				{
					bool flag = position.x < position2.x;
					if (key.Enemy != null && TankGame.instance.Boss != null && key.Enemy == TankGame.instance.Boss)
					{
						collisionMatrix.immovableLeft |= !flag;
						collisionMatrix.immovableRight |= flag;
					}
					else
					{
						collisionMatrix.movableLeft |= !flag;
						collisionMatrix.movableRight |= flag;
					}
				}
			}
		}
		if (position.x - collisionRadius <= moveBoundaries.min)
		{
			collisionMatrix.immovableLeft = true;
		}
		else if (!float.IsInfinity(moveBoundaries.max) && position.x + collisionRadius >= moveBoundaries.max)
		{
			collisionMatrix.immovableRight = true;
		}
		this.collisionMatrix = collisionMatrix;
		foreach (TankContainer item in list)
		{
			activeCollisions.Remove(item);
		}
	}

	public override void SetSpeed(float speed, float movement)
	{
		base.SetSpeed(speed, movement);
		base.Movement = movement;
	}

	public override void AddRecoil(Vector2 force)
	{
		recoilRigidbody.AddTorque(force.sqrMagnitude, ForceMode2D.Impulse);
	}

	public override void DoCannonMove(float time)
	{
		if (cannons.Length != 0 && cannonRoutine == null)
		{
			cannonRoutine = StartCoroutine(CannonLogic(base.BulletDef.perMissileTime));
		}
	}

	public override void PreDestroy()
	{
		if (destroyed)
		{
			return;
		}
		destroyed = true;
		Rigidbody2D[] rigidbodies = base.rigidbodies;
		foreach (Rigidbody2D rigidbody2D in rigidbodies)
		{
			if (!(rigidbody2D == null))
			{
				rigidbody2D.gameObject.layer = LayerMask.NameToLayer("Scrap");
				rigidbody2D.simulated = true;
			}
		}
		if (base.Rigidbody != null)
		{
			base.Rigidbody.simulated = false;
		}
		animator.enabled = false;
		SpringJoint2D[] array = springJoints;
		for (int i = 0; i < array.Length; i++)
		{
			UnityEngine.Object.Destroy(array[i]);
		}
		springJoints = new SpringJoint2D[0];
	}

	private IEnumerator ChassisRotationRoutine()
	{
		while (true)
		{
			float z = chassis.rotation.eulerAngles.z;
			float num = chassisRotationTarget * (float)((!base.Flipped) ? 1 : (-1));
			float t = Mathf.Abs(z - num) * Time.deltaTime * 10f;
			chassis.rotation = Quaternion.Euler(0f, chassis.rotation.eulerAngles.y, Mathf.LerpAngle(z, num, t));
			yield return new WaitForEndOfFrame();
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		int num = 1 << collision.gameObject.layer;
		int num2 = enemyLayerMask | LayerMask.GetMask("EnemyTank", "EnemyTankMain");
		if ((num & num2) == num)
		{
			TankContainer tankContainer = collision.gameObject.GetComponent<TankContainer>();
			if (!tankContainer)
			{
				tankContainer = collision.gameObject.GetComponentInParents<TankContainer>(4, num2);
			}
			if ((bool)tankContainer && !activeCollisions.ContainsKey(tankContainer))
			{
				activeCollisions.Add(tankContainer, ((Vector2)base.transform.position - (Vector2)tankContainer.transform.position).sqrMagnitude);
			}
		}
	}
}
