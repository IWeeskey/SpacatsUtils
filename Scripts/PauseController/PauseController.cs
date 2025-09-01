using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Spacats.Utils
{
    [DefaultExecutionOrder(-10)]
    public class PauseController : Controller
    {
        private static bool _paused = false;
        public static bool IsPaused => _paused;

        public delegate void PauseSwitched(bool _newState);
        public static event PauseSwitched OnPauseSwitched;

        protected override void COnEnable()
        {
            base.COnEnable();
        }

        protected override void COnDisable()
        {
            base.COnDisable();
        }

        public override void CSharedUpdate()
        {
            base.CSharedUpdate();
        }

        public void SwitchPause()
        {
            if (_paused) PauseOFF();
            else PauseON();
        }


        public void PauseON()
        {
            if (!Application.isPlaying) return;
            if (_paused) return;
            _paused = true;
            OnPauseSwitched?.Invoke(_paused);
        }

        public void PauseOFF()
        {
            if (!Application.isPlaying) return;
            if (!_paused) return;
            _paused = false;
            OnPauseSwitched?.Invoke(_paused);
        }
    }
}
