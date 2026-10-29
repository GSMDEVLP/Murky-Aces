using System;
using _Project.Develop.Runtime.Core.GameLoop.Application;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Core.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Movement.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Movement.Application;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Movement.Infrastructure
{
    public sealed class DroneMovementModule : IDroneRuntimeModule
    {
        private readonly DroneMovement _movement;
        private readonly IDroneFlightBody _flightBody;

        public DroneMovementModule(DroneMovement movement, IDroneFlightBody flightBody)
        {
            _movement = movement ??
                throw new ArgumentNullException(nameof(movement));
            _flightBody = flightBody ??
                throw new ArgumentNullException(nameof(flightBody));
        }

        public void Register(GameLoopRegistry registry)
        {
            _flightBody.Initialize();
            _flightBody.ResetDistanceSample();
            registry.RegisterFixedGameplay(_movement);
        }

        public void Unregister(GameLoopRegistry registry)
        {
            registry.UnregisterFixedGameplay(_movement);
            _flightBody.Stop();
        }
    }
}
