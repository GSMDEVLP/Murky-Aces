using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Stations
{
    public sealed class TankStationsInstaller : MonoInstaller
    {
        [Header("Driver Station")]
        [SerializeField] private CrewStationView _driverStationView;
        [SerializeField] private StationDisplayFeed _driverDisplayFeed;

        [Header("Gunner Station")]
        [SerializeField] private CrewStationView _gunnerStationView;
        [SerializeField] private StationDisplayFeed _gunnerDisplayFeed;


       public override void InstallBindings()
        {
            if (_driverStationView == null || _driverDisplayFeed == null ||
                _gunnerStationView == null || _gunnerDisplayFeed == null)
            {
                throw new InvalidOperationException(
                    "Driver or gunner station references are not assigned.");
            }

            Container.Bind<DriverStationAdapter>().AsSingle();
            Container.Bind<GunnerStationAdapter>().AsSingle();

            Container.Bind<CrewStationRegistry>()
                .FromMethod(context =>
                {
                    var driver = new CrewStationController(
                        new CrewStationOccupancy(),
                        _driverStationView,
                        context.Container.Resolve<DriverStationAdapter>(),
                        _driverDisplayFeed);

                    var gunner = new CrewStationController(
                        new CrewStationOccupancy(),
                        _gunnerStationView,
                        context.Container.Resolve<GunnerStationAdapter>(),
                        _gunnerDisplayFeed);

                    return new CrewStationRegistry(
                        new List<CrewStationController> { driver, gunner });
                })
                .AsSingle();

            Container.Bind<ITankRuntimeModule>()
                .To<TankStationsModule>()
                .AsCached();

            Container.Bind<CrewStationInteractable>()
                .FromComponentsInHierarchy()
                .AsCached()
                .NonLazy();
        }
    }
}