using UnityEngine;

public static class Wait
{
	private static WaitForEndOfFrame eof = new WaitForEndOfFrame();

	private static WaitForFixedUpdate fu = new WaitForFixedUpdate();

	public static WaitForEndOfFrame ForEndOfFrame()
	{
		return eof;
	}

	public static WaitForFixedUpdate ForFixedUpdate()
	{
		return fu;
	}
}
