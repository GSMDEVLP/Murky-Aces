using System;
using _Project.Develop.Runtime.Core.GameLoop.Application;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Core.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Movement.Application;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Movement.Infrastructure
{
    public sealed class TankMovementModule : ITankRuntimeModule
    {
        private readonly TankMovement _movement;

        public TankMovementModule(TankMovement movement)
        {
            _movement = movement ??
                throw new ArgumentNullException(nameof(movement));
        }

        public void Register(GameLoopRegistry registry)
        {
            registry.RegisterFixedGameplay(_movement);
        }

        public void Unregister(GameLoopRegistry registry)
        {
            registry.UnregisterFixedGameplay(_movement);
        }
    }
}