using System.Collections;
using UnityEngine;
#pragma warning disable 0414
public class DebugChests
{
	private static bool running;

	private static int maxIterations = 10000;

	private static int currentIterations = 0;

	private static readonly int MaxCardCount = 7438;

	public static IEnumerator Run()
	{
		if (running)
		{
			UnityEngine.Debug.LogWarning("Chest debugger already running! Wait for it to finish.");
			yield break;
		}
		running = true;
		running = false;
	}

	public static IEnumerator ProgressLogger(float logInterval = 1f)
	{
		while (true)
		{
			yield return new WaitForSeconds(logInterval);
			if (!running)
			{
				break;
			}
			int num = 0;
			for (int i = 0; i != Variables.instance.tanks.Length; i++)
			{
				num += PlayerDataManager.GetTankCardCount(Variables.instance.tanks[i]);
			}
			float percentage = Utilities.GetPercentage(num, MaxCardCount, 1);
			UnityEngine.Debug.LogFormat("Chests opened: {0}, approx.tank progress: {1}/{2} ({3}%)", currentIterations, num, MaxCardCount, percentage);
		}
	}
}
