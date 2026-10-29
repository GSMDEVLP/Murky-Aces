using System;
using _Project.Develop.Runtime.Core.GameLoop.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Movement.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Movement.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Movement.Application
{
    public sealed class DroneMovement : IFixedGameplayTickable
    {
        private readonly KamikazeDroneBehaviour _behaviour;
        private readonly IDroneFlightBody _flightBody;

        public KamikazeDroneState State => _behaviour.State;

        public DroneMovement(KamikazeDroneBehaviour behaviour, IDroneFlightBody flightBody)
        {
            _behaviour = behaviour ??
                throw new ArgumentNullException(nameof(behaviour));
            _flightBody = flightBody ??
                throw new ArgumentNullException(nameof(flightBody));
        }

        public void FixedTick(float fixedDeltaTime)
        {
            if (fixedDeltaTime <= 0f)
                return;

            _behaviour.Tick(_flightBody.MeasureTravelledDistance(), fixedDeltaTime);

            KamikazeDroneState state = _behaviour.State;
            _flightBody.ApplyFlight(state.Speed, state.HeadingOffsetDegrees);
        }
    }
}
