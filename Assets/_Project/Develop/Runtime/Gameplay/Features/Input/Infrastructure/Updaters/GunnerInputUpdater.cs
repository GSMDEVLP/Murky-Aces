using System;
using _Project.Develop.Runtime.Gameplay.Features.Input.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Input.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Input.Infrastructure.Readers;
using _Project.Develop.Runtime.Gameplay.Features.Player.Application;
using _Project.Develop.Runtime.Gameplay.Features.Player.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Input.Infrastructure.Updaters
{
    public sealed class GunnerInputUpdater : IInputIntentUpdater
    {
        private readonly GunnerActionsReader _reader;
        private readonly GunnerIntentBuffer _intent;
        private readonly PlayerInputPolicy _policy;

        public GunnerInputUpdater(
            GunnerActionsReader reader,
            GunnerIntentBuffer intent,
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
            GunnerActionsSnapshot actions = _reader.ReadGunnerActions();

            if (_policy.IsInteractionMenuOpen)
                actions = GunnerActionsSnapshot.Neutral;

            _intent.SetActions(actions);
        }
    }
}
