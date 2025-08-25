using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditor;
#endif
namespace Spacats.Utils
{
    public static class SceneLoaderHelper
    {
        private static bool _isLoading = false;
        public static bool IsLoading => _isLoading;
        public static void MarkActiveSceneDirty()
        {
            if (Application.isPlaying) return;
#if UNITY_EDITOR
            var scene = SceneManager.GetActiveScene();
            if (scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(scene);
            }
#endif
        }

        public static void SaveActiveScene()
        {
            if (Application.isPlaying) return;
#if UNITY_EDITOR
            var scene = SceneManager.GetActiveScene();
            if (scene.IsValid())
            {
                EditorSceneManager.SaveScene(scene);
            }
#endif
        }

        public static void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        {
            SceneManager.LoadScene(sceneName, mode);
        }

        public static bool IsSceneLoaded(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).name == sceneName)
                    return true;
            }
            return false;
        }

        public static Coroutine LoadSceneAsync(MonoBehaviour runner, string sceneName,
            Action<float> onProgress = null,
            LoadSceneMode mode = LoadSceneMode.Single)
        {
            if (_isLoading) return null;
            return runner.StartCoroutine(LoadSceneRoutine(sceneName, onProgress, mode));
        }

        private static IEnumerator LoadSceneRoutine(string sceneName, Action<float> onProgress, LoadSceneMode mode)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, mode);

            if (operation == null)
            {
                Debug.LogError($"Scene {sceneName} not found!");
                _isLoading = false;
                yield break;
            }

            operation.allowSceneActivation = false;

            while (operation.progress < 0.9f)
            {
                onProgress?.Invoke(operation.progress);
                yield return null;
            }
            onProgress?.Invoke(1f);
            _isLoading = false;
            operation.allowSceneActivation = true;
        }
    }
}