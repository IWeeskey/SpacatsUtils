#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Spacats.Utils
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public static class TimeTracker
    {
        private class TimerData
        {
            public Stopwatch Stopwatch;
            public string Tag;
        }

        private static readonly Dictionary<string, TimerData> _activeTimers = new();

        static TimeTracker()
        {
            ClearAll();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ClearOnRuntimeLoad()
        {
            ClearAll();
        }

        public static void Start(string tag)
        {
            if (_activeTimers.ContainsKey(tag))
            {
                _activeTimers[tag].Stopwatch.Restart();
            }
            else
            {
                _activeTimers[tag] = new TimerData
                {
                    Stopwatch = Stopwatch.StartNew(),
                    Tag = tag
                };
            }
        }

        public static (double,string) Finish(string tag, bool showLogs = true)
        {
            if (!_activeTimers.TryGetValue(tag, out var data))
            {
                UnityEngine.Debug.LogWarning($"[TimeTracker] No timer with tag '{tag}' found.");
                return (0f,"0");
            }

            data.Stopwatch.Stop();

            double ms = data.Stopwatch.Elapsed.TotalMilliseconds;
            //double micros = data.Stopwatch.ElapsedTicks / (Stopwatch.Frequency / 1_000_000.0);
            double fps = 1000.0 / ms;

            string result = $"[TimeTracker] '{tag}': {ms:F3} ms Potential FPS: {fps:F1}";
            if (showLogs) UnityEngine.Debug.Log(result);

            _activeTimers.Remove(tag);
            return (ms, result);
        }

        private static void ClearAll()
        {
            _activeTimers.Clear();
        }
    }
}
