using AI;
using System;
using UnityEngine;

[Serializable]
public class AIBehaviour
{
	[Flags]
	public enum BoosterReadyFlag
	{
		None = 0x0,
		Time = 0x1,
		Health = 0x2,
		Location = 0x4,
		Movement = 0x8,
		All = 0xF
	}

	public enum Behaviour
	{
		Aggressive,
		Defensive,
		Casual,
		Leader,
		Follower
	}

	public enum State
	{
		RequestingNewWaypoint,
		MovingToWaypoint,
		NeedingResolve,
		Resolving,
		Camping
	}

	private const int MaxRating = 999;

	private const int MaxUpgradeLevel = 14;

	private bool boosterUsed;

	private float totalTimeAwake;

	private float behaviourChangeTime;

	private float behaviourTimer;

	private Vector2 useBoosterLocation;

	private State state;

	private BoosterReadyFlag boosterReadyFlags;

	private BoosterGameplayType boosterType;

	private AIProfileSettings profile;

	private AIController.TankData self;

	public float UseBoosterTime
	{
		get;
		private set;
	}

	public float UseBoosterRadius
	{
		get;
		private set;
	}

	public float DistanceToEnemy
	{
		get;
		private set;
	}

	public float DistanceToTarget
	{
		get;
		private set;
	}

	public float DistanceToAlly
	{
		get;
		private set;
	}

	public float CurrentHealth => self.vehicle.CurrentHealthPercentage;

	public float DifficultyFactor
	{
		get;
		private set;
	}

	public float TargetRange
	{
		get;
		private set;
	}

	public float Speed => self.vehicle.Speed;

	public float MaxSpeed => self.vehicle.Stats.maxSpeed;

	public float MeleeContactDistance
	{
		get;
		private set;
	}

	public bool ShootLock
	{
		get;
		set;
	}

	public bool CanHitEnemy
	{
		get;
		set;
	}

	public bool CollidesWithPlayer
	{
		get;
		set;
	}

	public bool BehaviourLock
	{
		get;
		private set;
	}

	public bool IsStuck
	{
		get;
		private set;
	}

	public bool BehindEnemyLines
	{
		get;
		private set;
	}

	public bool IsLastManStanding
	{
		get;
		private set;
	}

	public bool TimeToUseBooster
	{
		get
		{
			if (!boosterUsed)
			{
				return boosterReadyFlags == BoosterReadyFlag.All;
			}
			return false;
		}
	}

	public bool WaypointSet => !Waypoint.IsNegativeInfinity();

	public bool InTargetRange => DistanceToTarget <= TargetRange;

	public bool CloseToTarget => DistanceToTarget <= profile.targetReachedRadius;

	public Vector2 Position => self.Position;

	public Vector2 Waypoint
	{
		get;
		private set;
	}

	public Vector2 ForwardVec
	{
		get;
		private set;
	}

	public Vector2 BackwardVec
	{
		get;
		private set;
	}

	public Vector2 MeleeContactPoint
	{
		get;
		private set;
	}

	public Behaviour CurrentBehaviour
	{
		get;
		private set;
	}

	public Direction Forward
	{
		get;
		private set;
	}

	public Direction Backward
	{
		get;
		private set;
	}

	public Direction MoveDirection
	{
		get;
		private set;
	}

	public Vehicle Vehicle => self.vehicle;

	public BulletDefinition BulletDef => Vehicle.BulletDef;

	public AIController.TankData PrimaryEnemy
	{
		get;
		private set;
	}

	public AIController.TankData PrimaryAlly
	{
		get;
		private set;
	}

