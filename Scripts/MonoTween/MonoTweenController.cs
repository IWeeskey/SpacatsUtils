using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Spacats.Utils
{
    [DefaultExecutionOrder(-10)]
    public class MonoTweenController : Controller
    {
        public bool IsPaused { get; set; }
        public int ActiveTweensCount => _activeCount;
        public int TweensListCount => _tweens.Count;
        public bool PerformMeasurements = false;

        private List<MonoTweenUnit> _tweens = new();
        private double _updateTimeMS = 0;
        private string _updateTimeString = "";
        private int _activeCount = 0;

        public double UpdateTimeMS => _updateTimeMS;
        public string UpdateTimeString => _updateTimeString;

        public override void ControllerOnSceneUnloading(Scene scene)
        {
            base.ControllerOnSceneUnloading(scene);
            BreakAll();
        }

        public void BreakAll()
        {
            _tweens.Clear();
            _activeCount = 0;
        }

        public override void ControllerSharedUpdate()
        {
            base.ControllerSharedUpdate();
            UpdateLogic();
        }

        private void UpdateLogic()
        {
            IsPaused = PauseController.IsPaused;

            if (PerformMeasurements)
            {
                TimeTracker.Start("MonoTweenController");
            }

            for (int i = _activeCount - 1; i >= 0; i--)
            {
                var tween = _tweens[i];
                tween.Update(Time.deltaTime, IsPaused);

                if (tween.IsComplete)
                {
                    int lastIndex = _activeCount - 1;
                    if (i != lastIndex)
                    {
                        _tweens[i] = _tweens[lastIndex];
                    }
                    _tweens[lastIndex] = null;
                    _activeCount--;

                    tween.OnEnd?.Invoke();
                }
            }

            if (PerformMeasurements)
            {
                (double, string) measureResult = TimeTracker.Finish("MonoTweenController", false);
                _updateTimeMS = measureResult.Item1;
                _updateTimeString = measureResult.Item2;
            }
        }

        private void Add(MonoTweenUnit tween)
        {
            if (_activeCount < _tweens.Count)
            {
                _tweens[_activeCount] = tween;
            }
            else
            {
                _tweens.Add(tween);
            }

            _activeCount++;
        }

        public void StartSingle(MonoTweenUnit unit)
        {
            Add(unit);
        }

        public void Stop(MonoTweenUnit unit)
        {
            unit.Stop();
        }

        public void StartChain(int repeatCount, params MonoTweenUnit[] tweens)
        {
            if (tweens.Length == 0) return;

            int currentRepeat = 0;

            void AddChain(int index)
            {
                if (index >= tweens.Length)
                {
                    if (repeatCount < 0 || currentRepeat < repeatCount)
                    {
                        currentRepeat++;
                        AddChain(0);
                    }
                    return;
                }

                var current = tweens[index];
                var originalOnEnd = current.OnEnd;

                current = new MonoTweenUnit(
                    current.Delay,
                    current.Duration,
                    current.OnStart,
                    current.LerpAction,
                    () =>
                    {
                        originalOnEnd?.Invoke();
                        AddChain(index + 1);
                    },
                    current.ApplyPause,
                    current.RepeatCount,
                    current.StepsCount
                );

                Add(current);
            }

            AddChain(0);
        }

    }
}
