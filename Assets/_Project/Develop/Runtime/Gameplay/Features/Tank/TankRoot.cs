using System;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Core.GameLoop.Application;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Movement;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Stations;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Turret.Infrastructure;
using System.Collections.Generic;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank
{
    public sealed class TankRoot : MonoBehaviour
    {
        private GameLoopRegistry _gameLoopRegistry;
        private TankMovement _tankMovement;
        private CrewStationRegistry _stationRegistry;
        private IReadOnlyList<CrewStationController> _stationControllers;
        private bool _isRegistered;
        private TurretAimRuntime _turretRuntime;
        private GunnerCameraPresenter _gunnerCameraPresenter;
        public TankMovement Movement => _tankMovement;

        [Inject]
        public void Construct(GameLoopRegistry gameLoopRegistry, 
            TankMovement tankMovement, 
            CrewStationRegistry stationRegistry, 
            TurretAimRuntime turretRuntime,
            GunnerCameraPresenter gunnerCameraPresenter)
        {
            _gameLoopRegistry = gameLoopRegistry ??
                throw new ArgumentNullException(
                    nameof(gameLoopRegistry));

            _tankMovement = tankMovement ??
                throw new ArgumentNullException(
                    nameof(tankMovement));

            _stationRegistry = stationRegistry ??
                throw new ArgumentNullException(
                    nameof(stationRegistry));

            _turretRuntime = turretRuntime ?? throw new ArgumentNullException(nameof(turretRuntime));
            _gunnerCameraPresenter = gunnerCameraPresenter ?? throw new ArgumentNullException(nameof(gunnerCameraPresenter));
            
            _stationControllers = _stationRegistry.Controllers;
            if (_stationControllers.Count == 0)
            {
                throw new ArgumentException(
                    "At least one crew station controller is required.",
                    nameof(stationRegistry));
            }

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

            if (_gameLoopRegistry == null || _tankMovement == null || _stationRegistry  == null)
            {
                return;
            }

            for (int i = 0; i < _stationControllers.Count; i++)
            {
                if (_stationControllers[i] == null)
                {
                    throw new InvalidOperationException(
                        "Crew station controller cannot be null.");
                }
            }

            for (int i = 0; i < _stationControllers.Count; i++)
            {
                _gameLoopRegistry.RegisterGameplay(
                    _stationControllers[i]);
            }

            _gameLoopRegistry.RegisterFixedGameplay(_tankMovement);
            _gameLoopRegistry.RegisterGameplay(_turretRuntime);
            _gameLoopRegistry.RegisterPresentation(_turretRuntime.Presentation);

            _gameLoopRegistry.RegisterPresentation(_gunnerCameraPresenter);
            _isRegistered = true;
        }

        private void Unregister()
        {
            if (_isRegistered == false)
                return;

            for (int i = 0; i < _stationControllers.Count; i++)
            {
                CrewStationController controller =
                    _stationControllers[i];

                if (controller == null)
                    continue;

                controller.ClearOutput();
                _gameLoopRegistry.UnregisterGameplay(controller);
            }

            _gameLoopRegistry.UnregisterFixedGameplay(_tankMovement);
            _turretRuntime.ClearInput();
            _gameLoopRegistry.UnregisterGameplay(_turretRuntime);
            _gameLoopRegistry.UnregisterPresentation(_turretRuntime.Presentation);
            _gameLoopRegistry.UnregisterPresentation(_gunnerCameraPresenter);

            _isRegistered = false;
        }
    }
}