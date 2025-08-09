using UnityEngine;

namespace Spacats.Utils
{
    public class AnimatorPause : MonoBehaviour
    {
        private Animator _selfAnimator;
        private bool _eventSubscribed;
        private float _speedBeforePause = 1f;

        private void Awake()
        {
            _selfAnimator = gameObject.GetComponent<Animator>();
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
                _speedBeforePause = _selfAnimator.speed;
                _selfAnimator.speed = 0f;
            }
            else
            {
                _selfAnimator.speed = _speedBeforePause;
            }
        }
    }
}
