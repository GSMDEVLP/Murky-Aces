using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Stations
{
    public sealed class TankStationsInstaller : MonoInstaller
    {
        [Header("Commander Station")]
        [SerializeField] private CrewStationView _commanderStationView;

        [Header("Driver Station")]
        [SerializeField] private CrewStationView _driverStationView;
        [SerializeField] private StationDisplayFeed _driverDisplayFeed;
    
        [Header("Gunner Station")]
        [SerializeField] private CrewStationView _gunnerStationView;
        [SerializeField] private StationDisplayFeed _gunnerDisplayFeed;


       public override void InstallBindings()
        {
            if (_driverStationView == null || _driverDisplayFeed == null ||
                _gunnerStationView == null || _gunnerDisplayFeed == null ||
                _commanderStationView == null)
            {
                throw new InvalidOperationException(
                    "Crew station references are not assigned.");
            }

            Container.Bind<DriverStationAdapter>().AsSingle();
            Container.Bind<GunnerStationAdapter>().AsSingle();
            Container.Bind<CommanderStationAdapter>().AsSingle();

            Container.Bind<CrewStationRegistry>()
                .FromMethod(context =>
                {
                    var commander = new CrewStationController(
                        new CrewStationOccupancy(),
                        _commanderStationView,
                        context.Container.Resolve<CommanderStationAdapter>());

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
                            new List<CrewStationController> { driver, gunner, commander });
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