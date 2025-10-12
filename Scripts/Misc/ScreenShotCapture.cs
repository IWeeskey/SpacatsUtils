using UnityEngine;
using System;

namespace Spacats.Utils
{
	[DefaultExecutionOrder(-10)]
	public class ScreenShotCapture : Controller
	{
		private static ScreenShotCapture _instance;
		public static ScreenShotCapture Instance
		{
			get
			{
				if (_instance == null) Debug.LogError("ScreenShotCapture is not initialized yet!");
				return _instance;
			}
		}
		public static bool HasInstance => _instance != null;

		public bool PerformLogic = true;
		
		[Tooltip("Path to save screenshots")]
		public string SavePath = "C:/";

		[Tooltip("Prefix to screenshot file name")]
		public string NamePrefix = "TestScreenShot";

		[Tooltip("KeyCode to capture screenshot")]
		public KeyCode CaptureKey = KeyCode.C;

		[Tooltip("KeyCode to pause editor")]
		public KeyCode EditorPauseKey = KeyCode.P;

		[Tooltip("KeyCode to pause game")]
		public KeyCode GamePauseKey = KeyCode.O;

		[Tooltip("Resolution scale of screenshots")]
		public int SuperSize = 1;

		[Tooltip("Change it to capture screenshot while not in play mode")]
		public bool EditorCaptureScreen = false;

		protected override void COnRegister()
		{
			base.COnRegister();
			_instance = this;
		}

		public override void CSharedUpdate(bool isGuiCall = false)
		{
			if (!PerformLogic) return;
			
			base.CSharedUpdate();
			CatchScreenshotCapture();
			CatchEditorPause();
			CatchGamePause();
		}

		private void CatchEditorPause()
		{
			if (!Input.GetKeyDown(EditorPauseKey)) return;
			Debug.Break();
		}
		
		private void CatchGamePause()
		{
			if (!Input.GetKeyDown(GamePauseKey)) return;
			if (!PauseController.HasInstance) return;
			PauseController.Instance.SwitchPause();
		}

		private void CatchScreenshotCapture()
		{
#if UNITY_EDITOR
			if (!Input.GetKeyDown(CaptureKey) && !EditorCaptureScreen) return;
			EditorCaptureScreen = false;
			string path = SavePath;
			string screenName = NamePrefix + "_" + DateTime.Now.Day.ToString() + "_" + DateTime.Now.Month.ToString() + "_" + DateTime.Now.Year.ToString() + "_" +
			                    DateTime.Now.Hour.ToString() + "_" + DateTime.Now.Minute.ToString() + "_" + DateTime.Now.Second.ToString() + ".png";

			ScreenCapture.CaptureScreenshot(path + screenName, SuperSize);
			Debug.Log(path + screenName);
#endif
		}
	}
}
