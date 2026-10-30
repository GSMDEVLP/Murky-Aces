using System;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Core.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Movement.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Movement.Application;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Behaviour.Application;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Movement.Infrastructure
{
    [DisallowMultipleComponent]
    public sealed class KamikazeDroneInstaller : MonoInstaller
    {
        [SerializeField] private KamikazeDroneConfig _config;
        [SerializeField] private DroneFlightBody _flightBody;

        public override void InstallBindings()
        {
            ValidateReferences();

            var settings = new DroneMovementSettings(
                _config.MaxSpeed,
                _config.Acceleration,
                _config.Deceleration,
                _config.LegDistance,
                _config.PauseDuration);

            var patrol = new KamikazeDroneBehaviour(
                settings,
                new System.Random(Guid.NewGuid().GetHashCode()));

            Container.Bind<DroneMovementSettings>().FromInstance(settings).AsSingle();
            Container.Bind<IDroneFlightBody>().FromInstance(_flightBody).AsSingle();
            Container.Bind<KamikazeDroneBehaviour>().FromInstance(patrol).AsSingle();
            Container.Bind<DronePursuit>().AsSingle();
            Container.Bind<DroneBehaviour>().AsSingle();
            Container.Bind<IDroneRuntimeModule>().To<DroneMovementModule>().AsSingle();
        }

        private void ValidateReferences()
        {
            if (_config == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(KamikazeDroneInstaller)} requires a config.");
            }

            if (_flightBody == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(KamikazeDroneInstaller)} requires a flight body.");
            }
        }
    }
}
