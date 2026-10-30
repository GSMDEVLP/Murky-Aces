using System;
using UnityEngine;
using _Project.Develop.Runtime.Core.GameLoop.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Movement.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Movement.Application;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Movement.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Targeting.Application;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Behaviour.Application
{
    public sealed class DroneBehaviour : IFixedGameplayTickable
    {
        private readonly KamikazeDroneBehaviour _patrol;
        private readonly DronePursuit _pursuit;
        private readonly DroneTargetSelector _targets;
        private readonly IDroneFlightBody _flightBody;

        public event Action OnAttackPointReached;

        public KamikazeDroneState PatrolState => _patrol.State;
        public Vector3? AttackDestination { get; private set; }
        public bool HasReachedAttackPoint { get; private set; }

        public DroneBehaviour(
            KamikazeDroneBehaviour patrol,
            DronePursuit pursuit,
            DroneTargetSelector targets,
            IDroneFlightBody flightBody)
        {
            _patrol = patrol ??
                throw new ArgumentNullException(nameof(patrol));

            _pursuit = pursuit ??
                throw new ArgumentNullException(nameof(pursuit));

            _targets = targets ??
                throw new ArgumentNullException(nameof(targets));

            _flightBody = flightBody ??
                throw new ArgumentNullException(nameof(flightBody));
        }

        public void FixedTick(float fixedDeltaTime)
        {
            if (fixedDeltaTime <= 0f)
                return;

            if (HasReachedAttackPoint)
            {
                _flightBody.Stop();
                return;
            }

            if (_targets.LastKnownPosition.HasValue)
                AttackDestination = _targets.LastKnownPosition.Value;

            if (AttackDestination.HasValue)
            {
                if (_pursuit.Tick(AttackDestination.Value, fixedDeltaTime))
                {
                    HasReachedAttackPoint = true;
                    OnAttackPointReached?.Invoke();
                }

                return;
            }

            float travelledDistance =
                _flightBody.MeasureTravelledDistance();

            _patrol.Tick(travelledDistance, fixedDeltaTime);

            KamikazeDroneState state = _patrol.State;

            _flightBody.ApplyFlight(
                state.Speed,
                state.HeadingOffsetDegrees);
        }
    }
}
