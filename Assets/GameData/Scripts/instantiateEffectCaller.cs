using System;
using UnityEngine;

public class instantiateEffectCaller : MonoBehaviour
{
	[Serializable]
	public class chainEffect
	{
		[NonSerialized]
		public bool isPlayed;

		public float activateTimer;

		public GameObject Effect;

		public Transform effectLocator;
	}

	[NonSerialized]
	public bool fired;

	private float timer;

	public float timeLimit;

	public chainEffect[] chainEffectList;

	private void Start()
	{
	}

	private void Update()
	{
		timer += Time.deltaTime;
		CheckTimer();
	}

	private void CheckTimer()
	{
		for (int i = 0; i < chainEffectList.Length; i++)
		{
			if (timer >= chainEffectList[i].activateTimer && !chainEffectList[i].isPlayed)
			{
				UnityEngine.Object.Instantiate(chainEffectList[i].Effect, chainEffectList[i].effectLocator.transform.position, chainEffectList[i].effectLocator.transform.rotation);
				chainEffectList[i].isPlayed = true;
			}
		}
		if (timer >= timeLimit)
		{
			fired = false;
			ResetTimers();
		}
	}

	public void ResetTimers()
	{
		for (int i = 0; i < chainEffectList.Length; i++)
		{
			chainEffectList[i].isPlayed = false;
		}
		timer = 0f;
	}
}
