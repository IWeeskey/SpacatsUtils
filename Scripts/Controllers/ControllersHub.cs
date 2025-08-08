using System.Collections;
using System.Collections.Generic;

using UnityEngine;

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
            ShowLogs = true;
            ShowSingletonLogs = true;
            AlwaysOnTop = true;
            CheckHierarchy();
        }

        protected override void SingletonOnEnable()
        {
            base.SingletonOnEnable();
            RefreshName();
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
                controller.ControllerOnSceneGUI(sceneView);
            }
        }
#endif

        protected override void SingletonSharedUpdate()
        {
            base.SingletonSharedUpdate();

            foreach (Controller controller in _controllers)
            {
                if (!controller.ExecuteInEditor && !Application.isPlaying)
                {
                    continue;
                }
                controller.ControllerSharedUpdate();
            }
        }
        #endregion
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

            TryToShowLog("RegisterController: " + controller.gameObject.name);
            _controllers.Add(controller);
            return true;
        }

        public bool UnRegisterController(Controller controller)
        {
            TryToShowLog("UnRegisterController: " + controller.gameObject.name);
            return _controllers.Remove(controller);
        }
    }
}
