//
// Procedural Lightning for Unity
// (c) 2015 Digital Ruby, LLC
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9

#define UNITY_4

#endif

using UnityEngine;
using System.Collections.Generic;

namespace DigitalRuby.ThunderAndLightning
{

#if UNITY_4

	public interface ICollisionHandler
	{
		void HandleCollision(GameObject obj, ParticleSystem.CollisionEvent[] positions, int collisionCount);
	}

#else

    public interface ICollisionHandler
    {
        void HandleCollision(GameObject obj, List<ParticleCollisionEvent> collision, int collisionCount);
    }

#endif

    /// <summary>
    /// This script simply allows forwarding collision events for the objects that collide with something. This
    /// allows you to have a generic collision handler and attach a collision forwarder to your child objects.
    /// In addition, you also get access to the game object that is colliding, along with the object being
    /// collided into, which is helpful.
    /// </summary>
//    [RequireComponent(typeof(ParticleSystem))]
//    public class LightningParticleCollisionForwarder : MonoBehaviour
//    {
//    }
}