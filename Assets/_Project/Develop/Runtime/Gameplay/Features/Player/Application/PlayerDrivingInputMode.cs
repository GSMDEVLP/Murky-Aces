using System;
using _Project.Develop.Runtime.Gameplay.Features.Player.Application.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Player.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Input.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Input.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Application
{
    public sealed class PlayerDrivingInputMode : IPlayerStationInputMode, IDrivingIntentSource
    {
        private readonly IPlayerInputContext _inputContext;
        private readonly DrivingIntentBuffer _drivingIntent;
        private readonly PlayerIntentBuffer _playerIntent;

        public StationControlContext Context => StationControlContext.Driving;
        public bool IsActive => _inputContext.IsUsingDrivingMap;

        public PlayerDrivingInputMode(IPlayerInputContext inputContext,
                DrivingIntentBuffer intent,
                PlayerIntentBuffer playerIntent)
        {
            _inputContext = inputContext ??
                throw new ArgumentNullException(
                    nameof(inputContext));

            _drivingIntent = intent ??
                throw new ArgumentNullException(
                    nameof(intent));
            _playerIntent = playerIntent ??
                throw new ArgumentNullException(nameof(playerIntent));
        }

        public bool TryActivate()
        {
            if (IsActive)
                return true;

            PrepareIntent();

            if (_inputContext.TryUseDrivingMap())
                return true;

            _drivingIntent.Clear();
            _playerIntent.SuppressInteractionUntilReleased();

            return false;
        }

        public bool TryDeactivate()
        {
            _drivingIntent.Clear();

            if (_inputContext.TryUsePlayerMap())
            {
                _playerIntent.SuppressInteractionUntilReleased();
                return true;
            }

            _drivingIntent.SuppressExitUntilReleased();

            return false;
        }

        public bool TryRestoreAfterFailedExit()
        {
            PrepareIntent();
            _playerIntent.SuppressInteractionUntilReleased();
            return _inputContext.TryUseDrivingMap();
        }

        public DrivingIntentSnapshot ReadDrivingIntent()
        {
            if (IsActive == false)
                return DrivingIntentSnapshot.Neutral;

            return new DrivingIntentSnapshot(
                _drivingIntent.Throttle,
                _drivingIntent.Steering,
                _drivingIntent.IsBraking);
        }

        public bool ConsumeExitRequest()
        {
            if (IsActive == false)
                return false;

            return _drivingIntent.ConsumeExitRequest();
        }

        private void PrepareIntent()
        {
            _drivingIntent.Clear();
            _drivingIntent.SuppressExitUntilReleased();
        }
    }
}