using System;
using System.Collections;
using UnityEngine;

public class TankContainer : Vehicle
{
	[Space(20f)]
	[Header("Tank Container")]
	public PlayerTankType tankType;

	public Transform airControlPointBack;

	[Header("Skinned sprites")]
	public SpriteRenderer[] gunSprite;

	public SpriteRenderer[] gunRecoilSprite;

	public SpriteRenderer[] turretSprite;

	public SpriteRenderer[] chassisLowerSprite;

	public SpriteRenderer[] chassisTopSprite;

	public SpriteRenderer[] cogwheelSprite;

	public SpriteRenderer[] smallWheelSprite;

	public SpriteRenderer[] wheelSprite;

	public SpriteRenderer[] suspensionSprite;

	[Header("Non-joint, non-chain components to flip")]
	public Transform[] componentsToFlip;

	public static int scrapCount;

	public Rigidbody2D Rigidbody
	{
		get;
		protected set;
	}

	public bool Flipped
	{
		get;
		protected set;
	}

	protected new void Awake()
	{
		base.Awake();
		Rigidbody = GetComponent<Rigidbody2D>();
	}

	public void SetSkin(TankSkin skin)
	{
		Action<SpriteRenderer[], Sprite> obj = delegate(SpriteRenderer[] renderers, Sprite sprite)
		{
			if (renderers.Length != 0 && sprite != null)
			{
				for (int i = 0; i < renderers.Length; i++)
				{
					renderers[i].sprite = sprite;
				}
			}
		};
		obj(gunSprite, skin.gun);
		obj(gunRecoilSprite, skin.gunRecoil);
		obj(turretSprite, skin.turret);
		obj(chassisLowerSprite, skin.chassisLower);
		obj(chassisTopSprite, skin.chassisTop);
		obj(cogwheelSprite, skin.cogwheel);
		obj(smallWheelSprite, skin.smallWheel);
		obj(wheelSprite, skin.wheel);
		obj(suspensionSprite, skin.suspension);
	}

	public override void DisableAliveStuff()
	{
		base.DisableAliveStuff();
		SetFumeEmission(0f);
		SetDustEmission(0f, 0f);
	}

	public void SetFumeEmission(float emission)
	{
		for (int i = 0; i < fumes.Length; i++)
		{
			if (fumes[i] != null)
			{
				ParticleSystem.EmissionModule emissionModule = fumes[i].emission;
				emissionModule.rateOverTime = new ParticleSystem.MinMaxCurve(emission * fumeMultiplier);
			}
		}
	}

	public override void SetDustColors(Color[] colors)
	{
		for (int i = 0; i < dustParticles.Length; i++)
		{
			if (!(dustParticles[i] == null))
			{
				ParticleSystem.MainModule mainModule = dustParticles[i].main;
				mainModule.startColor = new ParticleSystem.MinMaxGradient(colors[i]);
			}
		}
	}

	public void SetDustEmission(float emission, float xMultiplier)
	{
		if (!Physics2D.Raycast(base.transform.position, -base.transform.up, dustRaycastDistance, 1 << LayerMask.NameToLayer("Ground")).collider)
		{
			emission = 0f;
		}
		for (int i = 0; i < dustParticles.Length; i++)
		{
			if (!(dustParticles[i] == null))
			{
				ParticleSystem.EmissionModule emissionModule = dustParticles[i].emission;
				emissionModule.rateOverTime = new ParticleSystem.MinMaxCurve(emission);
				ParticleSystem.VelocityOverLifetimeModule velocityOverLifetimeModule = dustParticles[i].velocityOverLifetime;
				velocityOverLifetimeModule.xMultiplier = xMultiplier;
			}
		}
	}

	public void HideMuzzles()
	{
		for (int i = 0; i < bulletMuzzles.Length; i++)
		{
			bulletMuzzles[i].enabled = false;
		}
		for (int j = 0; j < cannons.Length; j++)
		{
			cannons[j].transform.localPosition = cannonPositions[j];
		}
	}

	public void ResetCannon()
	{
		if (cannonRoutine != null)
		{
			StopCoroutine(cannonRoutine);
			cannons[0].transform.localPosition = cannonPos;
			cannonRoutine = null;
		}
	}

	public override void DoCannonMove(float time)
	{
		if (cannons.Length != 0 && cannonRoutine == null)
		{
			cannonRoutine = StartCoroutine(CannonLogic(time));
		}
	}

	public override void AddRecoil(Vector2 force)
	{
		if ((bool)recoilRigidbody)
		{
			recoilRigidbody.AddForce(force);
		}
	}

