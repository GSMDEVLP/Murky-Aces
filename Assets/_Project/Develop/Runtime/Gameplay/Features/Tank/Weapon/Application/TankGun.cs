using System;
using UnityEngine;
using _Project.Develop.Runtime.Core.GameLoop.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Weapon.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Weapon.Configs;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Weapon.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Weapon.Application
{
    public sealed class TankGun : IGameplayTickable
    {
        private readonly ulong _tankId;
        private readonly GunChamber _chamber;
        private readonly GunLoadingService _loading;
        private readonly IGunLauncher _launcher;
        private readonly float _cooldownSeconds;

        private bool _enabled;
        private float _remainingCooldown;

        public TankGun(
            ulong tankId,
            GunChamber chamber,
            GunLoadingService loading,
            IGunLauncher launcher,
            GunConfig config)
        {
            _tankId = tankId;

            _chamber = chamber
                ?? throw new ArgumentNullException(nameof(chamber));

            _loading = loading
                ?? throw new ArgumentNullException(nameof(loading));

            _launcher = launcher
                ?? throw new ArgumentNullException(nameof(launcher));

            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (!config.IsValid)
            {
                throw new ArgumentException(
                    "Gun config is invalid.",
                    nameof(config));
            }

            _cooldownSeconds = config.FireCooldown;
        }

        public void SetEnabled(bool enabled)
        {
            _enabled = enabled;
        }

        public void Tick(float deltaTime)
        {
            if (!_enabled || deltaTime <= 0f)
                return;

            _remainingCooldown = Math.Max(
                0f,
                _remainingCooldown - deltaTime);
        }

        public bool TryFire(
            ulong actorId,
            Pose muzzlePose,
            Vector3 inheritedTankVelocity)
        {
            if (!_enabled ||
                _remainingCooldown > 0f ||
                !_chamber.IsLoaded)
            {
                return false;
            }

            var shell = _loading.LoadedShell;

            if (shell == null || !shell.IsValid)
                return false;

            string shellId = _chamber.LoadedShellId;

            var request = new GunShotRequest(
                shell,
                muzzlePose,
                inheritedTankVelocity,
                actorId,
                _tankId);

            if (!_launcher.TryLaunch(in request))
                return false;

            if (!_chamber.TryUnload(shellId))
            {
                throw new InvalidOperationException(
                    "Gun chamber changed during projectile launch.");
            }

            _remainingCooldown = _cooldownSeconds;
            return true;
        }
    }
}