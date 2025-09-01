#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Spacats.Utils
{
    [CustomEditor(typeof(GUIPermanentMessage), true)]
    public class GUIPermanentMessageEditor : Editor
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
            GUIPermanentMessage targetScript = (GUIPermanentMessage)target;
            targetScript.ShowCLogs = false;
            targetScript.ShowLogs = false;
            targetScript.UniqueTag = "";
        }

        private void DrawFields()
        {
            GUIPermanentMessage targetScript = (GUIPermanentMessage)target;

            SerializedProperty logicEnabled = serializedObject.FindProperty("LogicEnabled");
            EditorGUILayout.PropertyField(logicEnabled);

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

            SerializedProperty message = serializedObject.FindProperty("Message");
            EditorGUILayout.PropertyField(message);
        }
    }
}
#endif