using System;
using UnityEngine;

[ExecuteInEditMode]
public class particleColorChanger : MonoBehaviour
{
	[Serializable]
	public class colorChange
	{
		public string Name;

		public ParticleSystem[] colored_ParticleSystem;

		public Gradient customer_Gradient;
	}

	public colorChange[] colorChangeList;

	private void Update()
	{
		for (int i = 0; i < colorChangeList.Length; i++)
		{
			for (int j = 0; j < colorChangeList[i].colored_ParticleSystem.Length; j++)
			{
				ParticleSystem.ColorOverLifetimeModule overLifetimeModule = colorChangeList[i].colored_ParticleSystem[j].colorOverLifetime;
				overLifetimeModule.color = colorChangeList[i].customer_Gradient;
			}
		}
	}
}
