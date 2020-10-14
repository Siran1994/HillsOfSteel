using UnityEngine;

namespace DigitalRuby.ThunderAndLightning
{
	public class LightningBoltShapeSphereScript : LightningBoltPrefabScriptBase
	{
		[Header("Lightning Sphere Properties")]
		[Tooltip("Radius inside the sphere where lightning can emit from")]
		public float InnerRadius = 0.1f;

		[Tooltip("Radius of the sphere")]
		public float Radius = 4.0f;

#if UNITY_EDITOR

		protected override void OnDrawGizmos()
		{
			base.OnDrawGizmos();

			Gizmos.DrawWireSphere(transform.position, InnerRadius);
			Gizmos.DrawWireSphere(transform.position, Radius);
		}

#endif

		public override void CreateLightningBolt(LightningBoltParameters parameters)
		{
			Vector3 start = UnityEngine.Random.insideUnitSphere * InnerRadius;
			Vector3 end = UnityEngine.Random.onUnitSphere * Radius;

			parameters.Start = start;
			parameters.End = end;

			base.CreateLightningBolt(parameters);
		}
	}
}
