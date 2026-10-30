using System;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Core.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Targeting.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Targeting.Application;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Targeting.Infrastructure
{
    public sealed class DroneDetectionInstaller : MonoInstaller
    {
        [SerializeField] private DroneDetectionConfig _config;
        [SerializeField] private DroneVisionSensor _sensor;

        public override void InstallBindings()
        {
            if (_config == null || _sensor == null)
                throw new InvalidOperationException(
                    "Drone detection requires Config and Sensor.");

            _sensor.Initialize(_config);

            Container.Bind<IDroneVisionSensor>()
                .FromInstance(_sensor)
                .AsSingle();

            Container.Bind<DroneTargetScanner>()
                .AsSingle()
                .WithArguments(_config.ScanInterval);

            Container.Bind<DroneTargetSelector>()
                .AsSingle();

            Container.Bind<IDroneRuntimeModule>()
                .To<DroneDetectionModule>().AsSingle();
        }
    }
}
