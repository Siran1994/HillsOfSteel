using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Chain : MonoBehaviour
{
	private static float tiling = 6f;

	private static float size = 0.15f;

	[HideInInspector]
	public float wheelSpeed;

	private float offset;

	private List<ChainWayPoint> chainWayPoints;

	private Vector3[] vertices;

	private Vector2[] uvs;

	private int[] indices;

	private void OnEnable()
	{
		chainWayPoints = new List<ChainWayPoint>(base.transform.parent.GetComponentsInChildren<ChainWayPoint>());
		chainWayPoints.Add(chainWayPoints[0]);
		int num = 0;
		foreach (ChainWayPoint chainWayPoint in chainWayPoints)
		{
			num += chainWayPoint.pointAngles.Length;
		}
		if (vertices == null)
		{
			vertices = new Vector3[num * 2];
		}
		if (uvs == null)
		{
			uvs = new Vector2[num * 2];
		}
		if (indices == null)
		{
			indices = new int[num * 6];
		}
	}

	private void Update()
	{
		Vector3 position = base.transform.position;
		position.z = -0.5f;
		base.transform.position = position;
		float num = size;
		offset -= wheelSpeed * num * Time.deltaTime;
		if (chainWayPoints == null || chainWayPoints.Count == 0)
		{
			chainWayPoints = new List<ChainWayPoint>(base.transform.parent.GetComponentsInChildren<ChainWayPoint>());
			chainWayPoints.Add(chainWayPoints[0]);
		}
		int num2 = 0;
		foreach (ChainWayPoint chainWayPoint2 in chainWayPoints)
		{
			num2 += chainWayPoint2.pointAngles.Length;
		}
		if (vertices == null)
		{
			vertices = new Vector3[num2 * 2];
		}
		if (uvs == null)
		{
			uvs = new Vector2[num2 * 2];
		}
		int num3 = 0;
		for (int i = 0; i < chainWayPoints.Count; i++)
		{
			ChainWayPoint chainWayPoint = chainWayPoints[i];
			if (!chainWayPoint)
			{
				continue;
			}
			for (int j = 0; j < chainWayPoint.pointAngles.Length; j++)
			{
				vertices[num3] = chainWayPoint.GetInnerCoordinates(j);
				vertices[num3 + num2] = chainWayPoint.GetOuterCoordinates(j, size);
				if (num3 >= 1)
				{
					uvs[num3] = new Vector2(uvs[num3 - 1].x + Vector3.Distance(vertices[num3 + num2], vertices[num3 - 1 + num2]) * tiling, 0f);
					uvs[num3 + num2] = new Vector2(uvs[num3].x, 1f);
				}
				else
				{
					uvs[num3] = new Vector2((0f - offset) * num, 0f);
					uvs[num3 + num2] = new Vector2((0f - offset) * num, 1f);
				}
				num3++;
			}
		}
		if (indices == null)
		{
			indices = new int[num2 * 6];
		}
		for (int k = 0; k < num2 - 1; k++)
		{
			indices[k * 6] = k;
			indices[k * 6 + 1] = k + num2;
			indices[k * 6 + 2] = k + 1;
			indices[k * 6 + 3] = k + num2;
			indices[k * 6 + 4] = k + 1 + num2;
			indices[k * 6 + 5] = k + 1;
		}
		MeshFilter component = GetComponent<MeshFilter>();
		if (component.sharedMesh == null)
		{
			component.sharedMesh = new Mesh();
		}
		else
		{
			component.sharedMesh.Clear();
		}
		component.sharedMesh.vertices = vertices;
		component.sharedMesh.triangles = indices;
		component.sharedMesh.uv = uvs;
		component.sharedMesh.UploadMeshData(markNoLongerReadable: false);
	}

	public void Flip()
	{
		for (int i = 0; i < chainWayPoints.Count - 1; i++)
		{
			ChainWayPoint chainWayPoint = chainWayPoints[i];
			Vector3 localPosition = chainWayPoint.transform.localPosition;
			localPosition.x = 0f - localPosition.x;
			chainWayPoint.transform.localPosition = localPosition;
			for (int j = 0; j < chainWayPoint.pointAngles.Length; j++)
			{
				chainWayPoint.pointAngles[j] = 0f - chainWayPoint.pointAngles[j];
			}
		}
	}
}
