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
        [SerializeField] private List<Controller> _controllers = new List<Controller>();
        #region overrides
        protected override void SAwake()
        {
            base.SAwake();
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
            foreach (Controller controller in _controllers)
            {
                if (controller == null) continue;
                if (!controller.ExecuteInEditor && !Application.isPlaying)
                {
                    continue;
                }
                controller.COnSceneUnloading(scene);
            }
        }

        protected override void SOnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            base.SOnSceneLoaded(scene, mode);
            if (_controllers.Count == 0) return;

            for (int i = _controllers.Count - 1; i>=0; i--)
            {
                if (_controllers[i] == null) continue;
                if (!_controllers[i].ExecuteInEditor && !Application.isPlaying) continue;
                _controllers[i].ExternalOnSceneLoaded(scene, mode);
            }
        }

        protected override void SUpdate()
        {
            base.SUpdate();
            foreach (Controller controller in _controllers)
            {
                if (controller == null) continue;
                if (!controller.ExecuteInEditor && !Application.isPlaying)
                {
                    continue;
                }
                controller.CUpdate();
            }
        }

        protected override void SLateUpdate()
        {
            base.SLateUpdate();

            foreach (Controller controller in _controllers)
            {
                if (controller == null) continue;
                if (!controller.ExecuteInEditor && !Application.isPlaying)
                {
                    continue;
                }
                controller.CLateUpdate();
            }
        }


#if UNITY_EDITOR
        protected override void SingletonOnSceneGUI(SceneView sceneView)
        {
            base.SingletonOnSceneGUI(sceneView);

            foreach (Controller controller in _controllers)
            {
                if (controller == null) continue;
                controller.COnSceneGUI(sceneView);
            }
        }
#endif

        protected override void SSharedUpdate()
        {
            base.SSharedUpdate();

            foreach (Controller controller in _controllers)
            {
                if (controller == null) continue;
                if (!controller.ExecuteInEditor && !Application.isPlaying)
                {
                    continue;
                }
                controller.CSharedUpdate();
            }
        }
        #endregion

        private void HandleDestroyLogic()
        {
            foreach (Controller controller in _controllers)
            {
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
