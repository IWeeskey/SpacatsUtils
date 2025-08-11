#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Spacats.Utils
{
    [CustomEditor(typeof(PauseController), true)]
    public class PauseControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            PauseController targetScript = (PauseController)target;

            GUILayout.TextArea("Paused: " + PauseController.IsPaused);

            if (GUILayout.Button("Switch Pause"))
            {
                targetScript.SwitchPause();
            }

            if (GUILayout.Button("Pause ON"))
            {
                targetScript.PauseON();
            }

            if (GUILayout.Button("Pause OFF"))
            {
                targetScript.PauseOFF();
            }
        }
    }
}
#endif
