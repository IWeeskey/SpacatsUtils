using System.Collections;
using System.Collections.Generic;
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

        [Header("Display Settings")]
        [Range(0f, 1f)] public float PosX = 0.01f;
        [Range(0f, 1f)] public float PosY = 0.01f;
        [Range(0f, 0.05f)] public float FontSize = 0.025f;
        public Color FontColor = Color.gray;

        [Header("Logic Settings")]
        public bool LogicEnabled = true;

        [SerializeField]private string _mainMessage = "";
        [SerializeField]private string _fullString = "";
        [SerializeField]private List<string> _messageLines = new List<string>(); 
        
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
            
            FormFullString();
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            int mainFontSize = Mathf.RoundToInt(screenWidth * FontSize);

            GUIStyle mainStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = mainFontSize,
                normal = { textColor = Color.white }
            };

            float x = PosX * screenWidth;
            float y = PosY * screenHeight;
            float width = 2000f;
            float height = mainStyle.CalcHeight(new GUIContent(_fullString), width);
            
            GUI.color = FontColor;

            
            GUI.Label(new Rect(x, y, width, height), _fullString, mainStyle);
        }
        
        private void FormFullString()
        {
            _fullString = _mainMessage;

            for (int i = 0; i < _messageLines.Count; i++)
            {
                string value = _messageLines[i];
                _fullString +="\n" + i + ":" + value;
            }
        }

        public void SetMessageLine(string value, int index)
        {
            int startCount = _messageLines.Count;
            for (int i = startCount; i <= index; i++)
            {
                _messageLines.Add("");
            }
            _messageLines[index] = value;
        }
    }
}
