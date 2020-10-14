using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(GraphicRaycaster))]
public class MenuBase<T> : MonoBehaviour where T : MonoBehaviour
{
	public static T instance;

	public MenuSettings settings;
}
