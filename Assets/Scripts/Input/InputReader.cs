using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace constellations
{
    [CreateAssetMenu(menuName = "InputReader")]
    public class InputReader : ScriptableObject, GameInputs.IGameplayActions, GameInputs.IUIActions
    {
        private GameInputs gameInputs;

        private void OnEnable()
        {
            if (gameInputs == null)
            {
                gameInputs = new GameInputs();

                gameInputs.Gameplay.SetCallbacks(this);
                gameInputs.UI.SetCallbacks(this);

                SetGameplay();
            }
        }

        public void SetGameplay()
        {
            gameInputs.Gameplay.Enable();
            gameInputs.UI.Disable();
        }

        public void SetUI()
        {
            gameInputs.Gameplay.Disable();
            gameInputs.UI.Enable();
        }

        //various actions that will be performed depending on received inputs
        //the actual implementation will be handled in PlayerController
        public event Action<Vector2> MoveEvent;
        public event Action JumpEvent;
        public event Action JumpCanceledEvent;
        public event Action DashEvent;
        public event Action DashCanceledEvent;
        public event Action CrouchEvent;
        public event Action CrouchCanceledEvent;

        public event Action PauseEvent;
        public event Action ResumeEvent;

        public void OnMovement(InputAction.CallbackContext context)
        {
            MoveEvent?.Invoke(obj:context.ReadValue<Vector2>());
            //Debug.Log(message: $"Phase: {context.phase}, Value: {context.ReadValue<Vector2>()}");
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                JumpEvent?.Invoke();
            }
            if (context.phase == InputActionPhase.Canceled)
            {
                JumpCanceledEvent?.Invoke();
            }
        }

        public void OnDash(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                DashEvent?.Invoke();
            }
            if (context.phase == InputActionPhase.Canceled)
            {
                DashCanceledEvent?.Invoke();
            }
        }
        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                CrouchEvent?.Invoke();
            }
            if (context.phase == InputActionPhase.Canceled)
            {
                CrouchCanceledEvent?.Invoke();
            }
        }

        public void OnPause(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                PauseEvent?.Invoke();
                SetUI();
            }
        }

        public void OnResume(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                ResumeEvent?.Invoke();
                SetGameplay();
            }
        }
    }
}

