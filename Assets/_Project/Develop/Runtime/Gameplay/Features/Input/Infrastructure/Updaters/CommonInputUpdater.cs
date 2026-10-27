using System;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Input.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Player.Application;
using _Project.Develop.Runtime.Gameplay.Features.Player.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Input.Infrastructure.Updaters
{
    public sealed class CommonInputUpdater
    {
        private readonly ActiveCommonActionsReader _reader;
        private readonly PlayerIntentBuffer _intent;
        private readonly PlayerInputPolicy _policy;

        public CommonInputUpdater(
            ActiveCommonActionsReader reader,
            PlayerIntentBuffer intent,
            PlayerInputPolicy policy)
        {
            _reader = reader ??
                throw new ArgumentNullException(nameof(reader));

            _intent = intent ??
                throw new ArgumentNullException(nameof(intent));

            _policy = policy ??
                throw new ArgumentNullException(nameof(policy));
        }

        public void UpdateIntent(
            float deltaTime,
            float mouseLookDegreesPerPixel,
            float stickLookDegreesPerSecond)
        {
            CommonInputSnapshot actions = _reader.ReadCommonActions();

            float lookScale = actions.LookIsPointerDelta
                ? mouseLookDegreesPerPixel
                : stickLookDegreesPerSecond * deltaTime;

            Vector2 lookDelta =
                new Vector2(actions.LookX, actions.LookY) * lookScale;

            _intent.SetLook(
                _policy.IsInteractionMenuOpen
                    ? Vector2.zero
                    : lookDelta);

            _intent.SetInteraction(
                actions.InteractPressedThisFrame,
                actions.InteractHeld,
                actions.InteractReleasedThisFrame);

            _intent.SetInteractionMenuInput(
                actions.InteractionMenuNavigation,
                actions.InteractionMenuCancelPressed);

            _intent.SetDrop(actions.DropPressedThisFrame);
        }
    }
}
