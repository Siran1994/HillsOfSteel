using UnityEngine;

namespace DigitalRuby.ThunderAndLightning
{
	public class LightningBoltPrefabScript : LightningBoltPrefabScriptBase
	{
        [Header("Start/end")]
        [Tooltip("The source game object, can be null")]
        public GameObject Source;

        [Tooltip("The destination game object, can be null")]
        public GameObject Destination;

        [Tooltip("X, Y and Z for variance from the start point. Use positive values.")]
        public Vector3 StartVariance;

        [Tooltip("X, Y and Z for variance from the end point. Use positive values.")]
        public Vector3 EndVariance;

#if UNITY_EDITOR

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            if (Source != null)
            {
                Gizmos.DrawIcon(Source.transform.position, "LightningPathStart.png");
            }
            if (Destination != null)
            {
                Gizmos.DrawIcon(Destination.transform.position, "LightningPathNext.png");
            }
            if (Source != null && Destination != null)
            {
                Gizmos.DrawLine(Source.transform.position, Destination.transform.position);
                Vector3 direction = (Destination.transform.position - Source.transform.position);
                Vector3 center = (Source.transform.position + Destination.transform.position) * 0.5f;
                float arrowSize = Mathf.Min(2.0f, direction.magnitude) * 2.0f;

#if UNITY_5_6_OR_NEWER

                UnityEditor.Handles.ArrowHandleCap(0, center, Quaternion.LookRotation(direction), arrowSize, EventType.Repaint);

#else

                UnityEditor.Handles.ArrowCap(0, center, Quaternion.LookRotation(direction), arrowSize);

#endif

            }
        }

#endif

        public override void CreateLightningBolt(LightningBoltParameters parameters)
        {
            parameters.Start = (Source == null ? parameters.Start : Source.transform.position);
            parameters.End = (Destination == null ? parameters.End : Destination.transform.position);
            parameters.StartVariance = StartVariance;
            parameters.EndVariance = EndVariance;

            base.CreateLightningBolt(parameters);
        }
	}
}