	protected IEnumerator CannonLogic(float totalTime)
	{
		float time2 = 0f;
		cannonPos = cannons[0].transform.localPosition;
		while (time2 < totalTime * 0.05f)
		{
			if (cannons[0] != null)
			{
				cannons[0].transform.localPosition = Vector3.Lerp(cannonPos, cannonPos + new Vector3(cannonOffset, 0f, 0f), time2 / (totalTime * 0.05f));
			}
			time2 += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		time2 = 0f;
		while (time2 < totalTime * 0.95f)
		{
			if (cannons[0] != null)
			{
				cannons[0].transform.localPosition = Vector3.Lerp(cannonPos + new Vector3(cannonOffset, 0f, 0f), cannonPos, time2 / (totalTime * 0.95f));
			}
			time2 += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		if (cannons[0] != null)
		{
			cannons[0].transform.localPosition = cannonPos;
		}
		cannonRoutine = null;
	}

	public override void SetSpeed(float speed, float movement)
	{
		for (int i = 0; i < wheelJoints.Length; i++)
		{
			WheelJoint2D wheelJoint2D = wheelJoints[i];
			if ((bool)wheelJoint2D)
			{
				JointSuspension2D suspension = wheelJoint2D.suspension;
				suspension.frequency = Variables.instance.wheelSuspensionFrequency;
				suspension.dampingRatio = Variables.instance.wheelSuspensionDampening;
				wheelJoint2D.suspension = suspension;
				JointMotor2D motor = wheelJoint2D.motor;
				motor.motorSpeed = speed;
				wheelJoint2D.motor = motor;
			}
		}
		for (int j = 0; j < chains.Length; j++)
		{
			chains[j].wheelSpeed = speed;
		}
		float emission = 0f;
		if (movement != 0f)
		{
			emission = Mathf.Lerp(0f, Variables.instance.maxDustPartices, Mathf.Clamp01(1f - Mathf.Abs(speed) / Variables.instance.maxDustParticleSpeed));
		}
		SetDustEmission(emission, movement * 10f);
		SetFumeEmission(Variables.instance.defaultFumes + Variables.instance.speedFumes * Mathf.Abs(movement));
		if (audioSourceEngine != null)
		{
			audioSourceEngine.pitch = Mathf.Clamp(audioSourceEngine.pitch + (float)((movement != 0f) ? 1 : (-2)) * Variables.instance.engineSoundAlteringSpeed * Time.deltaTime, 1f, (tankType == PlayerTankType.Arachno) ? GetComponent<Walker>().engineMaxPitch : Variables.instance.engineMaxPitch);
			audioSourceEngine.volume = Mathf.Clamp(audioSourceEngine.volume + (float)((movement != 0f) ? 1 : (-2)) * Variables.instance.engineSoundAlteringSpeed * Time.deltaTime, (tankType == PlayerTankType.Arachno) ? 0f : Variables.instance.engineMinVolume, Variables.instance.engineMaxVolume);
		}
	}

	public void Flip()
	{
		Flipped = !Flipped;
		base.transform.localRotation *= Quaternion.Euler(0f, 180f, 0f);
		HingeJoint2D[] componentsInChildren = GetComponentsInChildren<HingeJoint2D>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Vector2 vector = componentsInChildren[i].connectedAnchor;
			vector.x = 0f - vector.x;
			componentsInChildren[i].connectedAnchor = vector;
			vector = componentsInChildren[i].anchor;
			vector.x = 0f - vector.x;
			componentsInChildren[i].anchor = vector;
		}
		SpringJoint2D[] componentsInChildren2 = GetComponentsInChildren<SpringJoint2D>(includeInactive: true);
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			Vector2 vector2 = componentsInChildren2[j].connectedAnchor;
			vector2.x = 0f - vector2.x;
			componentsInChildren2[j].connectedAnchor = vector2;
			vector2 = componentsInChildren2[j].anchor;
			vector2.x = 0f - vector2.x;
			componentsInChildren2[j].anchor = vector2;
		}
		WheelJoint2D[] componentsInChildren3 = GetComponentsInChildren<WheelJoint2D>(includeInactive: true);
		for (int k = 0; k < componentsInChildren3.Length; k++)
		{
			Vector2 vector3 = componentsInChildren3[k].connectedAnchor;
			vector3.x = 0f - vector3.x;
			componentsInChildren3[k].connectedAnchor = vector3;
			vector3 = componentsInChildren3[k].anchor;
			vector3.x = 0f - vector3.x;
			componentsInChildren3[k].anchor = vector3;
		}
		for (int l = 0; l < chains.Length; l++)
		{
			chains[l].transform.parent.localRotation *= Quaternion.Euler(0f, 180f, 0f);
			chains[l].Flip();
		}
		if (componentsToFlip != null)
		{
			for (int m = 0; m < componentsToFlip.Length; m++)
			{
				componentsToFlip[m].localRotation *= Quaternion.Euler(0f, 180f, 0f);
				Vector3 localPosition = componentsToFlip[m].localPosition;
				localPosition.x = 0f - localPosition.x;
				componentsToFlip[m].localPosition = localPosition;
			}
		}
	}
}
