using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Spacats.Utils
{
    public class ExampleControllerTestMonoScript : MonoBehaviour
    {
        void Awake()
        {
            ControllersHub.OnHubSceneLoaded += OnHubSceneLoaded;
            Debug.Log("ExampleControllerTestMonoScript Awake");
        }

        private void OnDestroy()
        {
            ControllersHub.OnHubSceneLoaded -= OnHubSceneLoaded;
        }

        void Start()
        {
            Debug.Log("ExampleControllerTestMonoScript START");
        }

        private void OnHubSceneLoaded(Scene scene)
        {
            Debug.Log("ExampleControllerTestMonoScript HUB INIT");
        }
    }
}
