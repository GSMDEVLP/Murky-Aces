using System;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Core.GameLoop.Abstractions;
using _Project.Develop.Runtime.Core.GameLoop.Application;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Application;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Infrastructure
{
    public sealed class KamikazeDroneRoot : MonoBehaviour, IFixedGameplayTickable
    {
        private DroneFlightBody _flightBody;
        private KamikazeDroneBehaviour _behaviour;
        private GameLoopRegistry _gameLoopRegistry;

        [Inject]
        public void Construct(DroneFlightBody flightBody, KamikazeDroneBehaviour behaviour, GameLoopRegistry gameLoopRegistry)
        {
            _gameLoopRegistry?.UnregisterFixedGameplay(this);

            _flightBody = flightBody ??
                throw new ArgumentNullException(nameof(flightBody));

            _behaviour = behaviour ??
                throw new ArgumentNullException(nameof(behaviour));

            _gameLoopRegistry = gameLoopRegistry ??
                throw new ArgumentNullException(nameof(gameLoopRegistry));

            _flightBody.Initialize();

            TryRegister();
        }

        private void OnEnable()
        {
            TryRegister();
        }

        private void OnDisable()
        {
            _gameLoopRegistry?.UnregisterFixedGameplay(this);

            if (_flightBody != null)
                _flightBody.Stop();
        }

        public void FixedTick(float fixedDeltaTime)
        {
            float travelledDistance =
                _flightBody.MeasureTravelledDistance();

            _behaviour.Tick(travelledDistance, fixedDeltaTime);

            KamikazeDroneState state = _behaviour.State;

            _flightBody.ApplyFlight(
                state.Speed,
                state.HeadingOffsetDegrees);
        }

        private void TryRegister()
        {
            if (isActiveAndEnabled == false ||
                _flightBody == null ||
                _behaviour == null ||
                _gameLoopRegistry == null)
            {
                return;
            }

            _flightBody.ResetDistanceSample();
            _gameLoopRegistry.RegisterFixedGameplay(this);
        }
    }
}