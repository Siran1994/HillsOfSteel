using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class Extensions
{
	public static string ToBase64(this string input)
	{
		input = Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
		return input;
	}

	public static string FromBase64(this string input)
	{
		input = Encoding.UTF8.GetString(Convert.FromBase64String(input));
		return input;
	}

	public static T Random<T>(this T[] list)
	{
		return list[UnityEngine.Random.Range(0, list.Length)];
	}

	public static T Random<T>(this List<T> list)
	{
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public static List<T> ToList<T>(this T[] arr)
	{
		List<T> list = new List<T>(arr.Length);
		for (int i = 0; i != arr.Length; i++)
		{
			list.Add(arr[i]);
		}
		return list;
	}

	public static bool Contains<T>(this IEnumerable<T> haystack, T needle)
	{
		foreach (T item in haystack)
		{
			if (item.Equals(needle))
			{
				return true;
			}
		}
		return false;
	}

	public static T RemoveRandom<T>(this HashSet<T> hashSet)
	{
		int num = UnityEngine.Random.Range(0, hashSet.Count);
		int num2 = 0;
		foreach (T item in hashSet)
		{
			if (num2 == num)
			{
				hashSet.Remove(item);
				return item;
			}
			num2++;
		}
		return default(T);
	}

	public static T[] Shift<T>(this T[] array, int offset = 1)
	{
		T[] array2 = new T[array.Length];
		if (offset > 0)
		{
			Array.Copy(array, offset, array2, 0, array.Length - offset);
		}
		else
		{
			UnityEngine.Debug.LogError("Implement Array.UnShift() and use that instead.");
		}
		return array2;
	}

	public static void Shuffle<T>(this IList<T> list)
	{
		System.Random random = new System.Random();
		for (int num = list.Count - 1; num > 1; num--)
		{
			int index = random.Next(num + 1);
			T value = list[index];
			list[index] = list[num];
			list[num] = value;
		}
	}

	public static List<Vector2> ToVector2List(this List<Vector3> list)
	{
		List<Vector2> list2 = new List<Vector2>(list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			list2.Add(list[i]);
		}
		return list2;
	}

	public static bool IsPositiveInfinity(this Vector2 v)
	{
		if (!float.IsPositiveInfinity(v.x))
		{
			return float.IsPositiveInfinity(v.y);
		}
		return true;
	}

	public static bool IsNegativeInfinity(this Vector2 v)
	{
		if (!float.IsNegativeInfinity(v.x))
		{
			return float.IsNegativeInfinity(v.y);
		}
		return true;
	}

	public static Vector2 ToVector2(this Vector3 v)
	{
		return v;
	}

	public static Vector2 Clamp(this Vector2 input, Vector2 min, Vector2 max, bool x = true, bool y = true)
	{
		Vector2 result = default(Vector2);
		result.x = (x ? Mathf.Clamp(input.x, min.x, max.x) : input.x);
		result.y = (y ? Mathf.Clamp(input.y, min.y, max.y) : input.y);
		return result;
	}

	public static Vector2 Rotate(this Vector2 input, float deg)
	{
		float f = deg * ((float)Math.PI / 180f);
		return new Vector2(input.x * Mathf.Cos(f) - input.y * Mathf.Sin(f), input.x * Mathf.Sin(f) + input.y * Mathf.Cos(f));
	}

	public static Vector2 Replace(this Vector2 input, float? x = default(float?), float? y = default(float?))
	{
		return new Vector2(x ?? input.x, y ?? input.y);
	}

	public static Vector3 Replace(this Vector3 input, float? x = default(float?), float? y = default(float?), float? z = default(float?))
	{
		return new Vector3(x ?? input.x, y ?? input.y, z ?? input.z);
	}

	public static T GetComponentInParents<T>(this GameObject root, int depth, int layerMask)
	{
		GameObject gameObject = root.transform.parent.gameObject;
		if (gameObject.GetComponent<T>() != null)
		{
			return gameObject.GetComponent<T>();
		}
		int num = 1 << gameObject.layer;
		if ((num & layerMask) != num || depth == 0)
		{
			return default(T);
		}
		return gameObject.GetComponentInParents<T>(depth - 1, layerMask);
	}
}
