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
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static T _instance;
        protected bool _applicationIsQuitting = false;
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
                        _instance.SingletonSetDefaultParameters();
                    }
                }
                return _instance;
            }
        }

        public static bool HasInstance => _instance != null;

        protected virtual void SingletonAwake() { CheckHierarchy(); TryToShowLog("Awake", 0, true); }
        protected virtual void SingletonOnEnable() { CheckHierarchy(); TryToShowLog("OnEnable", 0, true); }
        protected virtual void SingletonOnDisable() { CheckHierarchy(); TryToShowLog("OnDisable", 0, true); }
        protected virtual void SingletonOnDestroy() { TryToShowLog("OnDestroy", 0, true); }
        protected virtual void SingletonOnApplicationQuit() {  TryToShowLog("OnApplicationQuit", 0, true);}
        protected virtual void SingletonSetDefaultParameters() { TryToShowLog("SetDefaultParameters", 0, true); }


        /// <summary>
        /// Same as basic unity Update()
        /// </summary>
        protected virtual void SingletonUpdate() { }
        /// <summary>
        /// Same as basic unity LateUpdate()
        /// </summary>
        protected virtual void SingletonLateUpdate() { }
        /// <summary>
        /// Triggers every update + every scene gui. So it can work smoothly while in editor.
        /// </summary>
        protected virtual void SingletonSharedUpdate() { }

        public bool IsInstance => Instance == this;

        [Tooltip("Set this gameobject to be always on top in hierarchy")]
        public bool AlwaysOnTop = false;

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
                _applicationIsQuitting = false;
                if (Application.isPlaying) DontDestroyOnLoad(gameObject);
                SingletonAwake();
                SceneManagerHelper.MarkActiveSceneDirty();
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

        private void OnEnable()
        {
            if (!IsInstance) return;
            SingletonOnEnable();
#if UNITY_EDITOR
            SceneView.duringSceneGui += DuringSceneGui;
#endif
        }

        private void OnDisable()
        {
            if (!IsInstance) return;
            SingletonOnDisable();
#if UNITY_EDITOR
            SceneView.duringSceneGui -= DuringSceneGui;
#endif
        }

#if UNITY_EDITOR
        private void DuringSceneGui(SceneView sView)
        {
            SingletonOnSceneGUI(sView);
            SingletonSharedUpdate();
        }
#endif

        private void Update()
        {
            if (!IsInstance) return;
            SingletonUpdate();
            SingletonSharedUpdate();
        }

        private void LateUpdate()
        {
            if (!IsInstance) return;
            SingletonLateUpdate();
        }

        private void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
            SingletonOnApplicationQuit();
        }

        private void OnDestroy()
        {
            SingletonOnDestroy();
        }

        public virtual void CheckHierarchy()
        {
            if (!AlwaysOnTop) return;
            if (transform.parent != null) transform.parent = null;
            transform.SetAsFirstSibling();
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
            if (isSingletonLog && !ShowSingletonLogs && logType != 2) return;
            if (!isSingletonLog && !ShowLogs && logType != 2) return;

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
