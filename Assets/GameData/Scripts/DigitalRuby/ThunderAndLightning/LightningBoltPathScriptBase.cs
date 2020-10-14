using System.Collections.Generic;
using UnityEngine;

namespace DigitalRuby.ThunderAndLightning
{
	public abstract class LightningBoltPathScriptBase : LightningBoltPrefabScriptBase
	{
        [Header("Lightning Path Properties")]
        [Tooltip("The game objects to follow for the lightning path")]
        public ReorderableList_GameObject LightningPath;
        private readonly List<GameObject> currentPathObjects = new List<GameObject>();

#if UNITY_EDITOR

        [System.NonSerialized]
        private bool gizmosCleanedUp;

        [System.NonSerialized]
        private readonly List<GameObject> lastGizmos = new List<GameObject>();

        private void DoGizmoCleanup()
        {
            if (gizmosCleanedUp)
            {
                return;
            }

            gizmosCleanedUp = true;

            foreach (var obj in Resources.FindObjectsOfTypeAll(typeof(LightningGizmoScript)))
            {
                LightningGizmoScript s;
                s = obj as LightningGizmoScript;
                if (s == null)
                {
                    GameObject gObj = obj as GameObject;
                    if (gObj != null)
                    {
                        s = gObj.GetComponent<LightningGizmoScript>();
                    }
                }
                if (s != null)
                {
                    GameObject.DestroyImmediate(s, true);
                }
            }
        }

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            DoGizmoCleanup();

#if SHOW_LIGHTNING_PATH

            bool noLightningPath = (LightningPath == null || LightningPath.Count == 0);

#else

            bool noLightningPath = true;            

#endif

            // remove any objects that were taken out of the list and cleanup the gizmo script
            for (int i = lastGizmos.Count - 1; i >= 0; i--)
            {
                if (noLightningPath || !LightningPath.List.Contains(lastGizmos[i]))
                {
                    if (lastGizmos[i] != null)
                    {
                        LightningGizmoScript s = lastGizmos[i].GetComponent<LightningGizmoScript>();
                        if (s != null)
                        {
                            GameObject.DestroyImmediate(s, true);
                        }
                    }
                    lastGizmos.RemoveAt(i);
                }
            }

            // no objects, we are done
            if (noLightningPath)
            {
                return;
            }

            // add gizmo scripts and draw lines as needed
            Vector3 gizmoPosition;
            HashSet<GameObject> gizmos = new HashSet<GameObject>();
            Vector3? previousPoint = null;
            LightningGizmoScript gizmoScript;
            lastGizmos.Clear();

            for (int index = 0; index < LightningPath.List.Count; index++)
            {
                GameObject o = LightningPath.List[index];
                if (o == null || !o.activeInHierarchy)
                {
                    continue;
                }
                else if ((gizmoScript = o.GetComponent<LightningGizmoScript>()) == null)
                {
                    // we need to add the gizmo script so that this object can be selectable by tapping on the lightning bolt in the scene view
                    gizmoScript = o.AddComponent<LightningGizmoScript>();
                }
                gizmoScript.hideFlags = HideFlags.HideInInspector;

                // setup label based on whether we've seen this one before
                if (gizmos.Add(o))
                {
                    gizmoScript.Label = index.ToString();
                }
                else
                {
                    gizmoScript.Label += ", " + index.ToString();
                }

                gizmoPosition = o.transform.position;
                if (previousPoint != null && previousPoint.Value != gizmoPosition)
                {
                    // draw a line and arrow in the proper direction
                    Gizmos.DrawLine(previousPoint.Value, gizmoPosition);
                    Vector3 direction = (gizmoPosition - previousPoint.Value);
                    Vector3 center = (previousPoint.Value + gizmoPosition) * 0.5f;
                    float arrowSize = Mathf.Min(1.0f, direction.magnitude) * 2.0f;

#if UNITY_5_6_OR_NEWER

                    UnityEditor.Handles.ArrowHandleCap(0, center, Quaternion.LookRotation(direction), arrowSize, EventType.Repaint);

#else

                    UnityEditor.Handles.ArrowCap(0, center, Quaternion.LookRotation(direction), arrowSize);

#endif

                }

                previousPoint = gizmoPosition;
                lastGizmos.Add(o);
            }
        }

#endif

                    /// <summary>
                    /// Get the game objects in the path currently - null or inactive objects are not returned
                    /// </summary>
                    /// <returns>List of game objects in the path</returns>
                    protected List<GameObject> GetCurrentPathObjects()
        {
            currentPathObjects.Clear();
            if (LightningPath != null)
            {
                foreach (GameObject obj in LightningPath.List)
                {
                    if (obj != null && obj.activeInHierarchy)
                    {
                        currentPathObjects.Add(obj);
                    }
                }
            }
            return currentPathObjects;
        }

        /// <summary>
        /// Create lightning bolt path parameters
        /// </summary>
        /// <returns>Lightning bolt path parameters</returns>
        protected override LightningBoltParameters OnCreateParameters()
        {
            LightningBoltParameters p = base.OnCreateParameters();
            p.Generator = LightningGeneratorPath.GeneratorInstance;
            return p;
        }
	}
}
