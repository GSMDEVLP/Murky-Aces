using System;
using _Project.Develop.Runtime.Core.GameLoop.Application;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Behaviour.Application;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.Core.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Movement.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Movement.Infrastructure
{
    public sealed class DroneMovementModule : IEnemyRuntimeModule
    {
        private readonly DroneBehaviour _controller;
        private readonly IDroneFlightBody _flightBody;

        private bool _isActive;

        public DroneMovementModule(DroneBehaviour controller, IDroneFlightBody flightBody)
        {
            _controller = controller ??
                throw new ArgumentNullException(nameof(controller));

            _flightBody = flightBody ??
                throw new ArgumentNullException(nameof(flightBody));
        }

        public void Activate(GameLoopRegistry registry)
        {
            if (_isActive)
                return;

            _isActive = true;

            _flightBody.Initialize();
            _flightBody.ResetDistanceSample();

            registry.RegisterFixedGameplay(_controller);
        }

        public void Deactivate(GameLoopRegistry registry)
        {
            if (_isActive == false)
                return;

            _isActive = false;

            registry.UnregisterFixedGameplay(_controller);
            _flightBody.Stop();
        }
    }
}
