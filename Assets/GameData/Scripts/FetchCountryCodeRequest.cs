using System;

[Serializable]
public class FetchCountryCodeRequest : BackendRequest
{
	public const string RequestRoute = "fetchCountryCode";

	public override string Route => "fetchCountryCode";
}
