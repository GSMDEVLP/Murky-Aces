using System;
using _Project.Develop.Runtime.Core.GameLoop.Application;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Weapon.Application;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Core.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Weapon.Infrastructure
{
    public sealed class TankWeaponModule : ITankRuntimeModule
    {
        private readonly TankGun _gun;
        private readonly TankFireRequestHandler _fire;

        public TankWeaponModule(
            TankGun gun,
            TankFireRequestHandler fire)
        {
            _gun = gun ??
                throw new ArgumentNullException(nameof(gun));

            _fire = fire ??
                throw new ArgumentNullException(nameof(fire));
        }

        public void Register(GameLoopRegistry registry)
        {
            registry.RegisterGameplay(_gun);
            registry.RegisterGameplay(_fire);

            _gun.SetEnabled(true);
            _fire.SetEnabled(true);
        }

        public void Unregister(GameLoopRegistry registry)
        {
            _fire.SetEnabled(false);
            _gun.SetEnabled(false);

            registry.UnregisterGameplay(_fire);
            registry.UnregisterGameplay(_gun);
        }
    }
}
