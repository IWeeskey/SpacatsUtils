using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spacats.Utils
{
    public class TweenChainExampleIgnorePause : MonoBehaviour
    {
        public Transform AnimationTarget;
        public float TweenDuration = 1f;

        public List<Vector3> LocalPositions = new List<Vector3>();

        private MonoTweenController _cMonoTween;

        private void Awake()
        {
            CheckController();
            StartChainTween();
        }

        private void CheckController()
        {
            if (_cMonoTween == null) _cMonoTween = ControllersHub.Instance.GetController<MonoTweenController>();
        }

        private void StartChainTween()
        {
            MonoTweenUnit tw0 = new MonoTweenUnit(
                    delay: 0f,
                    duration: TweenDuration,
                    onStart: ()=> { },
                    lerpAction: (float lerp)=> { LerpAnimatonTarget(LocalPositions[0], LocalPositions[1], lerp); },
                    onEnd: () => { },
                    false
                );

            MonoTweenUnit tw1 = new MonoTweenUnit(
                   delay: 0f,
                   duration: TweenDuration,
                   onStart: () => { },
                   lerpAction: (float lerp) => { LerpAnimatonTarget(LocalPositions[1], LocalPositions[2], lerp); },
                   onEnd: () => { },
                   false
               );

            MonoTweenUnit tw2 = new MonoTweenUnit(
                   delay: 0f,
                   duration: TweenDuration,
                   onStart: () => { },
                   lerpAction: (float lerp) => { LerpAnimatonTarget(LocalPositions[2], LocalPositions[3], lerp); },
                   onEnd: () => { },
                   false
               );

            MonoTweenUnit tw3 = new MonoTweenUnit(
                   delay: 0f,
                   duration: TweenDuration,
                   onStart: () => { },
                   lerpAction: (float lerp) => { LerpAnimatonTarget(LocalPositions[3], LocalPositions[0], lerp); },
                   onEnd: () => { },
                   false
               );

            _cMonoTween.StartChain(-1, tw0, tw1, tw2, tw3);
        }

        private void LerpAnimatonTarget(Vector3 startPos, Vector3 targetPos, float lerpProgress)
        {
            if (AnimationTarget == null) return;
            AnimationTarget.localPosition = Vector3.Lerp(startPos, targetPos, lerpProgress);
        }
    }
}
