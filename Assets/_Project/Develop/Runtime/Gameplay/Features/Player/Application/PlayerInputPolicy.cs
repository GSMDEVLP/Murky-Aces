using System;
using _Project.Develop.Runtime.Gameplay.Features.Player.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Application
{
    public sealed class PlayerInputPolicy
    {
        private readonly PlayerIntentBuffer _intent;

        public bool IsInteractionMenuOpen { get; private set; }

        public PlayerInputPolicy(PlayerIntentBuffer intent)
        {
            _intent = intent ??
                throw new ArgumentNullException(nameof(intent));
        }

        public void SetInteractionMenuOpen(bool isOpen)
        {
            IsInteractionMenuOpen = isOpen;

            if (isOpen == false)
                return;

            _intent.ClearMovement();
            _intent.ClearLook();
        }
    }
}
