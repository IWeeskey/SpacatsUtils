using System.Collections;
using System.Collections.Generic;
using Spacats.Utils;
using UnityEngine;

namespace Spacats.Utils
{
    public class ExampleControllersGUIs : GUIButtons
    {
        public string SceneToLoad = "";


        protected override string GetButtonLabel(int index)
        {
            switch (index)
            {
                default: return base.GetButtonLabel(index);
                case 0: return "Load immediate: " + SceneToLoad;
            }
        }

        protected override void OnButtonClick(int index)
        {
            switch (index)
            {
                default: base.OnButtonClick(index); break;
                case 0: SceneController.Instance.LoadSceneImmediate(SceneToLoad); break;
            }
        }
    }
}
