using System;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.ArmoredPersonnelCarrier.Turret.Application;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.ArmoredPersonnelCarrier.Turret.Infrastructure
{
    public sealed class ApcTurretSearchInstaller : MonoInstaller
    {
        [SerializeField] private ApcTurretSearchConfig _config;

        public override void InstallBindings()
        {
            if (_config == null)
            {
                throw new InvalidOperationException(
                    "APC turret search requires Config.");
            }

            var search = new ApcTurretSearch(
                _config.SearchAngle,
                _config.DegreesPerSecond,
                new System.Random(Guid.NewGuid().GetHashCode()));

            Container.Bind<ApcTurretSearch>()
                .FromInstance(search)
                .AsSingle();

            Container.Bind<ApcTurretSearchConfig>()
                .FromInstance(_config)
                .AsSingle();
        }
    }
}