using System.Collections.Generic;
using UnityEngine;
namespace Spacats.Utils
{
    public class GUILogViewer : Controller
    {
        [Header("Log Window Settings")]
        [Range(0f, 1f)]
        [SerializeField] private float _topPercent = 0.0f;

        [Range(0f, 1f)]
        [SerializeField] private float _bottomPercent = 0.1f;

        [Range(0.01f, 0.2f)]
        [SerializeField] private float _fontSizePercent = 0.02f;

        [Header("Functionality")]
        public bool LoggingEnabled = true;
        [SerializeField] private bool _isLogOpen = false;
        public bool IsOpened => _isLogOpen;
        public void OpenLog() => _isLogOpen = true;
        public void CloseLog() => _isLogOpen = false;


        private Vector2 _scrollPosition;
        private readonly List<LogEntry> _logs = new List<LogEntry>();
        public void ClearLog() => _logs.Clear();

        private struct LogEntry
        {
            public string Message;
            public LogType Type;
        }

        protected override void ControllerOnEnable()
        {
            base.ControllerOnEnable();
            if (!LoggingEnabled) return;
            Application.logMessageReceived += HandleLog;
        }

        protected override void ControllerOnDisable()
        {
            base.ControllerOnDisable();
            if (!LoggingEnabled) return;
            Application.logMessageReceived -= HandleLog;
        }

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            if (!LoggingEnabled) return;
            _logs.Add(new LogEntry { Message = logString, Type = type });
        }

        private void OnGUI()
        {
            if (!Application.isPlaying) return;
            if (!LoggingEnabled || !_isLogOpen) return;

            float topY = Screen.height * _topPercent;
            float bottomY = Screen.height * _bottomPercent;
            float height = Screen.height - topY - bottomY;
            Rect windowRect = new Rect(0, topY, Screen.width, height);

            Color oldColor = GUI.color;
            GUI.color = new Color(0, 0, 0, 0.7f);
            GUI.DrawTexture(windowRect, Texture2D.whiteTexture);
            GUI.color = oldColor;

            GUI.skin.label.fontSize = Mathf.RoundToInt(Screen.width * _fontSizePercent);
            GUI.skin.label.wordWrap = true;

            float contentHeight = _logs.Count * (GUI.skin.label.lineHeight + 4);
            _scrollPosition = GUI.BeginScrollView(
                windowRect,
                _scrollPosition,
                new Rect(0, 0, Screen.width - 20, contentHeight)
            );

            float y = 0;
            foreach (var log in _logs)
            {
                GUI.contentColor = GetColorForLogType(log.Type);
                GUI.Label(
                    new Rect(5, y, Screen.width - 30, GUI.skin.label.lineHeight * 3),
                    log.Message
                );
                y += GUI.skin.label.lineHeight + 4;
            }

            GUI.contentColor = Color.white;
            GUI.EndScrollView();
        }

        private Color GetColorForLogType(LogType type)
        {
            switch (type)
            {
                case LogType.Warning:
                    return Color.yellow;
                case LogType.Error:
                case LogType.Exception:
                case LogType.Assert:
                    return Color.red;
                default:
                    return Color.white;
            }
        }

    }
}
