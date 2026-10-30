using System;
using _Project.Develop.Runtime.Core.GameLoop.Application;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Behaviour.Application;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Core.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Movement.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Movement.Infrastructure
{
    public sealed class DroneMovementModule : IDroneRuntimeModule
    {
        private readonly DroneBehaviour _controller;
        private readonly IDroneFlightBody _flightBody;

        public DroneMovementModule(DroneBehaviour controller, IDroneFlightBody flightBody)
        {
            _controller = controller ??
                throw new ArgumentNullException(nameof(controller));

            _flightBody = flightBody ??
                throw new ArgumentNullException(nameof(flightBody));
        }

        public void Register(GameLoopRegistry registry)
        {
            _flightBody.Initialize();
            _flightBody.ResetDistanceSample();

            registry.RegisterFixedGameplay(_controller);
        }

        public void Unregister(GameLoopRegistry registry)
        {
            registry.UnregisterFixedGameplay(_controller);
            _flightBody.Stop();
        }
    }
}