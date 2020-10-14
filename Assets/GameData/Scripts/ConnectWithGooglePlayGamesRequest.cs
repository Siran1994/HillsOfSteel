using System;

[Serializable]
[AuthenticationUnnecessary]
public class ConnectWithGooglePlayGamesRequest : BackendRequest
{
	public const string RequestRoute = "connectWithGooglePlayGames";

	public string playerId;

	public string authCode;

	public override string Route => "connectWithGooglePlayGames";
}
