using System;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Core.GameLoop.Abstractions;
using _Project.Develop.Runtime.Core.GameLoop.Application;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Movement;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Infrastructure.Debug
{
    public sealed class TankDebugInputSource : MonoBehaviour, IInputTickable
    {
        [SerializeField, Range(-1f, 1f)] private float _throttle;

        [SerializeField, Range(-1f, 1f)] private float _steering;

        [SerializeField] private bool _isBraking;

        private GameLoopRegistry _gameLoopRegistry;
        private TankMovement _tankMovement;
        private bool _isRegistered;

        [Inject]
        public void Construct(GameLoopRegistry gameLoopRegistry, TankMovement tankMovement)
        {
            _gameLoopRegistry = gameLoopRegistry ??
                throw new ArgumentNullException(
                    nameof(gameLoopRegistry));

            _tankMovement = tankMovement ??
                throw new ArgumentNullException(
                    nameof(tankMovement));

            TryRegister();
        }

        public void Tick(float deltaTime)
        {
            TankDrivingInput input = new TankDrivingInput(_throttle, _steering, _isBraking);
            _tankMovement.SetInput(input);
        }

        private void OnEnable()
        {
            TryRegister();
        }

        private void OnDisable()
        {
            _tankMovement?.ClearInput();
            Unregister();
        }

        private void OnDestroy()
        {
            Unregister();
        }

        private void TryRegister()
        {
            if (_isRegistered)
                return;

            if (!isActiveAndEnabled)
                return;

            if (_gameLoopRegistry == null)
                return;

            _gameLoopRegistry.RegisterInput(this);
            _isRegistered = true;
        }

        private void Unregister()
        {
            if (!_isRegistered)
                return;

            _gameLoopRegistry.UnregisterInput(this);
            _isRegistered = false;
        }
    }
}