using UnityEngine;
using System;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Spacats.Utils
{
    [ExecuteInEditMode]
    [Serializable]
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static T _instance;
        protected bool _applicationIsQuitting = false;
        public static bool HasInstance => _instance != null;

        protected virtual void SAwake() { CheckHierarchy(); TryToShowLog("Awake", LogType.Log, true); }
        protected virtual void SOnEnable() { CheckHierarchy(); TryToShowLog("OnEnable", LogType.Log, true); }
        protected virtual void SOnDisable() { CheckHierarchy(); TryToShowLog("OnDisable", LogType.Log, true); }
        protected virtual void SOnDestroy() { TryToShowLog("OnDestroy", LogType.Log, true); }
        protected virtual void SOnApplicationQuit() {  TryToShowLog("OnApplicationQuit", LogType.Log, true);}
        protected virtual void SSetDefaultParameters() { TryToShowLog("SetDefaultParameters", LogType.Log, true); }
        protected virtual void SOnSceneUnloading(Scene scene) { TryToShowLog("OnSceneUnloading", LogType.Log, true); }
        protected virtual void SOnSceneLoaded(Scene scene, LoadSceneMode mode) { TryToShowLog("OnSceneLoaded", LogType.Log, true); }
        /// <summary>
        /// Same as basic unity Update()
        /// </summary>
        protected virtual void SUpdate() { }
        /// <summary>
        /// Same as basic unity LateUpdate()
        /// </summary>
        protected virtual void SLateUpdate() { }
        /// <summary>
        /// Triggers every update + every scene gui. So it can work smoothly while in editor.
        /// </summary>
        protected virtual void SSharedUpdate(bool isGuiCall = false) { }

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<T>();

                    if (_instance == null)
                    {
                        GameObject go = new GameObject();
                        go.name = typeof(T).Name + "";
                        _instance = go.AddComponent<T>();
                        _instance.SSetDefaultParameters();
                    }
                }
                return _instance;
            }
        }

        public bool IsInstance => Instance == this;

        [Tooltip("Set this gameobject to be always on top in hierarchy")]
        public bool AlwaysOnTop = false;

        [Tooltip("Show basic logs, defined by inheritor (derived, child class)")]
        public bool ShowLogs = false;

        [Tooltip("Show singleton logs for testing purposes, such as 'SingletonAwake', 'SingletonOnEnable' etc.")]
        public bool ShowSLogs = false;

#if UNITY_EDITOR
        protected virtual void SingletonOnSceneGUI(SceneView sceneView) { }
#endif

        private void Awake()
        {
            if (_instance == null || _applicationIsQuitting)
            {
                _instance = this as T;
                _applicationIsQuitting = false;
                if (Application.isPlaying) DontDestroyOnLoad(gameObject);
                SAwake();
                SceneLoaderHelper.MarkActiveSceneDirty();
                return;
            }
            if (IsInstance) return;
            TryToShowLog("Trying to instantiate a second instance. Destroying the new one.",  LogType.Warning, true);

            if (!Application.isPlaying)
            {
                DestroyImmediate(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            if (!IsInstance) return;
            SOnEnable();
            SceneManager.sceneUnloaded += HandleSceneUnloaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
#if UNITY_EDITOR
            SceneView.duringSceneGui += DuringSceneGui;
#endif
        }

        private void OnDisable()
        {
            if (!IsInstance) return;
            SOnDisable();
            SceneManager.sceneUnloaded -= HandleSceneUnloaded;
            SceneManager.sceneLoaded -= HandleSceneLoaded;
#if UNITY_EDITOR
            SceneView.duringSceneGui -= DuringSceneGui;
#endif
        }

#if UNITY_EDITOR
        private void DuringSceneGui(SceneView sView)
        {
            SingletonOnSceneGUI(sView);
            SSharedUpdate(true);
        }
#endif

        private void HandleSceneUnloaded(Scene scene)
        {
            SOnSceneUnloading(scene);
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SOnSceneLoaded(scene, mode);
        }

        private void Update()
        {
            if (!IsInstance) return;
            SUpdate();
            SSharedUpdate(false);
        }

        private void LateUpdate()
        {
            if (!IsInstance) return;
            SLateUpdate();
        }

        private void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
            SOnApplicationQuit();
        }

        private void OnDestroy()
        {
            SOnDestroy();
        }

        public virtual void CheckHierarchy()
        {
            if (!AlwaysOnTop) return;
            if (transform.parent != null) transform.parent = null;
            transform.SetAsFirstSibling();
            transform.position = Vector3.zero;
            transform.localScale = Vector3.one;
            transform.eulerAngles = Vector3.zero;
        }

        protected virtual void TryToShowLog(string message, LogType logType = LogType.Log, bool isSingletonLog = false)
        {
            if (isSingletonLog && !ShowSLogs &&
                logType != LogType.Error && logType != LogType.Exception) return;

            if (!isSingletonLog && !ShowLogs &&
               (logType != LogType.Error && logType != LogType.Exception)) return;

            string prefix = $"[Spacats Singleton: {gameObject.name}] ";

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
    }
}
