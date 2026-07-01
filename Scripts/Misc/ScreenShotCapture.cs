using UnityEngine;
using System;
using UnityEngine.InputSystem;

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

		[Tooltip("Resolution scale of screenshots")]
		public int SuperSize = 1;

		[Tooltip("Change it to capture screenshot while not in play mode")]
		public bool EditorCaptureScreen = false;

		[SerializeField] private InputActionReference _captureScreenshotAction;
		[SerializeField] private InputActionReference _pauseEditorAction;
		[SerializeField] private InputActionReference _pauseGameAction;

		protected override void COnRegister()
		{
			base.COnRegister();
			_instance = this;

			// Fallback to global actions if references are not assigned
			if (_captureScreenshotAction == null && InputSystem.actions != null)
				_captureScreenshotAction = InputActionReference.Create(InputSystem.actions.FindAction("Debug/CaptureScreenshot"));
			
			if (_pauseEditorAction == null && InputSystem.actions != null)
				_pauseEditorAction = InputActionReference.Create(InputSystem.actions.FindAction("Debug/PauseEditor"));
			
			if (_pauseGameAction == null && InputSystem.actions != null)
				_pauseGameAction = InputActionReference.Create(InputSystem.actions.FindAction("Debug/PauseGame"));

			// Enable actions if they are not enabled (crucial for non-project-wide or custom setups)
			_captureScreenshotAction?.action?.Enable();
			_pauseEditorAction?.action?.Enable();
			_pauseGameAction?.action?.Enable();
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
			if (_pauseEditorAction == null || !_pauseEditorAction.action.WasPressedThisFrame()) return;
			Debug.Break();
		}
		
		private void CatchGamePause()
		{
			if (_pauseGameAction == null || !_pauseGameAction.action.WasPressedThisFrame()) return;
			if (!PauseController.HasInstance) return;
			PauseController.Instance.SwitchPause();
		}

		private void CatchScreenshotCapture()
		{
#if UNITY_EDITOR
			bool inputPressed = _captureScreenshotAction != null && _captureScreenshotAction.action.WasPressedThisFrame();
			if (!inputPressed && !EditorCaptureScreen) return;
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
