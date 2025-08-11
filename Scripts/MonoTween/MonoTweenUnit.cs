using System;
using UnityEngine;
namespace Spacats.Utils
{
    public class MonoTweenUnit
    {
        public float Delay { get; private set; }
        public float Duration { get; private set; }
        public Action OnStart { get; private set; }
        public Action<float> LerpAction { get; private set; }
        public Action OnEnd { get; private set; }
        public bool ApplyPause { get; private set; }
        public int RepeatCount { get; private set; }
        public int StepsCount { get; private set; }

        private float _time;
        private float _delayTimer;
        private int _currentRepeat;
        private bool _started;
        private int _lastStepIndex = -1;

        public bool IsComplete { get; private set; }
        private bool _isStopped;

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
                    OnStart?.Invoke();
                }
                else return;
            }

            _time += deltaTime;
            float t = Mathf.Clamp01(_time / Duration);

            if (StepsCount > 0)
            {
                int stepIndex = Mathf.RoundToInt(t * StepsCount);
                if (stepIndex != _lastStepIndex)
                {
                    _lastStepIndex = stepIndex;
                    LerpAction?.Invoke((float)stepIndex / StepsCount);
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
                }
                else
                {
                    OnEnd?.Invoke();
                    IsComplete = true;
                }
            }
        }
    }
}
