using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Spacats.Utils
{
    [DefaultExecutionOrder(-10)]
    public class SceneController : Controller
    {
        private bool _isLoading = false;

        private MonoTweenController _cMonoTween;
        private MonoTweenUnit SceneDelayTween;

        public bool IsLoading => _isLoading;
        public event UnityAction<string> OnLoadStarted;
        public event UnityAction<string, float> OnLoading;
        public event UnityAction<string> OnLoadFinished;

        private void CheckController()
        {
            if (_cMonoTween == null) _cMonoTween = ControllersHub.Instance.GetController<MonoTweenController>();
        }

        private void LoadSceneImmediateWithDelay(string sName, float delay)
        {
            if (SceneDelayTween == null)
            {
                SceneDelayTween = new MonoTweenUnit(
                       delay: delay,
                       duration: 0f,
                       onStart: () => { },
                       onLerp: (float lerp) => { },
                       onEnd: () => { SceneManager.LoadScene(sName, LoadSceneMode.Single); }
                   );
            }

            SceneDelayTween.Delay = delay;
            OnLoadStarted?.Invoke(sName); 
            _isLoading = true;
            //SceneDelayTween.Stop

        }


        public void LoadSceneImmediate(string sName, float delay = 0f)
        {
            if (_isLoading) return;
            CheckController();

            _isLoading = true;
            OnLoadStarted?.Invoke(sName);
            OnLoading?.Invoke(sName, 1f);
            SceneManager.LoadScene(sName,  LoadSceneMode.Single);
        }

        

        public void LoadSceneAsync(string sName)
        {
            if (_isLoading) return;
            CheckController();
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
            OnLoadFinished?.Invoke(scene.name);
        }

        public override void ControllerOnSceneUnloading(Scene scene)
        {
            base.ControllerOnSceneUnloading(scene);
        }
    }
}
