using UnityEngine;
namespace Spacats.Utils
{
    public class MonoTweenMeasureTest : GUIButtons
    {
        private MonoTweenController _cMonoTween;
        private GUILogViewer _cLogViewer;
        private GUIPermanentMessage _cPermMessage;

        [Header("MonoTweenTest Settings")]
        
        public int TweensCount = 100;
        public bool LogStartEndTweens = false;
        public bool LogProgressTweens = false;
        public float MinDuration = 0.5f;
        public float MaxDuration = 2f;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            CheckController();
        }

        private void StartTweens()
        {
            CheckController();

            TimeTracker.Start("StartTweens " + TweensCount.ToString());

            for (int i = 0; i < TweensCount; i++)
            {
                int id = i;
                float randomDuration = Random.Range(MinDuration, MaxDuration);
                var tween = new MonoTweenUnit(
                    delay: 0f,
                    duration: randomDuration,
                    onStart: LogStartEndTweens ? () => Debug.Log($"Tween {id} start") : null,
                    lerpAction: LogProgressTweens ? (t => Debug.Log($"Tween {id} progress {t:F2}")) : null,
                    onEnd: LogStartEndTweens ? () => Debug.Log($"Tween {id} end") : null
                );

                _cMonoTween.StartSingle(tween);
            }

            TimeTracker.Finish("StartTweens " + TweensCount.ToString());
        }

        private void Update()
        {
            if (_cPermMessage == null) return;
            _cPermMessage.Message = "Count: " + _cMonoTween.TweensCount + "; ms: " + _cMonoTween.UpdateTimeMS.ToString();
        }

        private void CheckController()
        {
            if (_cMonoTween == null)  _cMonoTween = ControllersHub.Instance.GetController<MonoTweenController>();
            if (_cLogViewer == null) _cLogViewer = ControllersHub.Instance.GetController<GUILogViewer>();
            if (_cPermMessage == null) _cPermMessage = ControllersHub.Instance.GetController<GUIPermanentMessage>();
        }

        protected override string GetButtonLabel(int index)
        {
            CheckController();
            switch (index)
            {
                default: return base.GetButtonLabel(index);
                case 0:
                    if (!_cLogViewer.LoggingEnabled) return "Logging Disabled";
                    return _cLogViewer.IsOpened ? "Hide Log" : "Show Log";
                case 1: return "Start \n tweens ("+ TweensCount + ")";
            } 
        }

        protected override void OnButtonClick(int index)
        {
            CheckController();
            switch (index)
            {
                default: base.OnButtonClick(index); break;
                case 0: SwitchShowHideLog(); break;
                case 1: StartTweens(); break;
            }
        }

        private void SwitchShowHideLog()
        {
            if (_cLogViewer.IsOpened) _cLogViewer.CloseLog();
            else _cLogViewer.OpenLog();
        }
    }
}
