using System;
using _Project.Develop.Runtime.Gameplay.Features.Input.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Input.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Movement;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Stations
{
    public sealed class DriverStationAdapter :
        ICrewStationRoleAdapter
    {
        private readonly TankMovement _tankMovement;

        private IDrivingIntentSource _intentSource;

        public bool IsActive => _intentSource != null;

        public DriverStationAdapter(TankMovement tankMovement)
        {
            _tankMovement = tankMovement ??
                throw new ArgumentNullException(
                    nameof(tankMovement));
        }

        public bool CanUse(IInteractionActor actor)
        {
            return actor is IDrivingIntentSource;
        }

        public bool TryActivate(IInteractionActor actor)
        {
            if (IsActive)
                return false;

            IDrivingIntentSource intentSource =
                actor as IDrivingIntentSource;

            if (intentSource == null)
                return false;

            _intentSource = intentSource;
            ClearOutput();

            return true;
        }

        public void Tick(float deltaTime)
        {
            if (_intentSource == null)
            {
                ClearOutput();
                return;
            }

            DrivingIntentSnapshot snapshot = _intentSource.ReadDrivingIntent();

            TankDrivingInput input =
                new TankDrivingInput(
                    snapshot.Throttle,
                    snapshot.Steering,
                    snapshot.IsBraking);

            _tankMovement.SetInput(input);
        }

        public void ClearOutput()
        {
            _tankMovement.ClearInput();
        }

        public void Deactivate()
        {
            ClearOutput();
            _intentSource = null;
        }
    }
}