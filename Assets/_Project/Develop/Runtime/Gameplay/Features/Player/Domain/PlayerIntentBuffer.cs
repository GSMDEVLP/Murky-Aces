using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Domain
{
    public sealed class PlayerIntentBuffer : IInteractionInput
    {
        public Vector2 Move { get; private set; }
        public Vector2 LookDelta { get; private set; }
        public bool SprintHeld { get; private set; }
        public bool InteractPressedThisFrame { get; private set; }
        public bool InteractHeld { get; private set; }
        public bool InteractReleasedThisFrame { get; private set; }
        public bool DropPressedThisFrame { get; private set; }

        public long ContextRevision { get; private set; }
        public int InteractionMenuNavigation { get; private set; }
        public bool InteractionMenuCancelPressed { get; private set; }
        public bool IsInteractionMenuOpen { get; private set; }

        private bool _jumpPending;
        private bool _interactionSuppressed;

        public void SetMovement(Vector2 move, bool sprintHeld)
        {
            Move = IsInteractionMenuOpen ? Vector2.zero : Vector2.ClampMagnitude(move, 1f);
            SprintHeld = !IsInteractionMenuOpen && sprintHeld;
        }

        public void SetLook(Vector2 lookDelta)
        {
            LookDelta = IsInteractionMenuOpen ? Vector2.zero : lookDelta;
        }
        public void ClearLook()
        {
            LookDelta = Vector2.zero;
        }

        public void SetInteraction(bool pressedThisFrame, bool held, bool releasedThisFrame)
        {
            if (_interactionSuppressed)
            {
                InteractPressedThisFrame = false;
                InteractHeld = false;
                InteractReleasedThisFrame = false;

                if (!held)
                    _interactionSuppressed = false;

                return;
            }

            InteractPressedThisFrame = pressedThisFrame;
            InteractHeld = held;
            InteractReleasedThisFrame = releasedThisFrame;
        }

        public void SuppressInteractionUntilReleased()
        {
            ContextRevision++;
            _interactionSuppressed = true;
            InteractPressedThisFrame = false;
            InteractHeld = false;
            InteractReleasedThisFrame = false;
        }

        public void SetDrop(bool pressedThisFrame)
        {
            DropPressedThisFrame = pressedThisFrame;
        }
        public void SetInteractionMenuInput(int navigation, bool cancelPressed)
        {
            InteractionMenuNavigation = navigation;
            InteractionMenuCancelPressed = cancelPressed;
        }

        public void SetInteractionMenuOpen(bool isOpen)
        {
            IsInteractionMenuOpen = isOpen;
            if (!isOpen) return;
            Move = Vector2.zero;
            LookDelta = Vector2.zero;
            SprintHeld = false;
            _jumpPending = false;
        }

        public void RequestJump()
        {
            if (!IsInteractionMenuOpen) _jumpPending = true;
        }

        public bool ConsumeJump()
        {
            bool requested = _jumpPending;
            _jumpPending = false;
            return requested;
        }
    }
}
