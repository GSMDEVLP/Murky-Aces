using System;
using UnityEngine;
using UnityEngine.InputSystem;
using _Project.Develop.Runtime.Gameplay.Features.Input.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Input.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Input.Infrastructure.Readers
{
    public sealed class DrivingActionsReader : ICommonActionsReader
    {
        private const string MapName = "Driving";

        private readonly PlayerInput _playerInput;
        private readonly InputActionMap _map;

        private readonly InputAction _throttle;
        private readonly InputAction _steering;
        private readonly InputAction _brake;
        private readonly InputAction _look;
        private readonly InputAction _interact;

        public InputMapId MapId => InputMapId.Driving;

        public bool IsActive =>
            _playerInput.currentActionMap == _map;

        public DrivingActionsReader(PlayerInput playerInput)
        {
            _playerInput = playerInput ??
                throw new ArgumentNullException(
                    nameof(playerInput));

            InputActionAsset actions = _playerInput.actions;

            if (actions == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerInput)} has no action asset.");
            }

            _map = actions.FindActionMap(MapName, true);
            _throttle = _map.FindAction("Throttle", true);
            _steering = _map.FindAction("Steering", true);
            _brake = _map.FindAction("Brake", true);
            _look = _map.FindAction("Look", true);
            _interact = _map.FindAction("Interact", true);
        }

        public DrivingActionsSnapshot ReadDrivingActions()
        {
            if (IsActive == false)
                return DrivingActionsSnapshot.Neutral;

            return new DrivingActionsSnapshot(
                _throttle.ReadValue<float>(),
                _steering.ReadValue<float>(),
                _brake.IsPressed());
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
                false);
        }
    }
}