	public AIBehaviour(AIController.TankData tankData, Behaviour startBehaviour)
	{
		self = tankData;
		Forward = ((self.team == Team.Left) ? Direction.Right : Direction.Left);
		Backward = (Direction)(0 - Forward);
		ForwardVec = new Vector2((float)Forward, 0f);
		BackwardVec = -ForwardVec;
		CanHitEnemy = false;
		Waypoint = Vector2.negativeInfinity;
		CollidesWithPlayer = true;
		DifficultyFactor = Mathf.Clamp01(self.rating / 999);
		boosterType = self.vehicle.Booster.type;
		useBoosterLocation = Vector2.negativeInfinity;
		if (AIController.instance.settings.startAsRandomProfile)
		{
			SetRandomBehaviour();
		}
		else
		{
			SetBehaviour(startBehaviour);
		}
		SetState(State.RequestingNewWaypoint);
		UseBoosterRadius = GetBoosterLocationPrecision();
		if (self.team == Team.Right)
		{
			TankGame.instance.SetLayerRecursive(Vehicle.transform, LayerMask.NameToLayer("PlayerTankRight"));
			Vehicle.BulletLayer = LayerMask.NameToLayer("EnemyBullet");
		}
		else
		{
			Vehicle.BulletLayer = LayerMask.NameToLayer("AllyBullet");
		}
		InitTriggers();
		UpdateProperties();
		UpdateBehaviour();
		UpdateBoosterTriggers();
	}

	public void Update(float dt)
	{
		if (self.IsActive)
		{
			UpdateShooting();
			if (Time.frameCount % 2 == (int)self.team)
			{
				UpdateProperties();
			}
			if (Time.frameCount % AIController.instance.AiCount == self.id - 1)
			{
				UpdateBehaviour();
				UpdateState();
				UpdateWaypoint();
			}
			UpdateBoosterTriggers();
			if (TimeToUseBooster)
			{
				UseBooster();
				TankGame.DoBooster(Vehicle);
			}
			Move();
			totalTimeAwake += dt;
			behaviourTimer += dt;
		}
	}

	private void UpdateShooting()
	{
		CanHitEnemy = false;
		if (ShootLock || !PrimaryEnemy.vehicle)
		{
			return;
		}
		Transform currentShootTransform = Vehicle.GetCurrentShootTransform();
		if (!currentShootTransform)
		{
			return;
		}
		BulletDefinition bulletDef = Vehicle.BulletDef;
		int mask = (self.team == Team.Right) ? LayerMask.GetMask("PlayerTank") : LayerMask.GetMask("PlayerTankRight");
		if (TankGame.instance.PredictTrajectory(bulletDef, currentShootTransform.position, currentShootTransform.right, mask, PrimaryEnemy.vehicle) == float.MaxValue || !(DistanceToEnemy < 20f))
		{
			return;
		}
		CanHitEnemy = true;
		ShootLock = true;
		float shootReactionDelay = GetShootReactionDelay();
		float afterShootDelay = GetAfterShootDelay();
		if (bulletDef.type == BulletType.Cannon || bulletDef.type == BulletType.Grenade || bulletDef.type == BulletType.Laser || bulletDef.type == BulletType.Missile)
		{
			TankGame.instance.Delay(shootReactionDelay, delegate
			{
				Vehicle.Shoot();
			});
			if (bulletDef.type != BulletType.Missile && bulletDef.type != BulletType.Laser)
			{
				TankGame.instance.Delay(shootReactionDelay + afterShootDelay, delegate
				{
					Vehicle.Shoot(val: false);
				});
			}
			TankGame.instance.Delay(shootReactionDelay + afterShootDelay + bulletDef.maxTime * UnityEngine.Random.value, delegate
			{
				ShootLock = false;
			});
		}
		else
		{
			TankGame.instance.Delay(shootReactionDelay, delegate
			{
				Vehicle.Shoot();
			});
			float t = bulletDef.warmupTime + Mathf.Max(0.5f, UnityEngine.Random.value) * bulletDef.maxTime;
			TankGame.instance.Delay(t, delegate
			{
				Vehicle.Shoot(val: false);
			});
			TankGame.instance.Delay(shootReactionDelay + afterShootDelay * UnityEngine.Random.value, delegate
			{
				ShootLock = false;
			});
		}
	}

