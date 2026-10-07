using System;
using _Project.Develop.Runtime.Core.GameLoop.Application;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Weapon.Application;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Weapon.Infrastructure
{
    public sealed class TankWeaponModule : ITankRuntimeModule
    {
        private readonly TankGun _gun;

        public TankWeaponModule(TankGun gun)
        {
            _gun = gun
                ?? throw new ArgumentNullException(nameof(gun));
        }

        public void Register(GameLoopRegistry registry)
        {
            registry.RegisterGameplay(_gun);
            _gun.SetEnabled(true);
        }

        public void Unregister(GameLoopRegistry registry)
        {
            _gun.SetEnabled(false);
            registry.UnregisterGameplay(_gun);
        }
    }
}