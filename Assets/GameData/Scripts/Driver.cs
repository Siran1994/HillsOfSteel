using System;
using UnityEngine;
#pragma warning disable 0649
public class Driver : MonoBehaviour
{
	[Serializable]
	public class Uniform
	{
		public Sprite hat;

		public Sprite torso;

		public Sprite arm;

		public Sprite leg;

		public Sprite foot;
	}

	[SerializeField]
	private SpriteRenderer hatRenderer;

	[SerializeField]
	private SpriteRenderer torsoRenderer;

	[SerializeField]
	private SpriteRenderer ragdollHatRenderer;

	[SerializeField]
	private SpriteRenderer ragdollTorsoRenderer;

	[SerializeField]
	private SpriteRenderer ragdollArmRenderer;

	[SerializeField]
	private SpriteRenderer ragdollLegRenderer;

	[SerializeField]
	private SpriteRenderer ragdollFootRenderer;

	public void SetUniform(Uniform sprites)
	{
		if (sprites.hat != null)
		{
			hatRenderer.sprite = sprites.hat;
			ragdollHatRenderer.sprite = sprites.hat;
		}
		if (sprites.torso != null)
		{
			torsoRenderer.sprite = sprites.torso;
			ragdollTorsoRenderer.sprite = sprites.torso;
		}
		if (sprites.arm != null)
		{
			ragdollArmRenderer.sprite = sprites.arm;
		}
		if (sprites.leg != null)
		{
			ragdollLegRenderer.sprite = sprites.leg;
		}
		if (sprites.foot != null)
		{
			ragdollFootRenderer.sprite = sprites.foot;
		}
	}
}
