using UnityEngine;

namespace Spacats.Utils
{
    [ExecuteInEditMode]
    public class GUIButtons : MonoBehaviour
    {
        [Header("GUIButtons settings")]
        public bool PerformLogic = true;
        public bool ExecuteInEditor = true;
        
        [Range(1, 20)]
        [SerializeField]
        private int _buttonCount = 3;

        [Range(0f, 1f)]
        [SerializeField]
        private float _buttonHeightPercent = 0.08f;
        
        [Range(0f, 1f)]
        [SerializeField]
        private float _buttonBottomPercent = 1f;

        [Range(1, 10)]
        [SerializeField]
        private float _buttonSpacing = 5f;

        [Range(0f, 1f)]
        [SerializeField]
        private float _fontScale = 0.4f;

        public int ButtonCount
        {
            get => _buttonCount;
            set => _buttonCount = Mathf.Max(1, 20);
        }

        private void OnGUI()
        {
            if (!PerformLogic) return;
            if (!Application.isPlaying && !ExecuteInEditor) return;
            
            if (_buttonCount <= 0) return;

            float buttonHeight = Screen.height * _buttonHeightPercent;
            float totalSpacing = _buttonSpacing * (_buttonCount + 1);
            float buttonWidth = (Screen.width - totalSpacing) / _buttonCount;
            float y = Screen.height*_buttonBottomPercent - buttonHeight - _buttonSpacing;

            GUI.skin.button.fontSize = Mathf.RoundToInt(buttonHeight * _fontScale);

            for (int i = 0; i < _buttonCount; i++)
            {
                float x = _buttonSpacing + i * (buttonWidth + _buttonSpacing);
                if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), GetButtonLabel(i)))
                {
                    OnButtonClick(i);
                }
            }
        }

        protected virtual void OnButtonClick(int index)
        {
            Debug.Log($"Button {index + 1} pressed");
        }

        protected virtual string GetButtonLabel(int index)
        {
            return $"Button {index + 1}";
        }
    }
}
