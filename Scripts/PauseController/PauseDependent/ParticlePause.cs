using UnityEngine;

namespace Spacats.Utils
{
    public class ParticlePause : MonoBehaviour
    {
        private ParticleSystem _selfParticleSystem;
        private bool _eventSubscribed;

        private void Awake()
        {
            _selfParticleSystem = gameObject.GetComponent<ParticleSystem>();
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
            if (_selfParticleSystem == null) return;

            if (value)
            {
                if (_selfParticleSystem.isPlaying) _selfParticleSystem.Pause(true);
            }
            else
            {
                if (_selfParticleSystem.isPaused) _selfParticleSystem.Play(true);
            }
        }
    }
}

