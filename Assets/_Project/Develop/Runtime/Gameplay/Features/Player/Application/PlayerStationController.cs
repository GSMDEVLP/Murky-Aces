using System;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Player.Application.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Input.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Input.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Application
{
    public sealed class PlayerStationController : IStationOccupant, IDrivingIntentSource, IStationIntentSource
    {
        private readonly IPlayerStationBody _body;
        private readonly PlayerStationCapabilities _capabilities;
        private readonly PlayerDrivingInputMode _drivingInputMode;
        private IInteractionScope _stationInteractionScope;

        private Transform _seatAnchor;
        private Transform _stationCameraAnchor;

        public bool IsInStation
        {
            get;
            private set;
        }

        public PlayerStationController(
            IPlayerStationBody body,
            PlayerStationCapabilities capabilities,
            PlayerDrivingInputMode drivingInputMode)
        {
            _body = body ??
                throw new ArgumentNullException(
                    nameof(body));

            _capabilities = capabilities ??
                throw new ArgumentNullException(
                    nameof(capabilities));

            _drivingInputMode = drivingInputMode ??
                throw new ArgumentNullException(
                    nameof(drivingInputMode));
        }

        public bool TryEnterStation(Transform seatAnchor,Transform cameraAnchor,IInteractionScope interactionScope)
        {
            if (CanEnter(seatAnchor,cameraAnchor,interactionScope) == false)
            {
                return false;
            }

            if (TryPrepareStationMode(interactionScope) == false)
            {
                return false;
            }

            if (_body.TryAttach(seatAnchor) == false)
            {
                RollbackStationMode();
                return false;
            }

            _seatAnchor = seatAnchor;
            _stationCameraAnchor = cameraAnchor;
            _stationInteractionScope = interactionScope;
            IsInStation = true;

            return true;
        }

        public bool TryExitStation(Transform exitAnchor)
        {
            if (_stationInteractionScope == null || 
                IsInStation == false ||
                exitAnchor == null ||
                _seatAnchor == null ||
                _stationCameraAnchor == null)
            {
                return false;
            }

            if (_body.CanDetach(exitAnchor) == false)
                return false;

            if (_drivingInputMode.TryDeactivate() == false)
                return false;

            if (_capabilities.TryRestore() == false)
            {
                _drivingInputMode
                    .TryRestoreAfterFailedExit();

                return false;
            }

            if (_body.TryDetach(exitAnchor) == false)
            {
                _capabilities.TryDisableForStation(_stationInteractionScope);

                _drivingInputMode.TryRestoreAfterFailedExit();

                return false;
            }

            IsInStation = false;
            _seatAnchor = null;
            _stationCameraAnchor = null;
            _stationInteractionScope = null;

            return true;
        }

        public DrivingIntentSnapshot ReadDrivingIntent()
        {
            if (IsInStation == false)
                return DrivingIntentSnapshot.Neutral;

            return _drivingInputMode
                .ReadDrivingIntent();
        }

        public bool ConsumeExitRequest()
        {
            return IsInStation &&
                   _drivingInputMode
                       .ConsumeExitRequest();
        }

        private bool CanEnter(
            Transform seatAnchor,
            Transform cameraAnchor,
            IInteractionScope interactionScope)
        {
            return IsInStation == false &&
                _body.IsAttached == false &&
                seatAnchor != null &&
                cameraAnchor != null &&
                interactionScope != null;
        }

        private bool TryPrepareStationMode(
            IInteractionScope interactionScope)
        {
            if (_capabilities.TryDisableForStation(
                    interactionScope) == false)
            {
                return false;
            }

            if (_drivingInputMode.TryActivate())
                return true;

            _capabilities.TryRestore();

            return false;
        }

        private void RollbackStationMode()
        {
            _drivingInputMode.TryDeactivate();
            _capabilities.TryRestore();
        }
    }
}