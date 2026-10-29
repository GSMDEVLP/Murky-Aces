using System;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Core.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Movement.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Movement.Application;

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

            var behaviour = new KamikazeDroneBehaviour(
                settings,
                new System.Random(Guid.NewGuid().GetHashCode()));

            var movement = new DroneMovement(behaviour, _flightBody);

            Container.Bind<IDroneFlightBody>().FromInstance(_flightBody).AsSingle();
            Container.Bind<KamikazeDroneBehaviour>().FromInstance(behaviour).AsSingle();
            Container.Bind<DroneMovement>().FromInstance(movement).AsSingle();
            Container.Bind<IDroneRuntimeModule>()
                .FromInstance(new DroneMovementModule(movement, _flightBody));
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
