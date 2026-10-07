using System;
using System.Threading;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Turret.Infrastructure;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Weapon.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Weapon.Application;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Weapon.Configs;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Weapon.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Weapon.Infrastructure
{
    public sealed class TankWeaponInstaller : MonoInstaller
    {
        private static long _nextRuntimeTankId;

        [SerializeField] private TankWeaponView _weaponView;
        [SerializeField] private GunConfig _config;
        [SerializeField] private UnityTurretRig _rig;
        [SerializeField] private RigidbodyGunLauncher _launcher;

        public override void InstallBindings()
        {
            TankRoot owner = GetComponentInParent<TankRoot>();

            if (owner == null ||
                _weaponView == null ||
                _config == null ||
                !_config.IsValid ||
                _rig == null ||
                _launcher == null)
            {
                throw new InvalidOperationException(
                    "Tank weapon references or config are missing or invalid.");
            }

            if (!_weaponView.transform.IsChildOf(owner.transform) ||
                !_rig.transform.IsChildOf(owner.transform) ||
                !_launcher.transform.IsChildOf(owner.transform))
            {
                throw new InvalidOperationException(
                    "Weapon components must belong to this tank.");
            }

            _launcher.InitializeOwner(owner.transform);

            var chamber = new GunChamber(_weaponView.WeaponId);
            var loading = new GunLoadingService(chamber);

            ulong tankId = unchecked(
                (ulong)Interlocked.Increment(ref _nextRuntimeTankId));

            var gun = new TankGun(
                tankId,
                chamber,
                loading,
                _launcher,
                _config);

            Container.Bind<GunChamber>()
                .FromInstance(chamber).AsSingle();

            Container.Bind<GunLoadingService>()
                .FromInstance(loading).AsSingle();

            Container.Bind<TankWeaponView>()
                .FromInstance(_weaponView).AsSingle();

            Container.Bind<GunConfig>()
                .FromInstance(_config).AsSingle();

            Container.Bind<ITurretRig>()
                .FromInstance(_rig).AsSingle();

            Container.Bind<IGunLauncher>()
                .FromInstance(_launcher).AsSingle();

            Container.Bind<TankGun>()
                .FromInstance(gun).AsSingle();

            Container.Bind<ITankRuntimeModule>()
                .FromInstance(new TankWeaponModule(gun));
        }
    }
}