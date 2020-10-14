using System;

[Serializable]
public class MultiChallengeResponse : BackendResponse
{
	public Challenge[] payload;
}
