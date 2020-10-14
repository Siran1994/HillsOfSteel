using UnityEngine;

namespace DigitalRuby.ThunderAndLightning
{
	public class DemoScriptRotate : MonoBehaviour
	{
		public Vector3 Rotation;

		private void Update()
		{
			base.gameObject.transform.Rotate(Rotation * Time.deltaTime);
		}
	}
}
