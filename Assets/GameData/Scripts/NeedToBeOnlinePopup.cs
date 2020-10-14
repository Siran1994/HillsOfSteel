using System;
using UnityEngine;

public class NeedToBeOnlinePopup : MenuBase<NeedToBeOnlinePopup>
{
	public static void DefaultAction()
	{
		OnlineAction();
	}

	public static void OnlineAction(Action onlineAction = null)
	{
		if (Application.internetReachability.Equals(NetworkReachability.NotReachable))
		{
			MenuController.ShowMenu<NeedToBeOnlinePopup>();
		}
		else
		{
			onlineAction?.Invoke();
		}
	}
}
