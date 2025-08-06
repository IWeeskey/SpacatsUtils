using UnityEngine;
using System;
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
        protected static bool _applicationIsQuitting = false;
        protected static bool _isDestroyed = false;
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
                    }
                }
                return _instance;
            }
        }

        public static bool HasInstance => _instance != null && !_isDestroyed;

        protected virtual void SingletonAwake() { TryToShowLog("SingletonAwake", 0, true); }
        protected virtual void SingletonOnEnable() { TryToShowLog("SingletonOnEnable", 0, true); }
        protected virtual void SingletonOnDisable() { TryToShowLog("SingletonOnDisable", 0, true); }

        /// <summary>
        /// Same as basic unity Update()
        /// </summary>
        protected virtual void SingletonUpdate() { }
        /// <summary>
        /// Triggers every update + every scene gui. So it can work smoothly while in editor.
        /// </summary>
        protected virtual void SingletonSharedUpdate() { }
        protected virtual void SingletonOnDestroy() { TryToShowLog("SingletonOnDestroy", 0, true); }
        protected virtual void SingletonOnApplicationQuit() {  TryToShowLog("SingletonOnApplicationQuit", 0, true);}

        public bool IsInstance => Instance == this;


        [Tooltip("Show basic logs, defined by inheritor (derived, child class)")]
        public bool ShowLogs = false;

        [Tooltip("Show singleton logs for testing purposes, such as 'SingletonAwake', 'SingletonOnEnable' etc.")]
        public bool ShowSingletonLogs = false;

#if UNITY_EDITOR
        protected virtual void SingletonOnSceneGUI(SceneView sceneView) { }
#endif

        private void Awake()
        {
            if (_instance == null || _applicationIsQuitting)
            {
                _instance = this as T;
                _isDestroyed = false;
                _applicationIsQuitting = false;
                if (Application.isPlaying) DontDestroyOnLoad(gameObject);
                SingletonAwake();

                return;
            }
            if (IsInstance) return;
            TryToShowLog("Trying to instantiate a second instance. Destroying the new one.", 1, true);

            if (!Application.isPlaying)
            {
                DestroyImmediate(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnEnable()
        {
            if (!IsInstance) return;
            SingletonOnEnable();
#if UNITY_EDITOR
            SceneView.duringSceneGui += DuringSceneGui;
#endif
        }

        protected virtual void OnDisable()
        {
            if (!IsInstance) return;
            SingletonOnDisable();
#if UNITY_EDITOR
            SceneView.duringSceneGui -= DuringSceneGui;
#endif
        }

        private void DuringSceneGui(SceneView sView)
        {
            SingletonOnSceneGUI(sView);
            SingletonSharedUpdate();
        }

        protected virtual void Update()
        {
            if (!IsInstance) return;
            SingletonUpdate();
            SingletonSharedUpdate();
        }

        protected virtual void OnApplicationQuit()
        {
            if (!IsInstance) return;

            _applicationIsQuitting = true;
            SingletonOnApplicationQuit();
        }

        protected virtual void OnDestroy()
        {
            if (!IsInstance) return;
            _isDestroyed = true;
            _instance = null;
            SingletonOnDestroy();
        }

        /// <summary>
        /// Log types: 
        /// 0 - normal message
        /// 1 - warning
        /// 2 - error
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logType"></param>
        protected virtual void TryToShowLog(string message, int logType = 0, bool isSingletonLog = false)
        {
            if (isSingletonLog && !ShowSingletonLogs) return;
            if (!isSingletonLog && !ShowLogs) return;

            string prefix = $"[Spacats Singleton: {typeof(T).Name}] ";

            switch (logType)
            {
                default: Debug.Log(prefix + message); break;
                case 1: Debug.LogWarning(prefix + message); break;
                case 2: Debug.LogError(prefix + message); break;
            }
        }
    }
}
