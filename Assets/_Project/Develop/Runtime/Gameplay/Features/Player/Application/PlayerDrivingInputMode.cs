using System;
using _Project.Develop.Runtime.Gameplay.Features.Player.Application.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Player.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Input.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Application
{
    public sealed class PlayerDrivingInputMode
    {
        private readonly IPlayerInputContext _inputContext;
        private readonly DrivingIntentBuffer _intent;

        public bool IsActive =>
            _inputContext.IsUsingDrivingMap;

        public PlayerDrivingInputMode(IPlayerInputContext inputContext, DrivingIntentBuffer intent)
        {
            _inputContext = inputContext ??
                throw new ArgumentNullException(
                    nameof(inputContext));

            _intent = intent ??
                throw new ArgumentNullException(
                    nameof(intent));
        }

        public bool TryActivate()
        {
            if (IsActive)
                return true;

            PrepareIntent();

            if (_inputContext.TryUseDrivingMap())
                return true;

            _intent.Clear();

            return false;
        }

        public bool TryDeactivate()
        {
            _intent.Clear();

            if (_inputContext.TryUsePlayerMap())
                return true;

            _intent.SuppressExitUntilReleased();

            return false;
        }

        public bool TryRestoreAfterFailedExit()
        {
            PrepareIntent();

            return _inputContext.TryUseDrivingMap();
        }

        public DrivingIntentSnapshot ReadDrivingIntent()
        {
            if (IsActive == false)
                return DrivingIntentSnapshot.Neutral;

            return new DrivingIntentSnapshot(
                _intent.Throttle,
                _intent.Steering,
                _intent.IsBraking);
        }

        public bool ConsumeExitRequest()
        {
            if (IsActive == false)
                return false;

            return _intent.ConsumeExitRequest();
        }

        private void PrepareIntent()
        {
            _intent.Clear();
            _intent.SuppressExitUntilReleased();
        }
    }
}