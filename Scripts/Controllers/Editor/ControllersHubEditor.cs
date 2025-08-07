using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Spacats.Utils
{
    [CustomEditor(typeof(ControllersHub), true)]
    public class ControllersHubnEditor : Editor
    {
        private int _tabIndex = 0;
        private readonly string[] _tabHeaders = { "Settings", "Info", "Default" };
        private bool _showSingletonSettings = false;
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            _tabIndex = GUILayout.Toolbar(_tabIndex, _tabHeaders);
            EditorGUILayout.Space();

            switch (_tabIndex)
            {
                case 0:
                    DrawSettingsTab();
                    break;
                case 1:
                    DrawInfoTab();
                    break;
                case 2:
                    DrawDefaultTab();
                    break;
            }
        }

        private void DrawSettingsTab()
        {
            ControllersHub targetScript = (ControllersHub)target;

            TryDrawSingletonParameters();

            serializedObject.ApplyModifiedProperties();
        }

        private void TryDrawSingletonParameters()
        {
            _showSingletonSettings = EditorGUILayout.Foldout(_showSingletonSettings, "Singleton Parameters", true);
            if (!_showSingletonSettings) return;

            //EditorGUILayout.LabelField("Singleton parameters", EditorStyles.boldLabel);

            SerializedProperty alwaysOnTop = serializedObject.FindProperty("AlwaysOnTop");
            EditorGUILayout.PropertyField(alwaysOnTop);

            SerializedProperty showLogs = serializedObject.FindProperty("ShowLogs");
            EditorGUILayout.PropertyField(showLogs);

            SerializedProperty showSingletonLogs = serializedObject.FindProperty("ShowSingletonLogs");
            EditorGUILayout.PropertyField(showSingletonLogs);
        }

        private void DrawInfoTab()
        {
            ControllersHub targetScript = (ControllersHub)target;
            GUILayout.TextArea("Is Instance: " + targetScript.IsInstance);
        }

        private void DrawDefaultTab()
        {
            DrawDefaultInspector();
        }
    }
}
