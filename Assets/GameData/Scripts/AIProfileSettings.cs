using System;
using UnityEngine;

public class AIProfileSettings : ScriptableObject
{
	[Serializable]
	public struct HealthTrigger
	{
		public float health;

		public float probability;

		public AIBehaviour.Behaviour behaviour;
	}

	public AIBehaviour.Behaviour type;

	[Header("Movement")]
	public float stationaryCheckTime;

	public float stationaryDistanceMax;

	public float targetReachedRadius;

	public float movementDampingFactor;

	public MinMaxFloat preferDistance;

	[Header("Behaviour changing")]
	public bool changeBehaviourByHealth;

	public HealthTrigger[] healthChangeBehaviours;

	[Tooltip("Change to a random new behaviour per interval")]
	public bool changeBehaviourByTime;

	public MinMaxFloat changeBehaviourTime;

	[Header("Boosters")]
	public MinMaxFloat useBoosterTime;

	[Range(0f, 1f)]
	public float useRepairKitAtHealth;

	[Range(0f, 1f)]
	public float useForceFieldAtHealth;

	public MinMaxFloat useAirstrikeDistance;
}
