using System;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Core.GameLoop.Application;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Movement;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Stations;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank
{
    public sealed class TankRoot : MonoBehaviour
    {
        private GameLoopRegistry _gameLoopRegistry;
        private TankMovement _tankMovement;
        private CrewStationController  _driverStationController;

        private bool _isRegistered;

        public TankMovement Movement => _tankMovement;

        [Inject]
        public void Construct(
            GameLoopRegistry gameLoopRegistry,
            TankMovement tankMovement,
            CrewStationController  driverStationController)
        {
            _gameLoopRegistry = gameLoopRegistry ??
                throw new ArgumentNullException(
                    nameof(gameLoopRegistry));

            _tankMovement = tankMovement ??
                throw new ArgumentNullException(
                    nameof(tankMovement));

            _driverStationController =
                driverStationController ??
                throw new ArgumentNullException(
                    nameof(driverStationController));

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

            if (isActiveAndEnabled == false)
                return;

            if (_gameLoopRegistry == null ||
                _tankMovement == null ||
                _driverStationController == null)
            {
                return;
            }

            _gameLoopRegistry.RegisterGameplay(
                _driverStationController);

            _gameLoopRegistry.RegisterFixedGameplay(
                _tankMovement);

            _isRegistered = true;
        }

        private void Unregister()
        {
            if (_isRegistered == false)
                return;

            _driverStationController.ClearOutput();

            _gameLoopRegistry.UnregisterGameplay(
                _driverStationController);

            _gameLoopRegistry.UnregisterFixedGameplay(
                _tankMovement);

            _isRegistered = false;
        }
    }
}