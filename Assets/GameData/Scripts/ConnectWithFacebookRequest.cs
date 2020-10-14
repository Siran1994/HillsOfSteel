using System;

[Serializable]
[AuthenticationUnnecessary]
public class ConnectWithFacebookRequest : BackendRequest
{
	public const string RequestRoute = "connectWithFacebook";

	public DateTime expirationTime;

	public string facebookTokenString;

	public string userId;

	public override string Route => "connectWithFacebook";
}
