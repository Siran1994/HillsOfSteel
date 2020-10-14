using System;
using System.Collections.Generic;
using UnityEngine;

namespace I2.Loc
{
	public static class GoogleLanguages
	{
		public struct LanguageCodeDef
		{
			public string Code;

			public string GoogleCode;
		}

		public static Dictionary<string, LanguageCodeDef> mLanguageDef;

		public static string GetLanguageCode(string Filter, bool ShowWarnings = false)
		{
			if (string.IsNullOrEmpty(Filter))
			{
				return string.Empty;
			}
			string[] filters = Filter.ToLowerInvariant().Split(" /(),".ToCharArray());
			foreach (KeyValuePair<string, LanguageCodeDef> item in mLanguageDef)
			{
				if (LanguageMatchesFilter(item.Key, filters))
				{
					return item.Value.Code;
				}
			}
			if (ShowWarnings)
			{
				UnityEngine.Debug.Log($"Language '{Filter}' not recognized. Please, add the language code to GoogleTranslation.cs");
			}
			return string.Empty;
		}

		public static List<string> GetLanguagesForDropdown(string Filter, string CodesToExclude)
		{
			string[] filters = Filter.ToLowerInvariant().Split(" /(),".ToCharArray());
			List<string> list = new List<string>();
			foreach (KeyValuePair<string, LanguageCodeDef> item in mLanguageDef)
			{
				if (string.IsNullOrEmpty(Filter) || LanguageMatchesFilter(item.Key, filters))
				{
					string text = string.Concat("[" + item.Value.Code + "]");
					if (!CodesToExclude.Contains(text))
					{
						list.Add(item.Key + " " + text);
					}
				}
			}
			for (int num = list.Count - 2; num >= 0; num--)
			{
				string text2 = list[num].Substring(0, list[num].IndexOf(" ["));
				if (list[num + 1].StartsWith(text2))
				{
					list[num] = text2 + "/" + list[num];
					list.Insert(num + 1, text2 + "/");
				}
			}
			return list;
		}

		public static string GetClosestLanguage(string Filter)
		{
			if (string.IsNullOrEmpty(Filter))
			{
				return string.Empty;
			}
			string[] filters = Filter.ToLowerInvariant().Split(" /(),".ToCharArray());
			foreach (KeyValuePair<string, LanguageCodeDef> item in mLanguageDef)
			{
				if (LanguageMatchesFilter(item.Key, filters))
				{
					return item.Key;
				}
			}
			return string.Empty;
		}

		private static bool LanguageMatchesFilter(string Language, string[] Filters)
		{
			Language = Language.ToLowerInvariant();
			int i = 0;
			for (int num = Filters.Length; i < num; i++)
			{
				if (Filters[i] != "")
				{
					if (!Language.Contains(Filters[i].ToLower()))
					{
						return false;
					}
					Language = Language.Remove(Language.IndexOf(Filters[i]), Filters[i].Length);
				}
			}
			return true;
		}

		public static string GetFormatedLanguageName(string Language)
		{
			string empty = string.Empty;
			int num = Language.IndexOf(" [");
			if (num > 0)
			{
				Language = Language.Substring(0, num);
			}
			num = Language.IndexOf('/');
			if (num > 0)
			{
				empty = Language.Substring(0, num);
				if (Language == empty + "/" + empty)
				{
					return empty;
				}
				Language = Language.Replace("/", " (") + ")";
			}
			return Language;
		}

		public static string GetCodedLanguage(string Language, string code)
		{
			string languageCode = GetLanguageCode(Language);
			if (string.Compare(code, languageCode, StringComparison.OrdinalIgnoreCase) == 0)
			{
				return Language;
			}
			return Language + " [" + code + "]";
		}

		public static void UnPackCodeFromLanguageName(string CodedLanguage, out string Language, out string code)
		{
			if (string.IsNullOrEmpty(CodedLanguage))
			{
				Language = string.Empty;
				code = string.Empty;
				return;
			}
			int num = CodedLanguage.IndexOf("[");
			if (num < 0)
			{
				Language = CodedLanguage;
				code = GetLanguageCode(Language);
			}
			else
			{
				Language = CodedLanguage.Substring(0, num).Trim();
				code = CodedLanguage.Substring(num + 1, CodedLanguage.IndexOf("]", num) - num - 1);
			}
		}

