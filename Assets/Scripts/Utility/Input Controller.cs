using MyBox;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace NKStudio
{
    public class InputController : MonoBehaviour
    {
        [ReadOnly]
        public bool IsPressing;
        public UnityAction<bool> OnPressAction;

        [ReadOnly] 
        public Vector2 TouchPosition;
        public UnityAction<Vector2> OnTouchPositionAction;

        public void OnPress(InputAction.CallbackContext ctx)
        {
            IsPressing = ctx.ReadValueAsButton();
            OnPressAction?.Invoke(IsPressing);
        }

        public void OnTouchPosition(InputAction.CallbackContext ctx)
        {
            TouchPosition = ctx.ReadValue<Vector2>();
            OnTouchPositionAction?.Invoke(TouchPosition);
        }
    }
}
