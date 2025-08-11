using UnityEngine;
namespace Spacats.Utils
{
    public class GUIFps : Controller
    {
        [Header("Display Settings")]
        [Range(0f, 1f)] public float PosX = 0.01f;
        [Range(0f, 1f)] public float PosY = 0.01f;
        [Range(0f, 0.05f)] public float FontSize = 0.025f;
        public Color FontColor = Color.gray;

        [Header("Logic Settings")]
        public bool LogicEnabled = true;
        public bool ShowExtra = true;

        private float _deltaTime;

        private float _min10FPS = 9999f;
        private float _min10Timer = 0f;

        private float _sum10FPS = 0f;
        private int _count10FPS = 0;
        private float _avg10Timer = 0f;
        private float _avg10Result = 0f;

        private float _sum1FPS = 0f;
        private int _count1FPS = 0;
        private float _avg1Timer = 0f;
        private float _avg1Result = 0f;

        public override void ControllerSharedUpdate()
        {
            base.ControllerSharedUpdate();
            DoLogic();
        }

        private void DoLogic()
        {
            if (!ExecuteInEditor && !Application.isPlaying) return;
            if (!LogicEnabled) return;

            _deltaTime = Time.unscaledDeltaTime;
            if (_deltaTime <= 0f || float.IsNaN(_deltaTime)) return;

            float fps = 1f / _deltaTime;

            // --- Min 30s ---
            _min10Timer += Time.unscaledDeltaTime;
            
            if (fps < _min10FPS)
                _min10FPS = fps;

            if (_min10Timer >= 10f)
            {
                _min10Timer = 0f;
                _min10FPS = fps;
            }

            // --- Avg 10s ---
            _avg10Timer += Time.unscaledDeltaTime;
            _sum10FPS += fps;
            _count10FPS++;

            _avg10Result = _sum10FPS / _count10FPS;

            if (_avg10Timer >= 10f)
            {
                _avg10Timer = 0f;
                _sum10FPS = 0f;
                _count10FPS = 0;
            }


            // --- Avg 1s ---
            _avg1Timer += Time.unscaledDeltaTime;
            _sum1FPS += fps;
            _count1FPS++;

            _avg1Result = _sum1FPS / _count1FPS;

            if (_avg1Timer >= 1.1f)
            {
                _avg1Timer = 0f;
                _sum1FPS = 0f;
                _count1FPS = 0;
            }
        }

        private void OnGUI()
        {
            if (!ExecuteInEditor && !Application.isPlaying) return;

            if (!LogicEnabled) return;

            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            int mainFontSize = Mathf.RoundToInt(screenWidth * FontSize);
            int smallFontSize = Mathf.RoundToInt(mainFontSize * 0.6f);

            GUIStyle mainStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = mainFontSize,
                normal = { textColor = Color.white }
            };

            GUIStyle smallStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = smallFontSize,
                normal = { textColor = Color.white }
            };

            float x = PosX * screenWidth;
            float y = PosY * screenHeight;

            GUI.color = FontColor;

            GUI.Label(new Rect(x, y, 300, 100), $"{_avg1Result:0}", mainStyle);

            if (ShowExtra)
            {
                y += mainFontSize + 5;
                GUI.Label(new Rect(x, y, 300, 50), $"Min 10s: {_min10FPS:0}", smallStyle);

                y += smallFontSize + 2;
                GUI.Label(new Rect(x, y, 300, 50), $"Avg 10s: {_avg10Result:0}", smallStyle);
            }
        }
    }
}