	public void UpdateProperties()
	{
		if (!self.IsActive || CurrentHealth <= 0f)
		{
			return;
		}
		IsStuck = (self.IsStationary && state != State.Camping);
		PrimaryEnemy = AIController.instance.GetEnemyLeader(self);
		PrimaryAlly = AIController.instance.GetClosestAlly(self);
		IsLastManStanding = (PrimaryAlly == self || PrimaryAlly.vehicle.CurrentHealth <= 0f);
		DistanceToEnemy = PrimaryEnemy.DistanceTo(self);
		DistanceToTarget = Mathf.Abs(Position.x - Waypoint.x);
		DistanceToAlly = PrimaryAlly.DistanceTo(self);
		if (PrimaryEnemy.vehicle != null && PrimaryEnemy.vehicle.circleSawContainer != null && self.vehicle != null && self.vehicle.circleSawContainer != null)
		{
			MeleeContactDistance = PrimaryEnemy.vehicle.circleSawContainer.localPosition.x + Vehicle.circleSawContainer.localPosition.x;
			MeleeContactPoint = PrimaryEnemy.Position + new Vector2(MeleeContactDistance, 0f) * (float)Backward;
		}
		BehindEnemyLines = ((Forward == Direction.Left) ? (PrimaryEnemy.Position.x > self.Position.x) : (PrimaryEnemy.Position.x < self.Position.x));
		MoveDirection = ((!Mathf.Approximately(Speed, 0f)) ? ((Mathf.Sign(Speed) < 0f) ? Backward : Forward) : Direction.Stationary);
		if (self.team != 0)
		{
			if (BehindEnemyLines && CollidesWithPlayer)
			{
				CollidesWithPlayer = false;
				TankGame.instance.SetArenaPlayerCollision(self.vehicle, CollidesWithPlayer);
			}
			else if (!PrimaryEnemy.IsPointBehind(Position + ForwardVec * MeleeContactDistance) && !CollidesWithPlayer)
			{
				CollidesWithPlayer = true;
				TankGame.instance.SetArenaPlayerCollision(self.vehicle, CollidesWithPlayer);
			}
		}
	}

