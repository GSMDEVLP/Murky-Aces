using System;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Input.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Input.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Input.Infrastructure.Readers;
using _Project.Develop.Runtime.Gameplay.Features.Player.Application;
using _Project.Develop.Runtime.Gameplay.Features.Player.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Input.Infrastructure.Updaters
{
    public sealed class WalkingInputUpdater : IInputIntentUpdater
    {
        private readonly PlayerActionsReader _reader;
        private readonly PlayerIntentBuffer _intent;
        private readonly PlayerInputPolicy _policy;

        public WalkingInputUpdater(
            PlayerActionsReader reader,
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

        public void UpdateIntent()
        {
            PlayerActionsSnapshot actions = _reader.ReadPlayerActions();

            if (_policy.IsInteractionMenuOpen)
                actions = PlayerActionsSnapshot.Neutral;

            _intent.SetMovement(
                new Vector2(actions.MoveX, actions.MoveY),
                actions.SprintHeld);

            if (actions.JumpHeld)
                _intent.RequestJump();
        }
    }
}
