using System;

[Serializable]
public class AuthenticateWithDeviceIdResponse : BackendResponse
{
	public BackendSessionToken sessionToken;

	public bool newUser;
}
