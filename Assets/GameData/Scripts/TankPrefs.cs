using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public static class TankPrefs
{
	private enum SaveLocation
	{
		Primary,
		Secondary
	}

	private static readonly char[] XORHashPreSalt = string.Format("{0}{1}", "x!|y(<X9*6a,]P*a.ymPgXzhVxhCrK06D7Orqa,_fT#$)rRE*a_ifG?z&y)ew=Y", "F]DE8^&8JCqT|}iU;lao#P;s94(^/)xB2Vhqk>5Yb.u8@P-mRMZz;VjW;_R}sam").ToCharArray();

	private static readonly char[] XORHashPostSalt = string.Format("{0}{1}", "=>GHC)Z36,.Qra7J8oN3_mrfyQUbz*E>S<]+SWc^p=WDd(ubXAVCXFR4(=GM10#", "B=DqL.<b50*{6EI_.A7HaMh?b?ocHb%pJ6;ouhG<4|'&@4CNDF53Kpa!nN*=.#Y").ToCharArray();

	public static readonly string SavePath = Application.persistentDataPath + "/hossavedata";

	public static readonly string SavePathAlt = Application.persistentDataPath + "/hossavebackup";

	public static readonly string SavePathBackup = Application.persistentDataPath + "/hossavebackupalt";

	public static readonly string LegacySavePath = Application.persistentDataPath + "/hossavegame";

	private static Dictionary<string, object> data;

	private static Dictionary<string, object> cloudData;

	private static SaveLocation nextSaveLocation;

	private static object fileLock = new object();

	private const int ERROR_HANDLE_DISK_FULL = 39;

	private const int ERROR_DISK_FULL = 112;

	private static StringBuilder localBuilder = new StringBuilder();

	private static StringBuilder cloudBuilder = new StringBuilder();

	private static StringWriter stringWriter;

	private static JsonWriter writer;

	private static StringBuilder sbXOR = new StringBuilder();

	public static bool HasCloudBeenFetched
	{
		get;
		set;
	}

	public static bool CloudSyncComplete
	{
		get;
		set;
	}

	public static bool IsSafeToSendCloud
	{
		get
		{
			if (HasCloudBeenFetched)
			{
				return CloudSyncComplete;
			}
			return false;
		}
	}

	public static bool LocalLoadSucceeded
	{
		get;
		private set;
	}

	public static bool IsInitialized
	{
		get;
		private set;
	}

	public static bool InitializationInProgress
	{
		get;
		private set;
	}

	public static bool SavingEOF
	{
		get;
		set;
	}

	public static DateTime LocalFileTime
	{
		get
		{
			if (data != null)
			{
				return DateTime.Parse((string)data["lastSaved"]);
			}
			return default(DateTime);
		}
	}

	public static DateTime CloudFileTime
	{
		get;
		private set;
	}

	public static void Initialize()
	{
		InitializationInProgress = true;
		TankPrefs_Initialize();
	}

	public static void SetInt(string key, int value)
	{
		TankPrefs_SetInt(key, value);
	}

	public static int GetInt(string key, int defaultValue = 0)
	{
		return TankPrefs_GetInt(key, defaultValue);
	}

	public static void SetString(string key, string value)
	{
		TankPrefs_SetString(key, value);
	}

	public static string GetString(string key, string defaultValue = "")
	{
		string text = TankPrefs_GetString(key);
		if (text == null || text.Length == 0)
		{
			return defaultValue;
		}
		return text;
	}

	public static void Save()
	{
		TankPrefs_Save();
	}

	public static void SaveAtEndOfFrame()
	{
		if (!SavingEOF)
		{
			SavingEOF = true;
		}
	}

	public static void SaveAndSendToCloud(bool forced = false)
	{
		if (forced)
		{
			HasCloudBeenFetched = true;
			CloudSyncComplete = true;
		}
		TankPrefs_SendAndSaveToCloud();
	}

	private static bool ConvertOldSaveFile()
	{
		if (GetInt("saveFileConverted") == 1)
		{
			return false;
		}
		Variables variables = Manager<BackendManager>.instance.variables;
		string[] array = new string[3]
		{
			"Armor",
			"Gun",
			"Tracks"
		};
		Tank[] tanks = variables.tanks;
		foreach (Tank tank in tanks)
		{
			if (GetInt(tank.id + "_unlocked") != 1)
			{
				continue;
			}
			SetInt(tank.id + "Cards", variables.GetMaxTankCardCount(tank.rarity));
			int num = 0;
			for (int j = 0; j != array.Length; j++)
			{
				int @int = GetInt(tank.id + array[j]);
				if (@int > num)
				{
					num = @int;
				}
			}
			SetInt(tank.id + "Level", num);
		}
		SetInt("saveFileConverted", 1);
		Save();
		return true;
	}

	public static void CheckAndCreateLongtermBackup()
	{
		if (LocalLoadSucceeded && PlayerDataManager.BeenInAppBefore)
		{
			try
			{
				if (File.Exists(SavePathBackup))
				{
					File.GetLastWriteTime(SavePathBackup);
					if (File.GetLastWriteTime(SavePathBackup).AddDays(5.0) < DateTime.Now)
					{
						CreateLocalBackup();
					}
				}
				else
				{
					CreateLocalBackup();
				}
			}
			catch (UnauthorizedAccessException)
			{
			}
		}
	}

	private static void TankPrefs_Initialize()
	{
		LoadFromFile(setIsInitialized: true);
	}

	private static void TankPrefs_SetInt(string key, int value)
	{
		data[key] = value;
	}

	private static void TankPrefs_SetString(string key, string value)
	{
		data[key] = value;
	}

	private static int TankPrefs_GetInt(string key, int defaultValue)
	{
		if (data.ContainsKey(key))
		{
			return Convert.ToInt32(data[key]);
		}
		return defaultValue;
	}

	private static string TankPrefs_GetString(string key)
	{
		if (data.ContainsKey(key))
		{
			return (string)data[key];
		}
		return "";
	}

	private static void TankPrefs_Save()
	{
		lock (fileLock)
		{
			data["lastSaved"] = DateTime.Now.ToString();
			FileStream fileStream = File.Open((nextSaveLocation == SaveLocation.Primary) ? SavePath : SavePathAlt, FileMode.Create);
			if (fileStream != null)
			{
				try
				{
					byte[] stringData = GetStringData(Encrypt(SerializeDictionary(data, localBuilder)));
					fileStream.Write(stringData, 0, stringData.Length);
					nextSaveLocation = ((nextSaveLocation == SaveLocation.Primary) ? SaveLocation.Secondary : SaveLocation.Primary);
				}
				catch (Exception e)
				{
					int num = Marshal.GetHRForException(e) & 0xFFFF;
					if (num != 39)
					{
					}
				}
			}
			fileStream.Close();
		}
	}

	private static void CreateLocalBackup()
	{
		try
		{
			using (FileStream fileStream = File.Open(SavePathBackup, FileMode.Create))
			{
				byte[] stringData = GetStringData(Encrypt(SerializeDictionary(data, localBuilder)));
				fileStream.Write(stringData, 0, stringData.Length);
			}
		}
		catch (Exception)
		{
		}
	}

	public static void CheckCloudSyncAndRestore()
	{
		GetCloudSave(delegate(bool result)
		{
			if (result && !CloudSyncComplete)
			{
				MenuController.ShowMenu<CloudBackupPopup>().Init(CloudFileTime.ToString(), delegate
				{
					CloudSyncComplete = true;
				}, delegate
				{
					CloudSyncComplete = true;
					PlayerDataManager.BeenInAppBefore = true;
					MenuController.UpdateTopMenu();
					MenuController.GetMenu<MainMenu>().UpdatePlayMenu();
				});
			}
		});
	}

	private static void CheckCloudSyncStatus()
	{
		if (HasCloudBeenFetched)
		{
			string[] array = new string[17]
			{
				"tank1_unlocked",
				"tank2_unlocked",
				"tank3_unlocked",
				"tank4_unlocked",
				"tank5_unlocked",
				"tank6_unlocked",
				"tank7_unlocked",
				"tank8_unlocked",
				"tank0Cards",
				"tank1Cards",
				"tank2Cards",
				"tank3Cards",
				"tank4Cards",
				"tank5Cards",
				"tank6Cards",
				"tank7Cards",
				"tank8Cards"
			};
			try
			{
				string[] array2 = array;
				foreach (string key in array2)
				{
					if (data.ContainsKey(key) && cloudData.ContainsKey(key))
					{
						if ((int)data[key] < (int)cloudData[key])
						{
							CloudSyncComplete = false;
							return;
						}
					}
					else if (cloudData.ContainsKey(key))
					{
						CloudSyncComplete = false;
						return;
					}
				}
			}
			catch (ArgumentOutOfRangeException)
			{
				CloudSyncComplete = false;
				return;
			}
			UnityEngine.Debug.Log("Cloud sync complete!");
			CloudSyncComplete = true;
		}
	}

	public static void OverrideLocalWithCloudData()
	{
		CloudSyncComplete = true;
		cloudData.Remove("localEncrypted");
		data = cloudData;
		ConvertOldSaveFile();
		TankPrefs_Save();
	}

	public static void GetCloudSave(Action<bool> after)
	{
		Manager<BackendManager>.instance.StartCoroutine(GetCloudSaveRoutine(after));
	}

	private static IEnumerator GetCloudSaveRoutine(Action<bool> after)
	{
		while (!BackendManager.IsAuthenticated)
		{
			yield return null;
		}
		BackendManager.FetchSaveGame(delegate(FetchSaveGameResponse response)
		{
			bool obj = false;
			if (response.error == BackendError.Ok)
			{
				cloudData = DeserializeDictionary(response.saveData.FromBase64());
				if (cloudData != null && cloudData.Count > 0 && cloudData.ContainsKey("lastSaved"))
				{
					CloudFileTime = DateTime.Parse(cloudData["lastSaved"] as string);
					obj = true;
					HasCloudBeenFetched = true;
					CheckCloudSyncStatus();
				}
			}
			if (after != null)
			{
				after(obj);
			}
		});
	}

	private static void TankPrefs_SendAndSaveToCloud()
	{
		TankPrefs_Save();
		if (IsSafeToSendCloud)
		{
			cloudData = data;
			cloudData["localEncrypted"] = 1;
			BackendManager.SendSaveGame(SerializeDictionary(cloudData, cloudBuilder).ToBase64(), delegate(PostSaveGameResponse response)
			{
				if (response.error == BackendError.Ok)
				{
					CloudSyncComplete = true;
				}
				else
				{
					CloudSyncComplete = false;
				}
			});
		}
	}

	public static string SerializeDictionary(Dictionary<string, object> dictionary, StringBuilder stringBuilder)
	{
		stringBuilder.Length = 0;
		stringWriter = new StringWriter(stringBuilder);
		writer = new JsonTextWriter(stringWriter);
		writer.WriteStartObject();
		foreach (string key in dictionary.Keys)
		{
			writer.WritePropertyName(key);
			string value = dictionary[key] as string;
			if (string.IsNullOrEmpty(value))
			{
				try
				{
					int value2 = (int)dictionary[key];
					writer.WriteValue(value2);
				}
				catch (InvalidCastException)
				{
				}
			}
			else
			{
				writer.WriteValue(value);
			}
		}
		writer.WriteEndObject();
		writer.Close();
		stringWriter.Close();
		stringWriter.Dispose();
		return stringBuilder.ToString();
	}

	public static Dictionary<string, object> DeserializeDictionary(string jsonString)
	{
		JsonReader jsonReader = new JsonTextReader(new StringReader(jsonString));
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		string key = "";
		while (jsonReader.Read())
		{
			if (jsonReader.Value != null)
			{
				switch (jsonReader.TokenType)
				{
				case JsonToken.PropertyName:
					key = (jsonReader.Value as string);
					break;
				case JsonToken.Integer:
				{
					int num = Convert.ToInt32(jsonReader.Value);
					dictionary[key] = num;
					break;
				}
				case JsonToken.String:
					dictionary[key] = (jsonReader.Value as string);
					break;
				}
			}
		}
		return dictionary;
	}

	public static bool IsBase64String(this string s)
	{
		s = s.Trim();
		if (s.Length % 4 == 0)
		{
			return Regex.IsMatch(s, "^[a-zA-Z0-9\\+/]*={0,3}$", RegexOptions.None);
		}
		return false;
	}

	private static string GetDataString(byte[] bytes)
	{
		string @string = Encoding.UTF8.GetString(bytes);
		if (@string.IsBase64String())
		{
			return @string.FromBase64();
		}
		return @string;
	}

	private static byte[] GetStringData(string s)
	{
		return Encoding.UTF8.GetBytes(s.ToBase64());
	}

	private static string DecryptAndGetDataString(string filePath)
	{
		try
		{
			FileStream fileStream = File.Open(filePath, FileMode.Open);
			byte[] array = new byte[fileStream.Length];
			fileStream.Read(array, 0, (int)fileStream.Length);
			fileStream.Close();
			string text = GetDataString(array);
			if (text.IsEncrypted())
			{
				text = Decrypt(text);
			}
			return text;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private static Dictionary<string, object> LoadDictionaryFromFile(string path)
	{
		lock (fileLock)
		{
			try
			{
				LocalLoadSucceeded = true;
				return DeserializeDictionary(DecryptAndGetDataString(path));
			}
			catch (Exception)
			{
				LocalLoadSucceeded = false;
			}
			return null;
		}
	}

	private static void LoadFromFile(bool setIsInitialized)
	{
		if (data != null)
		{
			return;
		}
		if (File.Exists(SavePath) && File.Exists(SavePathAlt))
		{
			try
			{
				if (File.GetLastWriteTimeUtc(SavePathAlt) > File.GetLastWriteTimeUtc(SavePath))
				{
					nextSaveLocation = SaveLocation.Primary;
					data = LoadDictionaryFromFile(SavePathAlt);
				}
			}
			catch (UnauthorizedAccessException)
			{
			}
		}
		if (data == null)
		{
			nextSaveLocation = SaveLocation.Secondary;
			data = LoadDictionaryFromFile(SavePath);
		}
		if (data == null)
		{
			nextSaveLocation = SaveLocation.Primary;
			data = LoadDictionaryFromFile(SavePathAlt);
		}
		if (data == null)
		{
			data = LoadDictionaryFromFile(SavePathBackup);
		}
		if (data == null)
		{
			data = LoadDictionaryFromFile(LegacySavePath);
			if (data != null)
			{
				ConvertOldSaveFile();
			}
		}
		if (data == null)
		{
			data = new Dictionary<string, object>();
		}
		IsInitialized = setIsInitialized;
		InitializationInProgress = setIsInitialized;
	}

	public static bool IsEncrypted(this string data)
	{
		try
		{
			DeserializeDictionary(Decrypt(data));
		}
		catch
		{
			return false;
		}
		return true;
	}

	public static string Encrypt(string input)
	{
		return XOR(XOR(input, XORHashPreSalt), XORHashPostSalt);
	}

	public static string Decrypt(string input)
	{
		return XOR(XOR(input, XORHashPostSalt), XORHashPreSalt);
	}

	private static string XOR(string input, char[] salt)
	{
		sbXOR.Length = 0;
		char[] array = input.ToCharArray();
		for (int i = 0; i != input.Length; i++)
		{
			uint num = array[i];
			uint num2 = salt[i % salt.Length];
			sbXOR.Append((char)(num ^ num2));
		}
		return sbXOR.ToString();
	}
}
