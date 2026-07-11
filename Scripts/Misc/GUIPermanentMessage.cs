using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Spacats.Utils
{
    [DefaultExecutionOrder(-10)]
    public class GUIPermanentMessage : Controller
    {
        private static GUIPermanentMessage _instance;
        public static GUIPermanentMessage Instance
        {
            get
            {
                if (_instance == null) Debug.LogError("GUIPermanentMessage is not registered yet!");
                return _instance;
            }
        }

        public static bool HasInstance => _instance != null;

        [Header("Display Settings")]
        [Range(0f, 1f)] public float PosX = 0.01f;
        [Range(0f, 1f)] public float PosY = 0.01f;
        [Range(0f, 0.05f)] public float FontSize = 0.025f;
        public Color FontColor = Color.gray;
        public Font MonoFont;

        [Header("Logic Settings")]
        public bool LogicEnabled = true;

        [SerializeField] private string _mainMessage = "";
        [SerializeField] private string _cachedFull = "";
        [SerializeField] private List<string> _messageLines = new List<string>();

        // Cached GUIStyle
        private GUIStyle _mainStyle;
        private int _lastScreenW;
        private int _lastScreenH;
        private int _lastFontSize;

        // StringBuilder для сборки без промежуточных аллокаций
        private static readonly StringBuilder _sb = new StringBuilder(512);

        public string Message
        {
            get { return _mainMessage; }
            set { _mainMessage = value; }
        }

        protected override void COnRegister()
        {
            base.COnRegister();
            _instance = this;
        }

        private void OnGUI()
        {
            if (!ExecuteInEditor && !Application.isPlaying) return;
            if (!LogicEnabled) return;

            BuildFullString();

            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            int mainFontSize = Mathf.RoundToInt(screenWidth * FontSize);
            Font mono = MonoFont != null ? MonoFont : GetDefaultMonospaceFont();

            if (_mainStyle == null || _lastScreenW != screenWidth || _lastScreenH != screenHeight || _lastFontSize != mainFontSize)
            {
                _lastScreenW = (int)screenWidth;
                _lastScreenH = (int)screenHeight;
                _lastFontSize = mainFontSize;
                _mainStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = mainFontSize,
                    font = mono,
                    normal = { textColor = Color.white }
                };
            }

            float x = PosX * screenWidth;
            float y = PosY * screenHeight;
            float width = 2000f;
            float height = _mainStyle.CalcHeight(new GUIContent(_cachedFull), width);

            GUI.color = FontColor;
            GUI.Label(new Rect(x, y, width, height), _cachedFull, _mainStyle);
        }

        private void BuildFullString()
        {
            _sb.Clear();
            _sb.Append(_mainMessage);

            for (int i = 0; i < _messageLines.Count; i++)
            {
                _sb.Append('\n');
                _sb.Append(i);
                _sb.Append(": ");
                _sb.Append(_messageLines[i]);
            }

            _cachedFull = _sb.ToString();
        }

        public void SetMessageLine(string value, int index)
        {
            int startCount = _messageLines.Count;
            for (int i = startCount; i <= index; i++)
            {
                _messageLines.Add("");
            }
            _messageLines[index] = value ?? "";
        }

        public void ClearAll()
        {
            _mainMessage = "";
            _messageLines.Clear();
            _cachedFull = "";
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
    }
}
