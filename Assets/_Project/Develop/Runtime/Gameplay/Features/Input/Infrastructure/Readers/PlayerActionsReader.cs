using System;
using UnityEngine;
using UnityEngine.InputSystem;
using _Project.Develop.Runtime.Gameplay.Features.Input.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Input.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Input.Infrastructure.Readers
{
    public sealed class PlayerActionsReader : ICommonActionsReader
    {
        private const string MapName = "Player";

        private readonly PlayerInput _playerInput;
        private readonly InputActionMap _map;

        private readonly InputAction _move;
        private readonly InputAction _look;
        private readonly InputAction _sprint;
        private readonly InputAction _jump;
        private readonly InputAction _interact;
        private readonly InputAction _drop;

        public InputMapId MapId => InputMapId.Player;

        public bool IsActive =>
            _playerInput.currentActionMap == _map;

        public PlayerActionsReader(PlayerInput playerInput)
        {
            _playerInput = playerInput ??
                throw new ArgumentNullException(
                    nameof(playerInput));

            InputActionAsset actions =
                _playerInput.actions;

            if (actions == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerInput)} has no action asset.");
            }

            _map = actions.FindActionMap(MapName, true);

            _move = _map.FindAction("Move", true);

            _look = _map.FindAction("Look", true);

            _sprint = _map.FindAction("Sprint", true);

            _jump = _map.FindAction("Jump", true);

            _interact = _map.FindAction("Interact", true);

            _drop = _map.FindAction("Drop", true);
        }

        public PlayerActionsSnapshot ReadPlayerActions()
        {
            if (IsActive == false)
                return PlayerActionsSnapshot.Neutral;

            Vector2 move =
                _move.ReadValue<Vector2>();

            return new PlayerActionsSnapshot(
                move.x,
                move.y,
                _sprint.IsPressed(),
                _jump.IsPressed());
        }

        public CommonInputSnapshot ReadCommonActions()
        {
            if (IsActive == false)
                return CommonInputSnapshot.Neutral;

            Vector2 look =
                _look.ReadValue<Vector2>();

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
    }
}