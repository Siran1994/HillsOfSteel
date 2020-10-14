using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace DigitalRuby.ThunderAndLightning
{
	public class LightningBolt
	{
        #region LineRendererMesh

        /// <summary>
        /// Class the encapsulates a game object, and renderer for lightning bolt meshes
        /// </summary>
        public class LineRendererMesh
        {
            #region Public variables

            public GameObject GameObject { get; private set; }

            public Material Material
            {
                get { return meshRenderer.sharedMaterial; }
                set { meshRenderer.sharedMaterial = value; }
            }

            public MeshRenderer MeshRenderer
            {
                get { return meshRenderer; }
            }

            public int Tag { get; set; }

            #endregion Public variables

            #region Public properties

            /// <summary>
            /// Custom transform, null if none
            /// </summary>
            public System.Action<LightningCustomTransformStateInfo> CustomTransform { get; set; }

            /// <summary>
            /// The transform component
            /// </summary>
            public Transform Transform { get; private set; }

            /// <summary>
            /// Is the line renderer empty?
            /// </summary>
            public bool Empty {  get { return vertices.Count == 0; } }

            #endregion Public properties

            #region Public methods

            public LineRendererMesh()
            {
                GameObject = new GameObject("LightningBoltMeshRenderer");
                GameObject.SetActive(false); // call Begin to activate

#if UNITY_EDITOR

                GameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;

#endif

                mesh = new Mesh { name = "ProceduralLightningMesh" };
                mesh.MarkDynamic();
                meshFilter = GameObject.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = mesh;
                meshRenderer = GameObject.AddComponent<MeshRenderer>();

#if !UNITY_4

                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                meshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;

#endif

#if UNITY_5_3_OR_NEWER

                meshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;

#endif

                meshRenderer.receiveShadows = false;
                Transform = GameObject.GetComponent<Transform>();
            }

            public void PopulateMesh()
            {

#if ENABLE_PROFILING

                System.Diagnostics.Stopwatch w = System.Diagnostics.Stopwatch.StartNew();

#endif

                if (vertices.Count == 0)
                {
                    mesh.Clear();
                }
                else
                {
                    PopulateMeshInternal();
                }

#if ENABLE_PROFILING

                Debug.LogFormat("MESH: {0}", w.Elapsed.TotalMilliseconds);

#endif

            }

            public bool PrepareForLines(int lineCount)
            {
                int vertexCount = lineCount * 4;
                if (vertices.Count + vertexCount > 64999)
                {
                    return false;
                }
                return true;
            }

            public void BeginLine(Vector3 start, Vector3 end, float radius, Color32 color, float colorIntensity, Vector4 fadeLifeTime, float glowWidthModifier, float glowIntensity)
            {
                Vector4 dir = (end - start);
                dir.w = radius;
                AppendLineInternal(ref start, ref end, ref dir, ref dir, ref dir, color, colorIntensity, ref fadeLifeTime, glowWidthModifier, glowIntensity);
            }

            public void AppendLine(Vector3 start, Vector3 end, float radius, Color32 color, float colorIntensity, Vector4 fadeLifeTime, float glowWidthModifier, float glowIntensity)
            {
                Vector4 dir = (end - start);
                dir.w = radius;
                Vector4 dirPrev1 = lineDirs[lineDirs.Count - 3];
                Vector4 dirPrev2 = lineDirs[lineDirs.Count - 1];
                AppendLineInternal(ref start, ref end, ref dir, ref dirPrev1, ref dirPrev2, color, colorIntensity, ref fadeLifeTime, glowWidthModifier, glowIntensity);
            }

            public void Reset()
            {
                CustomTransform = null;
                Tag++;
                GameObject.SetActive(false);
                mesh.Clear();
                indices.Clear();
                vertices.Clear();
                colors.Clear();
                lineDirs.Clear();
                ends.Clear();

#if UNITY_PRE_5_3

				texCoords.Clear();
				glowModifiers.Clear();
				fadeXY.Clear();
				fadeZW.Clear();

#else

                texCoordsAndGlowModifiers.Clear();
                fadeLifetimes.Clear();

#endif

                currentBoundsMaxX = currentBoundsMaxY = currentBoundsMaxZ = int.MinValue + boundsPadder;
                currentBoundsMinX = currentBoundsMinY = currentBoundsMinZ = int.MaxValue - boundsPadder;
            }

            #endregion Public methods

            #region Private variables

            private const int defaultListCapacity = 2048;

            private static readonly Vector2 uv1 = new Vector2(0.0f, 0.0f);
            private static readonly Vector2 uv2 = new Vector2(1.0f, 0.0f);
            private static readonly Vector2 uv3 = new Vector2(0.0f, 1.0f);
            private static readonly Vector2 uv4 = new Vector2(1.0f, 1.0f);

            private readonly List<int> indices = new List<int>(defaultListCapacity);
            private readonly List<Vector3> vertices = new List<Vector3>(defaultListCapacity);
            private readonly List<Vector4> lineDirs = new List<Vector4>(defaultListCapacity);
            private readonly List<Color32> colors = new List<Color32>(defaultListCapacity);
            private readonly List<Vector3> ends = new List<Vector3>(defaultListCapacity);

#if UNITY_PRE_5_3

            private readonly List<Vector2> texCoords = new List<Vector2>(defaultListCapacity);
			private readonly List<Vector2> glowModifiers = new List<Vector2>(defaultListCapacity);
			private readonly List<Vector2> fadeXY = new List<Vector2>(defaultListCapacity);
			private readonly List<Vector2> fadeZW = new List<Vector2>(defaultListCapacity);

#else

            private readonly List<Vector4> texCoordsAndGlowModifiers = new List<Vector4>(defaultListCapacity);
            private readonly List<Vector4> fadeLifetimes = new List<Vector4>(defaultListCapacity);

#endif

            private const int boundsPadder = 1000000000;
            private int currentBoundsMinX = int.MaxValue - boundsPadder;
            private int currentBoundsMinY = int.MaxValue - boundsPadder;
            private int currentBoundsMinZ = int.MaxValue - boundsPadder;
            private int currentBoundsMaxX = int.MinValue + boundsPadder;
            private int currentBoundsMaxY = int.MinValue + boundsPadder;
            private int currentBoundsMaxZ = int.MinValue + boundsPadder;

            private Mesh mesh;
            private MeshFilter meshFilter;
            private MeshRenderer meshRenderer;

            #endregion Private variables

            #region Private methods

            private void PopulateMeshInternal()
            {
                GameObject.SetActive(true);

#if UNITY_PRE_5_3

                mesh.vertices = vertices.ToArray();
                mesh.tangents = lineDirs.ToArray();
                mesh.colors32 = colors.ToArray();
				mesh.uv = texCoords.ToArray();
				mesh.uv2 = glowModifiers.ToArray();

// Unity 5.0 - 5.2.X has to use uv3 and uv4
// Unity 4.X does not support glow or fade or elapsed time
#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2

				mesh.uv3 = fadeXY.ToArray();
				mesh.uv4 = fadeZW.ToArray();

#endif

                mesh.normals = ends.ToArray();
                mesh.triangles = indices.ToArray();

#else

                mesh.SetVertices(vertices);
                mesh.SetTangents(lineDirs);
                mesh.SetColors(colors);
                mesh.SetUVs(0, texCoordsAndGlowModifiers);
                mesh.SetUVs(1, fadeLifetimes);
                mesh.SetNormals(ends);
                mesh.SetTriangles(indices, 0);

#endif

                Bounds b = new Bounds();
                Vector3 min = new Vector3(currentBoundsMinX - 2, currentBoundsMinY - 2, currentBoundsMinZ - 2);
                Vector3 max = new Vector3(currentBoundsMaxX + 2, currentBoundsMaxY + 2, currentBoundsMaxZ + 2);
                b.center = (max + min) * 0.5f;
                b.size = (max - min) * 1.2f;
                mesh.bounds = b;
            }

            private void UpdateBounds(ref Vector3 point1, ref Vector3 point2)
            {
                // r = y + ((x - y) & ((x - y) >> (sizeof(int) * CHAR_BIT - 1))); // min(x, y)
                // r = x - ((x - y) & ((x - y) >> (sizeof(int) * CHAR_BIT - 1))); // max(x, y)

                unchecked
                {
                    {
                        int xCalculation = (int)point1.x - (int)point2.x;
                        xCalculation &= (xCalculation >> 31);
                        int xMin = (int)point2.x + xCalculation;
                        int xMax = (int)point1.x - xCalculation;

                        xCalculation = currentBoundsMinX - xMin;
                        xCalculation &= (xCalculation >> 31);
                        currentBoundsMinX = xMin + xCalculation;

                        xCalculation = currentBoundsMaxX - xMax;
                        xCalculation &= (xCalculation >> 31);
                        currentBoundsMaxX = currentBoundsMaxX - xCalculation;
                    }
                    {
                        int yCalculation = (int)point1.y - (int)point2.y;
                        yCalculation &= (yCalculation >> 31);
                        int yMin = (int)point2.y + yCalculation;
                        int yMax = (int)point1.y - yCalculation;

                        yCalculation = currentBoundsMinY - yMin;
                        yCalculation &= (yCalculation >> 31);
                        currentBoundsMinY = yMin + yCalculation;

                        yCalculation = currentBoundsMaxY - yMax;
                        yCalculation &= (yCalculation >> 31);
                        currentBoundsMaxY = currentBoundsMaxY - yCalculation;
                    }
                    {
                        int zCalculation = (int)point1.z - (int)point2.z;
                        zCalculation &= (zCalculation >> 31);
                        int zMin = (int)point2.z + zCalculation;
                        int zMax = (int)point1.z - zCalculation;

                        zCalculation = currentBoundsMinZ - zMin;
                        zCalculation &= (zCalculation >> 31);
                        currentBoundsMinZ = zMin + zCalculation;

                        zCalculation = currentBoundsMaxZ - zMax;
                        zCalculation &= (zCalculation >> 31);
                        currentBoundsMaxZ = currentBoundsMaxZ - zCalculation;
                    }
                }
            }

            private void AddIndices()
            {
                int vertexIndex = vertices.Count;
                indices.Add(vertexIndex++);
                indices.Add(vertexIndex++);
                indices.Add(vertexIndex);
                indices.Add(vertexIndex--);
                indices.Add(vertexIndex);
                indices.Add(vertexIndex += 2);
            }

            private void AppendLineInternal(ref Vector3 start, ref Vector3 end, ref Vector4 dir, ref Vector4 dirPrev1, ref Vector4 dirPrev2,
                Color32 color, float colorIntensity, ref Vector4 fadeLifeTime, float glowWidthModifier, float glowIntensity)
            {
                AddIndices();
                color.a = (byte)Mathf.Lerp(0.0f, 255.0f, colorIntensity * 0.1f);

                Vector4 texCoord = new Vector4(uv1.x, uv1.y, glowWidthModifier, glowIntensity);

                vertices.Add(start);
                lineDirs.Add(dirPrev1);
                colors.Add(color);
                ends.Add(dir);

                vertices.Add(end);
                lineDirs.Add(dir);
                colors.Add(color);
                ends.Add(dir);

                dir.w = -dir.w;

                vertices.Add(start);
                lineDirs.Add(dirPrev2);
                colors.Add(color);
                ends.Add(dir);

                vertices.Add(end);
                lineDirs.Add(dir);
                colors.Add(color);
                ends.Add(dir);

#if UNITY_PRE_5_3

                texCoords.Add(uv1);
				texCoords.Add(uv2);
				texCoords.Add(uv3);
				texCoords.Add(uv4);
				glowModifiers.Add(new Vector2(texCoord.z, texCoord.w));
				glowModifiers.Add(new Vector2(texCoord.z, texCoord.w));
				glowModifiers.Add(new Vector2(texCoord.z, texCoord.w));
				glowModifiers.Add(new Vector2(texCoord.z, texCoord.w));
				fadeXY.Add(new Vector2(fadeLifeTime.x, fadeLifeTime.y));
				fadeXY.Add(new Vector2(fadeLifeTime.x, fadeLifeTime.y));
				fadeXY.Add(new Vector2(fadeLifeTime.x, fadeLifeTime.y));
				fadeXY.Add(new Vector2(fadeLifeTime.x, fadeLifeTime.y));
				fadeZW.Add(new Vector2(fadeLifeTime.z, fadeLifeTime.w));
				fadeZW.Add(new Vector2(fadeLifeTime.z, fadeLifeTime.w));
				fadeZW.Add(new Vector2(fadeLifeTime.z, fadeLifeTime.w));
				fadeZW.Add(new Vector2(fadeLifeTime.z, fadeLifeTime.w));

#else

                texCoordsAndGlowModifiers.Add(texCoord);
                texCoord.x = uv2.x;
                texCoord.y = uv2.y;
                texCoordsAndGlowModifiers.Add(texCoord);
                texCoord.x = uv3.x;
                texCoord.y = uv3.y;
                texCoordsAndGlowModifiers.Add(texCoord);
                texCoord.x = uv4.x;
                texCoord.y = uv4.y;
                texCoordsAndGlowModifiers.Add(texCoord);
                fadeLifetimes.Add(fadeLifeTime);
                fadeLifetimes.Add(fadeLifeTime);
                fadeLifetimes.Add(fadeLifeTime);
                fadeLifetimes.Add(fadeLifeTime);

#endif

                UpdateBounds(ref start, ref end);
            }

            #endregion Private methods
        }

        #endregion LineRendererMesh

        #region Public variables

        /// <summary>
        /// The maximum number of lights to allow for all lightning
        /// </summary>
        public static int MaximumLightCount = 128;

        /// <summary>
        /// The maximum number of lights to create per batch of lightning emitted
        /// </summary>
        public static int MaximumLightsPerBatch = 8;

        /// <summary>
        /// The current minimum delay until anything will start rendering
        /// </summary>
        public float MinimumDelay { get; private set; }

        /// <summary>
        /// Is there any glow for any of the lightning bolts?
        /// </summary>
        public bool HasGlow { get; private set; }

        /// <summary>
        /// Is this lightning bolt active any more?
        /// </summary>
        public bool IsActive { get { return elapsedTime < lifeTime; } }

        /// <summary>
        /// Camera mode
        /// </summary>
        public CameraMode CameraMode { get; private set; }

        private DateTime startTimeOffset;

        #endregion Public variables

        #region Public methods

        /// <summary>
        /// Default constructor
        /// </summary>
        public LightningBolt()
        {
        }

        public void SetupLightningBolt(LightningBoltDependencies dependencies)
        {
            if (dependencies == null || dependencies.Parameters.Count == 0)
            {
                Debug.LogError("Lightning bolt dependencies must not be null");
                return;
            }
            else if (this.dependencies != null)
            {
                Debug.LogError("This lightning bolt is already in use!");
                return;
            }

            this.dependencies = dependencies;
            CameraMode = dependencies.CameraMode;
            timeSinceLevelLoad = LightningBoltScript.TimeSinceStart;
            CheckForGlow(dependencies.Parameters);
            MinimumDelay = float.MaxValue;

#if !UNITY_WEBGL

            if (dependencies.ThreadState != null)
            {
                startTimeOffset = DateTime.UtcNow;
                dependencies.ThreadState.AddActionForBackgroundThread(ProcessAllLightningParameters);
            }
            else

#endif

            {
                ProcessAllLightningParameters();
            }
        }

        public bool Update()
        {
            elapsedTime += LightningBoltScript.DeltaTime;
            if (elapsedTime > maxLifeTime)
            {
                return false;
            }
            else if (hasLight)
            {
                UpdateLights();
            }
            return true;
        }

        public void Cleanup()
        {
            foreach (LightningBoltSegmentGroup g in segmentGroupsWithLight)
            {
                // cleanup lights
                foreach (Light l in g.Lights)
                {
                    CleanupLight(l);
                }
                g.Lights.Clear();
            }
            lock (groupCache)
            {
                foreach (LightningBoltSegmentGroup g in segmentGroups)
                {
                    groupCache.Add(g);
                }
            }
            hasLight = false;
            elapsedTime = 0.0f;
            lifeTime = 0.0f;
            maxLifeTime = 0.0f;
            if (dependencies != null)
            {
                dependencies.ReturnToCache(dependencies);
                dependencies = null;
            }

            // return all line renderers to cache
            foreach (LineRendererMesh m in activeLineRenderers)
            {
                if (m != null)
                {
                    m.Reset();
                    lineRendererCache.Add(m);
                }
            }
            segmentGroups.Clear();
            segmentGroupsWithLight.Clear();
            activeLineRenderers.Clear();
        }

        public LightningBoltSegmentGroup AddGroup()
        {
            LightningBoltSegmentGroup group;
            lock (groupCache)
            {
                if (groupCache.Count == 0)
                {
                    group = new LightningBoltSegmentGroup();
                }
                else
                {
                    int index = groupCache.Count - 1;
                    group = groupCache[index];
                    group.Reset();
                    groupCache.RemoveAt(index);
                }
            }
            segmentGroups.Add(group);
            return group;
        }

        /// <summary>
        /// Clear out all cached objects to free up memory
        /// </summary>
        public static void ClearCache()
        {
            foreach (LineRendererMesh obj in lineRendererCache)
            {
                if (obj != null)
                {
                    GameObject.Destroy(obj.GameObject);
                }
            }
            foreach (Light obj in lightCache)
            {
                if (obj != null)
                {
                    GameObject.Destroy(obj.gameObject);
                }
            }
            lineRendererCache.Clear();
            lightCache.Clear();
            lock (groupCache)
            {
                groupCache.Clear();
            }
        }

        #endregion Public methods

        #region Private variables

        // required dependencies to create lightning bolts
        private LightningBoltDependencies dependencies;

        // how long this bolt has been alive
        private float elapsedTime;

        // total life span of this bolt
        private float lifeTime;

        // either lifeTime or larger depending on if lights are lingering beyond the end of the bolt
        private float maxLifeTime;

        // does this lightning bolt have light?
        private bool hasLight;

        // saved in case of threading
        private float timeSinceLevelLoad;

        private readonly List<LightningBoltSegmentGroup> segmentGroups = new List<LightningBoltSegmentGroup>();
        private readonly List<LightningBoltSegmentGroup> segmentGroupsWithLight = new List<LightningBoltSegmentGroup>();
        private readonly List<LineRendererMesh> activeLineRenderers = new List<LineRendererMesh>();

        private static int lightCount;
        private static readonly List<LineRendererMesh> lineRendererCache = new List<LineRendererMesh>();
        private static readonly List<LightningBoltSegmentGroup> groupCache = new List<LightningBoltSegmentGroup>();
        private static readonly List<Light> lightCache = new List<Light>();

        #endregion Private variables

        #region Private methods

        private void CleanupLight(Light l)
        {
            if (l != null)
            {
                dependencies.LightRemoved(l);
                lightCache.Add(l);
                l.gameObject.SetActive(false);
                lightCount--;
            }
        }

        private void EnableLineRenderer(LineRendererMesh lineRenderer, int tag)
        {
            bool shouldPopulate = (lineRenderer != null && lineRenderer.GameObject != null && lineRenderer.Tag == tag && IsActive);
            if (shouldPopulate)
            {
                lineRenderer.PopulateMesh();
            }
        }

        private IEnumerator EnableLastRendererCoRoutine()
        {
            LineRendererMesh lineRenderer = activeLineRenderers[activeLineRenderers.Count - 1];
            int tag = ++lineRenderer.Tag; // in case it gets cleaned up for later

            yield return new WaitForSecondsLightning(MinimumDelay);

            EnableLineRenderer(lineRenderer, tag);
        }

        private LineRendererMesh GetOrCreateLineRenderer()
        {
            LineRendererMesh lineRenderer;

            while (true)
            {
                if (lineRendererCache.Count == 0)
                {
                    lineRenderer = new LineRendererMesh();
                }
                else
                {
                    int index = lineRendererCache.Count - 1;
                    lineRenderer = lineRendererCache[index];
                    lineRendererCache.RemoveAt(index);
                    if (lineRenderer == null || lineRenderer.Transform == null)
                    {
                        // destroyed by some other means, try again for cache...
                        continue;
                    }
                }
                break;
            }

            // clear parent - this ensures that the rotation and scale can be reset before assigning a new parent
            lineRenderer.Transform.parent = null;
            lineRenderer.Transform.rotation = Quaternion.identity;
            lineRenderer.Transform.localScale = Vector3.one;
            lineRenderer.Transform.parent = dependencies.Parent.transform;
            lineRenderer.GameObject.layer = dependencies.Parent.layer; // maintain the layer of the parent

            if (dependencies.UseWorldSpace)
            {
                lineRenderer.GameObject.transform.position = Vector3.zero;
            }
            else
            {
                lineRenderer.GameObject.transform.localPosition = Vector3.zero;
            }

            lineRenderer.Material = (HasGlow ? dependencies.LightningMaterialMesh : dependencies.LightningMaterialMeshNoGlow);
            if (!string.IsNullOrEmpty(dependencies.SortLayerName))
            {
                lineRenderer.MeshRenderer.sortingLayerName = dependencies.SortLayerName;
                lineRenderer.MeshRenderer.sortingOrder = dependencies.SortOrderInLayer;
            }
            else
            {
                lineRenderer.MeshRenderer.sortingLayerName = null;
                lineRenderer.MeshRenderer.sortingOrder = 0;
            }

            activeLineRenderers.Add(lineRenderer);

            return lineRenderer;
        }

        private void RenderGroup(LightningBoltSegmentGroup group, LightningBoltParameters p)
        {
            if (group.SegmentCount == 0)
            {
                return;
            }

#if UNITY_WEBGL

            float timeOffset = 0.0f;

#else

            float timeOffset = (dependencies.ThreadState == null ? 0.0f : (float)(DateTime.UtcNow - startTimeOffset).TotalSeconds);

#endif

            float timeStart = timeSinceLevelLoad + group.Delay + timeOffset;
            Vector4 fadeLifeTime = new Vector4(timeStart, timeStart + group.PeakStart, timeStart + group.PeakEnd, timeStart + group.LifeTime);
            float radius = group.LineWidth * 0.5f * LightningBoltParameters.Scale;
            int lineCount = (group.Segments.Count - group.StartIndex);
            float radiusStep = (radius - (radius * group.EndWidthMultiplier)) / (float)lineCount;

            // growth multiplier
            float timeStep;
            if (p.GrowthMultiplier > 0.0f)
            {
                timeStep = (group.LifeTime / (float)lineCount) * p.GrowthMultiplier;
                timeOffset = 0.0f;
            }
            else
            {
                timeStep = 0.0f;
                timeOffset = 0.0f;
            }

            LineRendererMesh currentLineRenderer = (activeLineRenderers.Count == 0 ? GetOrCreateLineRenderer() : activeLineRenderers[activeLineRenderers.Count - 1]);

            // if we have filled up the mesh, we need to start a new line renderer
            if (!currentLineRenderer.PrepareForLines(lineCount))
            {
                if (currentLineRenderer.CustomTransform != null)
                {
                    // can't create multiple meshes if using a custom transform callback
                    return;
                }

#if !UNITY_WEBGL

                if (dependencies.ThreadState != null)
                {
                    // we need to block until this action is run, Unity objects can only be modified and created on the main thread
                    dependencies.ThreadState.AddActionForMainThread(() =>
                    {
                        EnableCurrentLineRenderer();
                        currentLineRenderer = GetOrCreateLineRenderer();
                    }, true);
                }
                else

#endif

                {
                    EnableCurrentLineRenderer();
                    currentLineRenderer = GetOrCreateLineRenderer();
                }
            }

            currentLineRenderer.BeginLine(group.Segments[group.StartIndex].Start, group.Segments[group.StartIndex].End, radius, group.Color, p.Intensity, fadeLifeTime, p.GlowWidthMultiplier, p.GlowIntensity);
            for (int i = group.StartIndex + 1; i < group.Segments.Count; i++)
            {
                radius -= radiusStep;
                if (p.GrowthMultiplier < 1.0f)
                {
                    timeOffset += timeStep;
                    fadeLifeTime = new Vector4(timeStart + timeOffset, timeStart + group.PeakStart + timeOffset, timeStart + group.PeakEnd, timeStart + group.LifeTime);
                }
                currentLineRenderer.AppendLine(group.Segments[i].Start, group.Segments[i].End, radius, group.Color, p.Intensity, fadeLifeTime, p.GlowWidthMultiplier, p.GlowIntensity);
            }
        }

        private static IEnumerator NotifyBolt(LightningBoltDependencies dependencies, LightningBoltParameters p, Transform transform, Vector3 start, Vector3 end)
        {
            float delay = p.delaySeconds;
            float lifeTime = p.LifeTime;
            yield return new WaitForSecondsLightning(delay);
            if (dependencies.LightningBoltStarted != null)
            {
                dependencies.LightningBoltStarted(p, start, end);
            }
            LightningCustomTransformStateInfo state = (p.CustomTransform == null ? null : LightningCustomTransformStateInfo.GetOrCreateStateInfo());
            if (state != null)
            {
                state.Parameters = p;
                state.BoltStartPosition = start;
                state.BoltEndPosition = end;
                state.State = LightningCustomTransformState.Started;
                state.Transform = transform;
                p.CustomTransform(state);
                state.State = LightningCustomTransformState.Executing;
            }

            if (p.CustomTransform == null)
            {
                yield return new WaitForSecondsLightning(lifeTime);
            }
            else
            {
                while (lifeTime > 0.0f)
                {
                    p.CustomTransform(state);
                    lifeTime -= LightningBoltScript.DeltaTime;
                    yield return null;
                }
            }

            if (p.CustomTransform != null)
            {
                state.State = LightningCustomTransformState.Ended;
                p.CustomTransform(state);
                LightningCustomTransformStateInfo.ReturnStateInfoToCache(state);
            }
            if (dependencies.LightningBoltEnded != null)
            {
                dependencies.LightningBoltEnded(p, start, end);
            }
            LightningBoltParameters.ReturnParametersToCache(p);
        }

        private void ProcessParameters(LightningBoltParameters p, RangeOfFloats delay, LightningBoltDependencies depends)
        {
            Vector3 start, end;
            MinimumDelay = Mathf.Min(delay.Minimum, MinimumDelay);
            p.delaySeconds = delay.Random(p.Random);

            // apply LOD if specified
            if (depends.LevelOfDetailDistance > Mathf.Epsilon)
            {
                float d;
                if (p.Points.Count > 1)
                {
                    d = Vector3.Distance(depends.CameraPos, p.Points[0]);
                    d = Mathf.Min(Vector3.Distance(depends.CameraPos, p.Points[p.Points.Count - 1]));
                }
                else
                {
                    d = Vector3.Distance(depends.CameraPos, p.Start);
                    d = Mathf.Min(Vector3.Distance(depends.CameraPos, p.End));
                }
                int modifier = Mathf.Min(8, (int)(d / depends.LevelOfDetailDistance));
                p.Generations = Mathf.Max(1, p.Generations - modifier);
                p.GenerationWhereForksStopSubtractor = Mathf.Clamp(p.GenerationWhereForksStopSubtractor - modifier, 0, 8);
            }

            p.generationWhereForksStop = p.Generations - p.GenerationWhereForksStopSubtractor;
            lifeTime = Mathf.Max(p.LifeTime + p.delaySeconds, lifeTime);
            maxLifeTime = Mathf.Max(lifeTime, maxLifeTime);
            p.forkednessCalculated = (int)Mathf.Ceil(p.Forkedness * (float)p.Generations);
            if (p.Generations > 0)
            {
                p.Generator = p.Generator ?? LightningGenerator.GeneratorInstance;
                p.Generator.GenerateLightningBolt(this, p, out start, out end);
                p.Start = start;
                p.End = end;
            }
        }

        private void ProcessAllLightningParameters()
        {
            int maxLightsForEachParameters = MaximumLightsPerBatch / dependencies.Parameters.Count;
            RangeOfFloats delay = new RangeOfFloats();
            List<int> groupIndexes = new List<int>(dependencies.Parameters.Count + 1);
            int i = 0;

#if ENABLE_PROFILING

            System.Diagnostics.Stopwatch w = System.Diagnostics.Stopwatch.StartNew();

#endif

            foreach (LightningBoltParameters parameters in dependencies.Parameters)
            {
                delay.Minimum = parameters.DelayRange.Minimum + parameters.Delay;
                delay.Maximum = parameters.DelayRange.Maximum + parameters.Delay;
                parameters.maxLights = maxLightsForEachParameters;
                groupIndexes.Add(segmentGroups.Count);
                ProcessParameters(parameters, delay, dependencies);
            }
            groupIndexes.Add(segmentGroups.Count);

#if ENABLE_PROFILING

            w.Stop();
            UnityEngine.Debug.LogFormat("GENERATE: {0}", w.Elapsed.TotalMilliseconds);
            w.Reset();
            w.Start();

#endif

            LightningBoltDependencies dependenciesRef = dependencies;
            foreach (LightningBoltParameters parameters in dependenciesRef.Parameters)
            {
                Transform transform = RenderLightningBolt(parameters.quality, parameters.Generations, groupIndexes[i], groupIndexes[++i], parameters);

#if !UNITY_WEBGL

                if (dependenciesRef.ThreadState != null)
                {
                    dependenciesRef.ThreadState.AddActionForMainThread(() =>
                    {
                        dependenciesRef.StartCoroutine(NotifyBolt(dependenciesRef, parameters, transform, parameters.Start, parameters.End));
                    });
                }
                else

#endif

                {
                    dependenciesRef.StartCoroutine(NotifyBolt(dependenciesRef, parameters, transform, parameters.Start, parameters.End));
                }
            }

#if ENABLE_PROFILING

            w.Stop();
            UnityEngine.Debug.LogFormat("RENDER: {0}", w.Elapsed.TotalMilliseconds);

#endif

#if !UNITY_WEBGL

            if (dependencies.ThreadState != null)
            {
                dependencies.ThreadState.AddActionForMainThread(EnableCurrentLineRendererFromThread);
            }
            else

#endif

            {
                EnableCurrentLineRenderer();
                dependencies.AddActiveBolt(this);
            }
        }

        private void EnableCurrentLineRendererFromThread()
        {
            EnableCurrentLineRenderer();

#if !UNITY_WEBGL

            // clear the thread state, we verify in the Cleanup method that this is nulled out to ensure we are not cleaning up lightning that is still being generated
            dependencies.ThreadState = null;

#endif

            dependencies.AddActiveBolt(this);
        }

        private void EnableCurrentLineRenderer()
        {
            if (activeLineRenderers.Count == 0)
            {
                return;
            }
            // make sure the last renderer gets enabled at the appropriate time
            else if (MinimumDelay <= 0.0f)
            {
                EnableLineRenderer(activeLineRenderers[activeLineRenderers.Count - 1], activeLineRenderers[activeLineRenderers.Count - 1].Tag);
            }
            else
            {
                dependencies.StartCoroutine(EnableLastRendererCoRoutine());
            }
        }

        private void RenderParticleSystems(Vector3 start, Vector3 end, float trunkWidth, float lifeTime, float delaySeconds)
        {
            // only emit particle systems if we have a trunk - example, cloud lightning should not emit particles
            if (trunkWidth > 0.0f)
            {
                if (dependencies.OriginParticleSystem != null)
                {
                    // we have a strike, create a particle where the lightning is coming from
                    dependencies.StartCoroutine(GenerateParticleCoRoutine(dependencies.OriginParticleSystem, start, delaySeconds));
                }
                if (dependencies.DestParticleSystem != null)
                {
                    dependencies.StartCoroutine(GenerateParticleCoRoutine(dependencies.DestParticleSystem, end, delaySeconds + (lifeTime * 0.8f)));
                }
            }
        }

        private Transform RenderLightningBolt(LightningBoltQualitySetting quality, int generations, int startGroupIndex, int endGroupIndex, LightningBoltParameters parameters)
        {
            if (segmentGroups.Count == 0 || startGroupIndex >= segmentGroups.Count || endGroupIndex > segmentGroups.Count)
            {
                return null;
            }

            Transform transform = null;
            LightningLightParameters lp = parameters.LightParameters;
            if (lp != null)
            {
                if ((hasLight |= lp.HasLight))
                {
                    lp.LightPercent = Mathf.Clamp(lp.LightPercent, Mathf.Epsilon, 1.0f);
                    lp.LightShadowPercent = Mathf.Clamp(lp.LightShadowPercent, 0.0f, 1.0f);
                }
                else
                {
                    lp = null;
                }
            }

            LightningBoltSegmentGroup mainTrunkGroup = segmentGroups[startGroupIndex];
            Vector3 start = mainTrunkGroup.Segments[mainTrunkGroup.StartIndex].Start;
            Vector3 end = mainTrunkGroup.Segments[mainTrunkGroup.StartIndex + mainTrunkGroup.SegmentCount - 1].End;
            parameters.FadePercent = Mathf.Clamp(parameters.FadePercent, 0.0f, 0.5f);

            // create a new line renderer mesh right now if we have a custom transform
            if (parameters.CustomTransform != null)
            {
                LineRendererMesh currentLineRenderer = (activeLineRenderers.Count == 0 || !activeLineRenderers[activeLineRenderers.Count - 1].Empty ? null : activeLineRenderers[activeLineRenderers.Count - 1]);

                if (currentLineRenderer == null)
                {

#if !UNITY_WEBGL

                    if (dependencies.ThreadState != null)
                    {
                        // we need to block until this action is run, Unity objects can only be modified and created on the main thread
                        dependencies.ThreadState.AddActionForMainThread(() =>
                        {
                            EnableCurrentLineRenderer();
                            currentLineRenderer = GetOrCreateLineRenderer();
                        }, true);
                    }
                    else

#endif

                    {
                        EnableCurrentLineRenderer();
                        currentLineRenderer = GetOrCreateLineRenderer();
                    }
                }
                if (currentLineRenderer == null)
                {
                    return null;
                }

                currentLineRenderer.CustomTransform = parameters.CustomTransform;
                transform = currentLineRenderer.Transform;
            }

            for (int i = startGroupIndex; i < endGroupIndex; i++)
            {
                LightningBoltSegmentGroup group = segmentGroups[i];
                group.Delay = parameters.delaySeconds;
                group.LifeTime = parameters.LifeTime;
                group.PeakStart = group.LifeTime * parameters.FadePercent;
                group.PeakEnd = group.LifeTime - group.PeakStart;
                float peakGap = group.PeakEnd - group.PeakStart;
                float fadeOut = group.LifeTime - group.PeakEnd;
                group.PeakStart *= parameters.FadeInMultiplier;
                group.PeakEnd = group.PeakStart + (peakGap * parameters.FadeFullyLitMultiplier);
                group.LifeTime = group.PeakEnd + (fadeOut * parameters.FadeOutMultiplier);
                group.LightParameters = lp;
                RenderGroup(group, parameters);
            }

#if !UNITY_WEBGL

            if (dependencies.ThreadState != null)
            {
                dependencies.ThreadState.AddActionForMainThread(() =>
                {
                    RenderParticleSystems(start, end, parameters.TrunkWidth, parameters.LifeTime, parameters.delaySeconds);

                    // create lights only on the main trunk
                    if (lp != null)
                    {
                        CreateLightsForGroup(segmentGroups[startGroupIndex], lp, quality, parameters.maxLights);
                    }
                });
            }
            else

#endif

            {
                RenderParticleSystems(start, end, parameters.TrunkWidth, parameters.LifeTime, parameters.delaySeconds);

                // create lights only on the main trunk
                if (lp != null)
                {
                    CreateLightsForGroup(segmentGroups[startGroupIndex], lp, quality, parameters.maxLights);
                }
            }

            return transform;
        }

        private void CreateLightsForGroup(LightningBoltSegmentGroup group, LightningLightParameters lp, LightningBoltQualitySetting quality, int maxLights)
        {
            if (lightCount == MaximumLightCount || maxLights <= 0)
            {
                return;
            }

            float fadeOutTime = (lifeTime - group.PeakEnd) * lp.FadeOutMultiplier;
            float peakGap = (group.PeakEnd - group.PeakStart) * lp.FadeFullyLitMultiplier;
            float peakStart = group.PeakStart * lp.FadeInMultiplier;
            float peakEnd = peakStart + peakGap;
            float maxLifeWithLights = peakEnd + fadeOutTime;
            maxLifeTime = Mathf.Max(maxLifeTime, group.Delay + maxLifeWithLights);

            segmentGroupsWithLight.Add(group);

            int segmentCount = group.SegmentCount;
            float lightPercent, lightShadowPercent;
            if (quality == LightningBoltQualitySetting.LimitToQualitySetting)
            {
                int level = QualitySettings.GetQualityLevel();
                LightningQualityMaximum maximum;
                if (LightningBoltParameters.QualityMaximums.TryGetValue(level, out maximum))
                {
                    lightPercent = Mathf.Min(lp.LightPercent, maximum.MaximumLightPercent);
                    lightShadowPercent = Mathf.Min(lp.LightShadowPercent, maximum.MaximumShadowPercent);
                }
                else
                {
                    Debug.LogError("Unable to read lightning quality for level " + level.ToString());
                    lightPercent = lp.LightPercent;
                    lightShadowPercent = lp.LightShadowPercent;
                }
            }
            else
            {
                lightPercent = lp.LightPercent;
                lightShadowPercent = lp.LightShadowPercent;
            }

            maxLights = Mathf.Max(1, Mathf.Min(maxLights, (int)(segmentCount * lightPercent)));
            int nthLight = Mathf.Max(1, (int)((segmentCount / maxLights)));
            int nthShadows = maxLights - (int)((float)maxLights * lightShadowPercent);
            int nthShadowCounter = nthShadows;

            // add lights evenly spaced
            for (int i = group.StartIndex + (int)(nthLight * 0.5f); i < group.Segments.Count; i += nthLight)
            {
                if (AddLightToGroup(group, lp, i, nthLight, nthShadows, ref maxLights, ref nthShadowCounter))
                {
                    return;
                }
            }

            // Debug.Log("Lightning light count: " + lightCount.ToString());
        }

        private bool AddLightToGroup(LightningBoltSegmentGroup group, LightningLightParameters lp, int segmentIndex,
            int nthLight, int nthShadows, ref int maxLights, ref int nthShadowCounter)
        {
            Light light = GetOrCreateLight(lp);
            group.Lights.Add(light);
            Vector3 pos = (group.Segments[segmentIndex].Start + group.Segments[segmentIndex].End) * 0.5f;
            if (dependencies.CameraIsOrthographic)
            {
                if (dependencies.CameraMode == CameraMode.OrthographicXZ)
                {
                    pos.y = dependencies.CameraPos.y + lp.OrthographicOffset;
                }
                else
                {
                    pos.z = dependencies.CameraPos.z + lp.OrthographicOffset;
                }
            }
            if (dependencies.UseWorldSpace)
            {
                light.gameObject.transform.position = pos;
            }
            else
            {
                light.gameObject.transform.localPosition = pos;
            }
            if (lp.LightShadowPercent == 0.0f || ++nthShadowCounter < nthShadows)
            {
                light.shadows = LightShadows.None;
            }
            else
            {
                light.shadows = LightShadows.Soft;
                nthShadowCounter = 0;
            }

            // return true if no more lights possible, false otherwise
            return (++lightCount == MaximumLightCount || --maxLights == 0);
        }

        private Light GetOrCreateLight(LightningLightParameters lp)
        {
            Light light;
            while (true)
            {
                if (lightCache.Count == 0)
                {
                    GameObject lightningLightObject = new GameObject("LightningBoltLight");

#if UNITY_EDITOR

                    lightningLightObject.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;

#endif

                    light = lightningLightObject.AddComponent<Light>();
                    light.type = LightType.Point;
                    break;
                }
                else
                {
                    light = lightCache[lightCache.Count - 1];
                    lightCache.RemoveAt(lightCache.Count - 1);
                    if (light == null)
                    {
                        // may have been disposed or the level re-loaded
                        continue;
                    }
                    break;
                }
            }

#if UNITY_4

#else

            light.bounceIntensity = lp.BounceIntensity;
            light.shadowNormalBias = lp.ShadowNormalBias;

#endif

            light.color = lp.LightColor;
            light.renderMode = lp.RenderMode;
            light.range = lp.LightRange;
            light.shadowStrength = lp.ShadowStrength;
            light.shadowBias = lp.ShadowBias;
            light.intensity = 0.0f;
            light.gameObject.transform.parent = dependencies.Parent.transform;
            light.gameObject.SetActive(true);

            dependencies.LightAdded(light);

            return light;
        }

        private void UpdateLight(LightningLightParameters lp, IEnumerable<Light> lights, float delay, float peakStart, float peakEnd, float lifeTime)
        {
            if (elapsedTime < delay)
            {
                return;
            }

            // depending on whether we have hit the mid point of our lifetime, fade the light in or out

            // adjust lights for fade parameters
            float fadeOutTime = (lifeTime - peakEnd) * lp.FadeOutMultiplier;
            float peakGap = (peakEnd - peakStart) * lp.FadeFullyLitMultiplier;
            peakStart *= lp.FadeInMultiplier;
            peakEnd = peakStart + peakGap;
            lifeTime = peakEnd + fadeOutTime;
            float realElapsedTime = elapsedTime - delay;
            if (realElapsedTime >= peakStart)
            {
                if (realElapsedTime <= peakEnd)
                {
                    // fully lit
                    foreach (Light l in lights)
                    {
                        l.intensity = lp.LightIntensity;
                    }
                }
                else
                {
                    // fading out
                    float lerp = (realElapsedTime - peakEnd) / (lifeTime - peakEnd);
                    foreach (Light l in lights)
                    {
                        l.intensity = Mathf.Lerp(lp.LightIntensity, 0.0f, lerp);
                    }
                }
            }
            else
            {
                // fading in
                float lerp = realElapsedTime / peakStart;
                foreach (Light l in lights)
                {
                    l.intensity = Mathf.Lerp(0.0f, lp.LightIntensity, lerp);
                }
            }
        }

        private void UpdateLights()
        {
            foreach (LightningBoltSegmentGroup group in segmentGroupsWithLight)
            {
                UpdateLight(group.LightParameters, group.Lights, group.Delay, group.PeakStart, group.PeakEnd, group.LifeTime);
            }
        }

        private IEnumerator GenerateParticleCoRoutine(ParticleSystem p, Vector3 pos, float delay)
        {
            yield return new WaitForSecondsLightning(delay);

            p.transform.position = pos;

#if UNITY_PRE_5_3

            p.Emit((int)p.emissionRate);

#else

            int count;
            if (p.emission.burstCount > 0)
            {
                ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[p.emission.burstCount];
                p.emission.GetBursts(bursts);
                count = UnityEngine.Random.Range(bursts[0].minCount, bursts[0].maxCount + 1);
                p.Emit(count);
            }
            else
            {

#if UNITY_5_6_OR_NEWER

                ParticleSystem.MinMaxCurve rate = p.emission.rateOverTime;

#else

                ParticleSystem.MinMaxCurve rate = p.emission.rate;

#endif

                count = (int)((rate.constantMax - rate.constantMin) * 0.5f);
                count = UnityEngine.Random.Range(count, count * 2);
                p.Emit(count);
            }

#endif

        }

        private void CheckForGlow(IEnumerable<LightningBoltParameters> parameters)
        {
            // we need to know if there is glow so we can choose the glow or non-glow setting in the renderer
            foreach (LightningBoltParameters p in parameters)
            {
                HasGlow = (p.GlowIntensity >= Mathf.Epsilon && p.GlowWidthMultiplier >= Mathf.Epsilon);

                if (HasGlow)
                {
                    break;
                }
            }
        }

#endregion Private methods
	}
}
