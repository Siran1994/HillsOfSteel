using System.Collections.Generic;
using UnityEngine;

namespace DigitalRuby.ThunderAndLightning
{
	[RequireComponent(typeof(ParticleSystem))]
	public class LightningParticleCollisionForwarder : MonoBehaviour
	{
		[Tooltip("The script to forward the collision to. Must implement ICollisionHandler.")]
		public MonoBehaviour CollisionHandler;

		private ParticleSystem _particleSystem;

#if UNITY_4

		private ParticleSystem.CollisionEvent[] collisionEvents = new ParticleSystem.CollisionEvent[16];

#elif !UNITY_5_3_OR_NEWER

        private ParticleCollisionEvent[] collisionEvents = new ParticleCollisionEvent[16];

#else

		private readonly List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

#endif

		private void Start()
		{
			_particleSystem = GetComponent<ParticleSystem>();
		}

		private void OnParticleCollision(GameObject other)
		{
			ICollisionHandler i = CollisionHandler as ICollisionHandler;
			if (i != null)
			{

#if UNITY_4

				int numCollisionEvents = _particleSystem.GetCollisionEvents(other, collisionEvents);
				if (numCollisionEvents != 0)
				{
					i.HandleCollision(other, collisionEvents, numCollisionEvents);
				}

#elif UNITY_5_3_OR_NEWER

				int numCollisionEvents = _particleSystem.GetCollisionEvents(other, collisionEvents);
				if (numCollisionEvents != 0)
				{
					i.HandleCollision(other, collisionEvents, numCollisionEvents);
				}

#else

                int numCollisionEvents = _particleSystem.GetCollisionEvents(other, collisionEvents);
                if (numCollisionEvents != 0)
                {
                    i.HandleCollision(other, new List<ParticleCollisionEvent>(collisionEvents), numCollisionEvents);
                }

#endif

			}
		}

		/*
		public void OnCollisionEnter(Collision col)
		{
		    ICollisionHandler i = CollisionHandler as ICollisionHandler;
		    if (i != null)
		    {
		        i.HandleCollision(gameObject, col);
		    }
		}
		*/
	}
}
