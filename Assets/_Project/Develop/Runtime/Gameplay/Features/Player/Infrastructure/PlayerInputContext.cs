using System;
using UnityEngine.InputSystem;
using _Project.Develop.Runtime.Gameplay.Features.Player.Application.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Input.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Infrastructure
{
    public sealed class PlayerInputContext : IPlayerInputContext
    {
        private const string PlayerMapName = "Player";
        private const string DrivingMapName = "Driving";
        private const string GunnerMapName = "Gunner";
        private const string CommanderMapName = "Commander";

        private readonly PlayerInput _playerInput;

        public bool IsUsingPlayerMap => CurrentMap == InputMapId.Player;

        public bool IsUsingDrivingMap => CurrentMap == InputMapId.Driving;

        public bool IsUsingGunnerMap => CurrentMap == InputMapId.Gunner;

        public InputMapId CurrentMap
        {
            get
            {
                InputActionMap currentMap = _playerInput.currentActionMap;

                if (currentMap == null)
                    return InputMapId.None;

                switch (currentMap.name)
                {
                    case PlayerMapName:
                        return InputMapId.Player;

                    case DrivingMapName:
                        return InputMapId.Driving;

                    case GunnerMapName:
                        return InputMapId.Gunner;
                    case CommanderMapName:
                        return InputMapId.Commander;    
                    default:
                        return InputMapId.None;
                }
            }
        }
        public PlayerInputContext(PlayerInput playerInput)
        {
            _playerInput = playerInput ??
                throw new ArgumentNullException(
                    nameof(playerInput));
        }

        public bool TryUseMap(InputMapId map)
        {
            switch (map)
            {
                case InputMapId.Player:
                    return TrySwitchMap(PlayerMapName);

                case InputMapId.Driving:
                    return TrySwitchMap(DrivingMapName);

                case InputMapId.Gunner:
                    return TrySwitchMap(GunnerMapName);

                case InputMapId.Commander:
                    return TrySwitchMap(CommanderMapName);

                default:
                    return false;
            }
        }

        public bool TryUsePlayerMap()
        {
            return TryUseMap(InputMapId.Player);
        }

        public bool TryUseDrivingMap()
        {
            return TryUseMap(InputMapId.Driving);
        }

        public bool TryUseGunnerMap()
        {
            return TryUseMap(InputMapId.Gunner);
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
    }
}