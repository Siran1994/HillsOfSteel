using System;

[Serializable]
public class AdminInsertChallengeRequest : BackendRequest
{
	public const string RequestRoute = "adminForceInsertChallenge";

	public Challenge payload;

	public override string Route => "adminForceInsertChallenge";
}
