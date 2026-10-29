using System;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Movement.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Movement.Application
{
    public sealed class KamikazeDroneBehaviour
    {
        private const float DISTANCE_TOLERANCE = 0.01f;
        private const float MAX_TURN_ANGLE = 90f;
        private const float FULL_TURN = 360f;

        private readonly float _maxSpeed;
        private readonly float _acceleration;
        private readonly float _deceleration;
        private readonly float _legDistance;
        private readonly float _pauseDuration;
        private readonly Random _random;

        private KamikazeDroneMode _mode;
        private float _speed;
        private float _distanceTravelled;
        private float _pauseRemaining;
        private float _headingOffsetDegrees;

        public KamikazeDroneState State => new KamikazeDroneState(
            _mode, _speed, _distanceTravelled, _pauseRemaining, _headingOffsetDegrees);

        public KamikazeDroneBehaviour(
            DroneMovementSettings settings,
            Random random)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            _maxSpeed = settings.MaxSpeed;
            _acceleration = settings.Acceleration;
            _deceleration = settings.Deceleration;
            _legDistance = settings.LegDistance;
            _pauseDuration = settings.PauseDuration;
            _random = random ??
                throw new ArgumentNullException(nameof(random));
        }

        public void Tick(float travelledDistance, float deltaTime)
        {
            if (!IsFinite(travelledDistance))
                throw new ArgumentOutOfRangeException(nameof(travelledDistance));

            if (!IsFinite(deltaTime))
                throw new ArgumentOutOfRangeException(nameof(deltaTime));

            if (deltaTime <= 0f)
                return;

            switch (_mode)
            {
                case KamikazeDroneMode.Patrolling:
                    TickPatrol(travelledDistance, deltaTime);
                    break;

                case KamikazeDroneMode.Waiting:
                    TickWaiting(deltaTime);
                    break;
            }
        }

        private void TickPatrol(float travelledDistance, float deltaTime)
        {
            _distanceTravelled += Math.Max(0f, travelledDistance);

            float remainingDistance = Math.Max(
                0f,
                _legDistance - _distanceTravelled);

            if (remainingDistance <= DISTANCE_TOLERANCE)
            {
                BeginWaiting();
                return;
            }

            // Ограничение скорости по оставшемуся тормозному пути.
            float brakingSpeed = (float)Math.Sqrt(
                2f * _deceleration * remainingDistance);

            float desiredSpeed = Math.Min(_maxSpeed, brakingSpeed);

            float speedChange =
                desiredSpeed > _speed
                    ? _acceleration * deltaTime
                    : _deceleration * deltaTime;

            _speed = MoveTowards(
                _speed,
                desiredSpeed,
                speedChange);

            // Последний шаг не должен увести за конец участка.
            _speed = Math.Min(
                _speed,
                remainingDistance / deltaTime);
        }

        private void BeginWaiting()
        {
            _mode = KamikazeDroneMode.Waiting;
            _speed = 0f;
            _pauseRemaining = _pauseDuration;
        }

        private void TickWaiting(float deltaTime)
        {
            _pauseRemaining = Math.Max(
                0f,
                _pauseRemaining - deltaTime);

            if (_pauseRemaining > 0f)
                return;

            float turnAngle =
                ((float)_random.NextDouble() * 2f - 1f) *
                MAX_TURN_ANGLE;

            _headingOffsetDegrees =
                (_headingOffsetDegrees + turnAngle) % FULL_TURN;

            _distanceTravelled = 0f;
            _mode = KamikazeDroneMode.Patrolling;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static float MoveTowards(
            float current,
            float target,
            float maxDelta)
        {
            if (current < target)
                return Math.Min(current + maxDelta, target);

            return Math.Max(current - maxDelta, target);
        }
    }
}
