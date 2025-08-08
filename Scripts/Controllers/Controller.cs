using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Spacats.Utils
{
    [ExecuteInEditMode]
    [Serializable]
    public class Controller : MonoBehaviour
    {
        private bool _applicationIsQuitting = false;

        [Tooltip("Should it be executed while in editor")]
        public bool ExecuteInEditor = false;

        [Tooltip("Show basic logs, defined by inheritor (derived, child class)")]
        public bool ShowLogs = false;

        [Tooltip("Show controller logs for testing purposes, such as 'Awake', 'OnEnable' etc.")]
        public bool ShowControllerLogs = false;

        protected virtual void ControllerAwake() { RefreshName(); CheckHierarchy(); TryToShowLog("Awake", 0, true); }
        protected virtual void ControllerOnEnable() { RefreshName(); CheckHierarchy(); TryToShowLog("OnEnable", 0, true); }
        protected virtual void ControllerOnDisable() { TryToShowLog("OnDisable", 0, true); }
        protected virtual void ControllerOnDestroy() { TryToShowLog("OnDestroy", 0, true); }
        protected virtual void ControllerOnApplicationQuit() { TryToShowLog("OnApplicationQuit", 0, true); }

        /// <summary>
        /// Same as basic unity Update()
        /// </summary>
        public virtual void ControllerUpdate() { }
        /// <summary>
        /// Same as basic unity LateUpdate()
        /// </summary>
        public virtual void ControllerLateUpdate() { }
        /// <summary>
        /// Triggers every update + every scene gui. So it can work smoothly while in editor.
        /// </summary>
        public virtual void ControllerSharedUpdate() { }

#if UNITY_EDITOR
        public virtual void ControllerOnSceneGUI(SceneView sceneView) { }
#endif

        private void Awake()
        {
            ControllerAwake();
        }

        private void OnEnable()
        {
            ControllersHub.Instance.RegisterController(this);
            ControllerOnEnable();
        }

        private void OnDisable()
        {
            ControllersHub.Instance.UnRegisterController(this);
            ControllerOnDisable();
        }
        private void OnDestroy()
        {
            ControllerOnDestroy();
        }

        private void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
            ControllerOnApplicationQuit();
        }

        /// <summary>
        /// Log types: 
        /// 0 - normal message
        /// 1 - warning
        /// 2 - error
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logType"></param>
        protected virtual void TryToShowLog(string message, int logType = 0, bool isControllerLog = false)
        {
            if (isControllerLog && !ShowControllerLogs && logType!=2) return;
            if (!isControllerLog && !ShowLogs && logType != 2) return;

            string prefix = $"[Spacats Controller: {gameObject.name}] ";

            switch (logType)
            {
                default: Debug.Log(prefix + message); break;
                case 1: Debug.LogWarning(prefix + message); break;
                case 2: Debug.LogError(prefix + message); break;
            }
        }


        private void RefreshName()
        {
            gameObject.name = "[SpaCats] " + GetType().Name;
        }

        protected virtual void CheckHierarchy()
        {
            Transform hubTransform = ControllersHub.Instance.transform;
            if (transform.parent != hubTransform) transform.parent = hubTransform;
        }
    }
}
