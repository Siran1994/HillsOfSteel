using UnityEngine;

[ExecuteInEditMode]
public class SetSortingLayer : MonoBehaviour
{
	public string sortingLayerName;

	public int sortingOrder;

	private void Update()
	{
		Renderer component = GetComponent<Renderer>();
		if (component != null)
		{
			component.sortingLayerName = sortingLayerName;
			component.sortingOrder = sortingOrder;
		}
	}
}
