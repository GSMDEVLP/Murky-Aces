using System;
using _Project.Develop.Runtime.Core.GameLoop.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Input.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Input.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Movement;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Stations
{
    public sealed class DriverStationController : IGameplayTickable
    {
        private readonly DriverStation _station;
        private readonly DriverStationView _view;
        private readonly TankMovement _tankMovement;

        private IDrivingIntentSource _drivingIntentSource;

        private ulong? _currentOccupantId;
        private IStationOccupant _currentOccupant;

        public DriverStationController(DriverStation station, DriverStationView view, TankMovement tankMovement)
        {
            _station = station ??
                throw new ArgumentNullException(
                    nameof(station));

            _view = view ??
                throw new ArgumentNullException(
                    nameof(view));

            _tankMovement = tankMovement ??
                throw new ArgumentNullException(
                    nameof(tankMovement));
        }

        public bool CanBeginEnter(IInteractionActor actor)
        {
            if (actor == null ||
                _station.IsFree == false ||
                _view.CanEnter == false)
            {
                return false;
            }

            IStationOccupant occupant = actor as IStationOccupant;

            IDrivingIntentSource inputSource = actor as IDrivingIntentSource;

            return occupant != null &&
                   inputSource != null &&
                   occupant.IsInStation == false;
        }

        public bool CanContinueEnter(IInteractionActor actor)
        {
            if (actor == null ||
                _view.CanEnter == false ||
                _station.State !=
                DriverStationState.Entering ||
                _station.OccupantId != actor.Id)
            {
                return false;
            }

            IStationOccupant occupant = actor as IStationOccupant;

            IDrivingIntentSource inputSource = actor as IDrivingIntentSource;

            return occupant != null &&
                   inputSource != null &&
                   occupant.IsInStation == false;
        }

        public bool TryBeginEnter(IInteractionActor actor)
        {
            if (CanBeginEnter(actor) == false)
                return false;

            return _station.TryBeginEnter(actor.Id);
        }

        public bool TryCompleteEnter(IInteractionActor actor)
        {
            if (IsEnteringActor(actor) == false)
                return false;

            IStationOccupant occupant = actor as IStationOccupant;

            IDrivingIntentSource inputSource = actor as IDrivingIntentSource;

            if (occupant == null ||
                inputSource == null ||
                occupant.IsInStation ||
                occupant.TryEnterStation(
                    _view.DriverSeatAnchor,
                    _view.DriverCameraAnchor) == false)
            {
                _station.TryCancelEnter(actor.Id);
                ClearInput();
                return false;
            }

            if (_station.TryCompleteEnter(actor.Id))
            {
                _currentOccupantId = actor.Id;
                _currentOccupant = occupant;
                _drivingIntentSource = inputSource;

                return true;
            }

            RollbackPlayerEnter(occupant);
            _station.TryCancelEnter(actor.Id);
            ClearInput();

            return false;
        }

        public bool TryCancelEnter(IInteractionActor actor)
        {
            if (actor == null)
                return false;

            bool wasCancelled = _station.TryCancelEnter(actor.Id);

            if (wasCancelled)
                ClearInput();

            return wasCancelled;
        }

        public void Tick(float deltaTime)
        {
            if (HasActiveOccupant() == false)
            {
                ClearInput();
                return;
            }

            if (_drivingIntentSource
                .ConsumeExitRequest())
            {
                TryExitCurrentOccupant();
                return;
            }

            DrivingIntentSnapshot snapshot =
                _drivingIntentSource
                    .ReadDrivingIntent();

            TankDrivingInput input =
                new TankDrivingInput(
                    snapshot.Throttle,
                    snapshot.Steering,
                    snapshot.IsBraking);

            _tankMovement.SetInput(input);
        }

        private bool HasActiveOccupant()
        {
            return _currentOccupantId.HasValue &&
                _currentOccupant != null &&
                _drivingIntentSource != null &&
                _station.IsOccupiedBy(
                    _currentOccupantId.Value);
        }

        private bool TryExitCurrentOccupant()
        {
            if (HasActiveOccupant() == false ||
                _view.CanExit == false)
            {
                return false;
            }

            ulong occupantId = _currentOccupantId.Value;

            if (_station.TryBeginExit(occupantId) == false)
            {
                return false;
            }

            ClearInput();

            if (_currentOccupant.TryExitStation(_view.DriverExitAnchor) == false)
            {
                if (_station.TryCancelExit(occupantId) == false)
                {
                    throw new InvalidOperationException(
                        "DriverStation failed to rollback " +
                        "a rejected exit.");
                }

                return false;
            }

            if (_station.TryCompleteExit(occupantId) == false)
            {
                bool playerRolledBack = _currentOccupant.TryEnterStation(_view.DriverSeatAnchor, _view.DriverCameraAnchor);

                bool stationRolledBack = _station.TryCancelExit(occupantId);

                if (playerRolledBack == false || stationRolledBack == false)
                {
                    throw new InvalidOperationException("DriverStation exit rollback failed.");
                }
                return false;
            }

            ClearOccupant();

            return true;
        }

        private void ClearOccupant()
        {
            ClearInput();

            _currentOccupantId = null;
            _currentOccupant = null;
            _drivingIntentSource = null;
        }
        public void ClearInput()
        {
            _tankMovement.ClearInput();
        }

        private bool IsEnteringActor(IInteractionActor actor)
        {
            return actor != null &&
                   _station.State ==
                   DriverStationState.Entering &&
                   _station.OccupantId == actor.Id;
        }

        private void RollbackPlayerEnter(IStationOccupant occupant)
        {
            if (_view.DriverExitAnchor != null)
            {
                occupant.TryExitStation(_view.DriverExitAnchor);
            }
        }
    }
}