using System;

[Serializable]
public class DisconnectFromFacebookRequest : BackendRequest
{
	public const string RequestRoute = "disconnectFromFacebook";

	public override string Route => "disconnectFromFacebook";
}
