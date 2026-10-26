using System;
using UnityEngine;
using UnityEngine.InputSystem;
using _Project.Develop.Runtime.Gameplay.Features.Input.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Input.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Input.Infrastructure.Readers
{
    public sealed class GpsActionsReader : ICommonActionsReader
    {
        private readonly PlayerInput _playerInput;
        private readonly InputActionMap _map;
        private readonly InputAction _interact;
        private readonly InputAction _interactionNavigate;
        private readonly InputAction _interactionCancel;
        private readonly InputAction _zoom;
        private readonly InputAction _cursorDelta;
        private readonly InputAction _selectDestination;
        private readonly InputAction _clearRoute;
        public InputMapId MapId => InputMapId.GPS;

        public bool IsActive =>
            _playerInput.currentActionMap == _map;

        public GpsActionsReader(PlayerInput playerInput)
        {
            _playerInput = playerInput ??
                throw new ArgumentNullException(nameof(playerInput));

            InputActionAsset actions = _playerInput.actions;

            if (actions == null)
            {
                throw new InvalidOperationException(
                    "PlayerInput has no action asset.");
            }

            _map = actions.FindActionMap("GPS", true);
            _interact = _map.FindAction("Interact", true);
            _interactionNavigate = _map.FindAction("InteractionNavigate", true);
            _interactionCancel = _map.FindAction("InteractionCancel", true);
            _zoom = _map.FindAction("Zoom", true);
            _cursorDelta = _map.FindAction("CursorDelta", true);
            _selectDestination = _map.FindAction("SelectDestination", true);
            _clearRoute = _map.FindAction("ClearRoute", true);
        }

        public CommonInputSnapshot ReadCommonActions()
        {
            if (!IsActive)
                return CommonInputSnapshot.Neutral;

            return new CommonInputSnapshot(
                0f,
                0f,
                false,
                _interact.WasPressedThisFrame(),
                _interact.IsPressed(),
                _interact.WasReleasedThisFrame(),
                false,
                _interactionNavigate.WasPerformedThisFrame()
                    ? Math.Sign(_interactionNavigate.ReadValue<float>()) : 0,
                _interactionCancel.WasPressedThisFrame());
        }
        public float ReadZoom()
        {
            return IsActive
                ? _zoom.ReadValue<float>()
                : 0f;
        }
        public Vector2 ReadCursorDelta()
        {
            return IsActive
                ? _cursorDelta.ReadValue<Vector2>()
                : Vector2.zero;
        }
        public bool ReadSelectionPressed()
        {
            return IsActive && _selectDestination.WasPressedThisFrame();
        }

        public bool ReadSelectionHeld()
        {
            return IsActive && _selectDestination.IsPressed();
        }
        public bool ReadClearPressed()
        {
            return IsActive && _clearRoute.WasPressedThisFrame();
        }

        public bool ReadClearHeld()
        {
            return IsActive && _clearRoute.IsPressed();
        }
    }
}
