using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Spacats.Utils
{
    [DefaultExecutionOrder(-10)]
    public class GUIFps : Controller
    {
        [DllImport("psapi.dll", SetLastError = true)]
        private static extern bool GetProcessMemoryInfo(IntPtr hProcess, out PROCESS_MEMORY_COUNTERS counters, uint size);

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_MEMORY_COUNTERS
        {
            public uint cb;
            public uint PageFaultCount;
            public ulong PeakWorkingSetSize;
            public ulong WorkingSetSize;
            public ulong QuotaPeakPagedPoolUsage;
            public ulong QuotaPagedPoolUsage;
            public ulong QuotaPeakNonPagedPoolUsage;
            public ulong QuotaNonPagedPoolUsage;
            public ulong PagefileUsage;
            public ulong PeakPagefileUsage;
        }

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

        // Memory
        private Process _process;
        private long _memBytes;

        // Cached GUIStyles (чтобы не аллоцировать каждый OnGUI)
        private GUIStyle _mainStyle;
        private GUIStyle _smallStyle;
        private int _lastScreenW;
        private int _lastScreenH;

        private static readonly uint _memoryCountersSize = (uint)Marshal.SizeOf<PROCESS_MEMORY_COUNTERS>();

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

            // FrameTimingManager
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

            // Memory — цепочка fallback: P/Invoke → Process → Profiler
            if (!TryGetMemoryNative(out _memBytes))
            {
                try
                {
                    if (_process == null) _process = Process.GetCurrentProcess();
                    _memBytes = (long)_process.WorkingSet64;
                }
                catch
                {
                    _memBytes = (long)UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
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

        private static bool TryGetMemoryNative(out long bytes)
        {
            bytes = 0;
            try
            {
                if (GetProcessMemoryInfo(Process.GetCurrentProcess().Handle, out var counters, _memoryCountersSize))
                {
                    bytes = (long)counters.WorkingSetSize;
                    return bytes > 0;
                }
            }
            catch { }
            return false;
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

            if (_mainStyle == null || _lastScreenW != screenWidth || _lastScreenH != screenHeight)
            {
                _lastScreenW = (int)screenWidth;
                _lastScreenH = (int)screenHeight;
                _mainStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = mainFontSize,
                    font = mono,
                    normal = { textColor = Color.white }
                };
                _smallStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = smallFontSize,
                    font = mono,
                    normal = { textColor = Color.white }
                };
            }

            float x = PosX * screenWidth;
            float y = PosY * screenHeight;

            GUI.color = FontColor;

            float fps = _deltaTime > 0f ? 1f / _deltaTime : 0f;

            if (!ShowExtra)
            {
                // Только крупный FPS
                GUI.Label(new Rect(x, y, 300, 100), $"{fps:000}", _mainStyle);
            }
            else
            {
                // Полный список под ShowExtra

                // 1 — GPU device name
                GUI.Label(new Rect(x, y, screenWidth, 50), SystemInfo.graphicsDeviceName, _smallStyle);
                y += smallFontSize + 2;

                // 2 — CPU device name
                GUI.Label(new Rect(x, y, screenWidth, 50), SystemInfo.processorType, _smallStyle);
                y += smallFontSize + 2;

                // 3 — FPS
                GUI.Label(new Rect(x, y, 300, 50), $"FPS: {fps:000}", _smallStyle);
                y += smallFontSize + 2;

                // 4 — Memory
                double memMB = _memBytes / (1024.0 * 1024.0);
                GUI.Label(new Rect(x, y, 300, 50), $"MEM: {memMB.ToString("00.0", CultureInfo.CurrentCulture)} MB", _smallStyle);
                y += smallFontSize + 2;

                // 5-7 — CPU / GPU / CPU->GPU
                if (_timingReady)
                {
                    GUI.Label(new Rect(x, y, 300, 50), $"CPU: {_cpuMainMs:00.0} ms", _smallStyle);
                    y += smallFontSize + 2;
                    GUI.Label(new Rect(x, y, 300, 50), $"GPU: {_gpuMs:00.0} ms", _smallStyle);
                    y += smallFontSize + 2;
                    GUI.Label(new Rect(x, y, 300, 50), $"CPU->GPU: {_cpuRenderMs:00.0} ms", _smallStyle);
                }
                else
                {
                    GUI.Label(new Rect(x, y, 300, 50), "CPU: -- ms", _smallStyle);
                    y += smallFontSize + 2;
                    GUI.Label(new Rect(x, y, 300, 50), "GPU: -- ms", _smallStyle);
                    y += smallFontSize + 2;
                    GUI.Label(new Rect(x, y, 300, 50), "CPU->GPU: -- ms", _smallStyle);
                }
            }
        }
    }
}
