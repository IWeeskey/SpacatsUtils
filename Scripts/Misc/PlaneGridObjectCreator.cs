using System.Collections;
using UnityEngine;
namespace Spacats.Utils
{
    public class PlaneGridObjectCreator : MonoBehaviour
    {
        [Min(1)] public int ImmediateSize = 10;
        [Min(1)] public int PlaymodeSize = 10;
        [Min(0f)] public float Gap;
        public GameObject Prefab;
        public Transform Parent;

        private IEnumerator Start()
        {
            if (Parent == null)
            {
                Debug.LogError("GridObjectCreator Parent==null");
                yield break;
            }

            if (Prefab == null)
            {
                Debug.LogWarning("Prefab not assigned", this);
                yield break;
            }

            Clear();

            float half = (PlaymodeSize - 1) * Gap * 0.5f;

            for (int x = 0; x < PlaymodeSize; x++)
            {
                for (int z = 0; z < PlaymodeSize; z++)
                {
                    Vector3 localPos = new Vector3(x * Gap - half, 0f, z * Gap - half);
                    Instantiate(Prefab, Parent.position + localPos, Quaternion.identity, Parent);
                }

                yield return new WaitForSeconds(0.0f);
            }
        }

        public void GenerateImmediate()
        {
            if (Parent == null)
            {
                Debug.LogError("GridObjectCreator Parent==null");
                return;
            }

            if (Prefab == null)
            {
                Debug.LogWarning("Prefab not assigned", this);
                return;
            }

            Clear();

            float half = (ImmediateSize - 1) * Gap * 0.5f;

            for (int x = 0; x < ImmediateSize; x++)
            {
                for (int z = 0; z < ImmediateSize; z++)
                {
                    Vector3 localPos = new Vector3(x * Gap - half, 0f, z * Gap - half);
                    Instantiate(Prefab, Parent.position + localPos, Quaternion.identity, Parent);
                }
            }
        }

        public void Clear()
        {
            ClearChildren(Parent);
        }

        public static void ClearChildren(Transform target)
        {
            if (target == null)
            {
                Debug.LogError("ClearChildren of NULL target!");
                return;
            }

            for (int i = target.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(target.GetChild(i).gameObject);
            }
        }
    }
}
