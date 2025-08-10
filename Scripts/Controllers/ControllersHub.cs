using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Spacats.Utils
{
    [ExecuteInEditMode]
    [DefaultExecutionOrder(-10)]
    public class ControllersHub : Singleton<ControllersHub>
    {
        [SerializeField] private List<Controller> _controllers = new List<Controller>();
        private void RefreshName()
        {
            gameObject.name = "[SpaCats] ControllersHub";
        }

        #region overrides
        protected override void SingletonAwake()
        {
            base.SingletonAwake();
        }
        protected override void SingletonSetDefaultParameters()
        {
            base.SingletonSetDefaultParameters();
            ShowLogs = false;
            ShowSingletonLogs = false;
            AlwaysOnTop = true;
            CheckHierarchy();
        }

        protected override void SingletonOnEnable()
        {
            base.SingletonOnEnable();
            RefreshName();
            Clear();
        }

        protected override void SingletonOnDisable()
        {
            base.SingletonOnDisable();
            RefreshName();
            Clear();
        }

        protected override void SingletonOnDestroy()
        {
            base.SingletonOnDestroy();
            HandleDestroyLogic();
        }

        protected override void SingletonOnApplicationQuit()
        {
            base.SingletonOnApplicationQuit();
        }

        protected override void SingletonUpdate()
        {
            base.SingletonUpdate();
            foreach (Controller controller in _controllers)
            {
                if (controller == null) continue;
                if (!controller.ExecuteInEditor && !Application.isPlaying)
                {
                    continue;
                }
                controller.ControllerUpdate();
            }
        }

        protected override void SingletonLateUpdate()
        {
            base.SingletonLateUpdate();

            foreach (Controller controller in _controllers)
            {
                if (controller == null) continue;
                if (!controller.ExecuteInEditor && !Application.isPlaying)
                {
                    continue;
                }
                controller.ControllerLateUpdate();
            }
        }


#if UNITY_EDITOR
        protected override void SingletonOnSceneGUI(SceneView sceneView)
        {
            base.SingletonOnSceneGUI(sceneView);

            foreach (Controller controller in _controllers)
            {
                if (controller == null) continue;
                controller.ControllerOnSceneGUI(sceneView);
            }
        }
#endif

        protected override void SingletonSharedUpdate()
        {
            base.SingletonSharedUpdate();

            foreach (Controller controller in _controllers)
            {
                if (controller == null) continue;
                if (!controller.ExecuteInEditor && !Application.isPlaying)
                {
                    continue;
                }
                controller.ControllerSharedUpdate();
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

        private void Clear()
        {
            TryToShowLog("Clear");
            _controllers?.Clear();
        }

        public bool RegisterController(Controller controller)
        {
            if (_controllers.Contains(controller))
            {
                TryToShowLog("Controller already registered: " + controller.gameObject.name, 1);
                return false;
            }

            if (!IsUnique(controller))
            {
                TryToShowLog("Controller is not unique by tag: " + controller.gameObject.name, 1);
                return false;
            }

            TryToShowLog("RegisterController: " + controller.gameObject.name);
            _controllers.Add(controller);
            return true;
        }

        private bool IsUnique(Controller controller)
        {
            var targetType = controller.GetType();
            var group = _controllers .Where(c => c.GetType() == targetType).ToList();

            TryToShowLog($"Controller: {targetType.Name}, count: {group.Count}", 1);

            var values = group.Select(c => c.UniqueTag).ToList();

            if (values.Count == 0) return true;

            foreach (string uTag in values)
            {
                if (string.Equals(uTag, controller.UniqueTag))
                {
                    TryToShowLog($"{targetType.Name} same tag found!", 1);
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
                    TryToShowLog($"Controller of type {typeof(T).Name} not found.", 2);
                }
            }
            else
            {
                result = controllersOfType.FirstOrDefault(c => c.UniqueTag == tag);
                if (result == null)
                {
                    TryToShowLog($"Controller of type {typeof(T).Name} with tag '{tag}' not found.", 2);
                }
            }

            return result;
        }
    }
}
