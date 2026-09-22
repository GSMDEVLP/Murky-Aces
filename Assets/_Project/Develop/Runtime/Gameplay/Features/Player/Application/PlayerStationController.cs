using System;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Player.Application.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Input.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Input.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Application
{
    public sealed class PlayerStationController : IStationOccupant, IDrivingIntentSource
    {
        private readonly IPlayerStationBody _body;
        private readonly PlayerStationCapabilities _capabilities;
        private readonly IPlayerStationCamera _camera;
        private readonly PlayerDrivingInputMode _drivingInputMode;

        public bool IsInStation { get; private set; }

        public PlayerStationController(
            IPlayerStationBody body,
            PlayerStationCapabilities capabilities,
            IPlayerStationCamera camera,
            PlayerDrivingInputMode drivingInputMode)
        {
            _body = body ??
                throw new ArgumentNullException(
                    nameof(body));

            _capabilities = capabilities ??
                throw new ArgumentNullException(
                    nameof(capabilities));

            _camera = camera ??
                throw new ArgumentNullException(
                    nameof(camera));

            _drivingInputMode = drivingInputMode ??
                throw new ArgumentNullException(
                    nameof(drivingInputMode));
        }

        public bool TryEnterStation(Transform seatAnchor, Transform cameraAnchor)
        {
            if (CanEnter(seatAnchor, cameraAnchor) == false)
            {
                return false;
            }

            if (TryPrepareStationMode(
                    cameraAnchor) == false)
            {
                return false;
            }

            if (_body.TryAttach(seatAnchor) == false)
            {
                RollbackStationMode();
                return false;
            }

            IsInStation = true;

            return true;
        }

        public bool TryExitStation(Transform exitAnchor)
        {
            if (IsInStation == false ||
                exitAnchor == null)
            {
                return false;
            }

            if (_drivingInputMode.TryDeactivate() == false)
                return false;

            if (_body.TryDetach(exitAnchor) == false)
            {
                _drivingInputMode
                    .TryRestoreAfterFailedExit();

                return false;
            }

            if (_camera.TryRestoreWalkingAnchor() == false)
                return false;

            if (_capabilities.TryRestore() == false)
                return false;

            IsInStation = false;

            return true;
        }
        public DrivingIntentSnapshot ReadDrivingIntent()
        {
            if (IsInStation == false)
                return DrivingIntentSnapshot.Neutral;

            return _drivingInputMode.ReadDrivingIntent();
        }

        public bool ConsumeExitRequest()
        {
            return IsInStation &&
                _drivingInputMode.ConsumeExitRequest();
        }

        private bool CanEnter(Transform seatAnchor, Transform cameraAnchor)
        {
            return IsInStation == false &&
                   seatAnchor != null &&
                   _camera.CanUseAnchor(cameraAnchor);
        }

        private bool TryPrepareStationMode(Transform cameraAnchor)
        {
            if (_capabilities.TryDisableForStation() == false)
                return false;

            if (_camera.TryUseStationAnchor(
                    cameraAnchor) == false)
            {
                _capabilities.TryRestore();
                return false;
            }

            if (_drivingInputMode.TryActivate())
                return true;

            _camera.TryRestoreWalkingAnchor();
            _capabilities.TryRestore();

            return false;
        }

        private void RollbackStationMode()
        {
            _drivingInputMode.TryDeactivate();
            _camera.TryRestoreWalkingAnchor();
            _capabilities.TryRestore();
        }
    }
}