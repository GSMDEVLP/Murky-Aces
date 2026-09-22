using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Player.Domain;
using GameLoopTickable =
    _Project.Develop.Runtime.Core.GameLoop.Abstractions.ITickable;

namespace _Project.Develop.Runtime.Gameplay.Features.Input.Infrastructure
{
    public sealed class PlayerInputSystem : MonoBehaviour, GameLoopTickable
    {
        [SerializeField] private float _mouseLookDegreesPerPixel = 0.15f;
        [SerializeField] private float _stickLookDegreesPerSecond = 180f;

        private InputService _input;
        private PlayerIntentBuffer _playerIntent;
        private DrivingIntentBuffer _drivingIntent;

        [Inject]
        public void Construct(InputService input, PlayerIntentBuffer playerIntent, DrivingIntentBuffer drivingIntent)
        {
            _input = input;
            _playerIntent = playerIntent;
            _drivingIntent = drivingIntent;
        }

        public void Tick(float deltaTime)
        {
            UpdatePlayerIntent(deltaTime);
            UpdateDrivingIntent();
        }

        private void UpdatePlayerIntent(float deltaTime)
        {
            _playerIntent.SetMovement(
                _input.Move,
                _input.SprintHeld);

            float lookScale =
                _input.LookIsPointerDelta
                    ? _mouseLookDegreesPerPixel
                    : _stickLookDegreesPerSecond *
                      deltaTime;

            _playerIntent.SetLook(
                _input.Look * lookScale);

            if (_input.JumpPressedThisFrame)
                _playerIntent.RequestJump();

            _playerIntent.SetInteraction(
                _input.InteractPressedThisFrame,
                _input.InteractHeld,
                _input.InteractReleasedThisFrame);

            _playerIntent.SetDrop(
                _input.DropPressedThisFrame);
        }

        private void UpdateDrivingIntent()
        {
            _drivingIntent.SetDriving(
                _input.Throttle,
                _input.Steering,
                _input.BrakeHeld);

            _drivingIntent.SetExitInput(
                _input.ExitStationPressedThisFrame,
                _input.ExitStationHeld);
        }
    }
}