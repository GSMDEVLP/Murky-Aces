using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Develop.Runtime.Gameplay.Features.Input.Infrastructure
{
    public sealed class InputService
    {
        private const string PlayerMapName = "Player";
        private const string DrivingMapName = "Driving";

        private readonly PlayerInput _playerInput;

        private InputActionAsset _actions;

        private InputActionMap _playerMap;
        private InputActionMap _drivingMap;

        private InputAction _move;
        private InputAction _look;
        private InputAction _sprint;
        private InputAction _jump;
        private InputAction _attack;
        private InputAction _interact;
        private InputAction _drop;

        private InputAction _throttle;
        private InputAction _steering;
        private InputAction _brake;
        private InputAction _exitStation;

        public InputService(PlayerInput playerInput)
        {
            _playerInput = playerInput;
            CacheActions();
        }

        public Vector2 Move => _move.ReadValue<Vector2>();

        public Vector2 Look => _look.ReadValue<Vector2>();

        public bool LookIsPointerDelta => _look.activeControl?.device is Pointer;

        public bool SprintHeld => _sprint.IsPressed();
        public bool JumpPressedThisFrame => _jump.IsPressed();
        public bool AttackPressedThisFrame => _attack.WasPressedThisFrame();
        public bool InteractPressedThisFrame => _interact.WasPressedThisFrame();
        public bool InteractHeld => _interact.IsPressed();
        public bool InteractReleasedThisFrame => _interact.WasReleasedThisFrame();
        public bool DropPressedThisFrame => _drop.WasPressedThisFrame();
        public float Throttle => _throttle.ReadValue<float>();
        public float Steering => _steering.ReadValue<float>();
        public bool BrakeHeld => _brake.IsPressed();
        public bool ExitStationPressedThisFrame => _exitStation.WasPressedThisFrame();
        public bool ExitStationHeld => _exitStation.IsPressed();

        private void CacheActions()
        {
            _actions = _playerInput.actions;

            _playerMap = _actions.FindActionMap(PlayerMapName,true);
            _drivingMap = _actions.FindActionMap(DrivingMapName, true);

            _move = _playerMap.FindAction("Move", true);
            _look = _playerMap.FindAction("Look", true);
            _sprint = _playerMap.FindAction("Sprint", true);
            _jump = _playerMap.FindAction("Jump", true);
            _attack = _playerMap.FindAction("Attack", true);
            _interact = _playerMap.FindAction("Interact", true);
            _drop = _playerMap.FindAction("Drop", true);

            _throttle = _drivingMap.FindAction("Throttle", true);
            _steering = _drivingMap.FindAction("Steering", true);
            _brake = _drivingMap.FindAction("Brake", true);
            _exitStation = _drivingMap.FindAction("ExitStation", true);
        }
    }
}