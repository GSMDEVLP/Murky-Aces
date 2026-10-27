using System;
using _Project.Develop.Runtime.Gameplay.Features.Input.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Input.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Input.Infrastructure.Readers;
using _Project.Develop.Runtime.Gameplay.Features.Player.Application;
using _Project.Develop.Runtime.Gameplay.Features.Player.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Input.Infrastructure.Updaters
{
    public sealed class CommanderInputUpdater : IInputIntentUpdater
    {
        private readonly CommanderActionsReader _reader;
        private readonly CommanderIntentBuffer _intent;
        private readonly PlayerInputPolicy _policy;

        public CommanderInputUpdater(
            CommanderActionsReader reader,
            CommanderIntentBuffer intent,
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
            CommanderActionsSnapshot actions =
                _reader.ReadCommanderActions();

            if (_policy.IsInteractionMenuOpen)
                actions = CommanderActionsSnapshot.Neutral;

            _intent.SetExitInput(
                actions.ExitPressedThisFrame,
                actions.ExitHeld);
        }
    }
}
