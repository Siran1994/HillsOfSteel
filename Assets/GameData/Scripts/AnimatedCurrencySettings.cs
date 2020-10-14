using System;
using UnityEngine;

public struct AnimatedCurrencySettings
{
	public Action<int> onLeave;

	public Action<int> onArrive;

	public int count;

	public int countPerSprite;

	public string audioName;

	public Sprite icon;

	public Vector3 fromViewportSpace;

	public Vector3 toViewportSpace;
}
