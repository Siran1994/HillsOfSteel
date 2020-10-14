using UnityEngine;
using UnityEngine.SceneManagement;

namespace DigitalRuby.ThunderAndLightning
{
	public class DemoScriptReloadSceneEsc : MonoBehaviour
	{
		private void Start()
		{

		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				UnityEngine.SceneManagement.SceneManager.LoadScene(0);
			}
		}
	}
}
