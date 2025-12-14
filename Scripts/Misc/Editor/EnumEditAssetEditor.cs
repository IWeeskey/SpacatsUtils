#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace Spacats.Utils
{
    [CustomEditor(typeof(EnumEditAsset), true)]
    public class EnumEditAssetEditor : Editor
    {
        
        public override void OnInspectorGUI()
        {
            EnumEditAsset targetScript = (EnumEditAsset)target;
            DrawDefaultInspector();

            if (GUILayout.Button("Validate and Add"))
            {
                targetScript.AddToEnum();
            }
            
        }
    }
}
#endif
