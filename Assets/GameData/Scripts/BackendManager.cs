using GooglePlayGames;
using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class BackendManager : Manager<BackendManager>
{
	private const string SessionTokenKey = "sessionTokenKey";

	public Variables variables;

	private const string ServerAddress = "https://hillsofsteel.azurewebsites.net/api/hos/";

	private BackendSessionToken _sessionToken;

	private Coroutine pingServerRoutine;

	public static bool IsAuthenticated
	{
		get
		{
			if (Manager<BackendManager>.instance._sessionToken != null)
			{
				try
				{
					if (DateTime.Parse(Manager<BackendManager>.instance._sessionToken.expirationTimeUtc) > DateTime.UtcNow)
					{
						return true;
					}
				}
				catch (Exception)
				{
					return false;
				}
			}
			return false;
		}
	}

	public static bool IsRequestingChallenge
	{
		get;
		private set;
	}

	public static bool HasConnected
	{
		get;
		private set;
	}

	public static bool ConnectedWithFacebook
	{
		get
		{
			//if (FB.IsLoggedIn)
			//{
			//	return !string.IsNullOrEmpty(PlayerPrefs.GetString("facebookId", ""));
			//}
			return false;
		}
	}

	public string ToJson<T>(T request)
	{
		return JsonUtility.ToJson(request);
	}

	public byte[] ToPostData(string json)
	{
		return Encoding.UTF8.GetBytes(json);
	}

	private void EraseToken()
	{
		_sessionToken = null;
		PlayerPrefs.SetString("sessionTokenKey", "");
	}

	public void Initialize()
	{
		Manager<BackendManager>.instance = this;
		pingServerRoutine = StartCoroutine(PingServer());
		InitFacebook();
		string @string = PlayerPrefs.GetString("sessionTokenKey");
		if (!string.IsNullOrEmpty(@string))
		{
			_sessionToken = JsonUtility.FromJson<BackendSessionToken>(@string);
		}
		if (!IsAuthenticated && PlayerPrefs.GetInt("explicitSignOut") == 0)
		{
			_sessionToken = null;
			StartCoroutine(LoginAfterInitialization());
		}
		else if (!TankPrefs.IsInitialized || !TankPrefs.LocalLoadSucceeded)
		{
			StartCoroutine(LoginAfterInitialization());
		}
	}

	private IEnumerator LoginAfterInitialization()
	{
		while (!TankPrefs.IsInitialized || TankPrefs.GetInt("privacyPolicyAgreed") == 0)
		{
			yield return null;
		}
		ConnectWithPlayGamesServices();
		LoadingScreen.AddProgress(0.1f);
	}

	private IEnumerator PingServer()
	{
		while (true)
		{
			UnityWebRequest www = new UnityWebRequest("https://hillsofsteel.azurewebsites.net/api/hos/")
			{
				timeout = 10
			};
			yield return www.SendWebRequest();
			if (www.isNetworkError)
			{
				HasConnected = false;
				yield return new WaitForSeconds(2f);
			}
			else
			{
				HasConnected = true;
				yield return new WaitForSeconds(60f);
			}
		}
	}

	public static void ConnectWithPlayGamesServices()
	{
		Manager<BackendManager>.instance.StartCoroutine(ConnectWithPlayGamesServicesRoutine());
	}

	public static IEnumerator ConnectWithPlayGamesServicesRoutine()
	{
		while (!Social.localUser.authenticated)
		{
			yield return null;
		}
		ConnectWithGooglePlayGamesRequest connectWithGooglePlayGamesRequest = new ConnectWithGooglePlayGamesRequest();
		ConnectWithGooglePlayGamesResponse response2 = new ConnectWithGooglePlayGamesResponse();
		connectWithGooglePlayGamesRequest.playerId = Social.localUser.id;
//		connectWithGooglePlayGamesRequest.authCode = PlayGamesPlatform.Instance.GetServerAuthCode();
		Manager<BackendManager>.instance.SendRequest(connectWithGooglePlayGamesRequest, response2, delegate(ConnectWithGooglePlayGamesResponse response)
		{
			if (response.error == BackendError.Ok)
			{
				UnityEngine.Debug.Log("Google Play Games authentication succeeded!");
				Manager<BackendManager>.instance._sessionToken = response.sessionToken;
				PlayerPrefs.SetString("sessionTokenKey", JsonUtility.ToJson(response.sessionToken));
				HandleIsNewUser(response.newUser);
			}
			else
			{
				UnityEngine.Debug.LogError("Google Play Games authentication failed!");
				MenuController.ShowMenu<ConnectionErrorPopup>();
			}
		});
	}

	public static void InitFacebook()
	{
		//if (!FB.IsInitialized)
		//{
		//	FB.Init(delegate
		//	{
		//		if (FB.IsInitialized)
		//		{
		//			FB.ActivateApp();
		//		}
		//	}, delegate
		//	{
		//	});
		//}
		//else
		//{
		//	FB.ActivateApp();
		//}
	}

	public static void ConnectWithFacebook(Action onSuccess = null, Action onFailure = null)
	{
		Manager<BackendManager>.instance.StartCoroutine(LoginWithFacebook(onSuccess, onFailure));
	}

	private static IEnumerator LoginWithFacebook(Action onSuccess = null, Action onFailure = null)
	{
		yield return null;
		//FB.LogInWithReadPermissions(null, delegate(ILoginResult result)
		//{
		//	if (!string.IsNullOrEmpty(result.Error))
		//	{
		//		UnityEngine.Debug.LogErrorFormat("Error when connecting with facebook: {0}", result.Error);
		//		MenuController.ShowMenu<ConnectionErrorPopup>();
		//		if (onFailure != null)
		//		{
		//			onFailure();
		//		}
		//	}
		//	else if (!result.Cancelled)
		//	{
		//		TankAnalytics.NewAttributionEvent(TankAnalytics.AttributionEvent.PlatformConnected, null, "Facebook");
		//		ConnectWithFacebookRequest connectWithFacebookRequest = new ConnectWithFacebookRequest();
		//		ConnectWithFacebookResponse response2 = new ConnectWithFacebookResponse();
		//		connectWithFacebookRequest.expirationTime = result.AccessToken.ExpirationTime;
		//		connectWithFacebookRequest.facebookTokenString = result.AccessToken.TokenString;
		//		connectWithFacebookRequest.userId = result.AccessToken.UserId;
		//		PlayerPrefs.SetString("facebookId", result.AccessToken.UserId);
		//		Manager<BackendManager>.instance.SendRequest(connectWithFacebookRequest, response2, delegate(ConnectWithFacebookResponse response)
		//		{
		//			if (response.error == BackendError.Ok)
		//			{
		//				UnityEngine.Debug.Log("Successfully connected to Facebook.");
		//				if (!IsAuthenticated && !string.IsNullOrEmpty(response.sessionToken.token))
		//				{
		//					Manager<BackendManager>.instance._sessionToken = response.sessionToken;
		//					PlayerPrefs.SetString("sessionTokenKey", JsonUtility.ToJson(response.sessionToken));
		//				}
		//				if (onSuccess != null)
		//				{
		//					onSuccess();
		//				}
		//			}
		//			else
		//			{
		//				UnityEngine.Debug.LogError("Error when connecting with facebook.");
		//				MenuController.ShowMenu<ConnectionErrorPopup>();
		//				if (onFailure != null)
		//				{
		//					onFailure();
		//				}
		//			}
		//		});
		//	}
		//	else if (onFailure != null)
		//	{
		//		onFailure();
		//	}
		//});
	}

	public static void DisconnectWithFacebook()
	{
		PlayerPrefs.SetString("facebookId", "");
		//FB.LogOut();
		DisconnectFromFacebookRequest request = new DisconnectFromFacebookRequest();
		BackendResponse response2 = new BackendResponse();
		Manager<BackendManager>.instance.SendRequest(request, response2, delegate
		{
		});
	}

	private static void HandleIsNewUser(bool isNewUser)
	{
		if (isNewUser)
		{
			TankPrefs.HasCloudBeenFetched = true;
			TankPrefs.SetInt("whatsNewSeen", 1);
			TankPrefs.SaveAtEndOfFrame();
		}
		else
		{
			TankPrefs.GetCloudSave(delegate(bool result)
			{
				if (result)
				{
					if ((TankPrefs.LocalLoadSucceeded && !PlatformManager.ReconnectingWithGooglePlay) || TankPrefs.CloudSyncComplete)
					{
						return;
					}
					Time.timeScale = 0f;
					MenuController.ShowMenu<CloudBackupPopup>().Init(TankPrefs.CloudFileTime.ToString(), delegate
					{
						TankGame.Running = true;
						Time.timeScale = 1f;
						TankPrefs.CloudSyncComplete = true;
					}, delegate
					{
						PlayerDataManager.BeenInAppBefore = true;
						Time.timeScale = 1f;
						if (TankGame.Running)
						{
							TankGame.Running = false;
							MenuController.HideMenu<GameMenu>();
							LoadingScreen.ReloadGame(delegate
							{
								MenuController.ShowMenu<MainMenu>().UpdatePlayMenu();
							});
						}
						else
						{
							MenuController.UpdateTopMenu();
							MenuController.GetMenu<MainMenu>().UpdatePlayMenu();
						}
					});
				}
				else
				{
					UnityEngine.Debug.LogError("Cloud save not yet gotten, will try again!");
					HandleIsNewUser(isNewUser);
				}
				PlatformManager.ReconnectingWithGooglePlay = false;
			});
		}
	}

	public static void GetChallenge(int rating)
	{
		PlayerDataManager.ArenaMatchData = null;
		IsRequestingChallenge = true;
		rating = Mathf.Min(999, rating);
		ChallengeRequest challengeRequest = new ChallengeRequest();
		challengeRequest.rating = rating;
		ChallengeResponse response2 = new ChallengeResponse();
		Manager<BackendManager>.instance.SendRequest(challengeRequest, response2, delegate(ChallengeResponse response)
		{
			if (response.error == BackendError.Ok)
			{
				UnityEngine.Debug.Log("Challenge found successfully! " + response.payload.name + " " + response.payload.actualRating);
				PlayerDataManager.ArenaMatchData = new ArenaMatchData();
				PlayerDataManager.ArenaMatchData.arenaPayload = response.payload;
			}
			else
			{
				if (response.error == BackendError.InvalidToken)
				{
					UnityEngine.Debug.LogError("Players token is not valid");
				}
				else if (response.error == BackendError.NoChallengeFound)
				{
					UnityEngine.Debug.LogError("No actual challenge was found.");
				}
				UnityEngine.Debug.LogError("Challenge was not found!");
			}
			IsRequestingChallenge = false;
		});
	}

	public static void GetChallenges(int rating, int count)
	{
		PlayerDataManager.ArenaMultiMatchData = null;
		IsRequestingChallenge = true;
		rating = Mathf.Min(999, rating);
		MultiChallengeRequest request = new MultiChallengeRequest
		{
			rating = rating,
			count = count
		};
		MultiChallengeResponse response2 = new MultiChallengeResponse();
		Manager<BackendManager>.instance.SendRequest(request, response2, delegate(MultiChallengeResponse response)
		{
			if (response.error == BackendError.Ok)
			{
				PlayerDataManager.ArenaMultiMatchData = new ArenaMultiMatchData
				{
					arenaPayload = response.payload
				};
			}
			else
			{
				if (response.error == BackendError.InvalidToken)
				{
					UnityEngine.Debug.LogError("Players token is not valid");
				}
				else if (response.error == BackendError.NoChallengeFound)
				{
					UnityEngine.Debug.LogError("Not enough challenges in the database.");
				}
				UnityEngine.Debug.LogError("Challenge was not found! " + response.error);
			}
			IsRequestingChallenge = false;
		});
	}

	public static void SendChallenge(Challenge challenge, GameMode mode = GameMode.Arena)
	{
		PostChallengeRequest postChallengeRequest = new PostChallengeRequest
		{
			payload = challenge,
			versusMode = ((mode != GameMode.Arena2v2) ? 1 : 2)
		};
		postChallengeRequest.payload.rating = Mathf.Min(999, challenge.rating);
		PostChallengeResponse response2 = new PostChallengeResponse();
		Manager<BackendManager>.instance.SendRequest(postChallengeRequest, response2, delegate(PostChallengeResponse response)
		{
			if (response.error == BackendError.Ok)
			{
				UnityEngine.Debug.Log("Challenge sent successfully!");
			}
			else
			{
				UnityEngine.Debug.LogErrorFormat("Got an error from server: {0}", response.error.ToString());
			}
		});
	}

	public static void SendSaveGame(string saveData, Action<PostSaveGameResponse> callback)
	{
		PostSaveGameRequest postSaveGameRequest = new PostSaveGameRequest();
		PostSaveGameResponse response = new PostSaveGameResponse();
		postSaveGameRequest.saveData = saveData;
		Manager<BackendManager>.instance.SendRequest(postSaveGameRequest, response, callback);
	}

	public static void FetchSaveGame(Action<FetchSaveGameResponse> callback)
	{
		FetchSaveGameRequest request = new FetchSaveGameRequest();
		FetchSaveGameResponse response = new FetchSaveGameResponse();
		Manager<BackendManager>.instance.SendRequest(request, response, callback);
	}

	public static void FetchCountryCode(Action<FetchCountryCodeResponse> callback)
	{
		Manager<BackendManager>.instance.SendRequest(new FetchCountryCodeRequest(), new FetchCountryCodeResponse(), callback);
	}

	public void SendRequest<RequestType, ResponseType>(RequestType request, ResponseType response, Action<ResponseType> callback) where RequestType : BackendRequest where ResponseType : BackendResponse
	{
		response.error = BackendError.UnknownError;
		if ((AuthenticationUnnecessaryAttribute)Attribute.GetCustomAttribute(typeof(RequestType), typeof(AuthenticationUnnecessaryAttribute)) == null && !IsAuthenticated)
		{
			response.error = BackendError.RequestNeedsAuthentication;
			callback(response);
			return;
		}
		request.token = _sessionToken;
		string text = ToJson(request);
		UnityEngine.Debug.LogFormat("Sending request json: {0}", text);
		byte[] data = ToPostData(text);
		UnityWebRequest unityWebRequest = new UnityWebRequest("https://hillsofsteel.azurewebsites.net/api/hos/" + request.Route, "POST", new DownloadHandlerBuffer(), new UploadHandlerRaw(data));
		unityWebRequest.SetRequestHeader("Content-Type", "application/json");
		unityWebRequest.SetRequestHeader("Accept", "application/json");
		StartCoroutine(SendRequestRoutine(unityWebRequest, request, response, callback));
	}

	private IEnumerator SendRequestRoutine<RequestType, ResponseType>(UnityWebRequest www, RequestType request, ResponseType response, Action<ResponseType> callback) where RequestType : BackendRequest where ResponseType : BackendResponse
	{
		yield return www.SendWebRequest();
		try
		{
			JsonUtility.FromJsonOverwrite(www.downloadHandler.text, response);
			if (response.error == BackendError.InvalidToken)
			{
				EraseToken();
				Initialize();
			}
			else if (response.error == BackendError.Ok)
			{
				HasConnected = true;
				if (pingServerRoutine != null)
				{
					StopCoroutine(pingServerRoutine);
					pingServerRoutine = null;
				}
			}
		}
		catch (Exception)
		{
			UnityEngine.Debug.LogErrorFormat("Could not handle response from backend. Text was {0}", www.downloadHandler.text);
		}
		if (response.error == BackendError.InvalidToken)
		{
			while (!IsAuthenticated)
			{
				yield return null;
			}
			SendRequest(request, response, callback);
		}
		callback?.Invoke(response);
	}
}
