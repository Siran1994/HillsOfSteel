using UnityEngine;

public class UISafeAreaHandler : MonoBehaviour
{
	private void Update()
	{
		if (Utilities.IsIPhonePlus())
		{
			UnityEngine.Object.Destroy(this);
			return;
		}
		RectTransform component = GetComponent<RectTransform>();
		Rect pixelRect = GetComponentInParent<Canvas>().pixelRect;
		Rect safeArea = Screen.safeArea;
		Vector2 position = safeArea.position;
		Vector2 anchorMax = safeArea.position + safeArea.size;
		position.x /= pixelRect.width;
		position.y /= pixelRect.height;
		anchorMax.x /= pixelRect.width;
		anchorMax.y /= pixelRect.height;
		component.anchorMin = position;
		component.anchorMax = anchorMax;
	}
}
