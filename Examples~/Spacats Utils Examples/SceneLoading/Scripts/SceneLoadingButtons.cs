using UnityEngine;
using UnityEngine.SceneManagement;

namespace Spacats.Utils
{

    public class SceneLoadingButtons : GUIButtons
    {
        public string SceneToLoad = "";

        private void Awake()
        {
            GUIPermanentMessage.Instance.Message = SceneManager.GetActiveScene().name;
        }

        protected override string GetButtonLabel(int index)
        {
            switch (index)
            {
                default: return base.GetButtonLabel(index);
                case 0: return "Load immediate";
                case 1: return "Load async";
                case 2: return "Load immediate after 1 sec";
            }
        }

        protected override void OnButtonClick(int index)
        {
            switch (index)
            {
                default: base.OnButtonClick(index); break;
                case 0: SceneController.Instance.LoadSceneImmediate(SceneToLoad); break;
                case 1: SceneController.Instance.LoadSceneAsync(SceneToLoad); break;
                case 2: SceneController.Instance.LoadSceneImmediate(SceneToLoad,1f); break;
            }
        }
    }
}
