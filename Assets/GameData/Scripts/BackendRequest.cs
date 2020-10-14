using System;

[Serializable]
public abstract class BackendRequest
{
	public BackendSessionToken token;

	public abstract string Route
	{
		get;
	}
}
