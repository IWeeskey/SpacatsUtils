#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Spacats.Utils
{
    [CustomEditor(typeof(MonoTweenController), true)]
    public class MonoTweenControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            MonoTweenController targetScript = (MonoTweenController)target;
            SetDefaultParameters();
            DrawSingletonParameters();
            DrawFields();
            TryDrawMeasurementsInfo();
            serializedObject.ApplyModifiedProperties();
        }

        private void SetDefaultParameters()
        {
            MonoTweenController targetScript = (MonoTweenController)target;
            targetScript.ShowControllerLogs = false;
            targetScript.ShowLogs = false;
            targetScript.UniqueTag = "";
        }

        private void DrawSingletonParameters()
        {
            MonoTweenController targetScript = (MonoTweenController)target;

            SerializedProperty executeInEditor = serializedObject.FindProperty("ExecuteInEditor");
            EditorGUILayout.PropertyField(executeInEditor);
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
