using UnityEngine;

namespace Spacats.Utils
{
    public class TrailPause : MonoBehaviour
    {
        private TrailRenderer _selfTrail;
        private float _pauseTime;
        private float _resumeTime;
        private float _trailTime = 1.0f;
        private float _beforeFinish;
        private bool _eventSubscribed;

        private void Awake()
        {
            _selfTrail = gameObject.GetComponent<TrailRenderer>();
            _trailTime = _selfTrail.time;
            SubscribeEvents();
        }

        public void OnDestroy()
        {
            UnSubscribeEvents();
        }

        public void OnEnable()
        {
            SubscribeEvents();
        }

        public void OnDisable()
        {
            UnSubscribeEvents();
        }


        void SubscribeEvents()
        {
            if (_eventSubscribed) return;
            PauseController.OnPauseSwitched += PauseSwitched;
            _eventSubscribed = true;
        }

        void UnSubscribeEvents()
        {
            if (!_eventSubscribed) return;
            PauseController.OnPauseSwitched -= PauseSwitched;
            _eventSubscribed = false;
        }

        void PauseSwitched(bool value)
        {
            if (value)
            {
                _pauseTime = Time.time;
                _selfTrail.time = Mathf.Infinity;
            }
            else
            {
                _resumeTime = Time.time;
                _selfTrail.time = (_resumeTime - _pauseTime) + _trailTime;
            }
        }

        public void FinishTrail()
        {
            _beforeFinish = _selfTrail.time;
            _selfTrail.time = 0f;
        }

        public void ResumeFromeFinish()
        {
            _selfTrail.time = _beforeFinish;
        }
    }
}
