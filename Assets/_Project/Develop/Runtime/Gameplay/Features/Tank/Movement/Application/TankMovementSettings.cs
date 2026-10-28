using System;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Movement.Application
{
    public sealed class TankMovementSettings
    {
        public float ForwardMaxSpeed { get; }
        public float ReverseMaxSpeed { get; }
        public float ForwardAcceleration { get; }
        public float ReverseAcceleration { get; }
        public float TurnAcceleration { get; }
        public float Deceleration { get; }
        public float BrakeDeceleration { get; }
        public float TurnSpeed { get; }
        public float DirectionSwitchDelay { get; }

        public TankMovementSettings(
            float forwardMaxSpeed,
            float reverseMaxSpeed,
            float forwardAcceleration,
            float reverseAcceleration,
            float turnAcceleration,
            float deceleration,
            float brakeDeceleration,
            float turnSpeed,
            float directionSwitchDelay)
        {
            ValidateNonNegativeFinite(forwardMaxSpeed, nameof(forwardMaxSpeed));
            ValidateNonNegativeFinite(reverseMaxSpeed, nameof(reverseMaxSpeed));
            ValidateNonNegativeFinite(forwardAcceleration, nameof(forwardAcceleration));
            ValidateNonNegativeFinite(reverseAcceleration, nameof(reverseAcceleration));
            ValidateNonNegativeFinite(turnAcceleration, nameof(turnAcceleration));
            ValidateNonNegativeFinite(deceleration, nameof(deceleration));
            ValidateNonNegativeFinite(brakeDeceleration, nameof(brakeDeceleration));
            ValidateNonNegativeFinite(turnSpeed, nameof(turnSpeed));
            ValidateNonNegativeFinite(directionSwitchDelay, nameof(directionSwitchDelay));

            ForwardMaxSpeed = forwardMaxSpeed;
            ReverseMaxSpeed = reverseMaxSpeed;
            ForwardAcceleration = forwardAcceleration;
            ReverseAcceleration = reverseAcceleration;
            TurnAcceleration = turnAcceleration;
            Deceleration = deceleration;
            BrakeDeceleration = brakeDeceleration;
            TurnSpeed = turnSpeed;
            DirectionSwitchDelay = directionSwitchDelay;
        }

        private static void ValidateNonNegativeFinite(float value, string parameterName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
                throw new ArgumentOutOfRangeException(parameterName);
        }
    }
}
