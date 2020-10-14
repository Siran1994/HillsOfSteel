using System;
using UnityEngine;

public class ChainWayPoint : MonoBehaviour
{
	public float[] pointAngles;

	public float offset = 0.15f;

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = new Color(1f, 1f, 0.5f, 1f);
		for (int i = 0; i < pointAngles.Length; i++)
		{
			float f = pointAngles[i] * ((float)Math.PI / 180f);
			Vector3 a = new Vector3(Mathf.Sin(f), Mathf.Cos(f), 0f);
			Gizmos.DrawSphere(base.transform.position + a * offset, 0.02f);
		}
	}

	public Vector3 GetInnerCoordinates(int i)
	{
		if (pointAngles.Length < i)
		{
			return Vector3.zero;
		}
		float f = pointAngles[i] * ((float)Math.PI / 180f);
		Vector3 a = new Vector3(Mathf.Sin(f), Mathf.Cos(f), 0f);
		return base.transform.localPosition + a * offset;
	}

	public Vector3 GetOuterCoordinates(int i, float length)
	{
		if (pointAngles.Length < i)
		{
			return Vector3.zero;
		}
		float f = pointAngles[i] * ((float)Math.PI / 180f);
		Vector3 a = new Vector3(Mathf.Sin(f), Mathf.Cos(f), 0f);
		return base.transform.localPosition + a * (offset + length);
	}
}
