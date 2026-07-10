using UnityEngine;

namespace Spacats.Utils
{
    [DefaultExecutionOrder(-10)]
    public class GUIFps : Controller
    {
        [Header("Display Settings")]
        [Range(0f, 1f)] public float PosX = 0.01f;
        [Range(0f, 1f)] public float PosY = 0.01f;
        [Range(0f, 0.05f)] public float FontSize = 0.025f;
        public Color FontColor = Color.gray;
        public Font MonoFont;

        [Header("Logic Settings")]
        public bool LogicEnabled = true;
        public bool ShowExtra = true;

        private float _deltaTime;

        // Frame timings from FrameTimingManager
        private double _cpuMainMs;
        private double _cpuRenderMs;
        private double _gpuMs;
        private bool _timingReady;

        private static readonly FrameTiming[] _frameTimings = new FrameTiming[1];

        public override void CSharedUpdate(bool isGuiCall = false)
        {
            base.CSharedUpdate();
            if (isGuiCall) return;
            DoLogic();
        }

        private void DoLogic()
        {
            if (!ExecuteInEditor && !Application.isPlaying) return;
            if (!LogicEnabled) return;

            _deltaTime = Time.unscaledDeltaTime;

            // FrameTimingManager: захватываем данные по завершённому кадру.
            // Если фича включена в настройках (Player > Frame Timing Statistics),
            // то в cpuMainThreadFrameTime придёт реальное CPU время main thread.
            // Если не включено или платформа не поддерживает — вернётся 0.
            FrameTimingManager.CaptureFrameTimings();
            if (FrameTimingManager.GetLatestTimings(1, _frameTimings) > 0)
            {
                var t = _frameTimings[0];
                if (t.cpuMainThreadFrameTime > 0.0)
                {
                    _cpuMainMs = t.cpuMainThreadFrameTime;
                    _cpuRenderMs = t.cpuRenderThreadFrameTime;
                    _gpuMs = t.gpuFrameTime;
                    _timingReady = true;
                }
            }
        }

        private static Font _defaultMonoFont;
        private static Font GetDefaultMonospaceFont()
        {
            if (_defaultMonoFont != null) return _defaultMonoFont;

            string[] candidates = { "Consolas", "Courier New", "Courier", "Menlo", "monospace" };
            foreach (string name in candidates)
            {
                _defaultMonoFont = Font.CreateDynamicFontFromOSFont(name, 16);
                if (_defaultMonoFont != null && _defaultMonoFont.dynamic)
                {
                    _defaultMonoFont.name = name;
                    break;
                }
            }
            return _defaultMonoFont;
        }

        private void OnGUI()
        {
            if (!ExecuteInEditor && !Application.isPlaying) return;
            if (!LogicEnabled) return;

            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            int mainFontSize = Mathf.RoundToInt(screenWidth * FontSize);
            int smallFontSize = Mathf.RoundToInt(mainFontSize * 0.6f);

            Font mono = MonoFont != null ? MonoFont : GetDefaultMonospaceFont();

            GUIStyle mainStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = mainFontSize,
                font = mono,
                normal = { textColor = Color.white }
            };

            GUIStyle smallStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = smallFontSize,
                font = mono,
                normal = { textColor = Color.white }
            };

            float x = PosX * screenWidth;
            float y = PosY * screenHeight;

            GUI.color = FontColor;

            // Мгновенный FPS (без накопления)
            float fps = _deltaTime > 0f ? 1f / _deltaTime : 0f;
            GUI.Label(new Rect(x, y, 300, 100), $"{fps:000}", mainStyle);

            if (ShowExtra)
            {
                y += mainFontSize + 5;

                int labelWidth = 4;
                if (_timingReady)
                {
                    GUI.Label(new Rect(x, y, 300, 50), $"{"CPU".PadRight(labelWidth)}: {_cpuMainMs:00.0} ms", smallStyle);
                    y += smallFontSize + 2;
                    GUI.Label(new Rect(x, y, 300, 50), $"{"REND".PadRight(labelWidth)}: {_cpuRenderMs:00.0} ms", smallStyle);
                    y += smallFontSize + 2;
                    GUI.Label(new Rect(x, y, 300, 50), $"{"GPU".PadRight(labelWidth)}: {_gpuMs:00.0} ms", smallStyle);
                }
                else
                {
                    GUI.Label(new Rect(x, y, 300, 50), $"{"CPU".PadRight(labelWidth)}: -- ms", smallStyle);
                    y += smallFontSize + 2;
                    GUI.Label(new Rect(x, y, 300, 50), $"{"REND".PadRight(labelWidth)}: -- ms", smallStyle);
                    y += smallFontSize + 2;
                    GUI.Label(new Rect(x, y, 300, 50), $"{"GPU".PadRight(labelWidth)}: -- ms", smallStyle);
                }
            }
        }
    }
}
