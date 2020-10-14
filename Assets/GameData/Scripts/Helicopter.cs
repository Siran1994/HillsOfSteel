using System;
using System.Collections;
using UnityEngine;

public class Helicopter : Vehicle
{
	[Header("Helicopter")]
	[Range(-360f, 360f)]
	public float minAngle;

	[Range(-360f, 360f)]
	public float maxAngle;

	public float heightOscillationTime = 0.5f;

	public float heightOscillation = 0.5f;

	public float unitsAbovePlayer = 5f;

	public SpriteRenderer mainRotor;

	public SpriteRenderer rearRotor;

	private float targetAngle;

	private float targetSpeed;

	private float targetMovement;

	private float currentSpeed;

	private float currentMovement;

	private Vector2 currentExtra;

	public Vector2 ExtraMove
	{
		get;
		set;
	}

	private void FixedUpdate()
	{
		ExtraMove = Vector2.SmoothDamp(ExtraMove, Vector2.zero, ref currentExtra, 1f, 1000f, Time.fixedDeltaTime);
	}

	private new IEnumerator Start()
	{
		Rigidbody2D rb = GetComponent<Rigidbody2D>();
		float angleVel = 0f;
		float speedVel = 0f;
		float yVel = 0f;
		float movementVel = 0f;
		float time = UnityEngine.Random.Range(0f, (float)Math.PI * 2f);
		float rotorTime = 0f;
		float nextRotorScale = 0.25f;
		float prevRotorScale = 1f;
		float rearAngle = 0f;
		while (true)
		{
			yield return new WaitForFixedUpdate();
			Vector2 position = rb.position + (ExtraMove + new Vector2(currentSpeed * currentMovement, 0f)) * Time.fixedDeltaTime;
			if (TankGame.instance.playerTankContainer != null)
			{
				float num = Mathf.Max(TankGame.instance.playerTankContainer.transform.position.y + unitsAbovePlayer, TankGame.instance.GetGroundHeight(rb.position.x - TankGame.instance.transform.position.x) + 2f);
				position.y = Mathf.SmoothDamp(rb.position.y, num + Mathf.Cos(time * (float)Math.PI * 2f * heightOscillationTime) * heightOscillation, ref yVel, 1f);
				position.y += ExtraMove.y * Time.fixedDeltaTime;
			}
			rb.MovePosition(position);
			float target = (currentMovement < 0f) ? maxAngle : minAngle;
			float smoothTime = 0.5f;
			if (Mathf.Approximately(currentMovement, 0f))
			{
				target = 0f;
				smoothTime = 1f;
			}
			rb.MoveRotation(Mathf.SmoothDampAngle(rb.rotation, target, ref angleVel, smoothTime, float.PositiveInfinity, Time.fixedDeltaTime));
			currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedVel, 1.5f, float.PositiveInfinity, Time.fixedDeltaTime);
			currentMovement = Mathf.SmoothDamp(currentMovement, targetMovement, ref movementVel, 1.5f, float.PositiveInfinity, Time.fixedDeltaTime);
			if (rearRotor != null)
			{
				rearAngle += Time.fixedDeltaTime * 30f * 2.5f;
				rearRotor.transform.rotation = Quaternion.Euler(0f, 0f, 45f + Mathf.PingPong(rearAngle, 25f));
			}
			if (mainRotor != null)
			{
				if (rotorTime > 0.025f)
				{
					rotorTime -= 0.025f;
					float x = nextRotorScale;
					nextRotorScale = prevRotorScale;
					prevRotorScale = mainRotor.transform.localScale.x;
					mainRotor.transform.localScale = new Vector3(x, 1f, 1f);
				}
				rotorTime += Time.fixedDeltaTime;
			}
			time += Time.fixedDeltaTime;
		}
	}

	public override void SetSpeed(float speed, float movement)
	{
		targetSpeed = Mathf.Abs(speed);
		targetMovement = 0f - movement;
	}

	public override void DisableAliveStuff()
	{
		base.DisableAliveStuff();
		UnityEngine.Object.Destroy(GetComponent<Rigidbody2D>());
	}
}
