using System;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.Core.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.Core.Infrastructure;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.ArmoredPersonnelCarrier.Turret.Application;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.ArmoredPersonnelCarrier.Turret.Infrastructure
{
    public sealed class ApcTurretAimInstaller : MonoInstaller
    {
        [SerializeField] private ApcTurretAimConfig _config;
        [SerializeField] private Transform _turret;
        [SerializeField] private Transform _gun;

        public override void InstallBindings()
        {
            if (_config == null || _turret == null || _gun == null ||
                _gun.parent != _turret)
            {
                throw new InvalidOperationException(
                    "APC aiming requires Config, Turret and its direct child Gun.");
            }

            _config.Validate();

            Container.Bind<ApcTurretAim>()
                .FromInstance(new ApcTurretAim(
                    _config.MinPitch, _config.MaxPitch))
                .AsSingle();

            Container.Bind<IEnemyRuntimeModule>()
                .WithId(EnemyModuleGroup.Gameplay)
                .To<ApcTurretModule>()
                .AsSingle()
                .WithArguments(_turret, _gun, _config);
        }
    }
}
