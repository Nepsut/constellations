using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UIElements;

namespace constellations
{
    [CreateAssetMenu(menuName = "InputReader")]
    public class InputReader : ScriptableObject, GameInputs.IGameplayActions, GameInputs.IUIActions, GameInputs.IDialogueActions
    {
        private GameInputs gameInputs;
        public bool gameplayActive {get; private set;} = true;
        public bool dialogueActive {get; private set;} = false;
        public bool UIActive {get; private set;} = false;

        private void OnEnable()
        {
            if (gameInputs == null)
            {
                gameInputs = new GameInputs();

                gameInputs.Gameplay.SetCallbacks(this);
                gameInputs.UI.SetCallbacks(this);
                gameInputs.Dialogue.SetCallbacks(this);

                SetGameplay();
            }
        }

        public void SetGameplay()
        {
            gameInputs.Gameplay.Enable();
            gameInputs.Dialogue.Disable();
            gameInputs.UI.Disable();
            gameplayActive = true;
            dialogueActive = false;
            UIActive = false;
        }

        public void SetUI()
        {
            gameInputs.Gameplay.Disable();
            gameInputs.Dialogue.Disable();
            gameInputs.UI.Enable();
            gameplayActive = false;
            dialogueActive = false;
            UIActive = true;
        }

        public void SetDialogue()
        {
            gameInputs.Dialogue.Enable();
            gameInputs.Gameplay.Disable();
            gameInputs.UI.Disable();
            gameplayActive = false;
            dialogueActive = true;
            UIActive = false;
        }

        //various actions that will be performed depending on received inputs
        //the actual implementation will be handled in PlayerController and PlayerAttack
        public event Action<Vector2> MoveEvent;
        public event Action JumpEvent;
        public event Action JumpCanceledEvent;
        public event Action DashEvent;
        public event Action DashCanceledEvent;
        public event Action CrouchEvent;
        public event Action CrouchCanceledEvent;
        public event Action AttackEvent;
        public event Action AttackCanceledEvent;
        public event Action ScreamEvent;
        public event Action ScreamCanceledEvent;
        public event Action InteractEvent;
        public event Action InteractCanceledEvent;
        public event Action SubmitEvent;
        public event Action SubmitCanceledEvent;
        public event Action<Vector2> MoveEventDialogue;
        public event Action<Vector2> PointEventDialogue;
        public event Action ClickEvent;
        public event Action ClickCanceledEvent;
        public event Action<Vector2> ScrollEvent;
        public event Action SelectEvent;

        public event Action PauseEvent;
        public event Action ResumeEvent;

        public void OnMovement(InputAction.CallbackContext context)
        {
            MoveEvent?.Invoke(obj:context.ReadValue<Vector2>());
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
            }
        }

        public void OnResume(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                ResumeEvent?.Invoke();
            }
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                AttackEvent?.Invoke();
            }
            if (context.phase == InputActionPhase.Canceled)
            {
                AttackCanceledEvent?.Invoke();
            }
        }

        public void OnScream(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                ScreamEvent?.Invoke();
            }
            if (context.phase == InputActionPhase.Canceled)
            {
                ScreamCanceledEvent?.Invoke();
            }
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                InteractEvent?.Invoke();
            }
            if (context.phase == InputActionPhase.Canceled)
            {
                InteractCanceledEvent?.Invoke();
            }
        }

        public void OnSubmit(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                SubmitEvent?.Invoke();
            }
            if (context.phase == InputActionPhase.Canceled)
            {
                SubmitCanceledEvent?.Invoke();
            }
        }


        public void OnMoveDialogue(InputAction.CallbackContext context)
        {
            MoveEventDialogue?.Invoke(obj: context.ReadValue<Vector2>());
        }

        public void OnPoint(InputAction.CallbackContext context)
        {
            PointEventDialogue?.Invoke(obj: context.ReadValue<Vector2>());
        }

        public void OnClick(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                ClickEvent?.Invoke();
            }
            if (context.phase == InputActionPhase.Canceled)
            {
                ClickCanceledEvent?.Invoke();
            }
        }

        public void OnScroll(InputAction.CallbackContext context)
        {
            ScrollEvent?.Invoke(obj: context.ReadValue<Vector2>());
        }

        public void OnSelect(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                SelectEvent?.Invoke();
            }
        }
    }
}