	private void UpdateWaypoint()
	{
		if (state == State.NeedingResolve)
		{
			if (PrimaryEnemy.NearFrontBlocker)
			{
				SetWaypoint(PrimaryEnemy.Position, resolving: true);
			}
			else if (DistanceToEnemy < MeleeContactDistance * 0.5f && (!PrimaryEnemy.NearBlocker || !self.NearBlocker) && boosterType != BoosterGameplayType.CircleSaw)
			{
				SetWaypoint(GetClosestPOI(Backward), resolving: true);
			}
			else if (BehindEnemyLines && PrimaryEnemy.IsPointBehind(Waypoint) && !self.NearBackBlocker)
			{
				SetWaypoint(MeleeContactPoint, resolving: true);
			}
			else if (AIController.instance.IsPlayerCamping())
			{
				SetWaypoint(PrimaryEnemy.Position + BackwardVec * 2f);
			}
			else if (!BehindEnemyLines && DistanceToEnemy > profile.preferDistance.max && self.IsPointBehind(Waypoint))
			{
				SetWaypoint(GetClosestPOI(Forward), resolving: true);
			}
			else if (!IsLastManStanding && DistanceToAlly > profile.preferDistance.max)
			{
				SetWaypoint(PrimaryAlly.Position, resolving: true);
			}
		}
		if (state != 0)
		{
			return;
		}
		if (CurrentBehaviour == Behaviour.Aggressive || CurrentBehaviour == Behaviour.Casual)
		{
			if (PrimaryEnemy.NearBlocker && self.NearBlocker && BulletDef.type != BulletType.Missile)
			{
				Stop();
			}
			else if (boosterType == BoosterGameplayType.CircleSaw)
			{
				if (DistanceToEnemy > MeleeContactDistance - 1f)
				{
					SetWaypoint(MeleeContactPoint);
				}
				else
				{
					Stop();
				}
			}
			else if (IsCloseRangeWeapon())
			{
				if (DistanceToEnemy < MeleeContactDistance)
				{
					SetWaypoint(MeleeContactPoint + BackwardVec * 2f);
				}
				else if (DistanceToEnemy > BulletDef.damageRange)
				{
					SetWaypoint(MeleeContactPoint + BackwardVec * 2f);
				}
				else if (DistanceToEnemy < BulletDef.damageRange && !CanHitEnemy)
				{
					SetWaypoint(MeleeContactPoint);
				}
				else
				{
					Stop();
				}
			}
			else if (BulletDef.type == BulletType.Missile)
			{
				if (DistanceToEnemy > BulletDef.damageRange * 2f)
				{
					SetWaypoint(GetClosestPOI(Forward, 5f));
				}
				else if (DistanceToEnemy < profile.preferDistance.min)
				{
					SetWaypoint(GetClosestPOI(Backward));
				}
				else
				{
					Stop();
				}
			}
			else if (DistanceToEnemy > profile.preferDistance.max && !self.NearBackBlocker)
			{
				SetWaypoint(GetClosestPOI(Forward, 5f));
			}
			else if (!CanHitEnemy)
			{
				SetWaypoint((DistanceToEnemy < profile.preferDistance.min) ? (Position + BackwardVec * 2f) : (Position + ForwardVec * 2f));
			}
			else
			{
				Stop();
			}
		}
		else if (CurrentBehaviour == Behaviour.Defensive)
		{
			if (self.IsPointBehind(MeleeContactPoint))
			{
				SetWaypoint(MeleeContactPoint + BackwardVec * profile.preferDistance.min);
			}
			else if (DistanceToEnemy < profile.preferDistance.min && !self.NearBackBlocker)
			{
				SetWaypoint(GetClosestPOI(Backward, profile.preferDistance.min));
			}
			else if (DistanceToEnemy < profile.preferDistance.min && PrimaryEnemy.NearFrontBlocker)
			{
				SetWaypoint(GetClosestPOI(Forward));
			}
			else if (DistanceToEnemy > profile.preferDistance.max)
			{
				SetWaypoint(GetClosestPOI(Forward));
			}
			else if (!CanHitEnemy)
			{
				if (DistanceToEnemy < profile.preferDistance.min)
				{
					SetWaypoint(GetClosestPOI(Backward));
				}
				else
				{
					SetWaypoint(GetClosestPOI(Forward));
				}
			}
			else
			{
				Stop();
			}
		}
		else if (CurrentBehaviour == Behaviour.Leader)
		{
			if (self.IsPointBehind(PrimaryAlly.Position) && DistanceToAlly > profile.preferDistance.min)
			{
				if (DistanceToEnemy > profile.preferDistance.max)
				{
					Stop();
				}
				else
				{
					SetWaypoint(GetClosestPOI(Backward));
				}
			}
			else if (PrimaryEnemy.IsPointBehind(Waypoint) || BehindEnemyLines)
			{
				SetWaypoint(PrimaryEnemy.Position + BackwardVec * profile.preferDistance.min);
			}
			else if (DistanceToEnemy < profile.preferDistance.min)
			{
				SetWaypoint(GetClosestPOI(Backward));
			}
			else if (DistanceToEnemy > profile.preferDistance.max)
			{
				SetWaypoint(GetClosestPOI(Backward, 0f - DistanceToEnemy));
			}
			else if (IsCloseRangeWeapon() || boosterType == BoosterGameplayType.CircleSaw)
			{
				if (DistanceToEnemy > BulletDef.damageRange)
				{
					SetWaypoint(MeleeContactPoint + BackwardVec * profile.preferDistance.min);
				}
				else if (DistanceToEnemy < BulletDef.damageRange && !CanHitEnemy)
				{
					SetWaypoint(MeleeContactPoint + BackwardVec * profile.preferDistance.min);
				}
				else
				{
					Stop();
				}
			}
			else if (BulletDef.type == BulletType.Missile)
			{
				if (DistanceToEnemy > BulletDef.damageRange * 2f)
				{
					SetWaypoint(GetClosestPOI(Forward, 5f));
				}
				else
				{
					Stop();
				}
			}
			else if (!CanHitEnemy)
			{
				if (self.IsStationary)
				{
					SetWaypoint(MeleeContactPoint);
				}
				else
				{
					SetWaypoint(GetClosestPOI(Forward));
				}
			}
			else
			{
				Stop();
			}
		}
		else
		{
			if (CurrentBehaviour != Behaviour.Follower)
			{
				return;
			}
			if (!WaypointSet)
			{
				SetWaypoint(PrimaryAlly.Position);
			}
			if (DistanceToAlly > profile.preferDistance.max)
			{
				SetWaypoint(PrimaryAlly.Position);
				return;
			}
			if (!CanHitEnemy)
			{
				SetWaypoint(GetClosestPOI(Forward));
			}
			if (PrimaryEnemy.IsPointBehind(Waypoint) || BehindEnemyLines)
			{
				SetWaypoint(PrimaryAlly.Position + BackwardVec * profile.preferDistance.min);
			}
			if (!WaypointSet)
			{
				SetWaypoint(GetClosestPOI(Forward));
			}
		}
	}

