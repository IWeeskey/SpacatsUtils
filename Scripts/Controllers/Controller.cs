using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

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

        [Tooltip("Should it be executed while in editor")]
        public bool ExecuteInEditor = false;

        [Tooltip("Show basic logs, defined by inheritor (derived, child class)")]
        public bool ShowLogs = false;

        [Tooltip("Show controller logs for testing purposes, such as 'Awake', 'OnEnable' etc.")]
        public bool ShowCLogs = false;

        [Tooltip("If the list is empty, this controller will persist across all scenes. " +
            "Otherwise, it will automatically be destroyed when loading a scene whose name is not in the list.")]
        public List<string> PersistsAtScenes = new List<string>();

        protected bool _applicationIsQuitting = false;
        protected bool _registered = false;
        protected virtual void CAwake() { TryToShowLog("Awake", LogType.Log, true); }
        protected virtual void COnEnable() { TryToShowLog("OnEnable", LogType.Log, true); }
        protected virtual void COnDisable() { TryToShowLog("OnDisable", LogType.Log, true); }
        protected virtual void COnDestroy() { TryToShowLog("OnDestroy", LogType.Log, true); }
        protected virtual void COnApplicationQuit() { TryToShowLog("OnApplicationQuit", LogType.Log, true); }
        protected virtual void COnRegister() { TryToShowLog("OnRegister", LogType.Log, true); }
        public virtual void COnSceneUnloading(Scene scene) { TryToShowLog("OnSceneUnloading " + scene.name, LogType.Log, true); }
        public virtual void COnSceneLoaded(Scene scene, LoadSceneMode mode) { TryToShowLog("OnSceneLoaded " + scene.name, LogType.Log, true); }



        /// <summary>
        /// Same as basic unity Update()
        /// </summary>
        public virtual void CUpdate() { }
        /// <summary>
        /// Same as basic unity LateUpdate()
        /// </summary>
        public virtual void CLateUpdate() { }
        /// <summary>
        /// Triggers every update + every scene gui. So it can work smoothly while in editor.
        /// </summary>
        public virtual void CSharedUpdate() { }

#if UNITY_EDITOR
        public virtual void COnSceneGUI(SceneView sceneView) { }
#endif
        private void Awake()
        {
            RefreshName(); 
            CheckHierarchy();
            SceneLoaderHelper.MarkActiveSceneDirty();
            if (!ExecuteInEditor && !Application.isPlaying) return;
            CAwake();
            TryRegister();
        }

        private void OnEnable()
        {
            TryRegister();
            RefreshName(); 
            CheckHierarchy();

            if (!ExecuteInEditor && !Application.isPlaying) return;
            COnEnable();
        }
        private void TryRegister()
        {
            if (_registered) return;

            bool registerResult = ControllersHub.Instance.RegisterController(this);
            if (!registerResult)
            {
                TryToShowLog("Already registered!", LogType.Warning, true);

                DestroyController();
                return;
            }
            _registered = true;
            COnRegister();
        }

        private void OnDisable()
        {
            if (_registered) ControllersHub.Instance.UnRegisterController(this);
            if (!ExecuteInEditor && !Application.isPlaying) return;
            COnDisable();
        }
        private void OnDestroy()
        {
            if (!ExecuteInEditor && !Application.isPlaying) return;
            COnDestroy();
        }

        private void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
            COnApplicationQuit();
        }

        protected virtual void TryToShowLog(string message, LogType logType, bool isControllerLog = false)
        {
            if (isControllerLog && !ShowCLogs && 
                logType != LogType.Error && logType != LogType.Exception) return;

            if (!isControllerLog && !ShowLogs &&
               (logType != LogType.Error && logType != LogType.Exception)) return;

            string prefix = $"[Spacats Controller: {gameObject.name} {UniqueTag}] ";

            switch (logType)
            {
                default: Debug.Log(prefix + message); break;
                case LogType.Warning:
                case LogType.Assert:
                    Debug.LogWarning(prefix + message); break;
                case LogType.Error:
                case LogType.Exception:
                    Debug.LogError(prefix + message); break;
            }
        }


        private void RefreshName()
        {
            gameObject.name = "[SpaCats] " + GetType().Name + " " + UniqueTag;
        }

        protected virtual void CheckHierarchy()
        {
            Transform hubTransform = ControllersHub.Instance.transform;
            if (transform.parent != hubTransform) transform.parent = hubTransform;
            transform.position = Vector3.zero;
            transform.localScale = Vector3.one;
            transform.eulerAngles = Vector3.zero;
        }

        public void ExternalOnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (PersistsAtScenes.Count == 0)
            {
                COnSceneLoaded(scene, mode);
                return;
            }


            string loadedSceneName = scene.name;
            bool found = false;

            foreach (string sName in PersistsAtScenes)
            {
                if (string.Equals(sName, loadedSceneName))
                {
                    found = true;
                    break;
                }
            }

            if (found) COnSceneLoaded(scene, mode);
            else DestroyController();
        }

        public void DestroyController()
        {
            ControllersHub.Instance.UnRegisterController(this);
            if (!Application.isPlaying)
            {
                DestroyImmediate(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

    }
}
