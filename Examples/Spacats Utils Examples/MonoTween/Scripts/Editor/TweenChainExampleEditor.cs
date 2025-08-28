#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Spacats.Utils
{
    [CustomEditor(typeof(TweenChainExample), true)]
    public class TweenChainExampleEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            TweenChainExample targetScript = (TweenChainExample)target;

            DrawDefaultInspector();

            if (GUILayout.Button("Start"))
            {
                targetScript.StartTweens();
            }

            if (GUILayout.Button("Switch self pause"))
            {
                targetScript.SwitchPauseTweens();
            }

            if (GUILayout.Button("Stop"))
            {
                targetScript.StopTweens();
            }
        }
    }
}
#endif
