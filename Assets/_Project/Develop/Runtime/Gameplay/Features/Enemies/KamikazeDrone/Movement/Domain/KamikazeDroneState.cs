using System;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Movement.Domain
{
    public readonly struct KamikazeDroneState
    {
        public KamikazeDroneMode Mode { get; }
        public float Speed { get; }
        public float DistanceTravelled { get; }
        public float PauseRemaining { get; }

        // Угол относительно начального направления экземпляра.
        public float HeadingOffsetDegrees { get; }

        public KamikazeDroneState(
            KamikazeDroneMode mode,
            float speed,
            float distanceTravelled,
            float pauseRemaining,
            float headingOffsetDegrees)
        {
            if (mode != KamikazeDroneMode.Patrolling && mode != KamikazeDroneMode.Waiting)
                throw new ArgumentOutOfRangeException(nameof(mode));

            ValidateNonNegativeFinite(speed, nameof(speed));
            ValidateNonNegativeFinite(distanceTravelled, nameof(distanceTravelled));
            ValidateNonNegativeFinite(pauseRemaining, nameof(pauseRemaining));

            if (float.IsNaN(headingOffsetDegrees) || float.IsInfinity(headingOffsetDegrees))
                throw new ArgumentOutOfRangeException(nameof(headingOffsetDegrees));

            if ((mode == KamikazeDroneMode.Waiting && speed != 0f) ||
                (mode == KamikazeDroneMode.Patrolling && pauseRemaining != 0f))
            {
                throw new ArgumentException("Inconsistent drone patrol state.");
            }

            Mode = mode;
            Speed = speed;
            DistanceTravelled = distanceTravelled;
            PauseRemaining = pauseRemaining;
            HeadingOffsetDegrees = headingOffsetDegrees % 360f;
        }

        private static void ValidateNonNegativeFinite(float value, string parameterName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
                throw new ArgumentOutOfRangeException(parameterName);
        }
    }
}
