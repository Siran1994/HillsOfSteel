using System;
using UnityEngine;

[Serializable]
public class MinMaxInt : MinMax<int>
{
	public MinMaxInt(int minimum, int maximum)
	{
		min = minimum;
		max = maximum;
	}

	public int Random()
	{
		return UnityEngine.Random.Range(min, max);
	}

	public int ClampInside(int val)
	{
		return Mathf.Clamp(val, min, max);
	}

	public bool ContainsInclusive(int val)
	{
		if (val.CompareTo(min) >= 0)
		{
			return val.CompareTo(max) <= 0;
		}
		return false;
	}

	public bool ContainsExclusive(int val)
	{
		if (val.CompareTo(min) >= 0)
		{
			return val.CompareTo(max - 1) <= 0;
		}
		return false;
	}
}
