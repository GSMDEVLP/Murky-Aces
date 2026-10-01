using System;
using _Project.Develop.Runtime.Gameplay.Features.Input.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Input.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Player.Application.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Player.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Application
{
    public sealed class PlayerGunnerInputMode : IPlayerStationInputMode, IGunnerIntentSource
    {
        private readonly IPlayerInputContext _inputContext;
        private readonly GunnerIntentBuffer _gunnerIntent;
        private readonly PlayerIntentBuffer _playerIntent;

        public StationControlContext Context =>
            StationControlContext.Gunner;

        public bool IsActive => _inputContext.IsUsingGunnerMap;

        public PlayerGunnerInputMode(
            IPlayerInputContext inputContext,
            GunnerIntentBuffer gunnerIntent,
            PlayerIntentBuffer playerIntent)
        {
            _inputContext = inputContext ??
                throw new ArgumentNullException(nameof(inputContext));
            _gunnerIntent = gunnerIntent ??
                throw new ArgumentNullException(nameof(gunnerIntent));
            _playerIntent = playerIntent ??
                throw new ArgumentNullException(nameof(playerIntent));
        }

        public bool TryActivate()
        {
            if (IsActive)
                return true;

            PrepareIntent();

            if (!_inputContext.TryUseGunnerMap())
            {
                _gunnerIntent.Clear();
                return false;
            }

            _playerIntent.SuppressInteractionUntilReleased();
            return true;
        }

        public bool TryDeactivate()
        {
            _gunnerIntent.Clear();

            if (_inputContext.TryUsePlayerMap())
            {
                _playerIntent.SuppressInteractionUntilReleased();
                return true;
            }

            _gunnerIntent.SuppressButtonsUntilReleased();
            return false;
        }

        public bool TryRestoreAfterFailedExit()
        {
            PrepareIntent();

            if (!_inputContext.TryUseGunnerMap())
                return false;

            _playerIntent.SuppressInteractionUntilReleased();
            return true;
        }

        public GunnerIntentSnapshot ConsumeGunnerIntent()
        {
            return IsActive
                ? _gunnerIntent.ConsumeIntent()
                : GunnerIntentSnapshot.Neutral;
        }

        public bool ConsumeExitRequest()
        {
            return IsActive &&
                _gunnerIntent.ConsumeExitRequest();
        }

        private void PrepareIntent()
        {
            _gunnerIntent.Clear();
            _gunnerIntent.SuppressButtonsUntilReleased();
        }
    }
}