	private void UpdateBoosterTriggers()
	{
		if (boosterUsed)
		{
			return;
		}
		if (!IsBoosterFlagSet(BoosterReadyFlag.Time) && totalTimeAwake >= UseBoosterTime)
		{
			SetBoosterReady(BoosterReadyFlag.Time);
		}
		switch (boosterType)
		{
		case BoosterGameplayType.CircleSaw:
		case BoosterGameplayType.ForceField:
			break;
		case BoosterGameplayType.Airstrike:
			SetBoosterReady(BoosterReadyFlag.Location, DistanceToEnemy <= profile.useAirstrikeDistance.max && DistanceToEnemy >= profile.useAirstrikeDistance.min && !PrimaryEnemy.IsPointBehind(Position));
			SetBoosterReady(BoosterReadyFlag.Movement, MoveDirection != Forward);
			break;
		case BoosterGameplayType.RepairKit:
		{
			bool flag = !IsLastManStanding && PrimaryAlly.vehicle.CurrentHealthPercentage < profile.useRepairKitAtHealth && (!self.IsPointBehind(PrimaryAlly.Position) || DistanceToAlly < 5f);
			SetBoosterReady(BoosterReadyFlag.Movement, (MoveDirection != Backward && DistanceToEnemy > 15f) | flag);
			break;
		}
		case BoosterGameplayType.Shockwave:
			SetBoosterReady(BoosterReadyFlag.Location, DistanceToEnemy < 5f);
			break;
		case BoosterGameplayType.Mine:
			if (IsBoosterFlagSet(BoosterReadyFlag.Time))
			{
				if (CurrentBehaviour == Behaviour.Casual && (useBoosterLocation.IsNegativeInfinity() || (useBoosterLocation - Position).sqrMagnitude > 100f))
				{
					useBoosterLocation = GetClosestPOI(Backward);
				}
				else if (CurrentBehaviour == Behaviour.Aggressive && DistanceToEnemy < 20f && (useBoosterLocation.IsNegativeInfinity() || self.IsPointBehind(useBoosterLocation)))
				{
					useBoosterLocation = GetClosestPOI(Forward);
				}
				else if (CurrentBehaviour == Behaviour.Defensive && DistanceToEnemy < 30f && (useBoosterLocation.IsNegativeInfinity() || !self.IsPointBehind(useBoosterLocation)))
				{
					useBoosterLocation = GetClosestPOI(Backward);
				}
				SetBoosterReady(BoosterReadyFlag.Location, Mathf.Abs(useBoosterLocation.x - Position.x) <= UseBoosterRadius);
			}
			break;
		}
	}

	private void UpdateBehaviour()
	{
		if (!(profile == null))
		{
			if (IsLastManStanding && CurrentBehaviour != 0)
			{
				SetBehaviour(Behaviour.Aggressive);
				SetState(State.RequestingNewWaypoint);
			}
			if (profile.changeBehaviourByTime && behaviourTimer >= behaviourChangeTime)
			{
				behaviourTimer = 0f;
				SetRandomBehaviour();
			}
		}
	}

