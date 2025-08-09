#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Spacats.Utils
{
    [CustomEditor(typeof(SceneControllerUsageExample), true)]
    public class SceneControllerUsageExampleEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            SceneControllerUsageExample targetScript = (SceneControllerUsageExample)target;

            DrawDefaultInspector();

            if (GUILayout.Button("Load Async (while in play mode)"))
            {
                targetScript.LoadSceneAsync();
            }
        }
    }
}
#endif
