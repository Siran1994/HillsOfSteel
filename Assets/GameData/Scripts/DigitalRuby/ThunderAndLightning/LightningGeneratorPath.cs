using UnityEngine;

namespace DigitalRuby.ThunderAndLightning
{
	public class LightningGeneratorPath : LightningGenerator
	{
        public static readonly LightningGeneratorPath PathGeneratorInstance = new LightningGeneratorPath();

        public void GenerateLightningBoltPath(LightningBolt bolt, Vector3 start, Vector3 end, LightningBoltParameters p)
        {
            if (p.Points.Count < 2)
            {
                Debug.LogError("Lightning path should have at least two points");
                return;
            }

            int generation = p.Generations;
            int totalGenerations = generation;
            float offsetAmount, d;
            float chaosFactor = (generation == p.Generations ? p.ChaosFactor : p.ChaosFactorForks);
            int smoothingFactor = p.SmoothingFactor - 1;
            Vector3 distance, randomVector;
            LightningBoltSegmentGroup group = bolt.AddGroup();
            group.LineWidth = p.TrunkWidth;
            group.Generation = generation--;
            group.EndWidthMultiplier = p.EndWidthMultiplier;
            group.Color = p.Color;

            p.Start = p.Points[0] + start;
            p.End = p.Points[p.Points.Count - 1] + end;
            end = p.Start;

            for (int i = 1; i < p.Points.Count; i++)
            {
                start = end;
                end = p.Points[i];
                distance = (end - start);
                d = PathGenerator.SquareRoot(distance.sqrMagnitude);
                if (chaosFactor > 0.0f)
                {
                    if (bolt.CameraMode == CameraMode.Perspective)
                    {
                        end += (d * chaosFactor * RandomDirection3D(p.Random));
                    }
                    else if (bolt.CameraMode == CameraMode.OrthographicXY)
                    {
                        end += (d * chaosFactor * RandomDirection2D(p.Random));
                    }
                    else
                    {
                        end += (d * chaosFactor * RandomDirection2DXZ(p.Random));
                    }
                    distance = (end - start);
                }
                group.Segments.Add(new LightningBoltSegment { Start = start, End = end });

                offsetAmount = d * chaosFactor;
                RandomVector(bolt, ref start, ref end, offsetAmount, p.Random, out randomVector);

                if (ShouldCreateFork(p, generation, totalGenerations))
                {
                    Vector3 branchVector = distance * p.ForkMultiplier() * smoothingFactor * 0.5f;
                    Vector3 forkEnd = end + branchVector + randomVector;
                    GenerateLightningBoltStandard(bolt, start, forkEnd, generation, totalGenerations, 0.0f, p);
                }

                if (--smoothingFactor == 0)
                {
                    smoothingFactor = p.SmoothingFactor - 1;
                }
            }
        }

        protected override void OnGenerateLightningBolt(LightningBolt bolt, Vector3 start, Vector3 end, LightningBoltParameters p)
        {
            GenerateLightningBoltPath(bolt, start, end, p);
        }
	}
}
