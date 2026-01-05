using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Spacats.Utils
{
    [ExecuteInEditMode]
    [DefaultExecutionOrder(-20)]
    public class ControllersHub : Singleton<ControllersHub>
    {
        public static event Action OnHubUpdate;
        public static event Action OnHubFixedUpdate;
        public static event Action OnHubLateUpdate;
        public static event Action OnHubSharedUpdate;
        public static event Action OnHubClear;
        public static event Action<Scene> OnHubSceneUnloading;
        public static event Action<Scene> OnHubSceneLoaded;
        
        [SerializeField] private List<Controller> _controllers = new List<Controller>();

        private bool _sceneInitScheduled = false;
        private IEnumerator _sceneInitIEnumerator;
        #region overrides
        protected override void SAwake()
        {
            base.SAwake();
            _sceneInitScheduled = false;
        }
        
        protected override void SSetDefaultParameters()
        {
            base.SSetDefaultParameters();
            ShowLogs = false;
            ShowSLogs = false;
            AlwaysOnTop = true;
            CheckHierarchy();
        }

        protected override void SOnEnable()
        {
            base.SOnEnable();
            RefreshName();
            Clear();
        }

        protected override void SOnDisable()
        {
            base.SOnDisable();
            RefreshName();
            Clear();
        }

        protected override void SOnDestroy()
        {
            base.SOnDestroy();
            HandleDestroyLogic();
        }

        protected override void SOnApplicationQuit()
        {
            base.SOnApplicationQuit();
        }

        protected override void SOnSceneUnloading(Scene scene)
        {
            base.SOnSceneUnloading(scene);
            for (int i = _controllers.Count - 1; i >= 0; i--)
            {
                var controller = _controllers[i];
                if (controller == null) continue;
                if (!controller.ExecuteInEditor && !Application.isPlaying) continue;
                controller.COnSceneUnloading(scene);
            }
            OnHubSceneUnloading?.Invoke(scene);
        }

        protected override void SOnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            base.SOnSceneLoaded(scene, mode);

            if (_sceneInitScheduled) StopCoroutine(_sceneInitIEnumerator);
            _sceneInitScheduled = true;
            _sceneInitIEnumerator = BasicIEnumerators.WaitNextFrame(() =>
            {
                FinishOnSceneLoaded(scene, mode);
                _sceneInitScheduled = false;
            },2);
            
            StartCoroutine(_sceneInitIEnumerator);
        }


        private void FinishOnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            for (int i = _controllers.Count - 1; i>=0; i--)
            {
                if (_controllers[i] == null) continue;
                if (!_controllers[i].ExecuteInEditor && !Application.isPlaying) continue;
                _controllers[i].ExternalOnSceneLoaded(scene, mode);
            }
            OnHubSceneLoaded?.Invoke(scene);
            
            TryToShowLog("OnHubSceneLoaded", LogType.Log, false);
        }


        protected override void SFixedUpdate()
        {
            base.SFixedUpdate();
            OnHubFixedUpdate?.Invoke();
        }

        protected override void SUpdate()
        {
            base.SUpdate();
            //string orderCheck = "";
            for (int i = _controllers.Count - 1; i>=0; i--)
            {
                Controller controller = _controllers[i];
                if (controller == null) continue;
                if (!controller.ExecuteInEditor && !Application.isPlaying)
                {
                    continue;
                }
                
                //orderCheck += controller.gameObject.name + ", ";
                controller.CUpdate();
            }
            
            OnHubUpdate?.Invoke();
            //Debug.Log(orderCheck);
        }

        protected override void SLateUpdate()
        {
            base.SLateUpdate();

            for (int i = _controllers.Count - 1; i>=0; i--)
            {
                Controller controller = _controllers[i];
                if (controller ==null) continue;
                if (!controller.ExecuteInEditor && !Application.isPlaying)
                {
                    continue;
                }
                controller.CLateUpdate();
            }
            
            OnHubLateUpdate?.Invoke();
        }


