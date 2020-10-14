using UnityEngine;

public class DisableOnPlatform : MonoBehaviour
{
	public RuntimePlatform platform;

	private void Start()
	{
		if (Application.platform == platform)
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
