using System;

[Serializable]
public class PostChallengeRequest : BackendRequest
{
	public const string RequestRoute = "postChallenge";

	public Challenge payload;

	public int versusMode = 1;

	public override string Route => "postChallenge";
}
