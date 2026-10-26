using System;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Application;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Infrastructure
{
    [DisallowMultipleComponent]
    public sealed class KamikazeDroneInstaller : MonoInstaller
    {
        [SerializeField] private KamikazeDroneConfig _config;
        [SerializeField] private DroneFlightBody _flightBody;

        public override void InstallBindings()
        {
            ValidateReferences();

            Container.Bind<KamikazeDroneConfig>().FromInstance(_config).AsSingle();

            Container.Bind<DroneFlightBody>().FromInstance(_flightBody).AsSingle();

            Container.Bind<KamikazeDroneState>().AsSingle();

            Container.Bind<KamikazeDroneBehaviour>()
                .AsSingle()
                .WithArguments(
                    _config.MaxSpeed,
                    _config.Acceleration,
                    _config.Deceleration,
                    _config.LegDistance,
                    _config.PauseDuration,
                    new System.Random(Guid.NewGuid().GetHashCode()));
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