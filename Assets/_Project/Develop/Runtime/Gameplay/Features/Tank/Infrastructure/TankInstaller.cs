using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Movement;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Stations;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Turret;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Turret.Infrastructure;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Infrastructure
{
    public sealed class TankInstaller : MonoInstaller
    {
        [SerializeField] private TankMovementConfig _movementConfig;
        [SerializeField] private TurretAimConfig _turretAimConfig;
        [SerializeField] private DynamicTankMotionBody _motionBody;
        [SerializeField] private CrewStationView _driverStationView;
        [SerializeField] private StationDisplayFeed _driverDisplayFeed;
        [SerializeField] private UnityTurretRig _turretRig;


        [SerializeField] private Transform _gunnerCameraMount;
        [SerializeField] private Transform _gunnerEye;
        
        public override void InstallBindings()
        {
            ValidateReferences();

            BindTankRoot();

            TankMovement tankMovement = CreateAndBindMovement();
            CreateAndBindTurret();
            CreateAndBindGunnerCamera();
            CreateAndBindStations(tankMovement);
        }

        private void BindTankRoot()
        {
            Container.Bind<TankRoot>()
                .FromComponentOnRoot()
                .AsSingle();
        }

        private TankMovement CreateAndBindMovement()
        {
            TankMotionState motionState = new TankMotionState();

            TankMovement tankMovement = new TankMovement(_movementConfig, motionState, _motionBody);

            Container.Bind<TankMovementConfig>().FromInstance(_movementConfig).AsSingle();
            Container.Bind<TankMotionState>().FromInstance(motionState).AsSingle();
            Container.Bind<ITankMotionBody>().FromInstance(_motionBody).AsSingle();
            Container.Bind<TankMovement>().FromInstance(tankMovement).AsSingle();

            return tankMovement;
        }

        private void CreateAndBindGunnerCamera()
        {
            var presenter = new GunnerCameraPresenter(
                _gunnerCameraMount,
                _gunnerEye,
                _turretRig);

            Container.Bind<GunnerCameraPresenter>()
                .FromInstance(presenter)
                .AsSingle();
        }
        
        private void CreateAndBindStations(TankMovement tankMovement)
        {
            DriverStationAdapter driverAdapter = new DriverStationAdapter(tankMovement);

            CrewStationController driverController = CreateStation(_driverStationView, driverAdapter, _driverDisplayFeed);

            CrewStationRegistry stationRegistry = new CrewStationRegistry(
                    new List<CrewStationController>
                    {
                        driverController
                    });

            Container.Bind<CrewStationRegistry>().FromInstance(stationRegistry).AsSingle();
            Container.Bind<CrewStationInteractable>().FromComponentsInHierarchy().AsCached().NonLazy();
        }

        private static CrewStationController CreateStation(CrewStationView view, ICrewStationRoleAdapter roleAdapter, StationDisplayFeed displayFeed)
        {
            return new CrewStationController(new CrewStationOccupancy(), view, roleAdapter, displayFeed);
        }

        private void CreateAndBindTurret()
        {
            TurretAimState state = new TurretAimState(
                initialYaw: 0f,
                initialPitch: 0f,
                initialHullYaw: _turretRig.HullYaw,
                initialMode: _turretAimConfig.DefaultReferenceMode);

            TurretMechanism mechanism = new TurretMechanism(
                state,
                _turretAimConfig.TraverseDegreesPerSecond,
                _turretAimConfig.ElevationDegreesPerSecond,
                _turretAimConfig.MinPitch,
                _turretAimConfig.MaxPitch);

            TurretAimRuntime runtime = new TurretAimRuntime(mechanism, _turretRig);

            Container.Bind<TurretAimRuntime>().FromInstance(runtime).AsSingle();
        }

        private void ValidateReferences()
        {
            if (_movementConfig == null)
                throw new InvalidOperationException(
                    $"{nameof(TankMovementConfig)} is not assigned.");

            if (_motionBody == null)
                throw new InvalidOperationException(
                    $"{nameof(DynamicTankMotionBody)} is not assigned.");

            if (_driverStationView == null)
                throw new InvalidOperationException(
                    $"{nameof(CrewStationView)} is not assigned.");

            if (_driverDisplayFeed == null)
                throw new InvalidOperationException(
                    $"{nameof(StationDisplayFeed)} is not assigned.");

            if (_turretAimConfig == null || !_turretAimConfig.IsValid)
                throw new InvalidOperationException(
                    "TurretAimConfig is missing or invalid.");

            if (_turretRig == null)
                throw new InvalidOperationException(
                    "UnityTurretRig is not assigned.");
                        
        }
    }
}