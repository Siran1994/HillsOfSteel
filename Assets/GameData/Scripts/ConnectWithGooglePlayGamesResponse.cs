using System;

[Serializable]
public class ConnectWithGooglePlayGamesResponse : BackendResponse
{
	public BackendSessionToken sessionToken;

	public bool newUser;
}
