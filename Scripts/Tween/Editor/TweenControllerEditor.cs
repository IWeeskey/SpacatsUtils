#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Spacats.Utils
{
    [CustomEditor(typeof(TweenController), true)]
    public class TweenControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            TweenController targetScript = (TweenController)target;
            DrawSingletonParameters();
            //DrawDefaultInspector();
        }


        private void DrawSingletonParameters()
        {
            TweenController targetScript = (TweenController)target;
            //_showSingletonSettings = EditorGUILayout.Foldout(_showSingletonSettings, "Singleton Parameters", true);
            //if (!_showSingletonSettings) return;

            SerializedProperty showLogs = serializedObject.FindProperty("ShowLogs");
            EditorGUILayout.PropertyField(showLogs);

            SerializedProperty showSingletonLogs = serializedObject.FindProperty("ShowControllerLogs");
            EditorGUILayout.PropertyField(showSingletonLogs);
        }
    }
}
#endif
