using UnityEngine;
using System;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Spacats.Utils
{
    [ExecuteInEditMode]
    [Serializable]
    public class Controller : MonoBehaviour
    {
        [Header("Controller settings")]
        public string UniqueTag = "";
        protected bool _applicationIsQuitting = false;
        protected bool _registered = false;

        [Tooltip("Should it be executed while in editor")]
        public bool ExecuteInEditor = false;

        [Tooltip("Show basic logs, defined by inheritor (derived, child class)")]
        public bool ShowLogs = false;

        [Tooltip("Show controller logs for testing purposes, such as 'Awake', 'OnEnable' etc.")]
        public bool ShowControllerLogs = false;

        protected virtual void ControllerAwake() { TryToShowLog("Awake", 0, true); }
        protected virtual void ControllerOnEnable() { TryToShowLog("OnEnable", 0, true); }
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
            RefreshName(); 
            CheckHierarchy();
            SceneManagerHelper.MarkActiveSceneDirty();
            if (!ExecuteInEditor && !Application.isPlaying) return;
            ControllerAwake();
        }

        private void OnEnable()
        {
            bool registerResult = ControllersHub.Instance.RegisterController(this);
            if (!registerResult)
            {
                TryToShowLog("Already registered!", 1, true);

                if (!Application.isPlaying)
                {
                    DestroyImmediate(gameObject);
                }
                else
                {
                    Destroy(gameObject);
                }
                return;
            }

            _registered = true;

            RefreshName(); 
            CheckHierarchy();

            if (!ExecuteInEditor && !Application.isPlaying) return;
            ControllerOnEnable();
        }

        private void OnDisable()
        {
            if (_registered) ControllersHub.Instance.UnRegisterController(this);
            if (!ExecuteInEditor && !Application.isPlaying) return;
            ControllerOnDisable();
        }
        private void OnDestroy()
        {
            if (!ExecuteInEditor && !Application.isPlaying) return;
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

            string prefix = $"[Spacats Controller: {gameObject.name} {UniqueTag}] ";

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
