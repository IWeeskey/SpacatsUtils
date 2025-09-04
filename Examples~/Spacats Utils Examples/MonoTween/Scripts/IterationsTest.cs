using UnityEngine;
namespace Spacats.Utils
{
    public class IterationsTest : GUIButtons
    {
        [Header("MonoTweenTest Settings")]
        private int _oneMil = 1000000;

        private void Awake()
        {
            Application.targetFrameRate = 60;
        }

        private void Update()
        {

        }


        protected override string GetButtonLabel(int index)
        {
            switch (index)
            {
                default: return base.GetButtonLabel(index);
                case 0:
                    if (!GUILogViewer.Instance.LoggingEnabled) return "Logging Disabled";
                    return GUILogViewer.Instance.IsOpened ? "Hide Log" : "Show Log";
                case 1: return "Start 1mil";
                case 2: return "Start 100k";
                case 3: return "Start 10k";
            }
        }

        protected override void OnButtonClick(int index)
        {
            switch (index)
            {
                default: base.OnButtonClick(index); break;
                case 0: SwitchShowHideLog(); break;
                case 1: LaunchIterations(_oneMil); break;
                case 2: LaunchIterations(_oneMil/10); break;
                case 3: LaunchIterations(_oneMil / 100); break;
            }
        }

        private void SwitchShowHideLog()
        {
            if (GUILogViewer.Instance.IsOpened) GUILogViewer.Instance.CloseLog();
            else GUILogViewer.Instance.OpenLog();
        }

        private void LaunchIterations(int count)
        {
            TimeTracker.Start("IterationsTest");

            int counter = 0;
            for (int i = 0; i < count; i++)
            {
                counter += i & 1;
            }

            TimeTracker.Finish("IterationsTest");

            Debug.Log("Count: " + counter);
        }

    }
}
