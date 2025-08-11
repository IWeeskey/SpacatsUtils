using UnityEngine;

namespace Spacats.Utils
{
    public class TrailPause : MonoBehaviour
    {
        [Range(0.001f, 0.2f)]
        public float TimeRecoverySpeed = 0.02f;
        private TrailRenderer _selfTrail;
        private float _pauseTime;
        private float _resumeTime;
        private float _trailTime = 1.0f;
        private float _beforeFinish;
        private bool _eventSubscribed;
        private bool _paused = false;

        private void Awake()
        {
            _selfTrail = gameObject.GetComponent<TrailRenderer>();
            _trailTime = _selfTrail.time;
            SubscribeEvents();
        }

        private void OnDestroy()
        {
            UnSubscribeEvents();
        }

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnSubscribeEvents();
        }


        private void SubscribeEvents()
        {
            if (_eventSubscribed) return;
            PauseController.OnPauseSwitched += PauseSwitched;
            _eventSubscribed = true;
        }

        private void UnSubscribeEvents()
        {
            if (!_eventSubscribed) return;
            PauseController.OnPauseSwitched -= PauseSwitched;
            _eventSubscribed = false;
        }

        private void PauseSwitched(bool value)
        {
            _paused = value;
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

        private void Update()
        {
            if (_paused) return;
            if (_selfTrail.time > _trailTime) _selfTrail.time -= (_selfTrail.time - _trailTime) * TimeRecoverySpeed;
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
