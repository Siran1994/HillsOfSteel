using System;

[Serializable]
[AuthenticationUnnecessary]
public class AuthenticateWithDeviceIdRequest : BackendRequest
{
	public const string RequestRoute = "authenticateWithDeviceId";

	public string deviceId;

	public override string Route => "authenticateWithDeviceId";
}
