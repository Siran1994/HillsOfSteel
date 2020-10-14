using AI;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AIController
{
	public class TankData
	{
		public ushort id;

		public int rating;

		public float stationaryTimer;

		public Team team;

		public Vehicle vehicle;

		private float recentDistanceTravelled;

		private float stationaryCheckTime;

		private float stationaryDistanceMax;

		private Vector2 previousPosition = Vector2.zero;

		private Vector2 frontBlocker;

		private Vector2 backBlocker;

		public bool IsActive
		{
			get;
			private set;
		}

		public bool IsStationary
		{
			get;
			private set;
		}

		public bool NearFrontBlocker => Mathf.Abs(backBlocker.x - Position.x) < 10f;

		public bool NearBackBlocker => Mathf.Abs(frontBlocker.x - Position.x) < 10f;

		public bool NearBlocker
		{
			get
			{
				if (!NearFrontBlocker)
				{
					return NearBackBlocker;
				}
				return true;
			}
		}

		public Vector2 Position
		{
			get
			{
				if (vehicle == null || vehicle.transform == null || !IsActive)
				{
					return previousPosition;
				}
				return vehicle.transform.position;
			}
		}

		public TankData(Team team, Vehicle vehicle, int rating)
		{
			this.team = team;
			this.vehicle = vehicle;
			this.rating = rating;
			backBlocker = ((team == Team.Left) ? instance.backBlockerPosition : instance.frontBlockerPosition);
			frontBlocker = ((team == Team.Left) ? instance.frontBlockerPosition : instance.backBlockerPosition);
			previousPosition = Position;
			vehicle.SetHealth();
			IsActive = true;
		}

		public static bool operator ==(TankData ls, TankData rs)
		{
			return ls.id == rs.id;
		}

		public static bool operator !=(TankData ls, TankData rs)
		{
			return ls.id != rs.id;
		}

		public void Update(float dt)
		{
			if (IsActive)
			{
				vehicle.SetHealth();
				recentDistanceTravelled += Mathf.Abs(Position.x - previousPosition.x);
				previousPosition = Position;
				if (stationaryTimer >= stationaryCheckTime)
				{
					IsStationary = (recentDistanceTravelled <= stationaryDistanceMax);
					stationaryTimer = 0f;
					recentDistanceTravelled = 0f;
				}
				stationaryTimer += dt;
			}
		}

		public void SetDataFromProfile(AIProfileSettings profile)
		{
			stationaryCheckTime = profile.stationaryCheckTime;
			stationaryDistanceMax = profile.stationaryDistanceMax;
		}

		public float DistanceTo(TankData other)
		{
			return Mathf.Abs(Position.x - other.Position.x);
		}

		public bool IsPointBehind(Vector2 point)
		{
			if (point.IsNegativeInfinity() || point.IsPositiveInfinity())
			{
				return false;
			}
			if (team != 0)
			{
				return point.x > Position.x;
			}
			return point.x < Position.x;
		}

		public void Disable()
		{
			if (IsActive)
			{
				IsActive = false;
				if ((bool)vehicle)
				{
					vehicle.SetHealth();
					vehicle.SetMovement(0f);
					vehicle.SetSpeed(0f, 0f);
				}
			}
		}

		public override bool Equals(object obj)
		{
			TankData tankData = obj as TankData;
			if (tankData != null)
			{
				return id == tankData.id;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return 1877310944 + id.GetHashCode();
		}
	}

	public static AIController instance;

	[Header("ScriptableObjects")]
	public AISettings settings;

	[Header("Player & AI data filled in-game")]
	public List<TankData> redTeam;

	public List<TankData> blueTeam;

	public List<TankData> allPlayers;

	public List<AIBehaviour> aiBehaviours;

	[HideInInspector]
	public Vector2 backBlockerPosition;

	[HideInInspector]
	public Vector2 frontBlockerPosition;

	[HideInInspector]
	public List<Vector2> samplePointMinimas;

	[HideInInspector]
	public List<Vector2> samplePointMaximas;

	[HideInInspector]
	public List<Vector2> samplePointMinimaDerivatives;

	[HideInInspector]
	public List<Vector2> samplePointMaximaDerivatives;

	[HideInInspector]
	public List<Vector2> ltrUphillPositions;

	[HideInInspector]
	public List<Vector2> rtlUphillPositions;

	public int AiCount => aiBehaviours.Count;

	public void Init()
	{
		instance = this;
		backBlockerPosition = TankGame.instance.backBlockerContainer.position;
		frontBlockerPosition = TankGame.instance.frontBlockerContainer.position;
		FetchSamplePoints();
		GenerateHillData();
	}

	public void AddPlayers(TankData player, List<TankData> aiPlayers)
	{
		player.SetDataFromProfile(GetProfile(AIBehaviour.Behaviour.Defensive));
		allPlayers = new List<TankData>
		{
			player
		};
		allPlayers.AddRange(aiPlayers);
		redTeam = new List<TankData>();
		blueTeam = new List<TankData>();
		aiBehaviours = new List<AIBehaviour>();
		for (int i = 0; i < allPlayers.Count; i++)
		{
			allPlayers[i].id = (ushort)i;
			if (allPlayers[i].team == Team.Left)
			{
				redTeam.Add(allPlayers[i]);
			}
			else
			{
				blueTeam.Add(allPlayers[i]);
			}
		}
		if (aiPlayers.Count == 1)
		{
			aiBehaviours.Add(new AIBehaviour(aiPlayers[0], settings.startAsProfile));
			return;
		}
		for (int j = 1; j != redTeam.Count; j++)
		{
			aiBehaviours.Add(new AIBehaviour(redTeam[j], AIBehaviour.Behaviour.Aggressive));
		}
		for (int k = 0; k != blueTeam.Count; k++)
		{
			aiBehaviours.Add(new AIBehaviour(blueTeam[k], AIBehaviour.Behaviour.Aggressive));
		}
	}

	public void Update(float dt)
	{
		foreach (TankData allPlayer in allPlayers)
		{
			allPlayer.Update(dt);
		}
		foreach (AIBehaviour aiBehaviour in aiBehaviours)
		{
			aiBehaviour.Update(dt);
		}
	}

	private TankData GetLeader(Vector2 position, Team team, int ignoreId = -1)
	{
		float num = (team == Team.Left) ? 1f : (-1f);
		List<TankData> obj = (team == Team.Left) ? redTeam : blueTeam;
		float num2 = float.NegativeInfinity;
		TankData result = obj[0];
		foreach (TankData item in obj)
		{
			if (!(item.vehicle.CurrentHealth <= 0f) && item.id != ignoreId)
			{
				float num3 = item.Position.x * num;
				if (num3 > num2)
				{
					result = item;
					num2 = num3;
				}
			}
		}
		return result;
	}

	public TankData GetEnemyLeader(TankData self)
	{
		return GetLeader(self.Position, (self.team != Team.Right) ? Team.Right : Team.Left);
	}

	public TankData GetClosestAlly(TankData self)
	{
		return GetLeader(self.Position, self.team, self.id);
	}

	private void FetchSamplePoints()
	{
		samplePointMinimas = TankGame.instance.aiSamplePointMinima;
		samplePointMaximas = TankGame.instance.aiSamplePointMaxima;
		samplePointMinimaDerivatives = TankGame.instance.aiSamplePointMinimaDerivatives.ToVector2List();
		samplePointMaximaDerivatives = TankGame.instance.aiSamplePointMaximaDerivatives.ToVector2List();
	}

	private float GetGroundAngleAtLocation(float x)
	{
		float num = 0.5f;
		Vector2 b = new Vector2(x - num, TankGame.instance.GetGroundHeight(x - num));
		return Vector2.Angle(new Vector2(x + num, TankGame.instance.GetGroundHeight(x + num)) - b, Vector2.right);
	}

	private Vector2 GetUpHillShootPosition(Vector2 lowGround, Vector2 highGround)
	{
		float num = Mathf.Sign(highGround.x - lowGround.x);
		float num2 = Vector2.Angle(num * Vector2.right, (highGround - lowGround).normalized) + 5f;
		for (float num3 = lowGround.x; (num < 0f) ? (num3 > highGround.x) : (num3 < highGround.x); num3 += num * 0.15f)
		{
			if (Mathf.Abs(num2 - GetGroundAngleAtLocation(num3)) < 3f)
			{
				return new Vector2(num3, 0f);
			}
		}
		return lowGround;
	}

	private void GenerateHillData()
	{
		ltrUphillPositions = new List<Vector2>();
		rtlUphillPositions = new List<Vector2>();
		foreach (Vector2 samplePointMaxima in samplePointMaximas)
		{
			Vector2 lowGround = GetClosestMinima(samplePointMaxima, Direction.Left) ?? (samplePointMaxima + Vector2.left * 5f);
			ltrUphillPositions.Add(GetUpHillShootPosition(lowGround, samplePointMaxima));
			lowGround = (GetClosestMinima(samplePointMaxima, Direction.Right) ?? (samplePointMaxima + Vector2.right * 5f));
			rtlUphillPositions.Add(GetUpHillShootPosition(lowGround, samplePointMaxima));
		}
	}

	private Vector2? SearchForClosest(List<Vector2> list, Direction dir, Vector2 from)
	{
		Vector2 vector = Vector2.positiveInfinity;
		float num = float.PositiveInfinity;
		float f = float.PositiveInfinity;
		for (int i = 0; i < list.Count; i++)
		{
			if ((dir != Direction.Left || !(list[i].x > from.x)) && (dir != Direction.Right || !(list[i].x < from.x)))
			{
				float num2 = Mathf.Abs(list[i].x - from.x);
				if (num2 < num)
				{
					vector = list[i];
					num = num2;
				}
				if (!float.IsPositiveInfinity(f) && !float.IsPositiveInfinity(num2) && num > num2)
				{
					break;
				}
				f = num2;
			}
		}
		if (vector.IsPositiveInfinity())
		{
			return null;
		}
		return vector;
	}

	public Vector2? GetClosestMinima(Vector2 from, Direction dir)
	{
		return SearchForClosest(samplePointMinimas, dir, from);
	}

	public Vector2 GetClosestPOI(Vector2 position, Direction dir, Team team)
	{
		if (team == Team.Right)
		{
			return SearchForClosest(rtlUphillPositions, dir, position) ?? SearchForClosest(samplePointMinimas, dir, position) ?? SearchForClosest(samplePointMaximas, dir, position) ?? SearchForClosest(samplePointMaximaDerivatives, dir, position) ?? (position + (float)dir * Vector2.right);
		}
		return SearchForClosest(ltrUphillPositions, dir, position) ?? SearchForClosest(samplePointMinimas, dir, position) ?? SearchForClosest(samplePointMaximas, dir, position) ?? SearchForClosest(samplePointMaximaDerivatives, dir, position) ?? (position + (float)dir * Vector2.right);
	}

	public AIProfileSettings GetProfile(AIBehaviour.Behaviour behaviour)
	{
		AIProfileSettings[] profiles = settings.profiles;
		foreach (AIProfileSettings aIProfileSettings in profiles)
		{
			if (aIProfileSettings.type == behaviour)
			{
				return aIProfileSettings;
			}
		}
		return null;
	}

	public void SetBehaviourAll(AIBehaviour.Behaviour type)
	{
		foreach (AIBehaviour aiBehaviour in aiBehaviours)
		{
			aiBehaviour.LockBehaviour(type, overrideCurrent: true);
		}
	}

	public void ChargeAll()
	{
		foreach (AIBehaviour aiBehaviour in aiBehaviours)
		{
			aiBehaviour.UpdateProperties();
			aiBehaviour.LockBehaviour(AIBehaviour.Behaviour.Aggressive, overrideCurrent: true);
			aiBehaviour.SetWaypoint(aiBehaviour.MeleeContactPoint + aiBehaviour.BackwardVec, resolving: true);
		}
	}

	public void RepairKitUsed(Vector2 location)
	{
		foreach (AIBehaviour aiBehaviour in aiBehaviours)
		{
			aiBehaviour.SetWaypoint(location);
		}
	}

	public bool IsPlayerCamping()
	{
		TankData tankData = allPlayers[0];
		if (tankData.IsActive)
		{
			return tankData.IsStationary;
		}
		return false;
	}

	public void Disable()
	{
		foreach (TankData allPlayer in allPlayers)
		{
			allPlayer.Disable();
		}
	}
}
