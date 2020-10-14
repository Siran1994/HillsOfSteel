using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Vehicle : MonoBehaviour
{
	[Serializable]
	public class SetActiveAtHealth
	{
		public bool active;

		[Range(0f, 1f)]
		public float health;

		public GameObject gameObject;
	}

	[Serializable]
	public class TriggerAtHealth
	{
		public bool triggered;

		[Range(0f, 1f)]
		public float health;

		public Action action;

		public TriggerAtHealth(Action action, float health)
		{
			triggered = false;
			this.health = health;
			this.action = action;
		}

		public void Trigger()
		{
			triggered = true;
			action();
		}
	}

	public class FumeState
	{
		public Gradient gradient;

		public GradientColorKey[] colorKeys;

		public GradientAlphaKey[] alphaKeys;
	}

	private enum SmallBulletState
	{
		Starting,
		Shooting,
		Cooldown
	}

	public string deathSound = "death";

	public GameObject explosionPrefab;

	public GameObject blackSmokePrefab;

	public Transform cameraFollowTransform;

	public SetActiveAtHealth[] setActiveAtHealth;

	public ParticleSystem[] setActiveWhileMoving;

	public float activeWhileMovingLifetimeMin;

	public float activeWhileMovingLifetimeMax;

	public float activeWhileMovingSmoothTime = 1f;

	public ParticleSystem[] fumes;

	public float fumeMultiplier = 1f;

	[Header("Shooting")]
	public SpriteRenderer[] cannons;

	public float cannonOffset;

	public Transform turretMarker;

	public Transform turret;

	public Transform[] bulletShootTransforms;

	public Transform rotateBarrel;

	public float barrelRotateRate = 720f;

	public bool randomizeMuzzles;

	public float muzzleTime = 0.1f;

	public SpriteRenderer[] bulletMuzzles;

	public LineRenderer[] bulletLines;

	public ParticleSystem[] bulletSparks;

	public ParticleSystem[] bulletGroundSparks;

	public ParticleSystem[] shootParticles;

	public ParticleSystem lightningSparkles;

	public Transform lightningLiquidTank;

	public float lightningLiquidTankMoveFrom;

	public float lightningLiquidTankMoveTo;

	public Rigidbody2D recoilRigidbody;

	public Electricity electricity;

	public float shootShakeMultiplier = 1f;

	protected int currentShootTransform;

	protected Vector3[] cannonPositions;

	protected Vector3 cannonPos;

	protected Coroutine cannonRoutine;

	[Header("Driver")]
	public Transform torso;

	public Transform driverRagdoll;

	public Transform[] hats;

	public Transform[] ragdollHats;

	[Header("Transforms")]
	public Transform healthBarBase;

	public Transform healthBar;

	public Transform top;

	public Transform blackSmokeContainer;

	[Header("Audio")]
	public AudioSource audioSourceAimSound;

	public AudioSource audioSourceShoot;

	public AudioSource audioSourceEngine;

	[Header("Flamethrower (optional)")]
	public Transform flamethrower;

	public ParticleSystem[] flamerParticles;

	public float[] flamerEmissionRates;

	[Header("Dust (optional)")]
	public ParticleSystem[] dustParticles;

	public float dustRaycastDistance = 1f;

	[Header("Boosters")]
	public Transform circleSawContainer;

	public float forceFieldScale = 1f;

	public Vector3 forceFieldOffset;

	[Header("Reload lights")]
	public int[] lightsPerBatch;

	public SpriteRenderer[] reloadLights;

	[HideInInspector]
	public Rigidbody2D[] rigidbodies;

	[HideInInspector]
	public Rigidbody2D ragdollRigidbody;

	public Scrap[] scraps;

	[HideInInspector]
	public Joint2D[] topJoints;

	[HideInInspector]
	public WheelJoint2D[] wheelJoints;

	[HideInInspector]
	public PlayOnCollision[] playOnCollisions;

	[HideInInspector]
	public Collider2D[] collider2Ds;

	[HideInInspector]
	public SpriteRenderer[] spriteRenderers;

	[HideInInspector]
	public SpriteRenderer[] ragdollRenderers;

	[HideInInspector]
	public SpriteRenderer[] wheels;

	[HideInInspector]
	public ParticleSystem explosionParticles;

	[HideInInspector]
	public ParticleSystem blackSmoke;

	[HideInInspector]
	public Chain[] chains;

	[HideInInspector]
	public CircleCollider2D[] wheelColliders;

	private FumeState[] fumeStates;

	private Color healthBarTo;

	private Color healthBarFrom;

	private float healthBarT;

	private List<TriggerAtHealth> triggerAtHealth;

	private TankProgression.Stats _stats;

	public int hitMask;

	public int alliedLayerMask;

	public int enemyLayerMask;

	private SpriteRenderer sr;

	private Image image;

	private float cannonRoutineTime;

	public float ReloadTime
	{
		get;
		set;
	}

	public float ShootTimeTotal
	{
		get;
		set;
	}

	public float Speed
	{
		get;
		set;
	}

	public float Movement
	{
		get;
		set;
	}

	public float CurrentHealth
	{
		get;
		set;
	}

	public float CurrentHealthPercentage => CurrentHealth / _stats.health;

	public TankProgression.Stats Stats
	{
		get
		{
			return _stats;
		}
		set
		{
			_stats = value;
			CurrentHealth = _stats.health;
		}
	}

	public BulletDefinition BulletDef
	{
		get;
		set;
	}

	public bool BlinkHealth
	{
		get;
		set;
	}

	public bool HealthColors
	{
		get;
		private set;
	}

	public Vehicle LastDamageDealer
	{
		get;
		set;
	}

	public Enemy Enemy
	{
		get;
		set;
	}

	public int BulletTypeIndex
	{
		get;
		set;
	}

	public int BulletLayer
	{
		get;
		set;
	}

	public bool MuzzlesOn
	{
		get;
		set;
	}

	public bool WantingToShoot
	{
		get;
		set;
	}

	public float BarrelRateScale
	{
		get;
		set;
	}

	public Rigidbody2D hat
	{
		get;
		private set;
	}

	public Transform ragdollHat
	{
		get;
		private set;
	}

	public Booster Booster
	{
		get;
		set;
	}

	public ForceField ActiveForceField
	{
		get;
		set;
	}

	public Coroutine FlameRoutine
	{
		get;
		set;
	}

	public Coroutine BulletRoutine
	{
		get;
		set;
	}

	public Coroutine LightningRoutine
	{
		get;
		set;
	}

	public Coroutine MuzzleRoutine
	{
		get;
		set;
	}

	public Coroutine CannonRoutine
	{
		get;
		set;
	}

	public Coroutine BarrelRoutine
	{
		get;
		set;
	}

	public Coroutine LogicRoutine
	{
		get;
		set;
	}

	public AudioSource ShootSound
	{
		get;
		set;
	}

	public void RegisterController(BaseController controller)
	{
		controller.OnMove = (BaseController.Move)Delegate.Combine(controller.OnMove, new BaseController.Move(SetMovement));
		controller.OnShoot = (BaseController.Shoot)Delegate.Combine(controller.OnShoot, new BaseController.Shoot(Shoot));
		controller.OnUseBooster = (BaseController.UseBooster)Delegate.Combine(controller.OnUseBooster, new BaseController.UseBooster(UseBooster));
		controller.SetBooster(PlayerDataManager.GetSelectedBooster(Variables.instance.GetTank(PlayerDataManager.GetSelectedTank())));
	}

	public IEnumerator UseBooster()
	{
		if (this == TankGame.instance.playerTankContainer)
		{
			TankAnalytics.BoosterUsed(Booster.id);
		}
		switch (Booster.type)
		{
		case BoosterGameplayType.Airstrike:
		{
			int dir = (base.gameObject.layer == LayerMask.NameToLayer("PlayerTank")) ? 1 : (-1);
			Vector3 pos = base.transform.position + base.transform.right * 2f * dir;
			for (int i = 0; i < 10; i++)
			{
				GameObject gameObject3 = UnityEngine.Object.Instantiate(TankGame.instance.airstrikeMissilePrefab, pos + Vector3.right * i * 2f * dir + Vector3.up * 7.5f, Quaternion.Euler(0f, 0f, 90f), TankGame.instance.transform);
				Bullet bullet = new Bullet
				{
					transform = gameObject3.transform,
					definition = new BulletDefinition(),
					owner = base.transform,
					damage = Booster.GetValue(),
					startX = gameObject3.transform.position.x,
					bulletObject = null,
					cacheIndex = -1,
					hitMask = hitMask,
					explodeDelayed = false
				};
				bullet.definition.damageRange = 10f;
				bullet.definition.hitForce = 100f;
				bullet.definition.bulletGravityScale = 1f;
				TankGame.instance.bullets.Add(bullet);
				Rigidbody2D component = gameObject3.GetComponent<Rigidbody2D>();
				component.gravityScale = bullet.definition.bulletGravityScale;
				component.AddForce(base.transform.right, ForceMode2D.Impulse);
				gameObject3.GetComponent<BulletContainer>().Bullet = bullet;
				yield return new WaitForSeconds(0.25f);
			}
			break;
		}
		case BoosterGameplayType.RepairKit:
			UnityEngine.Object.Instantiate(TankGame.instance.bossHealthPack, base.transform.position + Vector3.up * 10f, Quaternion.identity, TankGame.instance.transform).GetComponent<HealthPack>().percentage = Booster.GetValue();
			break;
		case BoosterGameplayType.Shockwave:
		{
			GameObject gameObject4 = UnityEngine.Object.Instantiate(TankGame.instance.shockWavePrefab, base.transform);
			if (this != TankGame.instance.playerTankContainer)
			{
				Vector3 position2 = gameObject4.transform.position;
				gameObject4.transform.position = new Vector3(position2.x, position2.y, -0.5f);
				gameObject4.layer = LayerMask.NameToLayer("ShockwaveRight");
			}
			gameObject4.GetComponent<Shockwave>().progress = Booster.GetValue();
			gameObject4.GetComponent<Shockwave>().Owner = this;
			break;
		}
		case BoosterGameplayType.Mine:
		{
			Vector3 position = base.transform.position;
			position.y = TankGame.instance.GetGroundHeight(position.x);
			Vector2 from = new Vector2(position.x - 1f, TankGame.instance.GetGroundHeight(position.x - 1f));
			Vector2 to = new Vector2(position.x + 1f, TankGame.instance.GetGroundHeight(position.x + 1f));
			float z = Vector2.Angle(from, to);
			GameObject gameObject2 = UnityEngine.Object.Instantiate(TankGame.instance.minePrefab, position, Quaternion.Euler(0f, 0f, z), TankGame.instance.transform);
			Mine mine = gameObject2.GetComponentInChildren<Mine>();
			bool ally = this == TankGame.instance.playerTankContainer;
			if (PlayerDataManager.SelectedGameMode == GameMode.Arena2v2)
			{
				foreach (TankContainer arenaAlly in TankGame.instance.arenaAllies)
				{
					if (arenaAlly == this)
					{
						ally = true;
						break;
					}
				}
			}
			MenuController.Delay(2f, delegate
			{
				if (mine != null)
				{
					mine.mainSprite.enabled = (ally && !mine.Detonated);
				}
			});
			mine.IgnoreLayer(base.gameObject);
			mine.Booster = Booster;
			break;
		}
		case BoosterGameplayType.ForceField:
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(TankGame.instance.forceFieldPrefab, base.transform, worldPositionStays: true);
			ActiveForceField = gameObject.GetComponent<ForceField>();
			ActiveForceField.attachedVehicle = this;
			ActiveForceField.Booster = Booster;
			break;
		}
		}
	}

	public void SetHat(HatType type)
	{
		for (int i = 0; i < hats.Length; i++)
		{
			hats[i].gameObject.SetActive(type == (HatType)i);
			if (type == (HatType)i)
			{
				hat = hats[i].GetComponent<Rigidbody2D>();
			}
		}
		for (int j = 0; j < ragdollHats.Length; j++)
		{
			ragdollHats[j].gameObject.SetActive(type == (HatType)j);
			if (type == (HatType)j)
			{
				ragdollHat = ragdollHats[j];
			}
		}
	}

	protected void Awake()
	{
		triggerAtHealth = new List<TriggerAtHealth>();
		wheelColliders = new CircleCollider2D[wheels.Length];
		if (wheels != null)
		{
			for (int i = 0; i != wheels.Length; i++)
			{
				wheelColliders[i] = wheels[i].GetComponent<CircleCollider2D>();
			}
		}
		if (healthBar != null)
		{
			sr = healthBar.GetComponent<SpriteRenderer>();
			image = healthBar.GetComponent<Image>();
		}
	}

	private void OnEnable()
	{
		cannonPositions = new Vector3[cannons.Length];
		for (int i = 0; i < cannonPositions.Length; i++)
		{
			cannonPositions[i] = cannons[i].transform.localPosition;
		}
		fumeStates = new FumeState[fumes.Length];
		for (int j = 0; j < fumeStates.Length; j++)
		{
			fumeStates[j] = new FumeState();
			fumeStates[j].gradient = fumes[j].colorOverLifetime.color.gradient;
			fumeStates[j].colorKeys = new GradientColorKey[fumes[j].colorOverLifetime.color.gradient.colorKeys.Length];
			fumeStates[j].alphaKeys = new GradientAlphaKey[fumes[j].colorOverLifetime.color.gradient.alphaKeys.Length];
			for (int k = 0; k < fumeStates[j].alphaKeys.Length; k++)
			{
				fumeStates[j].alphaKeys[k] = fumes[j].colorOverLifetime.color.gradient.alphaKeys[k];
			}
		}
		for (int l = 0; l < bulletLines.Length; l++)
		{
			bulletLines[l].enabled = false;
		}
		HealthColors = true;
		StartCoroutine(HealthBarRoutine());
		StartCoroutine(ActiveWhileMovingRoutine());
		SetMovement(0f);
		BulletLayer = LayerMask.NameToLayer("Bullet");
		UpdateHitMask();
	}

	public void UpdateHitMask()
	{
		hitMask = 0;
		alliedLayerMask = 0;
		enemyLayerMask = 0;
		if (base.gameObject.layer == LayerMask.NameToLayer("PlayerTank"))
		{
			alliedLayerMask = LayerMask.GetMask("PlayerTank", "ForceField");
			enemyLayerMask = LayerMask.GetMask("PlayerTankRight", "ForceFieldRight", "EnemyTankPassable");
		}
		else
		{
			alliedLayerMask = LayerMask.GetMask("PlayerTankRight", "ForceFieldRight", "EnemyTankPassable");
			enemyLayerMask = LayerMask.GetMask("PlayerTank", "ForceField");
		}
		hitMask = enemyLayerMask;
	}

	protected void Start()
	{
		for (int i = 0; i < flamerParticles.Length; i++)
		{
			ParticleSystem.CollisionModule collision = flamerParticles[i].collision;
			collision.collidesWith = ((int)collision.collidesWith | (hitMask & ~alliedLayerMask));
		}
	}

	private IEnumerator ActiveWhileMovingRoutine()
	{
		float vel = 0f;
		float current = 0f;
		while (true)
		{
			float target = (!(Mathf.Abs(Movement) > 0f)) ? activeWhileMovingLifetimeMin : activeWhileMovingLifetimeMax;
			current = Mathf.SmoothDamp(current, target, ref vel, activeWhileMovingSmoothTime);
			if (setActiveWhileMoving.Length == 0)
			{
				break;
			}
			if (setActiveWhileMoving.Length != 0)
			{
				for (int i = 0; i < setActiveWhileMoving.Length; i++)
				{
					if (!(setActiveWhileMoving[i] == null))
					{
						ParticleSystem.MainModule mainModule = setActiveWhileMoving[i].main;
						ParticleSystem.MinMaxCurve minMaxCurve2 = mainModule.startLifetime = new ParticleSystem.MinMaxCurve(current);
					}
				}
			}
			yield return null;
		}
	}

	public void SetTankBooster(Booster b)
	{
		if (b.type == BoosterGameplayType.CircleSaw)
		{
			CircleSaw circleSaw = GetComponentInChildren<CircleSaw>(includeInactive: true);
			if (circleSaw == null)
			{
				circleSaw = UnityEngine.Object.Instantiate(Variables.instance.circleSawPrefab, circleSawContainer, worldPositionStays: false).GetComponent<CircleSaw>();
			}
			circleSaw.Booster = b;
			circleSaw.gameObject.SetActive(value: true);
		}
		else
		{
			CircleSaw componentInChildren = GetComponentInChildren<CircleSaw>(includeInactive: true);
			if (componentInChildren != null)
			{
				componentInChildren.gameObject.SetActive(value: false);
			}
		}
	}

	public void SetHealthBar(Transform healthBar, Transform healthBarBase, bool coloredHealth = true)
	{
		this.healthBar = healthBar;
		this.healthBarBase = healthBarBase;
		if (this.healthBar != null)
		{
			this.healthBar.gameObject.SetActive(value: true);
		}
		if (this.healthBarBase != null)
		{
			this.healthBarBase.gameObject.SetActive(value: true);
		}
		HealthColors = coloredHealth;
		if (healthBar != null)
		{
			sr = this.healthBar.GetComponent<SpriteRenderer>();
			image = this.healthBar.GetComponent<Image>();
		}
	}

	public void SetMovement(float movement)
	{
		Movement = movement;
		float num = Mathf.Sign(Speed);
		if (movement != 0f)
		{
			float acceleration = Stats.acceleration;
			Speed = Mathf.Clamp(Speed + movement * acceleration * Time.deltaTime, 0f - Stats.maxSpeed, Stats.maxSpeed);
		}
		else if (Speed != 0f)
		{
			if (Speed > 0f)
			{
				Speed = Mathf.Max(Speed - Stats.brake * Time.deltaTime, 0f);
			}
			else if (Speed < 0f)
			{
				Speed = Mathf.Min(Speed + Stats.brake * Time.deltaTime, 0f);
			}
			if (num != Mathf.Sign(Speed))
			{
				Speed = 0f;
			}
		}
		SetSpeed(Speed, movement);
	}

	public virtual void SetSpeed(float speed, float movement)
	{
	}

	public virtual void SetDustColors(Color[] colors)
	{
	}

	public virtual void DisableAliveStuff()
	{
		for (int i = 0; i < setActiveAtHealth.Length; i++)
		{
			if (0f <= setActiveAtHealth[i].health)
			{
				setActiveAtHealth[i].gameObject.SetActive(setActiveAtHealth[i].active);
			}
		}
		for (int j = 0; j < bulletMuzzles.Length; j++)
		{
			bulletMuzzles[j].enabled = false;
		}
		for (int k = 0; k < flamerParticles.Length; k++)
		{
			flamerParticles[k].Stop();
		}
		StopShooting();
		for (int l = 0; l < setActiveWhileMoving.Length; l++)
		{
			setActiveWhileMoving[l].gameObject.SetActive(value: false);
		}
	}

	public virtual void AddRecoil(Vector2 recoil)
	{
	}

	public virtual void DoCannonMove(float time)
	{
	}

	private void SetFumeColor(Color from, Color to, float t)
	{
		for (int i = 0; i < fumes.Length; i++)
		{
			if (fumes[i] != null && fumeStates[i].gradient != null)
			{
				for (int j = 0; j < fumeStates[i].colorKeys.Length; j++)
				{
					fumeStates[i].colorKeys[j].color = Color.Lerp(from, to, t);
				}
				fumeStates[i].gradient.SetKeys(fumeStates[i].colorKeys, fumeStates[i].alphaKeys);
				ParticleSystem.MinMaxGradient color = new ParticleSystem.MinMaxGradient(fumeStates[i].gradient);
				if (color.gradientMax != null || color.gradientMin != null)
				{
					ParticleSystem.ColorOverLifetimeModule colorOverLifetimeModule = fumes[i].colorOverLifetime;
					colorOverLifetimeModule.color = color;
				}
			}
		}
	}

	public Transform GetCurrentShootTransform()
	{
		if (BulletDef.type == BulletType.Small)
		{
			Transform result = bulletShootTransforms[currentShootTransform];
			currentShootTransform = (currentShootTransform + 1) % bulletShootTransforms.Length;
			return result;
		}
		return turretMarker;
	}

	public void TurnShootParticlesOn()
	{
		ParticleSystem[] array = shootParticles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Play();
		}
	}

	public void TurnShootParticlesOff()
	{
		ParticleSystem[] array = shootParticles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Stop();
		}
	}

	public virtual void StartShooting()
	{
		BarrelRoutine = StartCoroutine(Barrel());
		MuzzleRoutine = StartCoroutine(Muzzles());
		CannonRoutine = StartCoroutine(Cannons());
	}

	private IEnumerator Barrel()
	{
		if (rotateBarrel != null)
		{
			while (true)
			{
				rotateBarrel.transform.localRotation *= Quaternion.Euler(barrelRotateRate * Time.deltaTime * BarrelRateScale, 0f, 0f);
				yield return null;
			}
		}
		BarrelRoutine = null;
	}

	public virtual void StopShooting()
	{
		if (BarrelRoutine != null)
		{
			StopCoroutine(BarrelRoutine);
			BarrelRoutine = null;
		}
		if (MuzzleRoutine != null)
		{
			StopCoroutine(MuzzleRoutine);
			MuzzleRoutine = null;
		}
		if (CannonRoutine != null)
		{
			StopCoroutine(CannonRoutine);
			CannonRoutine = null;
		}
		if (BulletRoutine != null)
		{
			StopCoroutine(BulletRoutine);
			BulletRoutine = null;
		}
		if (LightningRoutine != null)
		{
			StopLightning();
			LightningRoutine = null;
		}
		SpriteRenderer[] array = bulletMuzzles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = false;
		}
		for (int j = 0; j < cannons.Length; j++)
		{
			cannons[j].transform.localPosition = cannonPositions[j];
		}
		StopShootSound();
		TurnShootParticlesOff();
		BarrelRateScale = 0f;
		if (electricity != null)
		{
			electricity.ClearTargets();
		}
	}

	public void StopShootSound()
	{
		if (ShootSound != null)
		{
			ShootSound.Stop();
			if (ShootSound == audioSourceShoot)
			{
				ShootSound = null;
			}
			else
			{
				UnityEngine.Object.Destroy(ShootSound.gameObject);
			}
		}
	}

	public IEnumerator Muzzles()
	{
		if (bulletMuzzles != null && bulletMuzzles.Length != 0)
		{
			int currentMuzzle = 0;
			while (true)
			{
				if (MuzzlesOn)
				{
					int i = 0;
					while (i < 2)
					{
						bulletMuzzles[currentMuzzle].enabled = !bulletMuzzles[currentMuzzle].enabled;
						if (MuzzlesOn)
						{
							yield return new WaitForSeconds(muzzleTime);
							int num = i + 1;
							i = num;
							continue;
						}
						bulletMuzzles[currentMuzzle].enabled = false;
						break;
					}
					currentMuzzle = ((!randomizeMuzzles) ? ((currentMuzzle + 1) % bulletMuzzles.Length) : UnityEngine.Random.Range(0, bulletMuzzles.Length));
				}
				yield return null;
			}
		}
		MuzzleRoutine = null;
	}

	public void TurnMuzzlesOff()
	{
		MuzzlesOn = false;
		SpriteRenderer[] array = bulletMuzzles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = false;
		}
	}

	public IEnumerator Cannons()
	{
		if (cannons != null && cannons.Length != 0)
		{
			int currentCannon = 0;
			while (true)
			{
				SpriteRenderer cannon = cannons[currentCannon];
				float totalTime = muzzleTime * 2f;
				Vector3 cannonPosition = cannonPositions[currentCannon];
				for (float time2 = 0f; time2 < totalTime * 0.1f; time2 += Time.deltaTime)
				{
					cannon.transform.localPosition = Vector3.Lerp(cannonPosition, cannonPosition + new Vector3(cannonOffset, 0f, 0f), time2 / (totalTime * 0.1f));
					yield return null;
				}
				for (float time2 = 0f; time2 < totalTime * 0.9f; time2 += Time.deltaTime)
				{
					cannon.transform.localPosition = Vector3.Lerp(cannonPosition + new Vector3(cannonOffset, 0f, 0f), cannonPosition, time2 / (totalTime * 0.9f));
					yield return null;
				}
				cannon.transform.localPosition = cannonPositions[currentCannon];
				currentCannon = (currentCannon + 1) % cannons.Length;
			}
		}
		CannonRoutine = null;
	}

	public void SpawnBullet(Vector3 from, Vector3 to, float time, bool hitGround = false)
	{
		if (hitGround)
		{
			for (int i = 0; i < bulletGroundSparks.Length; i++)
			{
				bulletGroundSparks[i].transform.position = to;
				bulletGroundSparks[i].Emit(10);
			}
		}
		else
		{
			for (int j = 0; j < bulletSparks.Length; j++)
			{
				bulletSparks[j].transform.position = to;
				bulletSparks[j].Emit(10);
			}
		}
		if (bulletLines == null || bulletLines.Length == 0)
		{
			return;
		}
		LineRenderer lineRenderer = null;
		int num = UnityEngine.Random.Range(0, bulletLines.Length);
		for (int k = 0; k < bulletLines.Length; k++)
		{
			int num2 = (num + k) % bulletLines.Length;
			if (!bulletLines[num2].enabled)
			{
				lineRenderer = bulletLines[num2];
				break;
			}
		}
		if (lineRenderer != null)
		{
			StartCoroutine(BulletLine(lineRenderer, from, to));
		}
	}

	private IEnumerator BulletLine(LineRenderer lr, Vector3 from, Vector3 to)
	{
		lr.enabled = true;
		lr.SetPosition(0, from);
		lr.SetPosition(1, to);
		lr.startColor = Color.white;
		lr.endColor = Color.white;
		for (float t = 0f; t < muzzleTime; t += Time.deltaTime)
		{
			Color color3 = lr.endColor = (lr.startColor = Color.Lerp(Color.white, new Color(1f, 1f, 1f, 0f), t / muzzleTime));
			yield return null;
		}
		lr.enabled = false;
	}

	public void SetFlamerActive(bool val)
	{
		for (int i = 0; i < flamerParticles.Length; i++)
		{
			ParticleSystem.EmissionModule emissionModule = flamerParticles[i].emission;
			ParticleSystem.MinMaxCurve minMaxCurve2 = emissionModule.rateOverTime = (val ? new ParticleSystem.MinMaxCurve(flamerEmissionRates[i]) : new ParticleSystem.MinMaxCurve(0f));
		}
	}

	public void SetHealth()
	{
		healthBarFrom = Color.white;
		healthBarTo = Color.white;
		if (HealthColors)
		{
			healthBarFrom = Color.yellow;
			healthBarTo = Color.red;
		}
		healthBarT = CurrentHealth / (0.5f * Stats.health);
		if ((double)CurrentHealth > (double)Stats.health * 0.5)
		{
			healthBarT = (CurrentHealth - 0.5f * Stats.health) / (Stats.health * 0.5f);
			if (HealthColors)
			{
				healthBarFrom = Color.green;
				healthBarTo = Color.yellow;
			}
		}
		float currentHealthPercentage = CurrentHealthPercentage;
		for (int i = 0; i < setActiveAtHealth.Length; i++)
		{
			if (setActiveAtHealth[i] != null && currentHealthPercentage <= setActiveAtHealth[i].health)
			{
				setActiveAtHealth[i].gameObject.SetActive(setActiveAtHealth[i].active);
			}
		}
		foreach (TriggerAtHealth item in triggerAtHealth)
		{
			if (item != null && !item.triggered && !(currentHealthPercentage > item.health))
			{
				item.Trigger();
			}
		}
		SetFumeColor(TankGame.instance.variables.nearDeathFumesColor, Color.white, Mathf.Clamp01(currentHealthPercentage));
	}

	protected IEnumerator HealthBarRoutine()
	{
		float blinkAmount = 0f;
		float vel = 0f;
		while (true)
		{
			if (healthBar != null && healthBarBase != null)
			{
				float t = 0f;
				if (BlinkHealth && (double)CurrentHealthPercentage < 0.33)
				{
					blinkAmount += Time.deltaTime * 3f;
					t = Mathf.PingPong(blinkAmount, 1f);
				}
				Color a = Color.Lerp(healthBarTo, healthBarFrom, healthBarT);
				a = Color.Lerp(a, Color.white, t);
				if ((bool)sr)
				{
					sr.color = a;
				}
				if ((bool)image)
				{
					image.color = a;
				}
				float target = Mathf.Clamp01(CurrentHealth / Mathf.Max(1f, Stats.health));
				if ((bool)sr)
				{
					float x = Mathf.SmoothDamp(healthBar.localScale.x, target, ref vel, 0.15f);
					healthBar.localScale = new Vector3(x, 1f, 1f);
				}
				else
				{
					float fillAmount = Mathf.SmoothDamp(image.fillAmount, target, ref vel, 0.15f);
					image.fillAmount = fillAmount;
				}
			}
			yield return null;
		}
	}

	public void DetachScraps(Transform newParent, float forceFactor, ScrapMode scrapMode = ScrapMode.Normal)
	{
		StartCoroutine(DetachScrapsRoutine(newParent, forceFactor, scrapMode));
	}

	private IEnumerator DetachScrapsRoutine(Transform newParent, float forceFactor, ScrapMode scrapMode = ScrapMode.Normal)
	{
		float num = 0f;
		bool flag = GetComponent<Helicopter>() != null;
		if (Enemy != null)
		{
			num = Enemy.settings.speedWhileAttacking * 0.75f;
		}
		for (int i = 0; i < scraps.Length; i++)
		{
			if ((bool)scraps[i])
			{
				scraps[i].transform.SetParent(newParent);
				if (scraps[i].sprite != null)
				{
					scraps[i].sprite.enabled = true;
				}
				scraps[i].gameObject.layer = LayerMask.NameToLayer("Scrap");
				scraps[i].body.simulated = true;
				scraps[i].collider2d.enabled = true;
				scraps[i].SetActive(scrapMode);
				TankGame.instance.AddScrap(1);
				scraps[i].body.AddForce(new Vector2((0f - (UnityEngine.Random.value - 0.5f)) * 70f, (UnityEngine.Random.value + 1f) * 150f) * forceFactor);
				scraps[i].body.AddTorque((UnityEngine.Random.value - 0.5f) * 20f * forceFactor);
				if (num != 0f)
				{
					scraps[i].body.AddForce(Vector2.left * num, ForceMode2D.Impulse);
				}
			}
		}
		yield return null;
	}

	public void ClearHealthTriggers()
	{
		triggerAtHealth = new List<TriggerAtHealth>();
	}

	public void AddHealthLossTrigger(TriggerAtHealth t)
	{
		if (!(CurrentHealthPercentage <= t.health))
		{
			triggerAtHealth.Add(t);
		}
	}

	public GameObject Shoot(bool val = true)
	{
		GameObject result = null;
		WantingToShoot = val;
		if (WantingToShoot)
		{
			switch (BulletDef.type)
			{
			case BulletType.Cannon:
			case BulletType.Grenade:
				if (ReloadTime <= 0f)
				{
					ReloadTime = Stats.reloadTime + 0.15f;
					result = ShootCannon();
				}
				WantingToShoot = false;
				break;
			case BulletType.Missile:
			case BulletType.Laser:
				if (BulletRoutine == null)
				{
					BulletRoutine = StartCoroutine(ShootCannonRoutine());
				}
				break;
			case BulletType.Flame:
				if (FlameRoutine == null)
				{
					FlameRoutine = StartCoroutine(FlameLogic());
				}
				break;
			case BulletType.Small:
				if (BulletRoutine == null)
				{
					BulletRoutine = StartCoroutine(SmallBulletLogic());
				}
				break;
			case BulletType.Lightning:
				if (LightningRoutine == null)
				{
					LightningRoutine = StartCoroutine(LightningLogic());
				}
				break;
			}
		}
		return result;
	}

	public GameObject ShootCannon()
	{
		return ShootCannon(Quaternion.identity);
	}

	public GameObject ShootCannon(Quaternion addAngle)
	{
		GameObject gameObject = null;
		Transform transform = turretMarker;
		if (transform == null)
		{
			return null;
		}
		AudioMap.PlayClipAt(BulletDef.shootAudioName, transform.position, AudioMap.instance.effectsMixerGroup);
		BulletObject bulletObject = null;
		if (TankGame.instance != null)
		{
			bulletObject = TankGame.instance.GetBullet(this);
		}
		else
		{
			bulletObject = TankGame.NewBullet(BulletDef, base.transform.parent.gameObject);
			UnityEngine.Object.Destroy(bulletObject.bullet, 1.2f);
			UnityEngine.Object.Destroy(bulletObject.groundHitExplosion, 1.2f);
			UnityEngine.Object.Destroy(bulletObject.shootParticles, 1.2f);
			UnityEngine.Object.Destroy(bulletObject.tankHitExplosion, 1.2f);
		}
		gameObject = bulletObject.bullet;
		gameObject.GetComponentInChildren<SpriteRenderer>(includeInactive: true).gameObject.SetActive(value: true);
		gameObject.transform.position = transform.position + new Vector3(0f, 0f, -0.1f);
		gameObject.transform.rotation = transform.rotation;
		gameObject.layer = BulletLayer;
		TrailRenderer componentInChildren = gameObject.GetComponentInChildren<TrailRenderer>();
		if (componentInChildren != null)
		{
			componentInChildren.sortingLayerName = "Trail";
		}
		Bullet bullet = new Bullet
		{
			transform = gameObject.transform,
			definition = BulletDef,
			owner = base.transform,
			damage = Stats.damage,
			startX = transform.position.x,
			bulletObject = bulletObject,
			cacheIndex = BulletTypeIndex,
			hitMask = hitMask,
			explodeDelayed = (BulletDef.type != BulletType.Cannon)
		};
		if (Enemy != null)
		{
			bullet.bulletObject.bullet.layer = LayerMask.NameToLayer("EnemyBullet");
		}
		if (TankGame.instance != null)
		{
			TankGame.instance.bullets.Add(bullet);
		}
		gameObject.SetActive(value: true);
		Rigidbody2D component = gameObject.GetComponent<Rigidbody2D>();
		float d = (Enemy == null) ? 1 : (-1);
		component.simulated = true;
		component.velocity = transform.right * BulletDef.speed * d;
		component.gravityScale = BulletDef.bulletGravityScale;
		gameObject.GetComponent<BulletContainer>().Bullet = bullet;
		AddRecoil(-transform.right * BulletDef.recoil * d);
		GameObject gameObject2 = bulletObject.shootParticles;
		gameObject2.SetActive(value: true);
		gameObject2.transform.position = transform.transform.position;
		gameObject2.transform.rotation = transform.transform.rotation;
		gameObject2.GetComponent<ParticleSystem>().Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
		gameObject2.GetComponent<ParticleSystem>().Play();
		TurnShootParticlesOn();
		if (TankGame.instance != null)
		{
			TankGame.instance.cameraShake += shootShakeMultiplier * Vector2.one * TankGame.instance.variables.cameraShootShake * 0.5f;
		}
		DoCannonMove(Stats.reloadTime);
		return gameObject;
	}

	private IEnumerator ShootCannonRoutine()
	{
		while (WantingToShoot && ReloadTime < BulletDef.maxTime)
		{
			if (cannonRoutineTime <= 0f)
			{
				Quaternion addAngle = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f - BulletDef.angleRandomization, BulletDef.angleRandomization, UnityEngine.Random.value));
				ShootCannon(addAngle);
				cannonRoutineTime += BulletDef.perMissileTime;
				ReloadTime += BulletDef.perMissileTime;
			}
			cannonRoutineTime -= Time.fixedDeltaTime;
			yield return new WaitForFixedUpdate();
		}
		while (ReloadTime >= BulletDef.maxTime * 0.7f)
		{
			ReloadTime -= Time.fixedDeltaTime;
			yield return new WaitForFixedUpdate();
		}
		BulletRoutine = null;
	}

	private IEnumerator FlameLogic()
	{
		Transform flame = flamethrower;
		float time = 0f;
		if (!(flame != null))
		{
			yield break;
		}
		while (WantingToShoot)
		{
			AudioSource src = AudioMap.PlayClipAt("flamerFiring", Vector3.zero, AudioMap.instance.effectsMixerGroup);
			src.loop = true;
			src.transform.SetParent(flame);
			while (WantingToShoot && ReloadTime < BulletDef.maxTime)
			{
				SetFlamerActive(val: true);
				int mask = LayerMask.GetMask("Ground");
				int num = mask | hitMask | LayerMask.GetMask("EnemyTankMain");
				num &= ~(1 << base.gameObject.layer);
				float d = (Enemy == null) ? 1 : (-1);
				RaycastHit2D[] array = Physics2D.CircleCastAll(flame.position, 1.5f, d * flame.right * base.transform.localScale.x, Mathf.Lerp(1f, 10f, time), num);
				for (int i = 0; i < array.Length; i++)
				{
					RaycastHit2D raycastHit2D = array[i];
					if (raycastHit2D.transform != null)
					{
						if (raycastHit2D.transform.gameObject.layer == mask)
						{
							break;
						}
						BulletContainer componentInParent = raycastHit2D.transform.GetComponentInParent<BulletContainer>();
						if ((bool)componentInParent)
						{
							TankGame.instance.bullets.Remove(componentInParent.Bullet);
							UnityEngine.Object.Destroy(componentInParent.gameObject);
						}
						ForceField component = raycastHit2D.transform.GetComponent<ForceField>();
						if (component != null && component.attachedVehicle != this)
						{
							component.Absorb(raycastHit2D.point, Stats.damage * Time.fixedDeltaTime);
							break;
						}
						Vehicle componentInParent2 = raycastHit2D.transform.GetComponentInParent<Vehicle>();
						if ((bool)componentInParent2)
						{
							componentInParent2.CurrentHealth -= Stats.damage * Time.fixedDeltaTime;
							componentInParent2.LastDamageDealer = this;
							break;
						}
					}
				}
				time += Time.fixedDeltaTime;
				ReloadTime += Time.fixedDeltaTime;
				yield return new WaitForFixedUpdate();
			}
			SetFlamerActive(val: false);
			UnityEngine.Object.Destroy(src.gameObject);
			while ((double)ReloadTime > 0.0 && (!WantingToShoot || !(ReloadTime < BulletDef.maxTime * 0.7f)))
			{
				ReloadTime -= Time.fixedDeltaTime;
				yield return new WaitForFixedUpdate();
			}
		}
		SetFlamerActive(val: false);
		ReloadTime = 0f;
		FlameRoutine = null;
	}

	private IEnumerator SmallBulletLogic()
	{
		Transform t = base.transform;
		SmallBulletState state = SmallBulletState.Starting;
		float warmupTime = 0f;
		float shootTime = 0f;
		float barrelRateScaleVel = 0f;
		AudioMap.AudioClipWithVolume shootClip = AudioMap.instance[BulletDef.shootAudioName];
		AudioMap.AudioClipWithVolume beginClip = AudioMap.instance[BulletDef.shootStartAudioName];
		AudioMap.AudioClipWithVolume endClip = AudioMap.instance[BulletDef.shootEndAudioName];
		StartShooting();
		Action setShooting = delegate
		{
			state = SmallBulletState.Shooting;
			BarrelRateScale = 1f;
			MuzzlesOn = true;
			TurnShootParticlesOn();
			if (shootClip.loopingSound)
			{
				if ((bool)audioSourceShoot)
				{
					AudioMap.PlayClip(shootClip, audioSourceShoot);
					ShootSound = audioSourceShoot;
				}
				else
				{
					ShootSound = AudioMap.PlayClipAt(shootClip, t.position, AudioMap.instance.effectsMixerGroup);
				}
			}
		};
		Action<SmallBulletState> setState = delegate(SmallBulletState s)
		{
			state = s;
			switch (s)
			{
			case SmallBulletState.Starting:
				TurnMuzzlesOff();
				if (BulletDef.warmupTime == 0f)
				{
					setShooting();
				}
				else
				{
					TurnMuzzlesOff();
					if (beginClip != null)
					{
						AudioMap.PlayClipAt(beginClip, t.position, AudioMap.instance.effectsMixerGroup);
					}
				}
				break;
			case SmallBulletState.Shooting:
				setShooting();
				break;
			case SmallBulletState.Cooldown:
				if (endClip != null)
				{
					AudioMap.PlayClipAt(endClip, t.position, AudioMap.instance.effectsMixerGroup);
				}
				TurnShootParticlesOff();
				if (Enemy != null)
				{
					t = null;
				}
				else
				{
					TurnMuzzlesOff();
					shootTime = 0f;
					StopShootSound();
				}
				break;
			}
		};
		while (!(t == null) && (Enemy == null || !(CurrentHealth <= 0f)))
		{
			switch (state)
			{
			case SmallBulletState.Starting:
				BarrelRateScale = Mathf.SmoothDamp(BarrelRateScale, 1f, ref barrelRateScaleVel, BulletDef.warmupTime);
				warmupTime += Time.fixedDeltaTime;
				ShootTimeTotal = Mathf.Max(ShootTimeTotal - Time.fixedDeltaTime, 0f);
				if (warmupTime > BulletDef.warmupTime)
				{
					if (WantingToShoot)
					{
						setState(SmallBulletState.Shooting);
					}
					else
					{
						setState(SmallBulletState.Cooldown);
					}
				}
				break;
			case SmallBulletState.Shooting:
				if (WantingToShoot)
				{
					MuzzlesOn = true;
					shootTime += Time.fixedDeltaTime;
					ShootTimeTotal += Time.fixedDeltaTime;
					if (ShootTimeTotal > BulletDef.maxTime)
					{
						setState(SmallBulletState.Cooldown);
					}
					else
					{
						if (!(shootTime > BulletDef.shootTime))
						{
							break;
						}
						shootTime -= BulletDef.shootTime;
						Transform transform = GetCurrentShootTransform();
						int num = hitMask | LayerMask.GetMask("Ground", "EnemyTankMain");
						num &= ~(1 << base.gameObject.layer);
						Vector3 vector = (Enemy == null) ? t.right : (-t.right);
						vector *= base.transform.localScale.x;
						vector.y += UnityEngine.Random.Range(BulletDef.targetVarianceMin, BulletDef.targetVarianceMax);
						RaycastHit2D[] array = Physics2D.RaycastAll(transform.position, vector, BulletDef.maxDistance, num);
						for (int i = 0; i < array.Length; i++)
						{
							RaycastHit2D raycastHit2D = array[i];
							float damageAdjustedWithPosition = TankGame.instance.variables.GetDamageAdjustedWithPosition(transform.position.x, raycastHit2D.point.x, Stats.damage * BulletDef.shootTime);
							ForceField forceField = (raycastHit2D.transform != null) ? raycastHit2D.transform.GetComponent<ForceField>() : null;
							if (forceField != null)
							{
								if (forceField.attachedVehicle != this)
								{
									AudioMap.PlayClipAt("machineGunMiss", raycastHit2D.point, AudioMap.instance.effectsMixerGroup);
									SpawnBullet(transform.position, raycastHit2D.point, BulletDef.shootTime);
									forceField.Absorb(raycastHit2D.point, damageAdjustedWithPosition);
									break;
								}
								continue;
							}
							bool flag = (bool)raycastHit2D.collider && raycastHit2D.transform.GetComponentInParent<Vehicle>() == null;
							if (flag)
							{
								AudioMap.PlayClipAt("machineGunMiss", raycastHit2D.point, AudioMap.instance.effectsMixerGroup);
							}
							if (!shootClip.loopingSound)
							{
								AudioMap.PlayClipAt(shootClip, t.position, AudioMap.instance.effectsMixerGroup);
							}
							if ((bool)raycastHit2D.collider)
							{
								SpawnBullet(transform.position, raycastHit2D.point, BulletDef.shootTime, flag);
								Vehicle componentInParent = raycastHit2D.transform.GetComponentInParent<Vehicle>();
								if (componentInParent != null)
								{
									componentInParent.LastDamageDealer = this;
									componentInParent.CurrentHealth -= damageAdjustedWithPosition;
								}
							}
							else
							{
								SpawnBullet(transform.position, transform.position + vector * 100f, BulletDef.shootTime, flag);
							}
							break;
						}
						AddRecoil(BulletDef.recoil * -vector);
						if (Enemy == null && TankGame.instance != null)
						{
							TankGame.instance.cameraShake += Vector2.one * TankGame.instance.variables.cameraShakeExplosion * 0.1f;
						}
					}
				}
				else
				{
					setState(SmallBulletState.Starting);
				}
				break;
			case SmallBulletState.Cooldown:
				ShootTimeTotal -= Time.fixedDeltaTime;
				warmupTime = Mathf.Max(warmupTime - Time.fixedDeltaTime, 0f);
				BarrelRateScale = Mathf.SmoothDamp(BarrelRateScale, 0f, ref barrelRateScaleVel, BulletDef.warmupTime);
				if (ShootTimeTotal <= 0f)
				{
					t = null;
				}
				else if (WantingToShoot && warmupTime <= 0f)
				{
					setState(SmallBulletState.Starting);
				}
				break;
			}
			yield return new WaitForFixedUpdate();
		}
		ShootTimeTotal = 0f;
		StopShooting();
	}

	private IEnumerator LightningLogic()
	{
		ShootSound = AudioMap.PlayClipAt("lightningFiring", Vector3.zero, AudioMap.instance.effectsMixerGroup);
		ShootSound.loop = true;
		TurnShootParticlesOn();
		float recoilTime = 0f;
		while (WantingToShoot && ReloadTime < BulletDef.maxTime)
		{
			electricity.AddTarget(electricity.transform);
			if (recoilTime > 0.2f)
			{
				recoilTime = 0f;
				recoilRigidbody.AddForce(Vector2.left * 1f, ForceMode2D.Impulse);
			}
			recoilTime += Time.fixedDeltaTime;
			int layerMask = hitMask & LayerMask.GetMask("ForceField", "ForceFieldRight");
			int num = (hitMask & LayerMask.GetMask("PlayerTank", "PlayerTankRight")) | LayerMask.GetMask("EnemyTankMain");
			num &= ~(1 << base.gameObject.layer);
			RaycastHit2D raycastHit2D = Physics2D.CircleCast(electricity.transform.position, 3f, electricity.transform.right, 15f, layerMask);
			RaycastHit2D raycastHit2D2 = Physics2D.CircleCast(electricity.transform.position, 3f, electricity.transform.right, 15f, num);
			ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
			emitParams.applyShapeToPosition = true;
			if (raycastHit2D2.transform != null)
			{
				ForceField forceField = (raycastHit2D.transform != null) ? raycastHit2D.transform.GetComponent<ForceField>() : null;
				if (forceField != null && forceField.attachedVehicle != this)
				{
					electricity.SetLightningEnd(raycastHit2D.point);
					electricity.TargetEnd();
					forceField.Absorb(raycastHit2D.point, Stats.damage * Time.fixedDeltaTime);
				}
				else
				{
					Vehicle componentInParent = raycastHit2D2.transform.GetComponentInParent<Vehicle>();
					if (componentInParent != null)
					{
						electricity.AddTarget(componentInParent.transform);
						RaycastHit2D[] array = Physics2D.CircleCastAll(componentInParent.transform.position, 15f, electricity.transform.right, 0f, num);
						componentInParent.CurrentHealth -= Stats.damage * Time.fixedDeltaTime;
						emitParams.position = componentInParent.transform.position;
						lightningSparkles.Emit(emitParams, 10);
						int num2 = 1;
						for (int i = 0; i < array.Length; i++)
						{
							Vehicle componentInParent2 = array[i].transform.GetComponentInParent<Vehicle>();
							if (componentInParent2 != null && componentInParent2 != componentInParent)
							{
								electricity.AddTarget(componentInParent2.transform);
								componentInParent2.CurrentHealth -= Stats.damage * Time.fixedDeltaTime / (float)(++num2);
								emitParams.position = componentInParent2.transform.position;
								lightningSparkles.Emit(emitParams, 10);
							}
						}
					}
				}
			}
			else
			{
				electricity.SetLightningEnd(electricity.transform.position + electricity.transform.right * 15f);
				electricity.TargetEnd();
			}
			ReloadTime += Time.fixedDeltaTime;
			yield return new WaitForFixedUpdate();
		}
		StopLightning();
		while ((double)ReloadTime > 0.0 && (!WantingToShoot || !(ReloadTime < BulletDef.maxTime * 0.7f)))
		{
			ReloadTime -= Time.fixedDeltaTime;
			yield return new WaitForFixedUpdate();
		}
		LightningRoutine = null;
	}

	private void StopLightning()
	{
		TurnShootParticlesOff();
		if (ShootSound != null)
		{
			ShootSound.Stop();
			UnityEngine.Object.Destroy(ShootSound.gameObject);
		}
		electricity.ClearTargets();
	}

	protected void Update()
	{
		if (BulletDef.type == BulletType.Cannon || BulletDef.type == BulletType.Grenade)
		{
			bool flag = ReloadTime <= 0f;
			ReloadTime -= Time.deltaTime;
			if (ReloadTime <= 0f && !flag)
			{
				AudioMap.PlayClipAt("tankGunReloaded", base.transform.position, AudioMap.instance.effectsMixerGroup);
			}
			int num = 0;
			for (int i = 0; i < lightsPerBatch.Length; i++)
			{
				for (int j = 0; j < lightsPerBatch[i]; j++)
				{
					float num2 = 1f - ReloadTime / Stats.reloadTime;
					float num3 = (float)j / (float)lightsPerBatch[i];
					reloadLights[num++].gameObject.SetActive(num3 <= num2);
				}
			}
		}
		if ((BulletDef.type == BulletType.Lightning || BulletDef.type == BulletType.Missile || BulletDef.type == BulletType.Laser) && !WantingToShoot)
		{
			ReloadTime = Mathf.Max(0f, ReloadTime - Time.deltaTime);
		}
		if (BulletDef.type == BulletType.Lightning && lightningLiquidTank != null)
		{
			Vector3 localPosition = lightningLiquidTank.transform.localPosition;
			localPosition.y = Mathf.Lerp(lightningLiquidTankMoveFrom, lightningLiquidTankMoveTo, ReloadTime / BulletDef.maxTime);
			lightningLiquidTank.transform.localPosition = localPosition;
		}
	}

	public virtual void PreDestroy()
	{
	}
}
