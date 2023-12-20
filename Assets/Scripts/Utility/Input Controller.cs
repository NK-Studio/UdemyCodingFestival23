using NaughtyAttributes;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NKStudio
{
    public class InputController : MonoBehaviour
    {
        [ReadOnly]
        public BoolReactiveProperty IsPressing = new();

        [ReadOnly]
        public Vector2ReactiveProperty TouchPosition = new();

        public void OnPress(InputAction.CallbackContext ctx)
        {
            IsPressing.Value = ctx.ReadValueAsButton();
        }

        public void OnTouchPosition(InputAction.CallbackContext ctx)
        {
            TouchPosition.Value = ctx.ReadValue<Vector2>();
        }
    }
}
