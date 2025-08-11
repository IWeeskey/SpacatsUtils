using System;
using UnityEngine;
namespace Spacats.Utils
{
    [Serializable]
    public class MonoTweenUnit
    {
        public string UnitID = "";
        public float Delay { get; set; }
        public float Duration { get; set; }
        public Action OnStart { get; set; }
        public Action<float> LerpAction { get; set; }
        public Action OnEnd { get; set; }
        public bool ApplyPause { get; set; }
        public int RepeatCount { get; set; }
        public int StepsCount { get; set; }

        [SerializeField] private float _time;
        private float _delayTimer;
        private int _currentRepeat;
        private bool _started;
        private int _lastStepIndex = -1;

        public bool IsComplete { get; private set; }
        private bool _isStopped;
        private float _stepDuration;
        private float _nextStepTime;

        public MonoTweenUnit(
            float delay,
            float duration,
            Action onStart,
            Action<float> lerpAction,
            Action onEnd,
            bool applyPause = true,
            int repeatCount = 0,
            int stepsCount = 0)
        {
            Delay = Mathf.Max(0f, delay);
            Duration = Mathf.Max(0.0001f, duration);
            OnStart = onStart;
            LerpAction = lerpAction;
            OnEnd = onEnd;
            ApplyPause = applyPause;
            RepeatCount = Mathf.Max(0, repeatCount);
            StepsCount = Mathf.Max(0, stepsCount);
        }

        public void Reset()
        {
            IsComplete = false;
            _isStopped = false;
            _started = false;
            _delayTimer = 0f;
            _currentRepeat = 0;
            _time = 0f;
            _lastStepIndex = -1;
            _nextStepTime = 0f;
        }

        public void Stop()
        {
            _isStopped = true;
        }

        public void Update(float deltaTime, bool isPaused)
        {
            if (IsComplete || _isStopped)
            {
                IsComplete = true;
                return;
            }

            if (ApplyPause && isPaused)
                return;

            if (!_started)
            {
                _delayTimer += deltaTime;
                if (_delayTimer >= Delay)
                {
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
                else return;
            }

            _time += deltaTime;
            float t = Mathf.Clamp01(_time / Duration);

            if (StepsCount > 0)
            {
                while (_time >= _nextStepTime && _lastStepIndex < StepsCount)
                {
                    _lastStepIndex++;
                    float stepProgress = Mathf.Clamp01((float)_lastStepIndex / StepsCount);
                    LerpAction?.Invoke(stepProgress);
                    _nextStepTime += _stepDuration;
                }
            }
            else
            {
                LerpAction?.Invoke(t);
            }

            if (t >= 1f)
            {
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
}
