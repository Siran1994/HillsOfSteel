using System.Collections;
using UnityEngine;

public class EasterEgg : MonoBehaviour
{
	private IEnumerator Start()
	{
		while (true)
		{
			float totalTime = 1f;
			float targetRotation2 = Mathf.Sign(UnityEngine.Random.Range(-1, 1)) * 30f;
			float prevRotation = base.transform.rotation.eulerAngles.z;
			int num;
			for (int i = 1; i <= 6; i = num)
			{
				totalTime /= (float)i;
				targetRotation2 = 0f - targetRotation2;
				targetRotation2 /= (float)i;
				for (float t = 0f; t < totalTime; t += Time.unscaledDeltaTime)
				{
					float t2 = LeanTween.easeInOutSine(0f, 1f, t / totalTime);
					float z = Mathf.LerpAngle(prevRotation, targetRotation2, t2);
					base.transform.rotation = Quaternion.Euler(0f, 0f, z);
					yield return null;
				}
				prevRotation = targetRotation2;
				num = i + 1;
			}
			yield return new WaitForSecondsRealtime(0.5f);
		}
	}

	private void OnMouseDown()
	{
		MenuController.ShowMenu<EasterEggPopup>();
	}
}
