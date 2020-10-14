using System;

[Serializable]
public class BackendSessionToken
{
	public ulong sequence;

	public string token;

	public string expirationTimeUtc;
}
