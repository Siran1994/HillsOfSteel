using System;

[Serializable]
public class ChallengeRequest : BackendRequest
{
	public const string RequestRoute = "challenge";

	public int rating;

	public override string Route => "challenge";
}
