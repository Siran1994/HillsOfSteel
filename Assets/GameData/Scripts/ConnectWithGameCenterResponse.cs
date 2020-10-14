using System;

[Serializable]
public class ConnectWithGameCenterResponse : BackendResponse
{
	public BackendSessionToken sessionToken;

	public bool newUser;
}
