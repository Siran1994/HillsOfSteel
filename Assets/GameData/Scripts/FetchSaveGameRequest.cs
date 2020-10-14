using System;

[Serializable]
public class FetchSaveGameRequest : BackendRequest
{
	public const string RequestRoute = "fetchSaveGame";

	public override string Route => "fetchSaveGame";
}
