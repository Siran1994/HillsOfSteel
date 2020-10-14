using System;

[Serializable]
public class MultiChallengeRequest : BackendRequest
{
	public const string RequestRoute = "challengeMulti";

	public int rating;

	public int count;

	public override string Route => "challengeMulti";
}
