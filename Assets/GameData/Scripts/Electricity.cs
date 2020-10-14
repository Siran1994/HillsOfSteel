using DigitalRuby.ThunderAndLightning;
using UnityEngine;

public class Electricity : MonoBehaviour
{
	public LightningBoltPathScript lightningBolt;

	public Transform lightningEnd;

	private Vector3 lightningEndOriginal;

	private void OnEnable()
	{
		lightningEndOriginal = lightningEnd.position;
		ClearTargets();
	}

	public void ClearTargets()
	{
		lightningBolt.LightningPath.List.Clear();
		lightningBolt.LightningPath.List.Add(base.transform.gameObject);
	}

	public void AddTarget(Transform target)
	{
		lightningBolt.LightningPath.List.Add(target.gameObject);
	}

	public void SetLightningEnd(Vector3 pos)
	{
		lightningEnd.position = pos;
	}

	public void ResetLightningEnd()
	{
		lightningEnd.position = lightningEndOriginal;
	}

	public void TargetEnd()
	{
		ClearTargets();
		lightningBolt.LightningPath.List.Add(lightningEnd.gameObject);
	}
}
