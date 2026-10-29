using System;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Movement.Application
{
    public sealed class DroneMovementSettings
    {
        public float MaxSpeed { get; }
        public float Acceleration { get; }
        public float Deceleration { get; }
        public float LegDistance { get; }
        public float PauseDuration { get; }

        public DroneMovementSettings(
            float maxSpeed,
            float acceleration,
            float deceleration,
            float legDistance,
            float pauseDuration)
        {
            ValidatePositiveFinite(maxSpeed, nameof(maxSpeed));
            ValidatePositiveFinite(acceleration, nameof(acceleration));
            ValidatePositiveFinite(deceleration, nameof(deceleration));
            ValidatePositiveFinite(legDistance, nameof(legDistance));

            if (legDistance <= 0.01f)
                throw new ArgumentOutOfRangeException(nameof(legDistance));

            if (float.IsNaN(pauseDuration) || float.IsInfinity(pauseDuration) || pauseDuration < 0f)
                throw new ArgumentOutOfRangeException(nameof(pauseDuration));

            MaxSpeed = maxSpeed;
            Acceleration = acceleration;
            Deceleration = deceleration;
            LegDistance = legDistance;
            PauseDuration = pauseDuration;
        }

        private static void ValidatePositiveFinite(float value, string parameterName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0f)
                throw new ArgumentOutOfRangeException(parameterName);
        }
    }
}
