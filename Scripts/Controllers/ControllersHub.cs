using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Spacats.Utils
{
    [ExecuteInEditMode]
    public class ControllersHub : Singleton<ControllersHub>
    {

        private void RefreshName()
        {
            gameObject.name = "[SpaCats] ControllersHub";
        }

        #region overrides
        protected override void SingletonOnEnable()
        {
            base.SingletonOnEnable();
            RefreshName();
        }

        protected override void SingletonOnDisable()
        {
            base.SingletonOnDisable();
            RefreshName();
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
        }

        protected override void SingletonLateUpdate()
        {
            base.SingletonLateUpdate();
        }

        protected override void SingletonOnSceneGUI(SceneView sceneView)
        {
            base.SingletonOnSceneGUI(sceneView);
        }

        protected override void SingletonSharedUpdate()
        {
            base.SingletonSharedUpdate();
        }
        #endregion


    }
}
