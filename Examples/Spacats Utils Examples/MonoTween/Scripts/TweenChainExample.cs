using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spacats.Utils
{
    public class TweenChainExample : MonoBehaviour
    {
        public Transform AnimationTarget;
        public float TweenDuration = 1f;

        public List<Vector3> LocalPositions = new List<Vector3>();

        private MonoTweenUnit _tween0;
        private MonoTweenUnit _tween1;
        private MonoTweenUnit _tween2;
        private MonoTweenUnit _tween3;
        private bool _paused = false;

        //private MonoTweenController _cMonoTween;

        private void Awake()
        {
            CreateChainTweens();
        }

        //private void CheckController()
        //{
        //    if (_cMonoTween == null) _cMonoTween = ControllersHub.Instance.GetController<MonoTweenController>();
        //}

        private void CreateChainTweens()
        {
            _tween0 = new MonoTweenUnit(
                    delay: 0f,
                    duration: TweenDuration,
                    onStart: ()=> { },
                    onLerp: (float lerp)=> { LerpAnimatonTarget(LocalPositions[0], LocalPositions[1], lerp); },
                    onEnd: () => { }
                );

            _tween1 = new MonoTweenUnit(
                   delay: 0f,
                   duration: TweenDuration,
                   onStart: () => { },
                   onLerp: (float lerp) => { LerpAnimatonTarget(LocalPositions[1], LocalPositions[2], lerp); },
                   onEnd: () => { }
               );

            _tween2 = new MonoTweenUnit(
                   delay: 0f,
                   duration: TweenDuration,
                   onStart: () => { },
                   onLerp: (float lerp) => { LerpAnimatonTarget(LocalPositions[2], LocalPositions[3], lerp); },
                   onEnd: () => { }
               );

            _tween3 = new MonoTweenUnit(
                   delay: 0f,
                   duration: TweenDuration,
                   onStart: () => { },
                   onLerp: (float lerp) => { LerpAnimatonTarget(LocalPositions[3], LocalPositions[0], lerp); },
                   onEnd: () => { }
               );

            
        }

        private void LerpAnimatonTarget(Vector3 startPos, Vector3 targetPos, float lerpProgress)
        {
            if (AnimationTarget == null) return;
            AnimationTarget.localPosition = Vector3.Lerp(startPos, targetPos, lerpProgress);
        }

        public void StartTweens()
        {
            MonoTweenController.Instance.StartChain(-1, _tween0, _tween1, _tween2, _tween3);
        }

        public void SwitchPauseTweens()
        {
            _paused = !_paused;
            MonoTweenController.Instance.PauseChain(_paused, _tween0, _tween1, _tween2, _tween3);
        }

        public void StopTweens()
        {
            MonoTweenController.Instance.StopChain(_tween0, _tween1, _tween2, _tween3);
        }
    }
}
