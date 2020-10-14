using System;
using UnityEngine;

[Serializable]
public class MinMaxFloat : MinMax<float>
{
	public MinMaxFloat(float minimum, float maximum)
	{
		min = minimum;
		max = maximum;
	}

	public float Random()
	{
		return UnityEngine.Random.Range(min, max);
	}

	public float ClampInside(float val)
	{
		return Mathf.Clamp(val, min, max);
	}
}
