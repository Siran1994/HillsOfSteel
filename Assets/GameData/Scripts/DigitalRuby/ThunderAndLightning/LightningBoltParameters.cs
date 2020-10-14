using System;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalRuby.ThunderAndLightning
{
	[Serializable]
	public sealed class LightningBoltParameters
	{
        #region Internal use only

        // INTERNAL USE ONLY!!!
        private static int randomSeed = Environment.TickCount;
        private static readonly List<LightningBoltParameters> cache = new List<LightningBoltParameters>();
        internal int generationWhereForksStop;
        internal int forkednessCalculated;
        internal LightningBoltQualitySetting quality;
        internal float delaySeconds;
        internal int maxLights;
        // END INTERNAL USE ONLY

        #endregion Internal use only

        /// <summary>
        /// Scale all scalar parameters by this value (i.e. trunk width, turbulence, turbulence velocity)
        /// </summary>
        public static float Scale = 1.0f;

        /// <summary>
        /// Contains quality settings for different quality levels. By default, this assumes 6 quality levels, so if you have your own
        /// custom quality setting levels, you may want to clear this dictionary out and re-populate it with your own limits
        /// </summary>
        public static readonly Dictionary<int, LightningQualityMaximum> QualityMaximums = new Dictionary<int, LightningQualityMaximum>();

        static LightningBoltParameters()
        {
            string[] names = QualitySettings.names;
            for (int i = 0; i < names.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        QualityMaximums[i] = new LightningQualityMaximum { MaximumGenerations = 3, MaximumLightPercent = 0, MaximumShadowPercent = 0.0f };
                        break;
                    case 1:
                        QualityMaximums[i] = new LightningQualityMaximum { MaximumGenerations = 4, MaximumLightPercent = 0, MaximumShadowPercent = 0.0f };
                        break;
                    case 2:
                        QualityMaximums[i] = new LightningQualityMaximum { MaximumGenerations = 5, MaximumLightPercent = 0.1f, MaximumShadowPercent = 0.0f };
                        break;
                    case 3:
                        QualityMaximums[i] = new LightningQualityMaximum { MaximumGenerations = 5, MaximumLightPercent = 0.1f, MaximumShadowPercent = 0.0f };
                        break;
                    case 4:
                        QualityMaximums[i] = new LightningQualityMaximum { MaximumGenerations = 6, MaximumLightPercent = 0.05f, MaximumShadowPercent = 0.1f };
                        break;
                    case 5:
                        QualityMaximums[i] = new LightningQualityMaximum { MaximumGenerations = 7, MaximumLightPercent = 0.025f, MaximumShadowPercent = 0.05f };
                        break;
                    default:
                        QualityMaximums[i] = new LightningQualityMaximum { MaximumGenerations = 8, MaximumLightPercent = 0.025f, MaximumShadowPercent = 0.05f };
                        break;
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public LightningBoltParameters()
        {
            unchecked
            {
                random = currentRandom = new System.Random(randomSeed++);
            }
            Points = new List<Vector3>();
        }

        /// <summary>
        /// Generator to create the lightning bolt from the parameters
        /// </summary>
        public LightningGenerator Generator;

        /// <summary>
        /// Start of the bolt
        /// </summary>
        public Vector3 Start;

        /// <summary>
        /// End of the bolt
        /// </summary>
        public Vector3 End;

        /// <summary>
        /// X, Y and Z radius variance from Start
        /// </summary>
        public Vector3 StartVariance;

        /// <summary>
        /// X, Y and Z radius variance from End
        /// </summary>
        public Vector3 EndVariance;

        /// <summary>
        /// Custom transform action, null if none
        /// </summary>
        public System.Action<LightningCustomTransformStateInfo> CustomTransform;

        private int generations;
        /// <summary>
        /// Number of generations (0 for just a point light, otherwise 1 - 8). Higher generations have lightning with finer detail but more expensive to create.
        /// </summary>
        public int Generations
        {
            get { return generations; }
            set
            {
                int v = Mathf.Clamp(value, 1, 8);

                if (quality == LightningBoltQualitySetting.UseScript)
                {
                    generations = v;
                }
                else
                {
                    LightningQualityMaximum maximum;
                    int level = QualitySettings.GetQualityLevel();
                    if (QualityMaximums.TryGetValue(level, out maximum))
                    {
                        generations = Mathf.Min(maximum.MaximumGenerations, v);
                    }
                    else
                    {
                        generations = v;
                        Debug.LogError("Unable to read lightning quality settings from level " + level.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// How long the bolt should live in seconds
        /// </summary>
        public float LifeTime;

        /// <summary>
        /// Minimum delay
        /// </summary>
        public float Delay;

        /// <summary>
        /// How long to wait in seconds before starting additional lightning bolts
        /// </summary>
        public RangeOfFloats DelayRange;

        /// <summary>
        /// How chaotic is the main trunk of lightning? (0 - 1). Higher numbers create more chaotic lightning.
        /// </summary>
        public float ChaosFactor;

        /// <summary>
        /// How chaotic are the forks of the lightning? (0 - 1). Higher numbers create more chaotic lightning.
        /// </summary>
        public float ChaosFactorForks = -1.0f;

        /// <summary>
        /// The width of the trunk
        /// </summary>
        public float TrunkWidth;

        /// <summary>
        /// The ending width of a segment of lightning
        /// </summary>
        public float EndWidthMultiplier = 0.5f;

        /// <summary>
        /// Intensity of the lightning
        /// </summary>
        public float Intensity = 1.0f;

        /// <summary>
        /// Intensity of the glow
        /// </summary>
        public float GlowIntensity;

        /// <summary>
        /// Glow width multiplier
        /// </summary>
        public float GlowWidthMultiplier;

        /// <summary>
        /// How forked the lightning should be, 0 for none, 1 for LOTS of forks
        /// </summary>
        public float Forkedness;

        /// <summary>
        /// This is subtracted from the initial generations value, and any generation below that cannot have a fork
        /// </summary>
        public int GenerationWhereForksStopSubtractor = 5;

        /// <summary>
        /// Tint color for the lightning, this is applied to both the lightning and the glow. Unlike the script properties for coloring which
        /// are applied per material, this is applied at the mesh level and as such different bolts on the same script can use different color values.
        /// </summary>
        public Color32 Color = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

        /// <summary>
        /// Used to generate random numbers. Not thread safe.
        /// </summary>
        public System.Random Random
        {
            get { return currentRandom; }
            set
            {
                random = value ?? random;
                currentRandom = (randomOverride ?? random);
            }
        }
        private System.Random random;
        private System.Random currentRandom;

        /// <summary>
        /// Override Random to a different Random. This gets set back to null when the parameters go back to the cache. Great for a one time bolt that looks a certain way.
        /// </summary>
        public System.Random RandomOverride
        {
            get { return randomOverride; }
            set
            {
                randomOverride = value;
                currentRandom = (randomOverride ?? random);
            }
        }
        private System.Random randomOverride;

        /// <summary>
        /// The percent of time the lightning should fade in and out (0 - 1). Example: 0.2 would fade in for 20% of the lifetime and fade out for 20% of the lifetime. Set to 0 for no fade.
        /// </summary>
        public float FadePercent = 0.15f;

        /// <summary>
        /// Modify the fade in time for FadePercent (0 - 1)
        /// </summary>
        public float FadeInMultiplier = 1.0f;

        /// <summary>
        /// Modify the fully lit time for FadePercent (0 - 1)
        /// </summary>
        public float FadeFullyLitMultiplier = 1.0f;

        /// <summary>
        /// Modify the fade out time for FadePercent (0 - 1)
        /// </summary>
        public float FadeOutMultiplier = 1.0f;

        private float growthMultiplier;
        /// <summary>
        /// A value between 0 and 0.999 that determines how fast the lightning should grow over the lifetime. A value of 1 grows slowest, 0 grows instantly
        /// </summary>
        public float GrowthMultiplier
        {
            get { return growthMultiplier; }
            set { growthMultiplier = Mathf.Clamp(value, 0.0f, 0.999f); }
        }

        /// <summary>
        /// Minimum distance multiplier for forks
        /// </summary>
        public float ForkLengthMultiplier = 0.6f;

        /// <summary>
        /// Variance of the fork distance (random range of 0 to n is added to ForkLengthMultiplier)
        /// </summary>
        public float ForkLengthVariance = 0.2f;

        /// <summary>
        /// Forks will have their end widths multiplied by this value
        /// </summary>
        public float ForkEndWidthMultiplier = 1.0f;

        /// <summary>
        /// Light parameters, null for none
        /// </summary>
        public LightningLightParameters LightParameters;

        /// <summary>
        /// Points for the trunk to follow - not all generators support this
        /// </summary>
        public List<Vector3> Points { get; set; }

        /// <summary>
        /// The amount of smoothing applied. For example, if there were 4 original points and smoothing / spline created 32 points, this value would be 8 - not all generators support this
        /// </summary>
        public int SmoothingFactor;

        /// <summary>
        /// Get a multiplier for fork distance
        /// </summary>
        /// <returns>Fork multiplier</returns>
        public float ForkMultiplier()
        {
            return ((float)Random.NextDouble() * ForkLengthVariance) + ForkLengthMultiplier;
        }

        /// <summary>
        /// Apply variance to a vector
        /// </summary>
        /// <param name="pos">Position</param>
        /// <param name="variance">Variance</param>
        /// <returns>New position</returns>
        public Vector3 ApplyVariance(Vector3 pos, Vector3 variance)
        {
            return new Vector3
            (
                pos.x + (((float)Random.NextDouble() * 2.0f) - 1.0f) * variance.x,
                pos.y + (((float)Random.NextDouble() * 2.0f) - 1.0f) * variance.y,
                pos.z + (((float)Random.NextDouble() * 2.0f) - 1.0f) * variance.z
            );
        }

        /// <summary>
        /// Reset parameters
        /// </summary>
        public void Reset()
        {
            Start = End = Vector3.zero;
            Generator = null;
            SmoothingFactor = 0;
            RandomOverride = null;
            CustomTransform = null;
            if (Points != null)
            {
                Points.Clear();
            }
        }

        /// <summary>
        /// Get or create lightning bolt parameters. If cache has parameters, one is taken, otherwise a new object is created. NOT thread safe.
        /// </summary>
        /// <returns>Lightning bolt parameters</returns>
        public static LightningBoltParameters GetOrCreateParameters()
        {
            LightningBoltParameters p;
            if (cache.Count == 0)
            {
                unchecked
                {
                    p = new LightningBoltParameters();
                }
            }
            else
            {
                int i = cache.Count - 1;
                p = cache[i];
                cache.RemoveAt(i);
            }
            return p;
        }

        /// <summary>
        /// Return parameters to cache. NOT thread safe.
        /// </summary>
        /// <param name="p">Parameters</param>
        public static void ReturnParametersToCache(LightningBoltParameters p)
        {
            if (!cache.Contains(p))
            {
                // reset variables that are state-machine dependant
                p.Reset();
                cache.Add(p);
            }
        }
	}
}
