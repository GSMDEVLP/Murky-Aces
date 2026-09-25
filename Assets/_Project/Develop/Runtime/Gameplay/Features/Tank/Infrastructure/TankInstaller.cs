using System;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Movement;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Stations;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Infrastructure
{
    public sealed class TankInstaller : MonoInstaller
    {
        [SerializeField] private TankMovementConfig _movementConfig;
        [SerializeField] private DynamicTankMotionBody _motionBody;
        [SerializeField] private DriverStationView _driverStationView;
        [SerializeField] private StationDisplayFeed _driverDisplayFeed;

        public override void InstallBindings()
        {
            ValidateReferences();

            Container.Bind<TankRoot>().FromComponentOnRoot().AsSingle();
            Container.Bind<TankMovementConfig>().FromInstance(_movementConfig).AsSingle();
            Container.Bind<TankMotionState>().AsSingle();
            Container.Bind<ITankMotionBody>().FromInstance(_motionBody).AsSingle();
            Container.Bind<TankMovement>().AsSingle();
            // Container.Bind<TankDebugInputSource>().FromComponentInHierarchy().AsSingle().NonLazy();
            Container.Bind<DriverStationView>().FromInstance(_driverStationView).AsSingle();
            Container.Bind<CrewStationOccupancy>().AsSingle();

            Container.Bind<DriverStationAdapter>().AsSingle();
            Container.Bind<ICrewStationRoleAdapter>().To<DriverStationAdapter>().FromResolve();
            Container.Bind<CrewStationController>().AsSingle();
            
            Container.Bind<DriverStationInteractable>().FromComponentInHierarchy().AsSingle().NonLazy();
            Container.Bind<StationDisplayFeed>().FromInstance(_driverDisplayFeed).AsSingle();
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
                    $"{nameof(DriverStationView)} is not assigned.");

            if (_driverDisplayFeed == null){
                throw new InvalidOperationException(
                    $"{nameof(StationDisplayFeed)} is not assigned.");
}
        }
    }
}