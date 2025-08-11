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

