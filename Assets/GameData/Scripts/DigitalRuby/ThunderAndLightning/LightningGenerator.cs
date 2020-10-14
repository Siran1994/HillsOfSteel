using System;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalRuby.ThunderAndLightning
{
	public class LightningGenerator
	{
        private void GetPerpendicularVector(ref Vector3 directionNormalized, out Vector3 side)
        {
            if (directionNormalized == Vector3.zero)
            {
                side = Vector3.right;
            }
            else
            {
                // use cross product to find any perpendicular vector around directionNormalized:
                // 0 = x * px + y * py + z * pz
                // => pz = -(x * px + y * py) / z
                // for computational stability use the component farthest from 0 to divide by
                float x = directionNormalized.x;
                float y = directionNormalized.y;
                float z = directionNormalized.z;
                float px, py, pz;
                float ax = Mathf.Abs(x), ay = Mathf.Abs(y), az = Mathf.Abs(z);
                if (ax >= ay && ay >= az)
                {
                    // x is the max, so we can pick (py, pz) arbitrarily at (1, 1):
                    py = 1.0f;
                    pz = 1.0f;
                    px = -(y * py + z * pz) / x;
                }
                else if (ay >= az)
                {
                    // y is the max, so we can pick (px, pz) arbitrarily at (1, 1):
                    px = 1.0f;
                    pz = 1.0f;
                    py = -(x * px + z * pz) / y;
                }
                else
                {
                    // z is the max, so we can pick (px, py) arbitrarily at (1, 1):
                    px = 1.0f;
                    py = 1.0f;
                    pz = -(x * px + y * py) / z;
                }
                side = new Vector3(px, py, pz).normalized;
            }
        }

        protected virtual void OnGenerateLightningBolt(LightningBolt bolt, Vector3 start, Vector3 end, LightningBoltParameters p)
        {
            GenerateLightningBoltStandard(bolt, start, end, p.Generations, p.Generations, 0.0f, p);
        }

        public bool ShouldCreateFork(LightningBoltParameters p, int generation, int totalGenerations)
        {
            return (generation > p.generationWhereForksStop && generation >= totalGenerations - p.forkednessCalculated && (float)p.Random.NextDouble() < p.Forkedness);
        }

        public void CreateFork(LightningBolt bolt, LightningBoltParameters p, int generation, int totalGenerations, Vector3 start, Vector3 midPoint)
        {
            if (ShouldCreateFork(p, generation, totalGenerations))
            {
                Vector3 branchVector = (midPoint - start) * p.ForkMultiplier();
                Vector3 splitEnd = midPoint + branchVector;
                GenerateLightningBoltStandard(bolt, midPoint, splitEnd, generation, totalGenerations, 0.0f, p);
            }
        }

        public void GenerateLightningBoltStandard(LightningBolt bolt, Vector3 start, Vector3 end, int generation, int totalGenerations, float offsetAmount, LightningBoltParameters p)
        {
            if (generation < 1)
            {
                return;
            }

            LightningBoltSegmentGroup group = bolt.AddGroup();
            group.Segments.Add(new LightningBoltSegment { Start = start, End = end });

            // every generation, get the percentage we have gone down and square it, this makes lines thinner
            float widthMultiplier = (float)generation / (float)totalGenerations;
            widthMultiplier *= widthMultiplier;

            Vector3 randomVector;
            group.LineWidth = p.TrunkWidth * widthMultiplier;
            group.Generation = generation;
            group.Color = p.Color;
            group.Color.a = (byte)(255.0f * widthMultiplier);
            group.EndWidthMultiplier = p.EndWidthMultiplier * p.ForkEndWidthMultiplier;
            if (offsetAmount <= 0.0f)
            {
                offsetAmount = (end - start).magnitude * (generation == totalGenerations ? p.ChaosFactor : p.ChaosFactorForks);
            }

            while (generation-- > 0)
            {
                int previousStartIndex = group.StartIndex;
                group.StartIndex = group.Segments.Count;
                for (int i = previousStartIndex; i < group.StartIndex; i++)
                {
                    start = group.Segments[i].Start;
                    end = group.Segments[i].End;

                    // determine a new direction for the split
                    Vector3 midPoint = (start + end) * 0.5f;

                    // adjust the mid point to be the new location
                    RandomVector(bolt, ref start, ref end, offsetAmount, p.Random, out randomVector);
                    midPoint += randomVector;

                    // add two new segments
                    group.Segments.Add(new LightningBoltSegment { Start = start, End = midPoint });
                    group.Segments.Add(new LightningBoltSegment { Start = midPoint, End = end });

                    CreateFork(bolt, p, generation, totalGenerations, start, midPoint);
                }

                // halve the distance the lightning can deviate for each generation down
                offsetAmount *= 0.5f;
            }
        }

        public Vector3 RandomDirection3D(System.Random r)
        {
            float z = (2.0f * (float)r.NextDouble()) - 1.0f; // z is in the range [-1,1]
            Vector3 planar = RandomDirection2D(r) * Mathf.Sqrt(1.0f - (z * z));
            planar.z = z;

            return planar;
        }

        public Vector3 RandomDirection2D(System.Random r)
        {
            float azimuth = (float)r.NextDouble() * 2.0f * Mathf.PI;
            return new Vector3(Mathf.Cos(azimuth), Mathf.Sin(azimuth), 0.0f);
        }

        public Vector3 RandomDirection2DXZ(System.Random r)
        {
            float azimuth = (float)r.NextDouble() * 2.0f * Mathf.PI;
            return new Vector3(Mathf.Cos(azimuth), 0.0f, Mathf.Sin(azimuth));
        }

        public void RandomVector(LightningBolt bolt, ref Vector3 start, ref Vector3 end, float offsetAmount, System.Random random, out Vector3 result)
        {
            if (bolt.CameraMode == CameraMode.Perspective)
            {
                Vector3 direction = (end - start).normalized;
                Vector3 side = Vector3.Cross(start, end);
                if (side == Vector3.zero)
                {
                    // slow path, rarely hit unless cross product is zero
                    GetPerpendicularVector(ref direction, out side);
                }
                else
                {
                    side.Normalize();
                }

                // generate random distance and angle
                float distance = (((float)random.NextDouble() + 0.1f) * offsetAmount);

#if DEBUG

                float rotationAngle = ((float)random.NextDouble() * 360.0f);
                result = Quaternion.AngleAxis(rotationAngle, direction) * side * distance;

#else

                // optimized path for RELEASE mode, skips two normalize and two multiplies in Quaternion.AngleAxis
                float rotationAngle = ((float)random.NextDouble() * Mathf.PI);
                direction *= (float)System.Math.Sin(rotationAngle);
                Quaternion rotation;
                rotation.x = direction.x;
                rotation.y = direction.y;
                rotation.z = direction.z;
                rotation.w =  (float)System.Math.Cos(rotationAngle);
                result = rotation * side * distance;

#endif

            }
            else if (bolt.CameraMode == CameraMode.OrthographicXY)
            {
                // XY plane
                end.z = start.z;
                Vector3 directionNormalized = (end - start).normalized;
                Vector3 side = new Vector3(-directionNormalized.y, directionNormalized.x, 0.0f);
                float distance = ((float)random.NextDouble() * offsetAmount * 2.0f) - offsetAmount;
                result = side * distance;
            }
            else
            {
                // XZ plane
                end.y = start.y;
                Vector3 directionNormalized = (end - start).normalized;
                Vector3 side = new Vector3(-directionNormalized.z, 0.0f, directionNormalized.x);
                float distance = ((float)random.NextDouble() * offsetAmount * 2.0f) - offsetAmount;
                result = side * distance;
            }
        }

        public void GenerateLightningBolt(LightningBolt bolt, LightningBoltParameters p)
        {
            Vector3 start, end;
            GenerateLightningBolt(bolt, p, out start, out end);
        }

        public void GenerateLightningBolt(LightningBolt bolt, LightningBoltParameters p, out Vector3 start, out Vector3 end)
        {
            start = p.ApplyVariance(p.Start, p.StartVariance);
            end = p.ApplyVariance(p.End, p.EndVariance);
            OnGenerateLightningBolt(bolt, start, end, p);
        }

        public static readonly LightningGenerator GeneratorInstance = new LightningGenerator();
	}
}
