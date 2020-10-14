using System;

[Serializable]
public class PostSaveGameRequest : BackendRequest
{
	public const string RequestRoute = "postSaveGame";

	public string saveData;

	public override string Route => "postSaveGame";
}
