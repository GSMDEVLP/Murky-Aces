using System;
using UnityEngine;
using UnityEngine.InputSystem;
using _Project.Develop.Runtime.Gameplay.Features.Input.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Input.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Input.Infrastructure.Readers
{
    public sealed class CommanderActionsReader : ICommonActionsReader
    {
        private readonly PlayerInput _playerInput;
        private readonly InputActionMap _map;

        private readonly InputAction _look;
        private readonly InputAction _interact;
        private readonly InputAction _drop;
        private readonly InputAction _exitStation;

        public InputMapId MapId => InputMapId.Commander;

        public bool IsActive =>
            _playerInput.currentActionMap == _map;

        public CommanderActionsReader(PlayerInput playerInput)
        {
            _playerInput = playerInput ??
                throw new ArgumentNullException(nameof(playerInput));

            InputActionAsset actions = _playerInput.actions;

            if (actions == null)
            {
                throw new InvalidOperationException(
                    "PlayerInput has no action asset.");
            }

            _map = actions.FindActionMap("Commander", true);

            _look = _map.FindAction("Look", true);
            _interact = _map.FindAction("Interact", true);
            _drop = _map.FindAction("Drop", true);
            _exitStation = _map.FindAction("ExitStation", true);
        }

        public CommonInputSnapshot ReadCommonActions()
        {
            if (!IsActive)
                return CommonInputSnapshot.Neutral;

            Vector2 look = _look.ReadValue<Vector2>();

            bool lookIsPointerDelta =
                _look.activeControl?.device is Pointer;

            return new CommonInputSnapshot(
                look.x,
                look.y,
                lookIsPointerDelta,
                _interact.WasPressedThisFrame(),
                _interact.IsPressed(),
                _interact.WasReleasedThisFrame(),
                _drop.WasPressedThisFrame());
        }

        public CommanderActionsSnapshot ReadCommanderActions()
        {
            if (!IsActive)
                return CommanderActionsSnapshot.Neutral;

            return new CommanderActionsSnapshot(
                _exitStation.WasPressedThisFrame(),
                _exitStation.IsPressed());
        }
    }
}