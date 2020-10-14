using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalRuby.ThunderAndLightning
{
	public class LightningBoltDependencies
	{
        /// <summary>
        /// Parent - do not access from threads
        /// </summary>
        public GameObject Parent;

        /// <summary>
        /// Material - do not access from threads
        /// </summary>
        public Material LightningMaterialMesh;

        /// <summary>
        /// Material no glow - do not access from threads
        /// </summary>
        public Material LightningMaterialMeshNoGlow;

        /// <summary>
        /// Origin particle system - do not access from threads
        /// </summary>
        public ParticleSystem OriginParticleSystem;

        /// <summary>
        /// Dest particle system - do not access from threads
        /// </summary>
        public ParticleSystem DestParticleSystem;

        /// <summary>
        /// Camera position
        /// </summary>
        public Vector3 CameraPos;

        /// <summary>
        /// Is camera 2D?
        /// </summary>
        public bool CameraIsOrthographic;

        /// <summary>
        /// Camera mode
        /// </summary>
        public CameraMode CameraMode;

        /// <summary>
        /// Use world space
        /// </summary>
        public bool UseWorldSpace;

        /// <summary>
        /// Level of detail distance
        /// </summary>
        public float LevelOfDetailDistance;

        /// <summary>
        /// Sort layer name
        /// </summary>
        public string SortLayerName;

        /// <summary>
        /// Order in layer
        /// </summary>
        public int SortOrderInLayer;

        /// <summary>
        /// Parameters
        /// </summary>
        public ICollection<LightningBoltParameters> Parameters;

#if !UNITY_WEBGL

        /// <summary>
        /// Thread state
        /// </summary>
        public LightningThreadState ThreadState;

#endif

        /// <summary>
        /// Method to start co-routines
        /// </summary>
        public Func<IEnumerator, Coroutine> StartCoroutine;

        /// <summary>
        /// Call this when a light is added
        /// </summary>
        public Action<Light> LightAdded;

        /// <summary>
        /// Call this when a light is removed
        /// </summary>
        public Action<Light> LightRemoved;

        /// <summary>
        /// Call this when the bolt becomes active
        /// </summary>
        public Action<LightningBolt> AddActiveBolt;

        /// <summary>
        /// Returns the dependencies to their cache
        /// </summary>
        public Action<LightningBoltDependencies> ReturnToCache;

        /// <summary>
        /// Runs when a lightning bolt is started (parameters, start, end)
        /// </summary>
        public Action<LightningBoltParameters, Vector3, Vector3> LightningBoltStarted;

        /// <summary>
        /// Runs when a lightning bolt is ended (parameters, start, end)
        /// </summary>
        public Action<LightningBoltParameters, Vector3, Vector3> LightningBoltEnded;
	}
}
