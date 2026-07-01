using UnityEngine;
namespace Spacats.Utils
{
    public class TimeTrackerExample : GUIButtons
    {
        private GUILogViewer _cLogViewer;

        private void CheckController()
        {
            if (_cLogViewer != null) return;
            _cLogViewer = ControllersHub.Instance.GetController<GUILogViewer>();
        }

        protected override string GetButtonLabel(int index)
        {
            CheckController();
            switch (index)
            {
                default: return base.GetButtonLabel(index);
                case 0: if (!_cLogViewer.LoggingEnabled) return "Logging Disabled";
                    return _cLogViewer.IsOpened?"Hide Log":"Show Log";
                case 1: return "Start measure";
                case 2: return "Finish measure";
            }
        }

        protected override void OnButtonClick(int index)
        {
            CheckController();
            switch (index)
            {
                default: base.OnButtonClick(index); break;
                case 0: SwitchShowHideLog(); break;
                case 1: TimeTracker.Start("Example"); Debug.Log("Measurement started"); break;
                case 2: TimeTracker.Finish("Example"); break;
            }
        }

        private void SwitchShowHideLog()
        {
            CheckController();
            if (_cLogViewer.IsOpened) _cLogViewer.CloseLog();
            else _cLogViewer.OpenLog();
        }
    }
}
