using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomScrollRect : ScrollRect
{
	public bool IsDragging
	{
		get;
		private set;
	}

	public override void OnBeginDrag(PointerEventData eventData)
	{
		IsDragging = true;
		base.OnBeginDrag(eventData);
	}

	public override void OnEndDrag(PointerEventData eventData)
	{
		IsDragging = false;
		base.OnEndDrag(eventData);
	}
}
