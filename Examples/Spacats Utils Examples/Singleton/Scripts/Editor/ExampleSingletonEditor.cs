#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Spacats.Utils
{
    [CustomEditor(typeof(ExampleSingleton), true)]
    public class ExampleSingletonEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            ExampleSingleton targetScript = (ExampleSingleton)target;

            GUILayout.TextArea("Is Instance: " + targetScript.IsInstance);
            DrawDefaultInspector();
        }
    }
}
#endif
