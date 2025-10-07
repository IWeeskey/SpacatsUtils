using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spacats.Utils
{
    [ExecuteInEditMode]
    public class TestPhysicAOIUnit : MonoBehaviour
    {
        public List<TestPhysicAOIUnit> OtherUnits;
        private void OnTriggerEnter(Collider other)
        {
            OtherUnits.Add(other.GetComponent<TestPhysicAOIUnit>());
        }

        private void OnTriggerExit(Collider other)
        {
            OtherUnits.Remove(other.GetComponent<TestPhysicAOIUnit>());
        }
    }
}
