using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TouchButton : Button
{
	private bool isTriggered;

	private bool isDown;

	public bool IsDown()
	{
		return isDown;
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
		isDown = true;
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
		isDown = false;
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		base.OnPointerDown(eventData);
		isTriggered = true;
	}

	public bool IsTriggered()
	{
		if (isTriggered)
		{
			isTriggered = false;
			return true;
		}
		return false;
	}

	protected override void OnDisable()
	{
		isDown = false;
	}
}
