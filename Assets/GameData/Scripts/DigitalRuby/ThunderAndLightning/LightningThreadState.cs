using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DigitalRuby.ThunderAndLightning
{
	public class LightningThreadState
	{

#if TASK_AVAILABLE

        private Task lightningThread;

#else

        /// <summary>
        /// Lightning thread
        /// </summary>
        private Thread lightningThread;

#endif

        /// <summary>
        /// Lightning thread event to notify background action available
        /// </summary>
        private AutoResetEvent lightningThreadEvent = new AutoResetEvent(false);

        /// <summary>
        /// List of background actions
        /// </summary>
        private readonly Queue<System.Action> actionsForBackgroundThread = new Queue<System.Action>();

        /// <summary>
        /// List of main thread actions and optional events to signal
        /// </summary>
        private readonly Queue<KeyValuePair<System.Action, ManualResetEvent>> actionsForMainThread = new Queue<KeyValuePair<System.Action, ManualResetEvent>>();

        /// <summary>
        /// Set to false to terminate
        /// </summary>
        public bool Running = true;

        private bool isTerminating;

        private bool UpdateMainThreadActionsOnce()
        {
            KeyValuePair<System.Action, ManualResetEvent> kv;
            lock (actionsForMainThread)
            {
                if (actionsForMainThread.Count == 0)
                {
                    return false;
                }
                kv = actionsForMainThread.Dequeue();
            }
            kv.Key();
            if (kv.Value != null)
            {
                kv.Value.Set();
            }
            return true;
        }

        private void BackgroundThreadMethod()
        {
            System.Action action = null;
            while (Running)
            {
                try
                {
                    if (!lightningThreadEvent.WaitOne(500))
                    {
                        continue;
                    }

                    tryActionAgain:
                    lock (actionsForBackgroundThread)
                    {
                        if (actionsForBackgroundThread.Count == 0)
                        {
                            continue;
                        }
                        action = actionsForBackgroundThread.Dequeue();
                    }
                    action();
                    goto tryActionAgain;
                }
                catch (ThreadAbortException)
                {

                }
                catch (Exception ex)
                {
                    Debug.LogErrorFormat("Lightning thread exception: {0}", ex);
                }
            }
        }

        /// <summary>
        /// Constructor - starts the thread
        /// </summary>
        public LightningThreadState()
        {

#if TASK_AVAILABLE

            lightningThread = Task.Factory.StartNew(BackgroundThreadMethod);

#else

            lightningThread = new Thread(new ThreadStart(BackgroundThreadMethod))
            {
                IsBackground = true,
                Name = "LightningBoltScriptThread"
            };
            lightningThread.Start();

#endif

        }

        /// <summary>
        /// Terminate and wait for thread end
        /// </summary>
        public void TerminateAndWaitForEnd()
        {
            isTerminating = true;
            while (true)
            {
                if (!UpdateMainThreadActionsOnce())
                {
                    lock (actionsForBackgroundThread)
                    {
                        if (actionsForBackgroundThread.Count == 0)
                        {
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Execute any main thread actions from the main thread
        /// </summary>
        public void UpdateMainThreadActions()
        {
            while (UpdateMainThreadActionsOnce()) { }
        }

        /// <summary>
        /// Add a main thread action
        /// </summary>
        /// <param name="action">Action</param>
        /// <param name="waitForAction">True to wait for completion, false if not</param>
        /// <returns>True if action added, false if in process of terminating the thread</returns>
        public bool AddActionForMainThread(System.Action action, bool waitForAction = false)
        {
            if (isTerminating)
            {
                return false;
            }
            ManualResetEvent evt = (waitForAction ? new ManualResetEvent(false) : null);
            lock (actionsForMainThread)
            {
                actionsForMainThread.Enqueue(new KeyValuePair<System.Action, ManualResetEvent>(action, evt));
            }
            if (evt != null)
            {
                evt.WaitOne(10000);
            }
            return true;
        }

        /// <summary>
        /// Add a background thread action
        /// </summary>
        /// <param name="action">Action</param>
        /// <returns>True if action added, false if in process of terminating the thread</returns>
        public bool AddActionForBackgroundThread(System.Action action)
        {
            if (isTerminating)
            {
                return false;
            }
            lock (actionsForBackgroundThread)
            {
                actionsForBackgroundThread.Enqueue(action);
            }
            lightningThreadEvent.Set();
            return true;
        }
	}
}
