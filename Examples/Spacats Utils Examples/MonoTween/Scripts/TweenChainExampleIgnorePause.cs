using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spacats.Utils
{
    public class TweenChainExampleIgnorePause : MonoBehaviour
    {
        public Transform AnimationTarget;
        public int ChainRepeatCount = 3;

        public int CurrentTweenPlayingIndex = 0;
        public int CurrentChainIndex = 0;

        public float TweenDuration = 1f;

        public List<Vector3> LocalPositions = new List<Vector3>();
        private void Awake()
        {
            StartChainTween();
        }

        private void StartChainTween()
        {
            MonoTweenUnit tw0 = new MonoTweenUnit(
                    delay: 0f,
                    duration: TweenDuration,
                    onStart: ()=> { },
                    onLerp: (float lerp)=> { LerpAnimatonTarget(LocalPositions[0], LocalPositions[1], lerp); },
                    onEnd: () => { },
                    false
                );

            tw0.OnStart = () => { CurrentTweenPlayingIndex = 0; CurrentChainIndex = tw0.ChainIndex; };

            MonoTweenUnit tw1 = new MonoTweenUnit(
                   delay: 0f,
                   duration: TweenDuration,
                   onStart: () => { CurrentTweenPlayingIndex = 1; },
                   onLerp: (float lerp) => { LerpAnimatonTarget(LocalPositions[1], LocalPositions[2], lerp); },
                   onEnd: () => { },
                   false
               );

            MonoTweenUnit tw2 = new MonoTweenUnit(
                   delay: 0f,
                   duration: TweenDuration,
                   onStart: () => { CurrentTweenPlayingIndex = 2; },
                   onLerp: (float lerp) => { LerpAnimatonTarget(LocalPositions[2], LocalPositions[3], lerp); },
                   onEnd: () => { },
                   false
               );

            MonoTweenUnit tw3 = new MonoTweenUnit(
                   delay: 0f,
                   duration: TweenDuration,
                   onStart: () => { CurrentTweenPlayingIndex = 3; },
                   onLerp: (float lerp) => { LerpAnimatonTarget(LocalPositions[3], LocalPositions[0], lerp); },
                   onEnd: () => { },
                   false
               );

            MonoTweenController.Instance.StartChain(ChainRepeatCount, tw0, tw1, tw2, tw3);
        }

        private void LerpAnimatonTarget(Vector3 startPos, Vector3 targetPos, float lerpProgress)
        {
            if (AnimationTarget == null) return;
            AnimationTarget.localPosition = Vector3.Lerp(startPos, targetPos, lerpProgress);
        }
    }
}
