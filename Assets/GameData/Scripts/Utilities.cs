using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

public static class Utilities
{
	public static float AspectRatio => Screen.width / Screen.height;

	public static bool IsIPhonePlus()
	{
		string deviceModel = SystemInfo.deviceModel;
		switch (deviceModel)
		{
		default:
			return deviceModel == "iPhone10,5";
		case "iPhone7,1":
		case "iPhone8,2":
		case "iPhone9,2":
		case "iPhone9,4":
		case "iPhone10,2":
			return true;
		}
	}

	public static bool GetRandomBool()
	{
		return Random.Range(0f, 1f) < 0.5f;
	}

	public static Vector2 GetClosestToVectorFromList(Vector2 needle, List<Vector2> haystack)
	{
		Vector2 result = Vector2.positiveInfinity;
		float num = float.PositiveInfinity;
		for (int i = 0; i < haystack.Count; i++)
		{
			float sqrMagnitude = (haystack[i] - needle).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				result = haystack[i];
				num = sqrMagnitude;
			}
		}
		return result;
	}

	public static string SanitizeInput(string input, char[] allowedSpecial = null, UnicodeCategory[] allowedTypes = null)
	{
		allowedSpecial = (allowedSpecial ?? new char[3]
		{
			'.',
			'_',
			'-'
		});
		allowedTypes = (allowedTypes ?? new UnicodeCategory[5]
		{
			UnicodeCategory.UppercaseLetter,
			UnicodeCategory.LowercaseLetter,
			UnicodeCategory.TitlecaseLetter,
			UnicodeCategory.OtherLetter,
			UnicodeCategory.DecimalDigitNumber
		});
		StringBuilder stringBuilder = new StringBuilder();
		foreach (char c in input)
		{
			if (allowedSpecial.Contains(c) || allowedTypes.Contains(char.GetUnicodeCategory(c)))
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}

	public static float GetPercentage(float numerator, float denominator, int decimals)
	{
		return (float)decimal.Round((decimal)numerator / (decimal)denominator * 100m, decimals);
	}
}