#if UNITY_EDITOR
        protected override void SingletonOnSceneGUI(SceneView sceneView)
        {
            base.SingletonOnSceneGUI(sceneView);

            for (int i = _controllers.Count - 1; i>=0; i--)
            {
                Controller controller = _controllers[i];
                if (controller == null) continue;
                controller.COnSceneGUI(sceneView);
            }
        }
#endif

        protected override void SSharedUpdate(bool isGuiCall = false)
        {
            base.SSharedUpdate(isGuiCall);

            for (int i = _controllers.Count - 1; i>=0; i--)
            {
                Controller controller = _controllers[i];
                if (controller == null) continue;
                if (!controller.ExecuteInEditor && !Application.isPlaying)
                {
                    continue;
                }
                controller.CSharedUpdate(isGuiCall);
            }
            OnHubSharedUpdate?.Invoke();
        }
        #endregion

        private void HandleDestroyLogic()
        {
            for (int i = _controllers.Count - 1; i>=0; i--)
            {
                Controller controller = _controllers[i];
                if (controller == null) continue;
                controller.transform.parent = null;
                controller.enabled = true;
            }
        }

        private void RefreshName()
        {
            gameObject.name = "[SpaCats] ControllersHub";
        }

        private void Clear()
        {
            TryToShowLog("Clear");
            _controllers?.Clear();
            OnHubClear?.Invoke();
        }

        public bool RegisterController(Controller controller)
        {
            if (_controllers.Contains(controller))
            {
                TryToShowLog("Controller already registered: " + controller.gameObject.name, LogType.Warning);
                return false;
            }

            if (!IsUnique(controller))
            {
                TryToShowLog("Controller is not unique by tag: " + controller.gameObject.name, LogType.Warning);
                return false;
            }

            TryToShowLog("RegisterController: " + controller.gameObject.name);
            _controllers.Add(controller);
            SortControllersBackwards();
            return true;
        }

        private bool IsUnique(Controller controller)
        {
            var targetType = controller.GetType();
            var group = _controllers
                .Where(c => c != null && c.GetType() == targetType)
                .ToList();

            TryToShowLog($"Controller: {targetType.Name}, count: {group.Count}", LogType.Warning);

            var values = group.Select(c => c.UniqueTag).ToList();

            if (values.Count == 0) return true;

            foreach (string uTag in values)
            {
                if (string.Equals(uTag, controller.UniqueTag))
                {
                    TryToShowLog($"{targetType.Name} same tag found!", LogType.Warning);
                    return false;
                }
            }

            return true;
        }

        private void SortControllersBackwards()
        {
            if (_controllers == null || _controllers.Count <= 1) return;

            _controllers.Sort((a, b) =>
            {
                if (ReferenceEquals(a, b)) return 0;
                if (a == null) return 1; // place nulls at the end
                if (b == null) return -1;

                // Descending by ExecuteOrder: higher first, lower last
                int orderCompare = b.ExecuteOrder.CompareTo(a.ExecuteOrder);
                if (orderCompare != 0) return orderCompare;

                // Deterministic tie-breakers
                // int tagCompare = string.Compare(a.UniqueTag, b.UniqueTag, System.StringComparison.Ordinal);
                // if (tagCompare != 0) return tagCompare;

                return a.GetInstanceID().CompareTo(b.GetInstanceID());
            });
        }

        public bool UnRegisterController(Controller controller)
        {
            TryToShowLog("UnRegisterController: " + controller.gameObject.name);
            return _controllers.Remove(controller);
        }

        public T GetController<T>(string tag = "") where T : Controller
        {
            var controllersOfType = _controllers.OfType<T>();

            T result;

            if (string.IsNullOrEmpty(tag))
            {
                result = controllersOfType.FirstOrDefault();
                if (result == null)
                {
                    TryToShowLog($"Controller of type {typeof(T).Name} not found.", LogType.Error);
                }
            }
            else
            {
                result = controllersOfType.FirstOrDefault(c => c.UniqueTag == tag);
                if (result == null)
                {
                    TryToShowLog($"Controller of type {typeof(T).Name} with tag '{tag}' not found.", LogType.Error);
                }
            }

            return result;
        }
    }
}
