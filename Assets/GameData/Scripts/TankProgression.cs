using System;
using UnityEngine;

[Serializable]
public struct TankProgression
{
	[Serializable]
	public struct Stats
	{
		public float damage;

		public float reloadTime;

		public float health;

		public float acceleration;

		public float maxSpeed;

		public float brake;

		[Tooltip("Garage slider visual only")]
		public float dps;
	}

	public Stats minStep;

	public Stats maxStep;

	public Stats GetStep(int level, int maxLevel)
	{
		float t = (float)level / (float)maxLevel;
		Stats result = default(Stats);
		result.damage = Mathf.Lerp(minStep.damage, maxStep.damage, t);
		result.reloadTime = Mathf.Lerp(minStep.reloadTime, maxStep.reloadTime, t);
		result.health = Mathf.Lerp(minStep.health, maxStep.health, t);
		result.acceleration = Mathf.Lerp(minStep.acceleration, maxStep.acceleration, t);
		result.maxSpeed = Mathf.Lerp(minStep.maxSpeed, maxStep.maxSpeed, t);
		result.brake = Mathf.Lerp(minStep.brake, maxStep.brake, t);
		result.dps = Mathf.Lerp(minStep.dps, maxStep.dps, t);
		return result;
	}
}
