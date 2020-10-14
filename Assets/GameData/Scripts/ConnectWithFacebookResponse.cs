using System;

[Serializable]
public class ConnectWithFacebookResponse : BackendResponse
{
	public BackendSessionToken sessionToken;

	public bool newUser;
}
