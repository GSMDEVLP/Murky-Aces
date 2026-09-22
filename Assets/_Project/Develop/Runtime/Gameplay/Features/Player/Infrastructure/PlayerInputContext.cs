using System;
using _Project.Develop.Runtime.Gameplay.Features.Player.Application.Abstractions;
using UnityEngine.InputSystem;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Infrastructure
{
    public sealed class PlayerInputContext : IPlayerInputContext
    {
        private const string PlayerMapName = "Player";
        private const string DrivingMapName = "Driving";

        private readonly PlayerInput _playerInput;

        public bool IsUsingPlayerMap => IsCurrentMap(PlayerMapName);

        public bool IsUsingDrivingMap => IsCurrentMap(DrivingMapName);

        public PlayerInputContext(PlayerInput playerInput)
        {
            _playerInput = playerInput ??
                throw new ArgumentNullException(
                    nameof(playerInput));
        }

        public bool TryUsePlayerMap()
        {
            return TrySwitchMap(PlayerMapName);
        }

        public bool TryUseDrivingMap()
        {
            return TrySwitchMap(DrivingMapName);
        }

        private bool TrySwitchMap(string mapName)
        {
            InputActionAsset actions = _playerInput.actions;

            if (actions == null)
                return false;

            InputActionMap targetMap = actions.FindActionMap(mapName, false);

            if (targetMap == null)
                return false;

            if (_playerInput.currentActionMap == targetMap)
            {
                return true;
            }

            _playerInput.SwitchCurrentActionMap(mapName);

            return _playerInput.currentActionMap == targetMap;
        }

        private bool IsCurrentMap(string mapName)
        {
            InputActionMap currentMap = _playerInput.currentActionMap;
            return currentMap != null && currentMap.name == mapName;
        }
    }
}