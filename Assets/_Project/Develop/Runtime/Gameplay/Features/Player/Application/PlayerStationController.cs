using System;
using System.Collections.Generic;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Crew.Application;
using _Project.Develop.Runtime.Gameplay.Features.Crew.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Input.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Player.Application.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Stations;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Application
{
    public sealed class PlayerStationController :
        IStationOccupant,
        IStationIntentSource
    {
        private readonly IPlayerStationBody _body;
        private readonly PlayerStationCapabilities _capabilities;
        private readonly IReadOnlyList<IPlayerStationInputMode> _inputModes;
        private readonly CrewTransitionCoordinator _transitions;

        private IPlayerStationInputMode _activeInputMode;
        private Func<bool> _releaseStation;

        private IInteractionScope _stationInteractionScope;
        private StationCapabilityProfile _stationCapabilityProfile;
        private Transform _seatAnchor;

        private CrewLocation _returnLocation;
        private CrewRoleId _stationRole;

        public bool IsInStation =>
            _transitions.Current.Kind == CrewLocationKind.Station;

        public PlayerStationController(
            IPlayerStationBody body,
            PlayerStationCapabilities capabilities,
            List<IPlayerStationInputMode> inputModes,
            CrewTransitionCoordinator transitions)
        {
            _body = body ??
                throw new ArgumentNullException(nameof(body));

            _capabilities = capabilities ??
                throw new ArgumentNullException(nameof(capabilities));

            _inputModes = inputModes ??
                throw new ArgumentNullException(nameof(inputModes));

            _transitions = transitions ??
                throw new ArgumentNullException(nameof(transitions));

            if (_inputModes.Count == 0)
            {
                throw new ArgumentException(
                    "At least one station input mode is required.",
                    nameof(inputModes));
            }
        }

        public bool TryEnterStation(
            long expectedRevision,
            CrewRoleId roleId,
            Transform seatAnchor,
            Transform cameraAnchor,
            IInteractionScope interactionScope,
            StationCapabilityProfile capabilityProfile)
        {
            CrewLocation source = _transitions.Current;

            if (expectedRevision != _transitions.Revision ||
                (source.Kind != CrewLocationKind.Outside &&
                 source.Kind != CrewLocationKind.Interior) ||
                _body.IsAttached ||
                _activeInputMode != null ||
                _capabilities.AreDisabledForStation ||
                seatAnchor == null ||
                cameraAnchor == null ||
                interactionScope == null ||
                capabilityProfile == null ||
                !Enum.IsDefined(typeof(CrewRoleId), roleId))
            {
                return false;
            }

            IPlayerStationInputMode mode =
                FindInputMode(capabilityProfile.ControlContext);

            if (mode == null)
                return false;

            return _transitions.TryTransition(
                expectedRevision,
                CrewLocation.AtStation(roleId),
                () =>
                {
                    _seatAnchor = seatAnchor;
                    _stationInteractionScope = interactionScope;
                    _stationCapabilityProfile = capabilityProfile;
                    _activeInputMode = mode;
                    _returnLocation = source;
                    _stationRole = roleId;

                    if (!_capabilities.TryApplyForStation(
                            capabilityProfile,
                            interactionScope))
                    {
                        return false;
                    }

                    if (!mode.TryActivate())
                        return false;

                    return _body.TryAttach(seatAnchor);
                },
                () =>
                {
                    bool restored = true;

                    if (_body.IsAttached)
                    {
                        restored =
                            _body.TryRestoreBeforeAttach() && restored;
                    }

                    restored = mode.TryDeactivate() && restored;

                    if (_capabilities.AreDisabledForStation)
                    {
                        restored =
                            _capabilities.TryRestoreBeforeStation() &&
                            restored;
                    }

                    if (restored)
                        ClearStationCache();

                    return restored;
                });
        }

        public bool TryExitStation(Transform exitAnchor)
        {
            return TryLeaveStation(exitAnchor, false, false);
        }

        public bool TryForceExitStation(Transform exitAnchor)
        {
            return TryLeaveStation(exitAnchor, true, false);
        }

        public bool TryRollbackStationEntry()
        {
            return TryLeaveStation(null, true, true);
        }

        private bool TryLeaveStation(
            Transform exitAnchor,
            bool force,
            bool restoreBeforeAttach)
        {
            if (!IsInStation ||
                _transitions.Current.StationRole != _stationRole ||
                !_body.IsAttached ||
                _activeInputMode == null ||
                _stationCapabilityProfile == null ||
                _stationInteractionScope == null ||
                _seatAnchor == null ||
                _returnLocation == null)
            {
                return false;
            }

            if (!restoreBeforeAttach &&
                (exitAnchor == null ||
                 (!force && !_body.CanDetach(exitAnchor))))
            {
                return false;
            }

            IPlayerStationInputMode mode = _activeInputMode;

            bool completed = _transitions.TryTransition(
                _transitions.Revision,
                _returnLocation,
                () =>
                {
                    if (!mode.TryDeactivate())
                        return false;

                    bool capabilitiesRestored = restoreBeforeAttach
                        ? _capabilities.TryRestoreBeforeStation()
                        : _capabilities.TryRestore();

                    if (!capabilitiesRestored)
                        return false;

                    if (restoreBeforeAttach)
                        return _body.TryRestoreBeforeAttach();

                    return force
                        ? _body.TryForceDetach(exitAnchor)
                        : _body.TryDetach(exitAnchor);
                },
                () =>
                {
                    bool restored = true;

                    if (!_body.IsAttached)
                    {
                        restored =
                            _body.TryAttach(_seatAnchor) && restored;
                    }

                    if (!_capabilities.AreDisabledForStation)
                    {
                        restored =
                            _capabilities.TryApplyForStation(
                                _stationCapabilityProfile,
                                _stationInteractionScope) &&
                            restored;
                    }

                    restored =
                        mode.TryRestoreAfterFailedExit() && restored;

                    return restored;
                });

            if (completed)
                ClearStationCache();

            return completed;
        }

        private void ClearStationCache()
        {
            _seatAnchor = null;
            _stationInteractionScope = null;
            _stationCapabilityProfile = null;
            _activeInputMode = null;
            _returnLocation = null;
        }

        public bool ConsumeExitRequest()
        {
            return IsInStation &&
                   _activeInputMode != null &&
                   _activeInputMode.ConsumeExitRequest();
        }

        private IPlayerStationInputMode FindInputMode(
            StationControlContext context)
        {
            IPlayerStationInputMode result = null;

            for (int i = 0; i < _inputModes.Count; i++)
            {
                IPlayerStationInputMode candidate = _inputModes[i];

                if (candidate == null || candidate.Context != context)
                    continue;

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
                : !IsInStation;
        }
    }
}