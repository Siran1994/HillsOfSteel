using System;
using UnityEngine;

[Serializable]
public class Tank : Collectible
{
	public PlayerTankType type;

	public TankProgression progression;

	public GameObject prefab;

	public BulletDefinition bullet;

	public float tankMass = 2.96f;

	public TankSkin[] tankSkins;

	public string[] boosters;

	public TankProgression.Stats GetMaxProgression()
	{
		return progression.GetStep(PlayerDataManager.GetTankUpgradeLevel(this), 14);
	}

	public TankProgression.Stats GetProgression(int level, int maxLevel = 14)
	{
		return progression.GetStep(level, maxLevel);
	}
}
