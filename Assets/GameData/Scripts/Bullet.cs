using System;
using UnityEngine;

[Serializable]
public class Bullet
{
	public BulletObject bulletObject;

	public Transform transform;

	public Transform owner;

	public float damage;

	public float startX;

	public BulletDefinition definition;

	public int cacheIndex;

	public int hitMask;

	public bool explodeDelayed;

	public float timeToLive;
}
