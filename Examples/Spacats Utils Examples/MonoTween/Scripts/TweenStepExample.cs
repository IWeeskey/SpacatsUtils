using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spacats.Utils
{
    public class TweenStepExample : MonoBehaviour
    {
        public Transform AnimationTarget;
        public float TweenDuration = 1f;

        [Range(1,20)]
        public int Steps = 4;

        public List<Vector3> LocalPositions = new List<Vector3>();

        private MonoTweenController _cMonoTween;
        [SerializeField]private MonoTweenUnit _stepTween;

        private void Awake()
        {
            CheckController();
            CreateTween();
            StartLoop();
        }

        private void CheckController()
        {
            if (_cMonoTween == null) _cMonoTween = ControllersHub.Instance.GetController<MonoTweenController>();
        }

        private void CreateTween()
        {
            _stepTween = new MonoTweenUnit(
                       delay: 0f,
                       duration: TweenDuration,
                       onStart: () => { ResetPosition(); },
                       onLerp: (float lerp) => { LerpAnimatonTarget(LocalPositions[0], LocalPositions[1], lerp); },
                       onEnd: () => { StartLoop(); },
                       true,
                       0,
                       Steps
                   );
        }

        private void StartLoop()
        {
            _stepTween.StepsCount = Steps;
            _stepTween.Duration = TweenDuration;
            _stepTween.Reset();
            _cMonoTween.StartSingle(_stepTween);
        }

        private void LerpAnimatonTarget(Vector3 startPos, Vector3 targetPos, float lerpProgress)
        {
            if (AnimationTarget == null) return;
            AnimationTarget.localPosition = Vector3.Lerp(startPos, targetPos, lerpProgress);
        }

        private void ResetPosition()
        {
            if (AnimationTarget == null) return;

            AnimationTarget.localPosition = LocalPositions[0];
        }
    }
}
