using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spacats.Utils
{
    public class PauseControllerGUIExample : GUIButtons
    {
        private PauseController _cPause;
        private void CheckController()
        {
            if (_cPause == null) _cPause = ControllersHub.Instance.GetController<PauseController>();
        }

        protected override string GetButtonLabel(int index)
        {
            CheckController();
            switch (index)
            {
                default: return base.GetButtonLabel(index);
                case 0:
                    return PauseController.IsPaused ? "Unpause" : "Pause";
            }
        }

        protected override void OnButtonClick(int index)
        {
            CheckController();
            switch (index)
            {
                default: base.OnButtonClick(index); break;
                case 0: if (PauseController.IsPaused) _cPause.PauseOFF(); else _cPause.PauseON(); break;
            }
        }
    }
}
