#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Spacats.Utils
{
    [CustomEditor(typeof(MonoTweenController), true)]
    public class MonoTweenControllerEditor : Editor
    {
        private int _tabIndex = 0;
        private readonly string[] _tabHeaders = { "Mono Tween Settings", "Controller Settings" };

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            SetDefaultParameters();

            _tabIndex = GUILayout.Toolbar(_tabIndex, _tabHeaders);
            EditorGUILayout.Space();

            switch (_tabIndex)
            {
                case 0:
                    DrawTweenSettings();
                    break;
                case 1:
                    DrawControllerSettings();
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawTweenSettings()
        {
            DrawFields();
            TryDrawMeasurementsInfo();
        }

        private void DrawControllerSettings()
        {
            SerializedProperty executeInEditor = serializedObject.FindProperty("ExecuteInEditor");
            EditorGUILayout.PropertyField(executeInEditor);

            SerializedProperty showLogs = serializedObject.FindProperty("ShowLogs");
            EditorGUILayout.PropertyField(showLogs);

            SerializedProperty showCLogs = serializedObject.FindProperty("ShowCLogs");
            EditorGUILayout.PropertyField(showCLogs);
        }

        private void SetDefaultParameters()
        {
            MonoTweenController targetScript = (MonoTweenController)target;
            targetScript.UniqueTag = "";
        }

        private void DrawFields()
        {
            MonoTweenController targetScript = (MonoTweenController)target;

            GUILayout.TextArea("Paused: " + targetScript.IsPaused);
            GUILayout.TextArea("Active Tweens: " + targetScript.ActiveTweensCount);
            GUILayout.TextArea("Tweens pool: " + targetScript.TweensListCount);

            SerializedProperty performMeasurements = serializedObject.FindProperty("PerformMeasurements");
            EditorGUILayout.PropertyField(performMeasurements);
        }

        private void TryDrawMeasurementsInfo()
        {
            MonoTweenController targetScript = (MonoTweenController)target;
            if (!targetScript.PerformMeasurements) return;

            EditorGUILayout.LabelField("Measurements", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Update time: " + targetScript.UpdateTimeString, EditorStyles.label);
            EditorGUILayout.LabelField("Update ms: " + targetScript.UpdateTimeMS, EditorStyles.label);
        }
    }
}
#endif
