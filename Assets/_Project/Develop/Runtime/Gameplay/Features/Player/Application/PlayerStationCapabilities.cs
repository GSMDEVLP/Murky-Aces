using System;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Application;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Player.Infrastructure.Movement;

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

        public bool TryDisableForStation(IInteractionScope interactionScope)
        {
            if (AreDisabledForStation ||
                interactionScope == null)
            {
                return false;
            }

            PlayerInteraction interaction =
                _interaction.Value;

            CacheState(interaction);

            if (interaction.TryRestrictTargetScope(
                    interactionScope) == false)
            {
                return false;
            }

            _movement.SetGameplayEnabled(false);

            _look.EnterCockpitMode();
            _look.SetGameplayEnabled(true);

            interaction.SetGameplayEnabled(true);

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