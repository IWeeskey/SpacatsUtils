using UnityEngine;
using UnityEngine.InputSystem;

namespace Spacats.Utils
{
    public class MouseMoverXZ : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private InputActionReference _mouseMoveAction;

        private void Awake()
        {
            // Fallback to global actions if reference is not assigned
            if (_mouseMoveAction == null && InputSystem.actions != null)
            {
                var action = InputSystem.actions.FindAction("Gameplay/MouseMove");
                if (action != null)
                    _mouseMoveAction = InputActionReference.Create(action);
            }
            
            _mouseMoveAction?.action?.Enable();
            RefreshPosition();
        }

        void Update()
        {
            RefreshPosition();
        }

        private void RefreshPosition()
        {
            if (_camera == null) _camera = Camera.main;

            Vector3 pos = transform.position;
            Vector2 mousePos2D = (_mouseMoveAction != null && _mouseMoveAction.action != null) ? _mouseMoveAction.action.ReadValue<Vector2>() : Vector2.zero;
            Vector3 mousePos = new Vector3(mousePos2D.x, mousePos2D.y, 0);
            mousePos.z = Mathf.Abs(_camera.transform.position.y - pos.y);

            Vector3 worldPos = _camera.ScreenToWorldPoint(mousePos);
            pos.x = worldPos.x;
            pos.z = worldPos.z;

            transform.position = pos;
        }
    }
}
