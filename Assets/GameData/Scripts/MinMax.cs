using System;

[Serializable]
public class MinMax<T> where T : IComparable<T>
{
	public T min;

	public T max;

	public bool Contains(T val)
	{
		if (val.CompareTo(min) >= 0)
		{
			return val.CompareTo(max) <= 0;
		}
		return false;
	}
}
