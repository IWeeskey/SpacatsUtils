using System;
using UnityEngine;
namespace Spacats.Utils
{
    [Serializable]
    public class MonoTweenUnit
    {
        private bool _isBroken;
        private bool _selfPaused;
        private bool _started;

        [SerializeField] private float _time;
        private float _delayTimer;
        private float _stepDuration;
        private float _nextStepTime;

        private int _currentRepeat;
        private int _lastStepIndex;

        public bool IsBroken => _isBroken;
        public bool ApplyGlobalPause { get; set; }
        public bool IsComplete { get; private set; }
        public float Delay { get; set; }
        public float Duration { get; set; }
        public int RepeatCount { get; set; }
        public int StepsCount { get; set; }
        public int ChainIndex;
        public string UnitID = "";
        public Action OnStart { get; set; }
        public Action<float> OnLerp { get; set; }
        public Action OnEnd { get; set; }
        public Action OnChain { get; set; }

        public MonoTweenUnit(
            float delay,
            float duration,
            Action onStart,
            Action<float> onLerp,
            Action onEnd,
            bool applyGlobalPause = true,
            int repeatCount = 0,
            int stepsCount = 0)
        {
            Delay = Mathf.Max(0f, delay);
            Duration = Mathf.Max(0.0001f, duration);
            OnStart = onStart;
            OnLerp = onLerp;
            OnEnd = onEnd;
            ApplyGlobalPause = applyGlobalPause;
            RepeatCount = Mathf.Max(0, repeatCount);
            StepsCount = Mathf.Max(0, stepsCount);
        }

        public void Start()
        {
            Reset();
            MonoTweenController.Instance.StartSingle(this);
        }

        public void Break()
        {
            _isBroken = true;
        }

        public void Reset()
        {
            IsComplete = false;
            _isBroken = false;
            _started = false;
            _selfPaused = false;
            _delayTimer = 0f;
            _currentRepeat = 0;
            _time = 0f;
            _lastStepIndex = -1;
            _nextStepTime = 0f;
        }

        public void SelfPauseON()
        {
            _selfPaused = true;
        }

        public void SelfPauseOFF()
        {
            _selfPaused = false;
        }

        public void Update(float deltaTime, bool isGlobalPaused)
        {
            if (IsComplete || _isBroken)
            {
                IsComplete = true;
                return;
            }

            if (ApplyGlobalPause && isGlobalPaused) return;

            if (_selfPaused) return;

            if (!_started)
            {
                _delayTimer += deltaTime;

                if (_delayTimer < Delay) return;

                _started = true;
                _time = 0f;
                _lastStepIndex = -1;

                if (StepsCount > 0)
                {
                    _stepDuration = Duration / StepsCount;
                    _nextStepTime = 0f;
                }

                OnStart?.Invoke();
            }

            _time += deltaTime;
            float t = Mathf.Clamp01(_time / Duration);

            if (StepsCount > 0)
            {
                while (_time >= _nextStepTime && _lastStepIndex < StepsCount)
                {
                    _lastStepIndex++;
                    float stepProgress = Mathf.Clamp01((float)_lastStepIndex / StepsCount);
                    OnLerp?.Invoke(stepProgress);
                    _nextStepTime += _stepDuration;
                }
            }
            else
            {
                OnLerp?.Invoke(t);
            }

            if (t < 1f) return;

            if (_currentRepeat < RepeatCount)
            {
                _currentRepeat++;
                _time = 0f;
                _lastStepIndex = -1;
                if (StepsCount > 0)
                {
                    _nextStepTime = 0f;
                }
            }
            else
            {
                IsComplete = true;
            }
        }
    }
}
