using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

#pragma warning disable 0618
namespace I2.Loc
{
	public static class GoogleTranslation
	{
		public static bool CanTranslate()
		{
			if (LocalizationManager.Sources.Count > 0)
			{
				return !string.IsNullOrEmpty(LocalizationManager.GetWebServiceURL());
			}
			return false;
		}

		public static void Translate(string text, string LanguageCodeFrom, string LanguageCodeTo, Action<string> OnTranslationReady)
		{
			WWW translationWWW = GetTranslationWWW(text, LanguageCodeFrom, LanguageCodeTo);
			CoroutineManager.pInstance.StartCoroutine(WaitForTranslation(translationWWW, OnTranslationReady, text));
		}

		private static IEnumerator WaitForTranslation(WWW www, Action<string> OnTranslationReady, string OriginalText)
		{
			yield return www;
			UseTranslation(www, OnTranslationReady, OriginalText);
		}

		private static void UseTranslation(WWW www, Action<string> OnTranslationReady, string OriginalText)
		{
			if (!string.IsNullOrEmpty(www.error))
			{
				UnityEngine.Debug.LogError(www.error);
				OnTranslationReady(string.Empty);
			}
			else
			{
				byte[] bytes = www.bytes;
				string obj = ParseTranslationResult(Encoding.UTF8.GetString(bytes, 0, bytes.Length), OriginalText);
				OnTranslationReady(obj);
			}
		}

		public static WWW GetTranslationWWW(string text, string LanguageCodeFrom, string LanguageCodeTo)
		{
			LanguageCodeFrom = GoogleLanguages.GetGoogleLanguageCode(LanguageCodeFrom);
			LanguageCodeTo = GoogleLanguages.GetGoogleLanguageCode(LanguageCodeTo);
			if (TitleCase(text) == text && text.ToUpper() != text)
			{
				text = text.ToLower();
			}
			return new WWW($"{LocalizationManager.GetWebServiceURL()}?action=Translate&list={LanguageCodeFrom}:{LanguageCodeTo}={Uri.EscapeUriString(text)}");
		}

		public static string ParseTranslationResult(string html, string OriginalText)
		{
			try
			{
				string text = html;
				if (TitleCase(OriginalText) == OriginalText && OriginalText.ToUpper() != OriginalText)
				{
					text = TitleCase(text);
				}
				return text;
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogError(ex.Message);
				return string.Empty;
			}
		}

		public static void Translate(List<TranslationRequest> requests, Action<List<TranslationRequest>> OnTranslationReady)
		{
			WWW translationWWW = GetTranslationWWW(requests);
			CoroutineManager.pInstance.StartCoroutine(WaitForTranslation(translationWWW, OnTranslationReady, requests));
		}

		public static WWW GetTranslationWWW(List<TranslationRequest> requests)
		{
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = true;
			foreach (TranslationRequest request in requests)
			{
				if (!flag)
				{
					stringBuilder.Append("<I2Loc>");
				}
				stringBuilder.Append(request.LanguageCode);
				stringBuilder.Append(":");
				for (int i = 0; i < request.TargetLanguagesCode.Length; i++)
				{
					if (i != 0)
					{
						stringBuilder.Append(",");
					}
					stringBuilder.Append(request.TargetLanguagesCode[i]);
				}
				stringBuilder.Append("=");
				string stringToEscape = (TitleCase(request.Text) == request.Text) ? request.Text.ToLowerInvariant() : request.Text;
				stringBuilder.Append(Uri.EscapeUriString(stringToEscape));
				flag = false;
				if (stringBuilder.Length > 4000)
				{
					break;
				}
			}
			return new WWW($"{LocalizationManager.GetWebServiceURL()}?action=Translate&list={stringBuilder.ToString()}");
		}

		private static IEnumerator WaitForTranslation(WWW www, Action<List<TranslationRequest>> OnTranslationReady, List<TranslationRequest> requests)
		{
			yield return www;
			if (!string.IsNullOrEmpty(www.error))
			{
				UnityEngine.Debug.LogError(www.error);
				OnTranslationReady(requests);
			}
			else
			{
				byte[] bytes = www.bytes;
				ParseTranslationResult(Encoding.UTF8.GetString(bytes, 0, bytes.Length), requests);
				OnTranslationReady(requests);
			}
		}

		public static string ParseTranslationResult(string html, List<TranslationRequest> requests)
		{
			if (html.StartsWith("<!DOCTYPE html>") || html.StartsWith("<HTML>"))
			{
				if (html.Contains("Service invoked too many times in a short time"))
				{
					return "";
				}
				return "There was a problem contacting the WebService. Please try again later";
			}
			string[] array = html.Split(new string[1]
			{
				"<I2Loc>"
			}, StringSplitOptions.None);
			string[] separator = new string[1]
			{
				"<i2>"
			};
			for (int i = 0; i < Mathf.Min(requests.Count, array.Length); i++)
			{
				TranslationRequest translationRequest = requests[i];
				translationRequest.Results = array[i].Split(separator, StringSplitOptions.None);
				if (TitleCase(translationRequest.Text) == translationRequest.Text)
				{
					for (int j = 0; j < translationRequest.Results.Length; j++)
					{
						translationRequest.Results[j] = TitleCase(translationRequest.Results[j]);
					}
				}
				requests[i] = translationRequest;
			}
			return "";
		}

		public static string UppercaseFirst(string s)
		{
			if (string.IsNullOrEmpty(s))
			{
				return string.Empty;
			}
			char[] array = s.ToLower().ToCharArray();
			array[0] = char.ToUpper(array[0]);
			return new string(array);
		}

		public static string TitleCase(string s)
		{
			if (string.IsNullOrEmpty(s))
			{
				return string.Empty;
			}
			return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s);
		}
	}
}
