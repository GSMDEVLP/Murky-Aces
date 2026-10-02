using System;
using UnityEngine;
using System.Collections.Generic;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Player.Application.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Input.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Application
{
    public sealed class PlayerStationController : IStationOccupant, IStationIntentSource
    {
        private readonly IPlayerStationBody _body;
        private readonly PlayerStationCapabilities _capabilities;
        private readonly IReadOnlyList<IPlayerStationInputMode> _inputModes;

        private IPlayerStationInputMode _activeInputMode;
        private Func<bool> _releaseStation;
        private IInteractionScope _stationInteractionScope;
        private StationCapabilityProfile _stationCapabilityProfile;
        private Transform _seatAnchor;
        private Transform _stationCameraAnchor;

        public bool IsInStation
        {
            get;
            private set;
        }

        public PlayerStationController(IPlayerStationBody body, PlayerStationCapabilities capabilities, List<IPlayerStationInputMode> inputModes)
        {
            _body = body ??
                throw new ArgumentNullException(nameof(body));

            _capabilities = capabilities ??
                throw new ArgumentNullException(nameof(capabilities));

            _inputModes = inputModes ??
                throw new ArgumentNullException(nameof(inputModes));

            if (_inputModes.Count == 0)
            {
                throw new ArgumentException(
                    "At least one station input mode is required.",
                    nameof(inputModes));
            }
        }

        public bool TryEnterStation(Transform seatAnchor,Transform cameraAnchor,IInteractionScope interactionScope, StationCapabilityProfile capabilityProfile)
        {
            if (CanEnter(seatAnchor,cameraAnchor,interactionScope,capabilityProfile) == false)
            {
                return false;
            }

            if (TryPrepareStationMode(interactionScope, capabilityProfile) == false)
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
            _stationCapabilityProfile = capabilityProfile;
            IsInStation = true;

            return true;
        }

        public bool TryExitStation(Transform exitAnchor)
        {
            return TryExitStationInternal(exitAnchor, force: false);
        }

        public bool TryForceExitStation(Transform exitAnchor)
        {
            return TryExitStationInternal(exitAnchor, force: true);
        }

        private bool TryExitStationInternal(Transform exitAnchor, bool force)
        {
            if (_activeInputMode == null ||
                _stationCapabilityProfile == null ||
                _stationInteractionScope == null ||
                IsInStation == false ||
                exitAnchor == null ||
                _seatAnchor == null ||
                _stationCameraAnchor == null)
            {
                return false;
            }

            if (force == false && _body.CanDetach(exitAnchor) == false)
                return false;

            if (_activeInputMode.TryDeactivate() == false)
                return false;

            if (_capabilities.TryRestore() == false)
            {
                _activeInputMode.TryRestoreAfterFailedExit();
                return false;
            }

            bool detached = force
                ? _body.TryForceDetach(exitAnchor)
                : _body.TryDetach(exitAnchor);

            if (detached == false)
            {
                _capabilities.TryApplyForStation(
                    _stationCapabilityProfile,
                    _stationInteractionScope);

                _activeInputMode.TryRestoreAfterFailedExit();
                return false;
            }

            IsInStation = false;
            _seatAnchor = null;
            _stationCameraAnchor = null;
            _stationInteractionScope = null;
            _stationCapabilityProfile = null;
            _activeInputMode = null;

            return true;
        }
        public bool ConsumeExitRequest()
        {
            return IsInStation &&
                _activeInputMode != null &&
                _activeInputMode.ConsumeExitRequest();
        }

        private bool CanEnter(
            Transform seatAnchor,
            Transform cameraAnchor,
            IInteractionScope interactionScope,
            StationCapabilityProfile capabilityProfile)
        {
            return IsInStation == false &&
                _body.IsAttached == false &&
                seatAnchor != null &&
                cameraAnchor != null &&
                interactionScope != null &&
                capabilityProfile != null;
        }

        private bool TryPrepareStationMode(IInteractionScope interactionScope, StationCapabilityProfile capabilityProfile)
        {
            if (capabilityProfile == null)
                return false;

            IPlayerStationInputMode inputMode = FindInputMode(capabilityProfile.ControlContext);

            if (inputMode == null)
                return false;

            if (_capabilities.TryApplyForStation(
                    capabilityProfile,
                    interactionScope) == false)
            {
                return false;
            }

            if (inputMode.TryActivate() == false)
            {
                _capabilities.TryRestore();
                return false;
            }

            _activeInputMode = inputMode;

            return true;
        }

        private void RollbackStationMode()
        {
            if (_activeInputMode != null)
            {
                _activeInputMode.TryDeactivate();
                _activeInputMode = null;
            }

            _capabilities.TryRestore();
        }

        private IPlayerStationInputMode FindInputMode(StationControlContext context)
        {
            IPlayerStationInputMode result = null;

            for (int i = 0; i < _inputModes.Count; i++)
            {
                IPlayerStationInputMode candidate =
                    _inputModes[i];

                if (candidate == null ||
                    candidate.Context != context)
                {
                    continue;
                }

                if (result != null)
                {
                    throw new InvalidOperationException(
                        $"Multiple station input modes registered for {context}.");
                }

                result = candidate;
            }

            return result;
        }
        public bool TrySetReleaseHandler(Func<bool> releaseHandler)
        {
            if (releaseHandler == null || _releaseStation != null)
                return false;

            _releaseStation = releaseHandler;
            return true;
        }

        public void ClearReleaseHandler(Func<bool> releaseHandler)
        {
            if (_releaseStation == releaseHandler)
                _releaseStation = null;
        }

        public bool TryReleaseStation()
        {
            return _releaseStation != null
                ? _releaseStation()
                : IsInStation == false;
        }
    }
}