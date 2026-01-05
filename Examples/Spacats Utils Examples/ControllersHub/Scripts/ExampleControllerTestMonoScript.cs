using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spacats.Utils
{
    public class ExampleControllerTestMonoScript : MonoBehaviour
    {
        void Awake()
        {
            ControllersHub.OnCHubSceneInit += OnCHubSceneInit;
            Debug.Log("ExampleControllerTestMonoScript Awake");
        }

        private void OnDestroy()
        {
            ControllersHub.OnCHubSceneInit -= OnCHubSceneInit;
        }

        void Start()
        {
            Debug.Log("ExampleControllerTestMonoScript START");
        }

        private void OnCHubSceneInit()
        {
            Debug.Log("ExampleControllerTestMonoScript HUB INIT");
        }
    }
}
