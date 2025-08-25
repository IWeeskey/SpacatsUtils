using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Spacats.Utils
{
    [DefaultExecutionOrder(-10)]
    public class SceneController : Controller
    {
        private bool _isLoading = false;

        public void LoadSceneImmediate(string sName)
        {
            if (_isLoading) return;
            _isLoading = true;
            SceneManager.LoadScene(sName,  LoadSceneMode.Single);
        }

        public void LoadSceneAsync(string sName)
        {
            if (_isLoading) return;
            if (!Application.isPlaying)
            {
                Debug.Log("Please enter play mode");
                return;
            }
            _isLoading = true;
            SceneLoaderHelper.LoadSceneAsync(this, sName,
                progress => { Debug.Log($"Loading progress: {progress * 100f}%"); },
                LoadSceneMode.Single);
        }

        public override void ControllerOnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            base.ControllerOnSceneLoaded(scene, mode);
            _isLoading = false;
        }

        public override void ControllerOnSceneUnloading(Scene scene)
        {
            base.ControllerOnSceneUnloading(scene);
        }
    }
}
