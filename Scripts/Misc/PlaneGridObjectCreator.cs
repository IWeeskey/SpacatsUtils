using System.Collections;
using UnityEngine;
namespace Spacats.Utils
{
    public class PlaneGridObjectCreator : MonoBehaviour
    {
        [Range(0f,1f)]public float Chance = 1f;
        [Min(1)] public int ImmediateSize = 10;
        [Min(1)] public int PlaymodeSize = 10;
        [Min(0f)] public float Gap;

        public Vector3 MinEulers;
        public Vector3 MaxEulers;

        public GameObject Prefab;
        public Transform Parent;

        public bool IEnumeratorCreation = true;

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

            Debug.Log("Trying to instantiate: " + PlaymodeSize * PlaymodeSize + " objects");

            for (int x = 0; x < PlaymodeSize; x++)
            {
                for (int z = 0; z < PlaymodeSize; z++)
                {
                    InstantiateAtGridPoint(x, z, PlaymodeSize);
                }

                if (IEnumeratorCreation) yield return new WaitForSeconds(0.0f);
            }

            yield return new WaitForSeconds(0.0f);
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

            Debug.Log("Trying to instantiate: " + ImmediateSize* ImmediateSize + " objects");
            TimeTracker.Start("PlaneGridObjectCreator");

            for (int x = 0; x < ImmediateSize; x++)
            {
                for (int z = 0; z < ImmediateSize; z++)
                {
                    InstantiateAtGridPoint(x,z, ImmediateSize);
                }
            }

            TimeTracker.Finish("PlaneGridObjectCreator", true);
        }
        private void InstantiateAtGridPoint(int x, int z, int size)
        {
            float chance = Random.Range(0f, 1f);
            
            if (Chance<chance) return;
            
            float half = (size - 1) * Gap * 0.5f;

            Vector3 localPos = new Vector3(x * Gap - half, 0f, z * Gap - half);
            Vector3 localEulers = new Vector3(Random.Range(MinEulers.x, MaxEulers.x), 
                Random.Range(MinEulers.y, MaxEulers.y), 
                Random.Range(MinEulers.z, MaxEulers.z));

            GameObject newGO = Instantiate(Prefab, Vector3.zero, Quaternion.identity, Parent);
            newGO.transform.localPosition = localPos;
            newGO.transform.localEulerAngles = localEulers;

            OnObjectInstantiated(newGO, x, z);
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

        protected virtual void OnObjectInstantiated(GameObject gObject, int x, int z)
        { 
        
        }
    }
}
