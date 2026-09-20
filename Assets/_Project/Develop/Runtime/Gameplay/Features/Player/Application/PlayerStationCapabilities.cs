using System;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Application;
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

        public bool AreDisabledForStation { get; private set; }

        public PlayerStationCapabilities(PlayerMovement movement, PlayerLook look, LazyInject<PlayerInteraction> interaction)
        {
            _movement = movement
                ?? throw new ArgumentNullException(nameof(movement));

            _look = look
                ?? throw new ArgumentNullException(nameof(look));

            _interaction = interaction
                ?? throw new ArgumentNullException(nameof(interaction));
        }

        public bool TryDisableForStation()
        {
            if (AreDisabledForStation)
                return false;

            PlayerInteraction interaction =
                _interaction.Value;

            CacheState(interaction);

            interaction.SetGameplayEnabled(false);
            _movement.SetGameplayEnabled(false);
            _look.SetGameplayEnabled(false);

            AreDisabledForStation = true;

            return true;
        }

        public bool TryRestore()
        {
            if (!AreDisabledForStation)
                return false;

            _movement.SetGameplayEnabled(
                _cachedMovementEnabled);

            _look.SetGameplayEnabled(
                _cachedLookEnabled);

            _interaction.Value.SetGameplayEnabled(
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