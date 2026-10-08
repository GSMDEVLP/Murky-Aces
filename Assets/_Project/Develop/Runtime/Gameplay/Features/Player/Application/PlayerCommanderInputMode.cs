using System;
using _Project.Develop.Runtime.Gameplay.Features.Input.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Player.Application.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Player.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Application
{
    public sealed class PlayerCommanderInputMode : IPlayerStationInputMode
    {
        private readonly IPlayerInputContext _inputContext;
        private readonly CommanderIntentBuffer _commanderIntent;
        private readonly PlayerIntentBuffer _playerIntent;

        public StationControlContext Context =>
            StationControlContext.Commander;

        public bool IsActive =>
            _inputContext.CurrentMap == InputMapId.Commander;

        public PlayerCommanderInputMode(
            IPlayerInputContext inputContext,
            CommanderIntentBuffer commanderIntent,
            PlayerIntentBuffer playerIntent)
        {
            _inputContext = inputContext ??
                throw new ArgumentNullException(nameof(inputContext));

            _commanderIntent = commanderIntent ??
                throw new ArgumentNullException(nameof(commanderIntent));

            _playerIntent = playerIntent ??
                throw new ArgumentNullException(nameof(playerIntent));
        }

        public bool TryActivate()
        {
            if (IsActive)
                return true;

            PrepareIntent();

            if (_inputContext.TryUseMap(InputMapId.Commander))
                return true;

            _commanderIntent.Clear();
            return false;
        }

        public bool TryDeactivate()
        {
            _commanderIntent.Clear();
            _playerIntent.SuppressInteractionUntilReleased();

            if (_inputContext.TryUseMap(InputMapId.Player))
                return true;

            _commanderIntent.SuppressExitUntilReleased();
            return false;
        }

        public bool TryRestoreAfterFailedExit()
        {
            PrepareIntent();
            return _inputContext.TryUseMap(InputMapId.Commander);
        }

        public bool ConsumeExitRequest()
        {
            return IsActive &&
                _commanderIntent.ConsumeExitRequest();
        }

        private void PrepareIntent()
        {
            _commanderIntent.Clear();
            _commanderIntent.SuppressExitUntilReleased();
            _playerIntent.SuppressInteractionUntilReleased();
        }
    }
}