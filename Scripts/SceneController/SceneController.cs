using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Spacats.Utils
{
    [DefaultExecutionOrder(-10)]
    public class SceneController : Controller
    {
        private static SceneController _instance;
        private bool _isLoading = false;
        private string _loadingSceneName = "";
        private MonoTweenUnit _sceneDelayTween;

        public static SceneController Instance
        {
            get
            {
                if (_instance == null) Debug.LogError("SceneController is not registered yet!");
                return _instance;
            }
        }
        public string LoadingSceneName => _loadingSceneName;
        public bool IsLoading => _isLoading;
        public event UnityAction<string> OnLoadStarted;
        public event UnityAction<string, float> OnLoading;
        public event UnityAction<string> OnLoadFinished;

        protected override void COnRegister()
        {
            base.COnRegister();
            _instance = this;
        }

        private void LoadSceneImmediateWithDelay(string sName, float delay)
        {
            if (_sceneDelayTween == null)
            {
                _sceneDelayTween = new MonoTweenUnit(
                       delay: delay,
                       duration: 0f,
                       onStart: () => { },
                       onLerp: (float lerp) => { },
                       onEnd: () => { SceneManager.LoadScene(sName, LoadSceneMode.Single); }
                   );
            }

            _sceneDelayTween.Delay = delay;
            OnLoadStarted?.Invoke(sName);
            _sceneDelayTween.Start();
        }

        public void LoadSceneImmediate(string sName, float delay = 0f)
        {
            if (_isLoading) return;
            _isLoading = true;

            _loadingSceneName = sName;

            if (delay > 0f)
            {
                LoadSceneImmediateWithDelay(sName, delay);
                return;
            }

            OnLoadStarted?.Invoke(sName);
            OnLoading?.Invoke(sName, 1f);
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
            _loadingSceneName = sName;
            SceneLoaderHelper.LoadSceneAsync(this, sName,
                progress => { Debug.Log($"Loading progress: {progress * 100f}%"); },
                LoadSceneMode.Single);
        }

        public override void COnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!_registered) return;

            base.COnSceneLoaded(scene, mode);

            _isLoading = false;
            _loadingSceneName = "";
            OnLoadFinished?.Invoke(scene.name);
        }

        public override void COnSceneUnloading(Scene scene)
        {
            base.COnSceneUnloading(scene);
        }
    }
}
