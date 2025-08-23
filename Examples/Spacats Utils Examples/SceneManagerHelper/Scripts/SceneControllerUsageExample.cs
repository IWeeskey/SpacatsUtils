using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Spacats.Utils
{
    public class SceneControllerUsageExample : MonoBehaviour
    {
        public string SceneNameToLoad = "";

        public void LoadScene()
        {
            SceneManagerHelper.LoadScene(SceneNameToLoad);
        }

        public void LoadSceneAsync()
        {
            if (!Application.isPlaying)
            {
                Debug.Log("Please enter play mode");
                return;
            }
            SceneManagerHelper.LoadSceneAsync(this, SceneNameToLoad,
                progress => { Debug.Log($"Loading progress: {progress * 100f}%"); },
                LoadSceneMode.Single);
        }
    }
}

