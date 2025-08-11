using System.Collections.Generic;
using UnityEngine;
namespace Spacats.Utils
{
    [DefaultExecutionOrder(-10)]
    public class MonoTweenController : Controller
    {
        public bool IsPaused { get; set; }

        private List<MonoTweenUnit> _tweens = new();
        public int TweensCount => _tweens.Count;

        public bool PerformMeasurements = false;
        private double _updateTimeMS = 0;
        private string _updateTimeString = "";

        public double UpdateTimeMS => _updateTimeMS;
        public string UpdateTimeString => _updateTimeString;

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

            for (int i = _tweens.Count - 1; i >= 0; i--)
            {
                var tween = _tweens[i];
                tween.Update(Time.deltaTime, IsPaused);

                if (tween.IsComplete)
                {
                    _tweens.RemoveAt(i);
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
            _tweens.Add(tween);
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
