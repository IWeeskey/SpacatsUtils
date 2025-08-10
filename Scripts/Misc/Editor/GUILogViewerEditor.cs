#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Spacats.Utils
{
    [CustomEditor(typeof(GUILogViewer), true)]
    public class GUILogViewerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            SetDefaultParameters();
            DrawLogFields();
            serializedObject.ApplyModifiedProperties();
        }

        private void SetDefaultParameters()
        {
            GUILogViewer targetScript = (GUILogViewer)target;
            targetScript.ExecuteInEditor = false;
            targetScript.ShowControllerLogs = false;
            targetScript.ShowLogs = false;
            targetScript.UniqueTag = "";
        }

        private void DrawLogFields()
        {
            GUILogViewer targetScript = (GUILogViewer)target;
            GUILayout.TextArea("Is Opened: " + targetScript.IsOpened);

            SerializedProperty loggingEnabled = serializedObject.FindProperty("LoggingEnabled");
            EditorGUILayout.PropertyField(loggingEnabled);

            SerializedProperty topPercent = serializedObject.FindProperty("_topPercent");
            EditorGUILayout.PropertyField(topPercent);

            SerializedProperty bottomPercent = serializedObject.FindProperty("_bottomPercent");
            EditorGUILayout.PropertyField(bottomPercent);

            SerializedProperty fontSizePercent = serializedObject.FindProperty("_fontSizePercent");
            EditorGUILayout.PropertyField(fontSizePercent);

        }
    }
}
#endif