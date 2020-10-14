using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UTNotifications
{
	public static class SampleUtils
	{
		public static string UniqueName(Transform transform)
		{
			if (transform == null)
			{
				return SceneManager.GetActiveScene().name;
			}
			return UniqueName(transform.parent) + "." + transform.name;
		}

		public static string GenerateDeviceUniqueIdentifier()
		{
			string text = null;
			try
			{
				using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("universal.tools.notifications.Manager"))
				{
					text = androidJavaClass.CallStatic<string>("getDeviceId", Array.Empty<object>());
				}
			}
			catch (AndroidJavaException exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
			if (string.IsNullOrEmpty(text))
			{
				UnityEngine.Debug.LogWarning("Failed to get a device id. Using a default id instead.");
				text = "00000000000000000000000000000000";
			}
			return GetMd5Hash(text);
		}

		private static string GetMd5Hash(string input)
		{
			if (string.IsNullOrEmpty(input))
			{
				return "";
			}
			byte[] array = new MD5CryptoServiceProvider().ComputeHash(Encoding.Default.GetBytes(input));
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < array.Length; i++)
			{
				stringBuilder.Append(array[i].ToString("x2"));
			}
			return stringBuilder.ToString();
		}
	}
}
