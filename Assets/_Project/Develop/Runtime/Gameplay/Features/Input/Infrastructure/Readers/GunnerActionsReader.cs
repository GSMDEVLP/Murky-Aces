using System;
using UnityEngine;
using UnityEngine.InputSystem;
using _Project.Develop.Runtime.Gameplay.Features.Input.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Input.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Input.Infrastructure.Readers
{
    public sealed class GunnerActionsReader : ICommonActionsReader
    {
        private const string MapName = "Gunner";

        private readonly PlayerInput _playerInput;
        private readonly InputActionMap _map;

        private readonly InputAction _aim;
        private readonly InputAction _fire;
        private readonly InputAction _zoom;
        private readonly InputAction _toggleReferenceMode;
        private readonly InputAction _look;
        private readonly InputAction _interact;
        private readonly InputAction _exitStation;
        private readonly InputAction _drop;

        public InputMapId MapId => InputMapId.Gunner;
        public bool IsActive => _playerInput.currentActionMap == _map;

        public GunnerActionsReader(PlayerInput playerInput)
        {
            _playerInput = playerInput ??
                throw new ArgumentNullException(nameof(playerInput));

            InputActionAsset actions = _playerInput.actions;
            if (actions == null)
                throw new InvalidOperationException(
                    $"{nameof(PlayerInput)} has no action asset.");

            _map = actions.FindActionMap(MapName, true);

            _aim = _map.FindAction("Aim", true);
            _fire = _map.FindAction("Fire", true);
            _zoom = _map.FindAction("Zoom", true);
            _toggleReferenceMode =
                _map.FindAction("ToggleReferenceMode", true);
            _look = _map.FindAction("Look", true);
            _interact = _map.FindAction("Interact", true);
            _exitStation = _map.FindAction("ExitStation", true);
            _drop = _map.FindAction("Drop", true);
        }

        public GunnerActionsSnapshot ReadGunnerActions()
        {
            if (!IsActive)
                return GunnerActionsSnapshot.Neutral;

            Vector2 aim = _aim.ReadValue<Vector2>();

            return new GunnerActionsSnapshot(
                aim.x,
                aim.y,
                _fire.WasPressedThisFrame(),
                _fire.IsPressed(),
                _zoom.IsPressed(),
                _toggleReferenceMode.WasPressedThisFrame(),
                _toggleReferenceMode.IsPressed(),
                _exitStation.WasPressedThisFrame(),
                _exitStation.IsPressed());
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
    }
}