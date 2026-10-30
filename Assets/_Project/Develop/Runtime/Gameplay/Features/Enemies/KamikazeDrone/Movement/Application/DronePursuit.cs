using System;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Movement.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Movement.Application
{
    public sealed class DronePursuit
    {
        private const float ARRIVAL_TOLERANCE = 0.05f;

        private readonly DroneMovementSettings _settings;
        private readonly IDroneFlightBody _flightBody;

        public DronePursuit(
            DroneMovementSettings settings,
            IDroneFlightBody flightBody)
        {
            _settings = settings ??
                throw new ArgumentNullException(nameof(settings));

            _flightBody = flightBody ??
                throw new ArgumentNullException(nameof(flightBody));
        }

        public bool Tick(Vector3 destination, float fixedDeltaTime)
        {
            if (fixedDeltaTime <= 0f)
                return false;

            Vector3 offset = destination - _flightBody.Position;
            float distance = offset.magnitude;

            if (distance <= ARRIVAL_TOLERANCE)
            {
                _flightBody.Stop();
                return true;
            }

            float currentSpeed = _flightBody.Velocity.magnitude;

            float rate = currentSpeed > _settings.MaxSpeed
                ? _settings.Deceleration
                : _settings.Acceleration;

            float speed = Mathf.MoveTowards(
                currentSpeed,
                _settings.MaxSpeed,
                rate * fixedDeltaTime);

            speed = Mathf.Min(speed, distance / fixedDeltaTime);

            _flightBody.ApplyVelocity(offset / distance * speed);

            return false;
        }
    }
}