	private void UpdateState()
	{
		if (state != State.Camping && !WaypointSet)
		{
			SetState(State.RequestingNewWaypoint);
		}
		if (state != State.Resolving && state != State.NeedingResolve)
		{
			if ((DistanceToEnemy < MeleeContactDistance * 0.5f && (!PrimaryEnemy.NearBlocker || !self.NearBlocker) && boosterType != BoosterGameplayType.CircleSaw) || (BehindEnemyLines && !PrimaryEnemy.IsPointBehind(Waypoint) && state != State.Camping) || (PrimaryEnemy.IsPointBehind(Waypoint) && !PrimaryEnemy.NearFrontBlocker) || AIController.instance.IsPlayerCamping() || (!BehindEnemyLines && DistanceToEnemy > profile.preferDistance.max && self.IsPointBehind(Waypoint)))
			{
				SetState(State.NeedingResolve);
				return;
			}
			if (CurrentBehaviour == Behaviour.Follower && DistanceToAlly > profile.preferDistance.max)
			{
				SetState(State.RequestingNewWaypoint);
				return;
			}
		}
		if (state == State.Camping)
		{
			if (self.stationaryTimer >= profile.stationaryCheckTime)
			{
				SetState(State.RequestingNewWaypoint);
			}
		}
		else if (CloseToTarget)
		{
			SetState(State.RequestingNewWaypoint);
		}
	}

	public void InitTriggers()
	{
		Vehicle.AddHealthLossTrigger(new Vehicle.TriggerAtHealth(delegate
		{
			TankGame.instance.ArenaPlayerDied(self);
		}, 0f));
		switch (boosterType)
		{
		case BoosterGameplayType.Airstrike:
			SetBoosterReady(BoosterReadyFlag.Health);
			break;
		case BoosterGameplayType.RepairKit:
			SetBoosterReady(BoosterReadyFlag.Location);
			if (profile != null && CurrentHealth > profile.useRepairKitAtHealth)
			{
				Vehicle.AddHealthLossTrigger(new Vehicle.TriggerAtHealth(delegate
				{
					SetBoosterReady(BoosterReadyFlag.Health);
				}, profile.useRepairKitAtHealth));
			}
			else
			{
				SetBoosterReady(BoosterReadyFlag.Health);
			}
			break;
		case BoosterGameplayType.CircleSaw:
			SetBoosterReady(BoosterReadyFlag.All);
			break;
		case BoosterGameplayType.Shockwave:
			SetBoosterReady(BoosterReadyFlag.Health | BoosterReadyFlag.Movement);
			break;
		case BoosterGameplayType.Mine:
			SetBoosterReady(BoosterReadyFlag.Health | BoosterReadyFlag.Movement);
			break;
		case BoosterGameplayType.ForceField:
			SetBoosterReady(BoosterReadyFlag.Time | BoosterReadyFlag.Location | BoosterReadyFlag.Movement);
			if (profile != null && CurrentHealth > profile.useForceFieldAtHealth)
			{
				Vehicle.AddHealthLossTrigger(new Vehicle.TriggerAtHealth(delegate
				{
					SetBoosterReady(BoosterReadyFlag.Health);
				}, profile.useForceFieldAtHealth));
			}
			else
			{
				SetBoosterReady(BoosterReadyFlag.Health);
			}
			break;
		}
	}

	private void InitProfile()
	{
		if (!(profile == null))
		{
			UseBoosterTime = profile.useBoosterTime.Random();
			behaviourChangeTime = profile.changeBehaviourTime.Random();
			behaviourTimer = 0f;
			self.SetDataFromProfile(profile);
		}
	}

	public void UseBooster()
	{
		boosterUsed = true;
		if (boosterType == BoosterGameplayType.RepairKit)
		{
			SetWaypoint(useBoosterLocation, resolving: true);
			AIController.instance.RepairKitUsed(Position);
		}
	}

	public void SetBoosterReady(BoosterReadyFlag flag, bool value = true)
	{
		if (value)
		{
			SetBoosterFlag(flag);
		}
		else
		{
			UnSetBoosterFlag(flag);
		}
	}

	private void SetBoosterFlag(BoosterReadyFlag flag)
	{
		boosterReadyFlags |= flag;
	}

	private void UnSetBoosterFlag(BoosterReadyFlag flag)
	{
		boosterReadyFlags &= ~flag;
	}