		public static string GetGoogleLanguageCode(string InternationalCode)
		{
			foreach (KeyValuePair<string, LanguageCodeDef> item in mLanguageDef)
			{
				if (InternationalCode == item.Value.Code)
				{
					return (!string.IsNullOrEmpty(item.Value.GoogleCode)) ? item.Value.GoogleCode : InternationalCode;
				}
			}
			return InternationalCode;
		}

		public static List<string> GetAllInternationalCodes()
		{
			HashSet<string> hashSet = new HashSet<string>();
			foreach (KeyValuePair<string, LanguageCodeDef> item in mLanguageDef)
			{
				hashSet.Add(item.Value.Code);
			}
			return new List<string>(hashSet);
		}

		static GoogleLanguages()
		{
			Dictionary<string, LanguageCodeDef> dictionary = new Dictionary<string, LanguageCodeDef>(StringComparer.Ordinal);
			LanguageCodeDef value = new LanguageCodeDef
			{
				Code = "af"
			};
			dictionary.Add("Afrikaans", value);
			value = new LanguageCodeDef
			{
				Code = "sq"
			};
			dictionary.Add("Albanian", value);
			value = new LanguageCodeDef
			{
				Code = "ar"
			};
			dictionary.Add("Arabic", value);
			value = new LanguageCodeDef
			{
				Code = "ar-DZ",
				GoogleCode = "ar"
			};
			dictionary.Add("Arabic/Algeria", value);
			value = new LanguageCodeDef
			{
				Code = "ar-BH",
				GoogleCode = "ar"
			};
			dictionary.Add("Arabic/Bahrain", value);
			value = new LanguageCodeDef
			{
				Code = "ar-EG",
				GoogleCode = "ar"
			};
			dictionary.Add("Arabic/Egypt", value);
			value = new LanguageCodeDef
			{
				Code = "ar-IQ",
				GoogleCode = "ar"
			};
			dictionary.Add("Arabic/Iraq", value);
			value = new LanguageCodeDef
			{
				Code = "ar-JO",
				GoogleCode = "ar"
			};
			dictionary.Add("Arabic/Jordan", value);
			value = new LanguageCodeDef
			{
				Code = "ar-KW",
				GoogleCode = "ar"
			};
			dictionary.Add("Arabic/Kuwait", value);
			value = new LanguageCodeDef
			{
				Code = "ar-LB",
				GoogleCode = "ar"
			};
			dictionary.Add("Arabic/Lebanon", value);
			value = new LanguageCodeDef
			{
				Code = "ar-LY",
				GoogleCode = "ar"
			};
			dictionary.Add("Arabic/Libya", value);
			value = new LanguageCodeDef
			{
				Code = "ar-MA",
				GoogleCode = "ar"
			};
			dictionary.Add("Arabic/Morocco", value);
			value = new LanguageCodeDef
			{
				Code = "ar-OM",
				GoogleCode = "ar"
			};
			dictionary.Add("Arabic/Oman", value);
			value = new LanguageCodeDef
			{
				Code = "ar-QA",
				GoogleCode = "ar"
			};
			dictionary.Add("Arabic/Qatar", value);
			value = new LanguageCodeDef
			{
				Code = "ar-SA",
				GoogleCode = "ar"
			};
			dictionary.Add("Arabic/Saudi Arabia", value);
			value = new LanguageCodeDef
			{
				Code = "ar-SY",
				GoogleCode = "ar"
			};
			dictionary.Add("Arabic/Syria", value);
			value = new LanguageCodeDef
			{
				Code = "ar-TN",
				GoogleCode = "ar"
			};
			dictionary.Add("Arabic/Tunisia", value);
			value = new LanguageCodeDef
			{
				Code = "ar-AE",
				GoogleCode = "ar"
			};
			dictionary.Add("Arabic/U.A.E.", value);
			value = new LanguageCodeDef
			{
				Code = "ar-YE",
				GoogleCode = "ar"
			};
			dictionary.Add("Arabic/Yemen", value);
			value = new LanguageCodeDef
			{
				Code = "hy"
			};
			dictionary.Add("Armenian", value);
			value = new LanguageCodeDef
			{
				Code = "az"
			};
			dictionary.Add("Azerbaijani", value);
			value = new LanguageCodeDef
			{
				Code = "eu"
			};
			dictionary.Add("Basque", value);
			value = new LanguageCodeDef
			{
				Code = "eu-ES",
				GoogleCode = "eu"
			};
			dictionary.Add("Basque/Spain", value);
			value = new LanguageCodeDef
			{
				Code = "be"
			};
			dictionary.Add("Belarusian", value);
			value = new LanguageCodeDef
			{
				Code = "bs"
			};
			dictionary.Add("Bosnian", value);
			value = new LanguageCodeDef
			{
				Code = "bg"
			};
			dictionary.Add("Bulgariaa", value);
			value = new LanguageCodeDef
			{
				Code = "ca"
			};
			dictionary.Add("Catalan", value);
			value = new LanguageCodeDef
			{
				Code = "zh",
				GoogleCode = "zh-CN"
			};
			dictionary.Add("Chinese", value);
			value = new LanguageCodeDef
			{
				Code = "zh-HK",
				GoogleCode = "zh-TW"
			};
			dictionary.Add("Chinese/Hong Kong", value);
			value = new LanguageCodeDef
			{
				Code = "zh-MO",
				GoogleCode = "zh-CN"
			};
			dictionary.Add("Chinese/Macau", value);
			value = new LanguageCodeDef
			{
				Code = "zh-CN",
				GoogleCode = "zh-CN"
			};
			dictionary.Add("Chinese/PRC", value);
			value = new LanguageCodeDef
			{
				Code = "zh-CN",
				GoogleCode = "zh-CN"
			};
			dictionary.Add("Chinese/Simplified", value);
			value = new LanguageCodeDef
			{
				Code = "zh-SG",
				GoogleCode = "zh-CN"
			};
			dictionary.Add("Chinese/Singapore", value);
			value = new LanguageCodeDef
			{
				Code = "zh-TW",
				GoogleCode = "zh-TW"
			};
			dictionary.Add("Chinese/Taiwan", value);
			value = new LanguageCodeDef
			{
				Code = "zh-TW",
				GoogleCode = "zh-TW"
			};
			dictionary.Add("Chinese/Traditional", value);
			value = new LanguageCodeDef
			{
				Code = "hr"
			};
			dictionary.Add("Croatian", value);
			value = new LanguageCodeDef
			{
				Code = "hr-BA",
				GoogleCode = "hr"
			};
			dictionary.Add("Croatian/Bosnia and Herzegovina", value);
			value = new LanguageCodeDef
			{
				Code = "cs"
			};
			dictionary.Add("Czech", value);
			value = new LanguageCodeDef
			{
				Code = "da"
			};
			dictionary.Add("Danish", value);
			value = new LanguageCodeDef
			{
				Code = "nl"
			};
			dictionary.Add("Dutch", value);
			value = new LanguageCodeDef
			{
				Code = "nl-BE",
				GoogleCode = "nl"
			};
			dictionary.Add("Dutch/Belgium", value);
			value = new LanguageCodeDef
			{
				Code = "nl-NL",
				GoogleCode = "nl"
			};
			dictionary.Add("Dutch/Netherlands", value);
			value = new LanguageCodeDef
			{
				Code = "en"
			};
			dictionary.Add("English", value);
			value = new LanguageCodeDef
			{
				Code = "en-AU",
				GoogleCode = "en"
			};
			dictionary.Add("English/Australia", value);
			value = new LanguageCodeDef
			{
				Code = "en-BZ",
				GoogleCode = "en"
			};
			dictionary.Add("English/Belize", value);
			value = new LanguageCodeDef
			{
				Code = "en-CA",
				GoogleCode = "en"
			};
			dictionary.Add("English/Canada", value);
			value = new LanguageCodeDef
			{
				Code = "en-CB",
				GoogleCode = "en"
			};
			dictionary.Add("English/Caribbean", value);
			value = new LanguageCodeDef
			{
				Code = "en-IE",
				GoogleCode = "en"
			};
			dictionary.Add("English/Ireland", value);
			value = new LanguageCodeDef
			{
				Code = "en-JM",
				GoogleCode = "en"
			};
			dictionary.Add("English/Jamaica", value);
			value = new LanguageCodeDef
			{
				Code = "en-NZ",
				GoogleCode = "en"
			};
			dictionary.Add("English/New Zealand", value);
			value = new LanguageCodeDef
			{
				Code = "en-PH",
				GoogleCode = "en"
			};
			dictionary.Add("English/Republic of the Philippines", value);
			value = new LanguageCodeDef
			{
				Code = "en-ZA",
				GoogleCode = "en"
			};
			dictionary.Add("English/South Africa", value);
			value = new LanguageCodeDef
			{
				Code = "en-TT",
				GoogleCode = "en"
			};
			dictionary.Add("English/Trinidad", value);
			value = new LanguageCodeDef
			{
				Code = "en-GB",
				GoogleCode = "en"
			};
			dictionary.Add("English/United Kingdom", value);
			value = new LanguageCodeDef
			{
				Code = "en-US",
				GoogleCode = "en"
			};
			dictionary.Add("English/United States", value);
			value = new LanguageCodeDef
			{
				Code = "en-ZW",
				GoogleCode = "en"
			};
			dictionary.Add("English/Zimbabwe", value);
			value = new LanguageCodeDef
			{
				Code = "eo"
			};
			dictionary.Add("Esperanto", value);
			value = new LanguageCodeDef
			{
				Code = "et"
			};
			dictionary.Add("Estonian", value);
			value = new LanguageCodeDef
			{
				Code = "fo"
			};
			dictionary.Add("Faeroese", value);
			value = new LanguageCodeDef
			{
				Code = "tl"
			};
			dictionary.Add("Filipino", value);
			value = new LanguageCodeDef
			{
				Code = "fi"
			};
			dictionary.Add("Finnish", value);
			value = new LanguageCodeDef
			{
				Code = "fr"
			};
			dictionary.Add("French", value);
			value = new LanguageCodeDef
			{
				Code = "fr-BE",
				GoogleCode = "fr"
			};
			dictionary.Add("French/Belgium", value);
			value = new LanguageCodeDef
			{
				Code = "fr-CA",
				GoogleCode = "fr"
			};
			dictionary.Add("French/Canada", value);
			value = new LanguageCodeDef
			{
				Code = "fr-FR",
				GoogleCode = "fr"
			};
			dictionary.Add("French/France", value);
			value = new LanguageCodeDef
			{
				Code = "fr-LU",
				GoogleCode = "fr"
			};
			dictionary.Add("French/Luxembourg", value);
			value = new LanguageCodeDef
			{
				Code = "fr-MC",
				GoogleCode = "fr"
			};
			dictionary.Add("French/Principality of Monaco", value);
			value = new LanguageCodeDef
			{
				Code = "fr-CH",
				GoogleCode = "fr"
			};
			dictionary.Add("French/Switzerland", value);
			value = new LanguageCodeDef
			{
				Code = "gl"
			};
			dictionary.Add("Galician", value);
			value = new LanguageCodeDef
			{
				Code = "gl-ES",
				GoogleCode = "gl"
			};
			dictionary.Add("Galician/Spain", value);
			value = new LanguageCodeDef
			{
				Code = "ka"
			};
			dictionary.Add("Georgian", value);
			value = new LanguageCodeDef
			{
				Code = "de"
			};
			dictionary.Add("German", value);
			value = new LanguageCodeDef
			{
				Code = "de-AT",
				GoogleCode = "de"
			};
			dictionary.Add("German/Austria", value);
			value = new LanguageCodeDef
			{
				Code = "de-DE",
				GoogleCode = "de"
			};
			dictionary.Add("German/Germany", value);
			value = new LanguageCodeDef
			{
				Code = "de-LI",
				GoogleCode = "de"
			};
			dictionary.Add("German/Liechtenstein", value);
			value = new LanguageCodeDef
			{
				Code = "de-LU",
				GoogleCode = "de"
			};
			dictionary.Add("German/Luxembourg", value);
			value = new LanguageCodeDef
			{
				Code = "de-CH",
				GoogleCode = "de"
			};
			dictionary.Add("German/Switzerland", value);
			value = new LanguageCodeDef
			{
				Code = "el"
			};
			dictionary.Add("Greek", value);
			value = new LanguageCodeDef
			{
				Code = "gu"
			};
			dictionary.Add("Gujarati", value);
			value = new LanguageCodeDef
			{
				Code = "he",
				GoogleCode = "iw"
			};
			dictionary.Add("Hebrew", value);
			value = new LanguageCodeDef
			{
				Code = "hi"
			};
			dictionary.Add("Hindi", value);
			value = new LanguageCodeDef
			{
				Code = "hu"
			};
			dictionary.Add("Hungarian", value);
			value = new LanguageCodeDef
			{
				Code = "is"
			};
			dictionary.Add("Icelandic", value);
			value = new LanguageCodeDef
			{
				Code = "id"
			};
			dictionary.Add("Indonesian", value);
			value = new LanguageCodeDef
			{
				Code = "ga"
			};
			dictionary.Add("Irish", value);
			value = new LanguageCodeDef
			{
				Code = "it"
			};
			dictionary.Add("Italian", value);
			value = new LanguageCodeDef
			{
				Code = "it-IT",
				GoogleCode = "it"
			};
			dictionary.Add("Italian/Italy", value);
			value = new LanguageCodeDef
			{
				Code = "it-CH",
				GoogleCode = "it"
			};
			dictionary.Add("Italian/Switzerland", value);
			value = new LanguageCodeDef
			{
				Code = "ja"
			};
			dictionary.Add("Japanese", value);
			value = new LanguageCodeDef
			{
				Code = "kn"
			};
			dictionary.Add("Kannada", value);
			value = new LanguageCodeDef
			{
				Code = "kk"
			};
			dictionary.Add("Kazakh", value);
			value = new LanguageCodeDef
			{
				Code = "ko"
			};
			dictionary.Add("Korean", value);
			value = new LanguageCodeDef
			{
				Code = "ku"
			};
			dictionary.Add("Kurdish", value);
			value = new LanguageCodeDef
			{
				Code = "ky"
			};
			dictionary.Add("Kyrgyz", value);
			value = new LanguageCodeDef
			{
				Code = "la"
			};
			dictionary.Add("Latin", value);
			value = new LanguageCodeDef
			{
				Code = "lv"
			};
			dictionary.Add("Latvian", value);
			value = new LanguageCodeDef
			{
				Code = "lt"
			};
			dictionary.Add("Lithuanian", value);
			value = new LanguageCodeDef
			{
				Code = "mk"
			};
			dictionary.Add("Macedonian", value);
			value = new LanguageCodeDef
			{
				Code = "ms"
			};
			dictionary.Add("Malay", value);
			value = new LanguageCodeDef
			{
				Code = "ms-BN",
				GoogleCode = "ms"
			};
			dictionary.Add("Malay/Brunei Darussalam", value);
			value = new LanguageCodeDef
			{
				Code = "ms-MY",
				GoogleCode = "ms"
			};
			dictionary.Add("Malay/Malaysia", value);
			value = new LanguageCodeDef
			{
				Code = "ml"
			};
			dictionary.Add("Malayalam", value);
			value = new LanguageCodeDef
			{
				Code = "mt"
			};
			dictionary.Add("Maltese", value);
			value = new LanguageCodeDef
			{
				Code = "mi"
			};
			dictionary.Add("Maori", value);
			value = new LanguageCodeDef
			{
				Code = "mr"
			};
			dictionary.Add("Marathi", value);
			value = new LanguageCodeDef
			{
				Code = "mn"
			};
			dictionary.Add("Mongolian", value);
			value = new LanguageCodeDef
			{
				Code = "ns",
				GoogleCode = "nso"
			};
			dictionary.Add("Northern Sotho", value);
			value = new LanguageCodeDef
			{
				Code = "nb",
				GoogleCode = "no"
			};
			dictionary.Add("Norwegian", value);
			value = new LanguageCodeDef
			{
				Code = "nn",
				GoogleCode = "no"
			};
			dictionary.Add("Norwegian/Nynorsk", value);
			value = new LanguageCodeDef
			{
				Code = "ps"
			};
			dictionary.Add("Pashto", value);
			value = new LanguageCodeDef
			{
				Code = "fa"
			};
			dictionary.Add("Persian", value);
			value = new LanguageCodeDef
			{
				Code = "pl"
			};
			dictionary.Add("Polish", value);
			value = new LanguageCodeDef
			{
				Code = "pt"
			};
			dictionary.Add("Portuguese", value);
			value = new LanguageCodeDef
			{
				Code = "pt-BR",
				GoogleCode = "pt"
			};
			dictionary.Add("Portuguese/Brazil", value);
			value = new LanguageCodeDef
			{
				Code = "pt-PT",
				GoogleCode = "pt"
			};
			dictionary.Add("Portuguese/Portugal", value);
			value = new LanguageCodeDef
			{
				Code = "pa"
			};
			dictionary.Add("Punjabi", value);
			value = new LanguageCodeDef
			{
				Code = "qu"
			};
			dictionary.Add("Quechua", value);
			value = new LanguageCodeDef
			{
				Code = "qu-BO",
				GoogleCode = "qu"
			};
			dictionary.Add("Quechua/Bolivia", value);
			value = new LanguageCodeDef
			{
				Code = "qu-EC",
				GoogleCode = "qu"
			};
			dictionary.Add("Quechua/Ecuador", value);
			value = new LanguageCodeDef
			{
				Code = "qu-PE",
				GoogleCode = "qu"
			};
			dictionary.Add("Quechua/Peru", value);
			value = new LanguageCodeDef
			{
				Code = "rm",
				GoogleCode = "ro"
			};
			dictionary.Add("Rhaeto-Romanic", value);
			value = new LanguageCodeDef
			{
				Code = "ro"
			};
			dictionary.Add("Romanian", value);
			value = new LanguageCodeDef
			{
				Code = "ru"
			};
			dictionary.Add("Russian", value);
			value = new LanguageCodeDef
			{
				Code = "ru-MO",
				GoogleCode = "ru"
			};
			dictionary.Add("Russian/Republic of Moldova", value);
			value = new LanguageCodeDef
			{
				Code = "sr"
			};
			dictionary.Add("Serbian", value);
			value = new LanguageCodeDef
			{
				Code = "sr-BA",
				GoogleCode = "sr"
			};
			dictionary.Add("Serbian/Bosnia and Herzegovina", value);
			value = new LanguageCodeDef
			{
				Code = "sr-SP",
				GoogleCode = "sr"
			};
			dictionary.Add("Serbian/Serbia and Montenegro", value);
			value = new LanguageCodeDef
			{
				Code = "sk"
			};
			dictionary.Add("Slovak", value);
			value = new LanguageCodeDef
			{
				Code = "sl"
			};
			dictionary.Add("Slovenian", value);
			value = new LanguageCodeDef
			{
				Code = "es"
			};
			dictionary.Add("Spanish", value);
			value = new LanguageCodeDef
			{
				Code = "es-AR",
				GoogleCode = "es"
			};
			dictionary.Add("Spanish/Argentina", value);
			value = new LanguageCodeDef
			{
				Code = "es-BO",
				GoogleCode = "es"
			};
			dictionary.Add("Spanish/Bolivia", value);
			value = new LanguageCodeDef
			{
				Code = "es-ES",
				GoogleCode = "es"
			};
			dictionary.Add("Spanish/Castilian", value);
			value = new LanguageCodeDef
			{
				Code = "es-CL",
				GoogleCode = "es"
			};
			dictionary.Add("Spanish/Chile", value);
			value = new LanguageCodeDef
			{
				Code = "es-CO",
				GoogleCode = "es"
			};
			dictionary.Add("Spanish/Colombia", value);
			value = new LanguageCodeDef
			{
				Code = "es-CR",
				GoogleCode = "es"
			};
			dictionary.Add("Spanish/Costa Rica", value);
			value = new LanguageCodeDef
			{
				Code = "es-DO",
				GoogleCode = "es"
			};
			dictionary.Add("Spanish/Dominican Republic", value);
			value = new LanguageCodeDef
			{
				Code = "es-EC",
				GoogleCode = "es"
			};
			dictionary.Add("Spanish/Ecuador", value);
			value = new LanguageCodeDef
			{
				Code = "es-SV",
				GoogleCode = "es"
			};
			dictionary.Add("Spanish/El Salvador", value);
			value = new LanguageCodeDef
			{
				Code = "es-GT",
				GoogleCode = "es"
			};
			dictionary.Add("Spanish/Guatemala", value);
			value = new LanguageCodeDef
			{
				Code = "es-HN",
				GoogleCode = "es"
			};
			dictionary.Add("Spanish/Honduras", value);
			value = new LanguageCodeDef
			{
				Code = "es-MX",
				GoogleCode = "es"
			};
			dictionary.Add("Spanish/Mexico", value);
			value = new LanguageCodeDef
			{
				Code = "es-NI",
				GoogleCode = "es"
			};
			dictionary.Add("Spanish/Nicaragua", value);
			value = new LanguageCodeDef
			{
				Code = "es-PA",
				GoogleCode = "es"
			};
			dictionary.Add("Spanish/Panama", value);
			value = new LanguageCodeDef
			{
				Code = "es-PY",
				GoogleCode = "es"
			};
			dictionary.Add("Spanish/Paraguay", value);
			value = new LanguageCodeDef
			{
				Code = "es-PE",
				GoogleCode = "es"
			};
			dictionary.Add("Spanish/Peru", value);
			value = new LanguageCodeDef
			{
				Code = "es-PR",
				GoogleCode = "es"
			};
			dictionary.Add("Spanish/Puerto Rico", value);
			value = new LanguageCodeDef
			{
				Code = "es"
			};
			dictionary.Add("Spanish/Spain", value);
			value = new LanguageCodeDef
			{
				Code = "es-UY",
				GoogleCode = "es"
			};
			dictionary.Add("Spanish/Uruguay", value);
			value = new LanguageCodeDef
			{
				Code = "es-VE",
				GoogleCode = "es"
			};
			dictionary.Add("Spanish/Venezuela", value);
			value = new LanguageCodeDef
			{
				Code = "sw"
			};
			dictionary.Add("Swahili", value);
			value = new LanguageCodeDef
			{
				Code = "sv"
			};
			dictionary.Add("Swedish", value);
			value = new LanguageCodeDef
			{
				Code = "sv-FI",
				GoogleCode = "sv"
			};
			dictionary.Add("Swedish/Finland", value);
			value = new LanguageCodeDef
			{
				Code = "sv-SE",
				GoogleCode = "sv"
			};
			dictionary.Add("Swedish/Sweden", value);
			value = new LanguageCodeDef
			{
				Code = "ta"
			};
			dictionary.Add("Tamil", value);
			value = new LanguageCodeDef
			{
				Code = "tt"
			};
			dictionary.Add("Tatar", value);
			value = new LanguageCodeDef
			{
				Code = "te"
			};
			dictionary.Add("Telugu", value);
			value = new LanguageCodeDef
			{
				Code = "th"
			};
			dictionary.Add("Thai", value);
			value = new LanguageCodeDef
			{
				Code = "tr"
			};
			dictionary.Add("Turkish", value);
			value = new LanguageCodeDef
			{
				Code = "uk"
			};
			dictionary.Add("Ukrainian", value);
			value = new LanguageCodeDef
			{
				Code = "ur"
			};
			dictionary.Add("Urdu", value);
			value = new LanguageCodeDef
			{
				Code = "uz"
			};
			dictionary.Add("Uzbek", value);
			value = new LanguageCodeDef
			{
				Code = "vi"
			};
			dictionary.Add("Vietnamese", value);
			value = new LanguageCodeDef
			{
				Code = "cy"
			};
			dictionary.Add("Welsh", value);
			value = new LanguageCodeDef
			{
				Code = "xh"
			};
			dictionary.Add("Xhosa", value);
			value = new LanguageCodeDef
			{
				Code = "yi"
			};
			dictionary.Add("Yiddish", value);
			value = new LanguageCodeDef
			{
				Code = "zu"
			};
			dictionary.Add("Zulu", value);
			mLanguageDef = dictionary;
		}
	}
}
