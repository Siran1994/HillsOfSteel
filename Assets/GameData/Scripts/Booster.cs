using System;
using UnityEngine;

[Serializable]
public class Booster : Collectible
{
	public BoosterGameplayType type;

	public BoosterGameplayActivation activationType;

	public string tankName;

	public Sprite inGameIcon;

	public float minValue;

	public float maxValue;

	public bool MaxLevel
	{
		get;
		set;
	}

	public bool HasMaxCount => Count == MaxCount;

	public int Level
	{
		get;
		set;
	}

	public int Count
	{
		get;
		set;
	}

	public int NextLevelCount
	{
		get;
		set;
	}

	public int ThisLevelCount
	{
		get;
		set;
	}

	public int MaxCount
	{
		get;
		set;
	}

	public float GetValue()
	{
		return Mathf.Lerp(minValue, maxValue, (float)Level / 14f);
	}
}
