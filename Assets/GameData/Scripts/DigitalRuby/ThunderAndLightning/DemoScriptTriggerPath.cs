using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DigitalRuby.ThunderAndLightning
{
	public class DemoScriptTriggerPath : MonoBehaviour
	{
		public LightningSplineScript Script;
		public UnityEngine.UI.Toggle SplineToggle;

		private readonly List<Vector3> points = new List<Vector3>();

		private void Start()
		{
			Script.ManualMode = true;
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				DemoScript.ReloadCurrentScene();
				return;
			}
			else if (Input.GetMouseButton(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
			{
				Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				if (Camera.main.orthographic)
				{
					worldPos.z = 0.0f;
				}
				if (points.Count == 0 || (points[points.Count - 1] - worldPos).magnitude > 8.0f)
				{
					points.Add(worldPos);
					Script.Trigger(points, SplineToggle.isOn);
				}
			}
		}
	}
}
