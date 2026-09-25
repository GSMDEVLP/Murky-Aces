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
        private InputAction _playerLook;
        private InputAction _sprint;
        private InputAction _jump;
        private InputAction _attack;
        private InputAction _playerInteract;
        private InputAction _drivingInteract;
        private InputAction _drop;

        private InputAction _throttle;
        private InputAction _steering;
        private InputAction _brake;
        private InputAction _drivingLook;
        private InputAction _exitStation;

        public InputService(PlayerInput playerInput)
        {
            _playerInput = playerInput;
            CacheActions();
        }

        public Vector2 Move => _move.ReadValue<Vector2>();

        public Vector2 Look => GetActiveLookAction().ReadValue<Vector2>();

        public bool LookIsPointerDelta => GetActiveLookAction().activeControl?.device is Pointer;

        public bool SprintHeld => _sprint.IsPressed();

        public bool JumpPressedThisFrame => _jump.IsPressed();

        public bool AttackPressedThisFrame => _attack.WasPressedThisFrame();

        public bool InteractPressedThisFrame => GetActiveInteractAction().WasPressedThisFrame();

        public bool InteractHeld => GetActiveInteractAction().IsPressed();

        public bool InteractReleasedThisFrame => GetActiveInteractAction().WasReleasedThisFrame();

        public bool DropPressedThisFrame => _drop.WasPressedThisFrame();

        public float Throttle => _throttle.ReadValue<float>();

        public float Steering => _steering.ReadValue<float>();

        public bool BrakeHeld => _brake.IsPressed();

        public bool ExitStationPressedThisFrame => _exitStation.WasPressedThisFrame();

        public bool ExitStationHeld => _exitStation.IsPressed();

        private InputAction GetActiveLookAction()
        {
            if (_playerInput.currentActionMap == _drivingMap)
            {
                return _drivingLook;
            }

            return _playerLook;
        }
        private InputAction GetActiveInteractAction()
        {
            if (_playerInput.currentActionMap == _drivingMap)
            {
                return _drivingInteract;
            }

            return _playerInteract;
        }

        private void CacheActions()
        {
            _actions = _playerInput.actions;

            _playerMap =  _actions.FindActionMap(PlayerMapName,true);

            _drivingMap = _actions.FindActionMap(DrivingMapName,true);

            _move = _playerMap.FindAction("Move",true);

            _playerLook = _playerMap.FindAction("Look",true);

            _playerInteract = _playerMap.FindAction("Interact", true);

            _sprint = _playerMap.FindAction("Sprint",true);

            _jump = _playerMap.FindAction("Jump",true);

            _attack = _playerMap.FindAction("Attack", true);

            _drop = _playerMap.FindAction("Drop",true);

            _throttle = _drivingMap.FindAction("Throttle",true);

            _steering = _drivingMap.FindAction("Steering",true);

            _brake = _drivingMap.FindAction("Brake", true);

            _drivingLook = _drivingMap.FindAction("Look", true);

            _drivingInteract = _drivingMap.FindAction("Interact", true);

            _exitStation =_drivingMap.FindAction("ExitStation",true);
        }
    }
}