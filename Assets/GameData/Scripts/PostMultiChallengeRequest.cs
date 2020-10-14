using System;

[Serializable]
public class PostMultiChallengeRequest : BackendRequest
{
	public const string RequestRoute = "postMultiChallenge";

	public Challenge payload;

	public int versusMode;

	public override string Route => "postMultiChallenge";
}
