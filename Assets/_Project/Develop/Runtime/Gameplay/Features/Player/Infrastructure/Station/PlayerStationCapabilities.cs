using System;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Application;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Player.Infrastructure.Movement;
using _Project.Develop.Runtime.Gameplay.Features.Player.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Infrastructure.Station
{
    public sealed class PlayerStationCapabilities : IPlayerStationCapabilities
    {
        private readonly PlayerMovement _movement;
        private readonly PlayerLook _look;
        private readonly InteractionControl _interactionControl;

        private bool _cachedMovementEnabled;
        private bool _cachedLookEnabled;
        private bool _cachedInteractionEnabled;

        private Quaternion _cachedWalkingView;
        private bool _hasCachedWalkingView;

        public bool AreDisabledForStation
        {
            get;
            private set;
        }

        public PlayerStationCapabilities(PlayerMovement movement, PlayerLook look, InteractionControl interactionControl)
        {
            _movement = movement ??
                throw new ArgumentNullException(
                    nameof(movement));

            _look = look ??
                throw new ArgumentNullException(
                    nameof(look));

            _interactionControl = interactionControl ??
                throw new ArgumentNullException(
                    nameof(interactionControl));
        }

        public bool TryApplyForStation(IStationCapabilityProfile profile, IInteractionScope interactionScope)
        {
            if (AreDisabledForStation || profile == null)
            {
                return false;
            }

            if (profile.UsesCockpitLook && profile.HasValidLookSettings == false)
            {
                return false;
            }

            if (profile.AllowsInteraction && interactionScope == null)
            {
                return false;
            }

            CacheState();

            if (profile.AllowsInteraction)
            {
                if (_interactionControl.TryRestrictTargetScope(interactionScope) == false)
                {
                    return false;
                }

                _interactionControl.SetGameplayEnabled(true);
            }
            else
            {
                _interactionControl.ClearTargetScope();
                _interactionControl.SetGameplayEnabled(false);
            }

            _movement.SetGameplayEnabled(
                profile.BlocksLocomotion == false);

            if (profile.UsesCockpitLook)
            {
                _look.EnterCockpitMode(profile.YawLimits, profile.PitchLimits, profile.InitialLookAngles);
                _look.SetGameplayEnabled(true);
            }
            else
            {
                _look.SetGameplayEnabled(false);
            }

            AreDisabledForStation = true;

            return true;
        }

        public bool TryRestore()
        {
            if (AreDisabledForStation == false)
                return false;

            _movement.SetGameplayEnabled(
                _cachedMovementEnabled);

            _look.EnterWalkingMode();
            _look.SetGameplayEnabled(
                _cachedLookEnabled);

            _interactionControl.ClearTargetScope();

            _interactionControl.SetGameplayEnabled(
                _cachedInteractionEnabled);

            AreDisabledForStation = false;

            return true;
        }

        private void CacheState()
        {
            _hasCachedWalkingView =
                _look.TryCaptureWalkingView(out _cachedWalkingView);
            _cachedMovementEnabled =
                _movement.IsGameplayEnabled;

            _cachedLookEnabled =
                _look.IsGameplayEnabled;

            _cachedInteractionEnabled =
                _interactionControl.IsGameplayEnabled;
        }

        public bool TryRestoreBeforeStation()
        {
            if (!TryRestore())
                return false;

            return !_hasCachedWalkingView ||
                _look.TryRestoreWalkingView(_cachedWalkingView);
        }
    }
}
