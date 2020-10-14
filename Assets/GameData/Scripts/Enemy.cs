using System;

[Serializable]
public class Enemy
{
	public Variables.EnemyTank settings;

	public Vehicle vehicleContainer;

	public bool hasBeenInView;

	public int currentShot;

	public int typeIndex;

	public void NextShot()
	{
		currentShot = (currentShot + 1) % settings.shots.Length;
	}

	public float GetAimDuration()
	{
		return settings.shots[currentShot].aimDuration;
	}

	public float GetShootDuration()
	{
		return settings.shots[currentShot].shootDuration;
	}

	public float GetMoveDuration()
	{
		return settings.shots[currentShot].moveDuration;
	}
}
