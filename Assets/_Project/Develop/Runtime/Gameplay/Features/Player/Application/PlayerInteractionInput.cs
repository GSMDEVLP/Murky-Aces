using System;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Player.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Application
{
    public sealed class PlayerInteractionInput : IInteractionInput
    {
        private readonly PlayerIntentBuffer _intent;
        private readonly PlayerInputPolicy _policy;

        public long ContextRevision => _intent.ContextRevision;

        public bool InteractPressedThisFrame =>
            _intent.InteractPressedThisFrame;

        public bool InteractHeld => _intent.InteractHeld;

        public bool InteractReleasedThisFrame =>
            _intent.InteractReleasedThisFrame;

        public bool DropPressedThisFrame =>
            _intent.DropPressedThisFrame;

        public int InteractionMenuNavigation =>
            _intent.InteractionMenuNavigation;

        public bool InteractionMenuCancelPressed =>
            _intent.InteractionMenuCancelPressed;

        public PlayerInteractionInput(
            PlayerIntentBuffer intent,
            PlayerInputPolicy policy)
        {
            _intent = intent ??
                throw new ArgumentNullException(nameof(intent));

            _policy = policy ??
                throw new ArgumentNullException(nameof(policy));
        }

        public void SetInteractionMenuOpen(bool isOpen)
        {
            _policy.SetInteractionMenuOpen(isOpen);
        }

        public void SuppressInteractionUntilReleased()
        {
            _intent.SuppressInteractionUntilReleased();
        }
    }
}
