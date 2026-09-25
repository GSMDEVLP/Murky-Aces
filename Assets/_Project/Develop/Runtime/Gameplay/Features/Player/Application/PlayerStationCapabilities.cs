using System;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Application;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Player.Infrastructure.Movement;
using _Project.Develop.Runtime.Gameplay.Features.Player.Application.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Application
{
    public sealed class PlayerStationCapabilities
    {
        private readonly PlayerMovement _movement;
        private readonly PlayerLook _look;
        private readonly LazyInject<PlayerInteraction> _interaction;

        private bool _cachedMovementEnabled;
        private bool _cachedLookEnabled;
        private bool _cachedInteractionEnabled;

        public bool AreDisabledForStation
        {
            get;
            private set;
        }

        public PlayerStationCapabilities(
            PlayerMovement movement,
            PlayerLook look,
            LazyInject<PlayerInteraction> interaction)
        {
            _movement = movement ??
                throw new ArgumentNullException(
                    nameof(movement));

            _look = look ??
                throw new ArgumentNullException(
                    nameof(look));

            _interaction = interaction ??
                throw new ArgumentNullException(
                    nameof(interaction));
        }

        public bool TryApplyForStation(StationCapabilityProfile profile, IInteractionScope interactionScope)
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

            PlayerInteraction interaction = _interaction.Value;

            CacheState(interaction);

            if (profile.AllowsInteraction)
            {
                if (interaction.TryRestrictTargetScope(interactionScope) == false)
                {
                    return false;
                }

                interaction.SetGameplayEnabled(true);
            }
            else
            {
                interaction.ClearTargetScope();
                interaction.SetGameplayEnabled(false);
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

            PlayerInteraction interaction =
                _interaction.Value;

            interaction.ClearTargetScope();

            interaction.SetGameplayEnabled(
                _cachedInteractionEnabled);

            AreDisabledForStation = false;

            return true;
        }

        private void CacheState(
            PlayerInteraction interaction)
        {
            _cachedMovementEnabled =
                _movement.IsGameplayEnabled;

            _cachedLookEnabled =
                _look.IsGameplayEnabled;

            _cachedInteractionEnabled =
                interaction.IsGameplayEnabled;
        }
    }
}