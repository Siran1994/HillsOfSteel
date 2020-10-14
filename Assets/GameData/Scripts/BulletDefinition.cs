using System;
using UnityEngine;

[Serializable]
public class BulletDefinition
{
	public BulletType type;

	public string shootAudioName;

	public string shootStartAudioName;

	public string shootEndAudioName;

	public GameObject particleEffectShoot;

	public GameObject particleEffectBulletHitGround;

	public GameObject particleEffectBulletHitTank;

	[Header("Cannon & Grenade & Missile")]
	public GameObject prefab;

	public float speed;

	public float bulletGravityScale = 2f;

	public float recoil = 400f;

	public float hitForce = 25f;

	public float damageRange = 10f;

	[Header("Missile")]
	public float perMissileTime = 0.25f;

	public float angleRandomization = 15f;

	[Header("Grenade")]
	public float explosionDelay = 1f;

	[Header("Bullet")]
	public float warmupTime;

	public float shootTime = 0.1f;

	public float targetVarianceMin = -0.025f;

	public float targetVarianceMax = 0.025f;

	[Header("Bullet & Flame")]
	public float maxDistance = 40f;

	[Header("Bullet & Missile & Flame")]
	public float maxTime = 3f;

	[Header("Laser")]
	public bool preventSpriteRotation;
}
