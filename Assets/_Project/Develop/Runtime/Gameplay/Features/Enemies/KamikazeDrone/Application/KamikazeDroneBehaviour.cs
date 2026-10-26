using System;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Application
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

        public KamikazeDroneState State { get; }

        public KamikazeDroneBehaviour(
            float maxSpeed,
            float acceleration,
            float deceleration,
            float legDistance,
            float pauseDuration,
            Random random,
            KamikazeDroneState state)
        {
            bool hasValidSettings =
                maxSpeed > 0f &&
                acceleration > 0f &&
                deceleration > 0f &&
                legDistance > DISTANCE_TOLERANCE &&
                pauseDuration >= 0f;

            if (hasValidSettings == false)
                throw new ArgumentException("Invalid drone patrol settings.");

            _maxSpeed = maxSpeed;
            _acceleration = acceleration;
            _deceleration = deceleration;
            _legDistance = legDistance;
            _pauseDuration = pauseDuration;
            _random = random ??
                throw new ArgumentNullException(nameof(random));
            State = state ??
                throw new ArgumentNullException(nameof(state));
        }

        public void Tick(float travelledDistance, float deltaTime)
        {
            if (deltaTime <= 0f)
                return;

            switch (State.Mode)
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
            State.DistanceTravelled += Math.Max(0f, travelledDistance);

            float remainingDistance = Math.Max(
                0f,
                _legDistance - State.DistanceTravelled);

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
                desiredSpeed > State.Speed
                    ? _acceleration * deltaTime
                    : _deceleration * deltaTime;

            State.Speed = MoveTowards(
                State.Speed,
                desiredSpeed,
                speedChange);

            // Последний шаг не должен увести за конец участка.
            State.Speed = Math.Min(
                State.Speed,
                remainingDistance / deltaTime);
        }

        private void BeginWaiting()
        {
            State.Mode = KamikazeDroneMode.Waiting;
            State.Speed = 0f;
            State.PauseRemaining = _pauseDuration;
        }

        private void TickWaiting(float deltaTime)
        {
            State.PauseRemaining = Math.Max(
                0f,
                State.PauseRemaining - deltaTime);

            if (State.PauseRemaining > 0f)
                return;

            float turnAngle =
                ((float)_random.NextDouble() * 2f - 1f) *
                MAX_TURN_ANGLE;

            State.HeadingOffsetDegrees =
                (State.HeadingOffsetDegrees + turnAngle) % FULL_TURN;

            State.DistanceTravelled = 0f;
            State.Mode = KamikazeDroneMode.Patrolling;
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