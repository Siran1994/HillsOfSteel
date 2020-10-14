using System;

[Serializable]
[AuthenticationUnnecessary]
public class ConnectWithGameCenterRequest : BackendRequest
{
	public const string RequestRoute = "connectWithGameCenter";

	public string playerId;

	public string bundleId;

	public string publicKeyUrl;

	public string signature;

	public string salt;

	public ulong timeStamp;

	public override string Route => "connectWithGameCenter";
}
