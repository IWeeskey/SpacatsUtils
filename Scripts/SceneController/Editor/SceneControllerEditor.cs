#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Spacats.Utils
{
    [CustomEditor(typeof(SceneController), true)]
    public class SceneControllerEditor : Editor
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
            SceneController targetScript = (SceneController)target;
            targetScript.ShowControllerLogs = false;
            targetScript.ShowLogs = false;
            targetScript.UniqueTag = "";
            targetScript.PersistsAtScenes.Clear();
        }

        private void DrawFields()
        {
            SceneController targetScript = (SceneController)target;

            GUILayout.TextArea("Is Loading: " + targetScript.IsLoading);
            GUILayout.TextArea("LoadingSceneName: " + targetScript.LoadingSceneName);

        }
    }
}
#endif
