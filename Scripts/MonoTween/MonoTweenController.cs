using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Spacats.Utils
{
    [DefaultExecutionOrder(-10)]
    public class MonoTweenController : Controller
    {
        private static MonoTweenController _instance;
        private List<MonoTweenUnit> _tweens = new();
        private double _updateTimeMS = 0;
        private string _updateTimeString = "";
        private int _activeCount = 0;

        public static MonoTweenController Instance
        {
            get
            {
                if (_instance == null) Debug.LogError("MonoTweenController is not initialized yet!");
                return _instance;
            }
        }

        public bool IsPaused { get; set; }
        public int ActiveTweensCount => _activeCount;
        public int TweensListCount => _tweens.Count;
        public bool PerformMeasurements = false;
        public double UpdateTimeMS => _updateTimeMS;
        public string UpdateTimeString => _updateTimeString;

        protected override void OnRegister()
        {
            base.OnRegister();
            _instance = this;
        }

        public override void ControllerOnSceneUnloading(Scene scene)
        {
            base.ControllerOnSceneUnloading(scene);
            BreakAll();
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

                    if (tween.IsBroken) continue;

                    tween.OnEnd?.Invoke();
                    tween.OnChain?.Invoke();
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

        public void StartSingle(MonoTweenUnit unit)
        {
            Add(unit);
        }

        public void Break(MonoTweenUnit unit)
        {
            unit.Break();
        }

        public void StartChain(int repeatCount, params MonoTweenUnit[] tweens)
        {
            if (tweens.Length == 0) return;
            tweens[0].ChainIndex = 0;

            for (int i = 0; i < tweens.Length; i++)
            {
                int index = i;
                
                if (index < tweens.Length - 1)
                {
                    tweens[index].OnChain = () =>
                    {
                        tweens[index + 1].Start();
                    };
                    continue;
                }

                tweens[index].OnChain = () => 
                {
                    tweens[0].ChainIndex++;
                    if (repeatCount<0 || tweens[0].ChainIndex < repeatCount) tweens[0].Start();
                };
            }

            tweens[0].Start();
        }

        public void StopChain(params MonoTweenUnit[] tweens)
        {
            if (tweens.Length == 0) return;
            for (int i = 0; i < tweens.Length; i++)
            {
                tweens[i].Break();
            }
        }

        public void PauseChain(bool pause, params MonoTweenUnit[] tweens)
        {
            if (tweens.Length == 0) return;
            for (int i = 0; i < tweens.Length; i++)
            {
                if (pause) tweens[i].SelfPauseON();
                else tweens[i].SelfPauseOFF();
            }
        }
    }
}
