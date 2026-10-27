using System;
using _Project.Develop.Runtime.Gameplay.Features.Input.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Input.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Input.Infrastructure.Readers;
using _Project.Develop.Runtime.Gameplay.Features.Player.Application;
using _Project.Develop.Runtime.Gameplay.Features.Player.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Input.Infrastructure.Updaters
{
    public sealed class DrivingInputUpdater : IInputIntentUpdater
    {
        private readonly DrivingActionsReader _reader;
        private readonly DrivingIntentBuffer _intent;
        private readonly PlayerInputPolicy _policy;

        public DrivingInputUpdater(
            DrivingActionsReader reader,
            DrivingIntentBuffer intent,
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
            DrivingActionsSnapshot actions = _reader.ReadDrivingActions();

            if (_policy.IsInteractionMenuOpen)
                actions = DrivingActionsSnapshot.Neutral;

            _intent.SetDriving(
                actions.Throttle,
                actions.Steering,
                actions.BrakeHeld);
        }
    }
}
