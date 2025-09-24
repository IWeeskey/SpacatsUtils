using UnityEngine;

namespace Spacats.Utils
{
    [DefaultExecutionOrder(-10)]
    public class PauseController : Controller
    {
        private static PauseController _instance;
        public static PauseController Instance
        {
            get
            {
                if (_instance == null) Debug.LogError("PauseController is not initialized yet!");
                return _instance;
            }
        }
        public static bool HasInstance => _instance != null;
        
        private static bool _paused = false;
        public static bool IsPaused => _paused;

        public delegate void PauseSwitched(bool _newState);
        public static event PauseSwitched OnPauseSwitched;

        protected override void COnRegister()
        {
            base.COnRegister();
            _instance = this;
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
