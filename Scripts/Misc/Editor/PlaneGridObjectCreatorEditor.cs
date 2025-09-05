using UnityEditor;
using UnityEngine;

namespace Spacats.Utils
{
    [CustomEditor(typeof(PlaneGridObjectCreator))]
    public class PlaneGridObjectCreatorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            PlaneGridObjectCreator targetScript = (PlaneGridObjectCreator)target;

            DrawDefaultInspector();

            if (GUILayout.Button("Create Now"))
            {
                targetScript.GenerateImmediate();
            }

            if (GUILayout.Button("Clear"))
            {
                targetScript.Clear();
            }
        }

    }
}
