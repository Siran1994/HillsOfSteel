using System;
using System.Net.Sockets;
using UnityEngine;

public class WaitForSocketAsync : CustomYieldInstruction
{
	private bool _completed;

	public override bool keepWaiting => !_completed;

	private void Completed(object o, SocketAsyncEventArgs e)
	{
		_completed = true;
	}

	public WaitForSocketAsync(Func<SocketAsyncEventArgs, bool> operation, SocketAsyncEventArgs e)
	{
		e.Completed += Completed;
		_completed = !operation(e);
	}
}
