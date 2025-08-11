using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spacats.Utils
{
    [DefaultExecutionOrder(-10)]
    public class GUIPermanentMessage : Controller
    {
        [Header("Display Settings")]
        [Range(0f, 1f)] public float PosX = 0.01f;
        [Range(0f, 1f)] public float PosY = 0.01f;
        [Range(0f, 0.05f)] public float FontSize = 0.025f;
        public Color FontColor = Color.gray;

        [Header("Logic Settings")]
        public bool LogicEnabled = true;

        public string Message = "";

        private void OnGUI()
        {
            if (!ExecuteInEditor && !Application.isPlaying) return;

            if (!LogicEnabled) return;

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

            GUI.color = FontColor;

            GUI.Label(new Rect(x, y, 2000, 100), Message, mainStyle);
        }
    }
}
