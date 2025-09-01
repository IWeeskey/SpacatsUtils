#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Spacats.Utils
{
    [CustomEditor(typeof(GUIFps), true)]
    public class GUIFpsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            SetDefaultParameters();
            DrawFields();
            serializedObject.ApplyModifiedProperties();
        }

        private void SetDefaultParameters()
        {
            GUIFps targetScript = (GUIFps)target;
            targetScript.ShowCLogs = false;
            targetScript.ShowLogs = false;
            targetScript.UniqueTag = "";
        }

        private void DrawFields()
        {
            GUIFps targetScript = (GUIFps)target;

            SerializedProperty logicEnabled = serializedObject.FindProperty("LogicEnabled");
            EditorGUILayout.PropertyField(logicEnabled);

            SerializedProperty showExtra = serializedObject.FindProperty("ShowExtra");
            EditorGUILayout.PropertyField(showExtra);

            SerializedProperty executeInEditor = serializedObject.FindProperty("ExecuteInEditor");
            EditorGUILayout.PropertyField(executeInEditor);

            SerializedProperty posX = serializedObject.FindProperty("PosX");
            EditorGUILayout.PropertyField(posX);

            SerializedProperty posY = serializedObject.FindProperty("PosY");
            EditorGUILayout.PropertyField(posY);

            SerializedProperty fontSize = serializedObject.FindProperty("FontSize");
            EditorGUILayout.PropertyField(fontSize);

            SerializedProperty fontColor = serializedObject.FindProperty("FontColor");
            EditorGUILayout.PropertyField(fontColor);
        }
    }
}
#endif