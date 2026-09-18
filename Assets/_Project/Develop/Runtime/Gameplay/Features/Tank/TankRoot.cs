using System;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Core.GameLoop.Application;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Movement;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank
{
    public sealed class TankRoot : MonoBehaviour
    {
        private GameLoopRegistry _gameLoopRegistry;
        private TankMovement _tankMovement;

        private bool _isRegistered;

        public TankMovement Movement => _tankMovement;

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

        private void OnEnable()
        {
            TryRegister();
        }

        private void OnDisable()
        {
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

            if (_gameLoopRegistry == null ||
                _tankMovement == null)
            {
                return;
            }

            _gameLoopRegistry.RegisterFixedGameplay(_tankMovement);

            _isRegistered = true;
        }

        private void Unregister()
        {
            if (!_isRegistered)
                return;

            _gameLoopRegistry.UnregisterFixedGameplay(_tankMovement);

            _isRegistered = false;
        }
    }
}