	public bool IsBoosterFlagSet(BoosterReadyFlag flag)
	{
		return (boosterReadyFlags & flag) == flag;
	}

	private void SetState(State newState)
	{
		if (state != newState)
		{
			state = newState;
		}
	}

	private float EvaluateCurve(AnimationCurve curve, MinMaxFloat minMaxContainer)
	{
		float num = UnityEngine.Random.Range(0f - AIController.instance.settings.curveVarianceMax, AIController.instance.settings.curveVarianceMax) * (minMaxContainer.max - minMaxContainer.min);
		float num2 = Mathf.Lerp(minMaxContainer.max, minMaxContainer.min, curve.Evaluate(DifficultyFactor));
		return minMaxContainer.ClampInside(num2 + num);
	}

	public float GetShootReactionDelay()
	{
		return EvaluateCurve(AIController.instance.settings.shootReactionCurve, AIController.instance.settings.shootReactionDelay);
	}

	public float GetAfterShootDelay()
	{
		return EvaluateCurve(AIController.instance.settings.afterShootDelayCurve, AIController.instance.settings.afterShootDelay);
	}

	public float GetBoosterLocationPrecision()
	{
		return EvaluateCurve(AIController.instance.settings.boosterLocationPrecisionCurve, AIController.instance.settings.useBoosterTargetRadius);
	}

	public float GetCloseToTargetPrecision()
	{
		return EvaluateCurve(AIController.instance.settings.closeToTargetPrecisionCurve, new MinMaxFloat(0f, profile.targetReachedRadius));
	}

	public void SetBehaviour(Behaviour input)
	{
		if (!BehaviourLock)
		{
			profile = AIController.instance.GetProfile(input);
			CurrentBehaviour = input;
			InitProfile();
			SetState(State.RequestingNewWaypoint);
		}
	}

	public void SetRandomBehaviour()
	{
		SetBehaviour(Behaviour.Aggressive);
	}

	public void LockBehaviour(Behaviour input, bool overrideCurrent = false)
	{
		if (overrideCurrent)
		{
			BehaviourLock = false;
		}
		SetBehaviour(input);
		BehaviourLock = true;
	}

	public void SetWaypoint(Vector2 location, bool resolving = false)
	{
		Waypoint = location.Clamp(AIController.instance.backBlockerPosition + Vector2.right * profile.preferDistance.min, AIController.instance.frontBlockerPosition + Vector2.left * profile.preferDistance.min, x: true, y: false);
		SetState((!resolving) ? State.MovingToWaypoint : State.Resolving);
	}

	private void Move()
	{
		TargetRange = (CloseToTarget ? 0f : (Mathf.Abs(Speed * 0.004f) + profile.targetReachedRadius));
		float num = 0f;
		if ((TargetRange == 0f && DistanceToTarget <= 0.8f) || !WaypointSet)
		{
			Stop();
		}
		else if (InTargetRange)
		{
			Stop();
		}
		else if (Waypoint.x < Position.x)
		{
			num = -1f;
		}
		else if (Waypoint.x > Position.x)
		{
			num = 1f;
		}
		float z = Vehicle.transform.rotation.z;
		if ((z > 70f && z < 90f) || (z < 290f && z > 270f))
		{
			num = 0f - num;
		}
		Vehicle.SetMovement(0f - num);
	}

	private void Stop()
	{
		SetState(State.Camping);
		TargetRange = float.PositiveInfinity;
		Waypoint = Vector2.negativeInfinity;
	}

	private bool IsCloseRangeWeapon()
	{
		if (BulletDef.type != BulletType.Flame && BulletDef.type != BulletType.Lightning)
		{
			return BulletDef.type == BulletType.Small;
		}
		return true;
	}

	private Vector2 GetClosestPOI(Direction direction, float offset = 3f)
	{
		return GetClosestPOI(Position, direction, offset);
	}

	private Vector2 GetClosestPOI(Vector2 position, Direction direction, float offset = 3f)
	{
		Vector2 b = Vector2.right * offset * (float)direction;
		return AIController.instance.GetClosestPOI(position + b, direction, self.team);
	}

	private void Log(string text)
	{
	}
}
