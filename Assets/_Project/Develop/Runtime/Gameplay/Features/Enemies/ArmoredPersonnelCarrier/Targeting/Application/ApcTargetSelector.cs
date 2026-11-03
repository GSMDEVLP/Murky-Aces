using System;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Combat.Targeting.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Combat.Targeting.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.ArmoredPersonnelCarrier.Targeting.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.ArmoredPersonnelCarrier.Targeting.Application
{
    public sealed class ApcTargetSelector
    {
        private readonly ApcTargetScanner _scanner;
        private readonly IApcVisionSensor _sensor;
        private ICombatTarget _currentTarget;
        private ICombatTarget _lostTarget;

        public ICombatTarget CurrentTarget =>
            _currentTarget != null && _currentTarget.IsAvailable
                ? _currentTarget
                : null;

        public Vector3? AimPosition { get; private set; }

        public ApcTargetSelector(ApcTargetScanner scanner, IApcVisionSensor sensor)
        {
            _scanner = scanner ??
                throw new ArgumentNullException(nameof(scanner));
            _sensor = sensor ??
                throw new ArgumentNullException(nameof(sensor));
        }

        public void Refresh()
        {
            if (_lostTarget != null && !_lostTarget.IsAvailable)
                _lostTarget = null;

            ICombatTarget current = CurrentTarget;

            if (current != null && _sensor.CanTrack(current))
            {
                if (_lostTarget != null &&
                    GetPriority(_lostTarget.Kind) < GetPriority(current.Kind) &&
                    _sensor.CanTrack(_lostTarget))
                {
                    _currentTarget = _lostTarget;
                    _lostTarget = null;
                    AimPosition = _currentTarget.WorldPosition;
                    return;
                }

                AimPosition = current.WorldPosition;
                return;
            }

            RememberLostTarget(current);
            _currentTarget = null;
            AimPosition = null;

            Vector3 origin = _sensor.WorldPosition;
            ICombatTarget best = _lostTarget != null && _sensor.CanTrack(_lostTarget)
                ? _lostTarget
                : null;
            int bestPriority = best != null
                ? GetPriority(best.Kind)
                : int.MaxValue;
            float bestDistance = best != null
                ? (best.WorldPosition - origin).sqrMagnitude
                : float.PositiveInfinity;

            foreach (ICombatTarget candidate in _scanner.DetectedTargets)
            {
                if (candidate == null || !candidate.IsAvailable)
                    continue;

                int priority = GetPriority(candidate.Kind);
                if (priority == int.MaxValue)
                    continue;

                float distance =
                    (candidate.WorldPosition - origin).sqrMagnitude;

                if (best == null ||
                    priority < bestPriority ||
                    (priority == bestPriority &&
                     (distance < bestDistance ||
                      (distance == bestDistance &&
                       candidate.TargetId < best.TargetId))))
                {
                    best = candidate;
                    bestPriority = priority;
                    bestDistance = distance;
                }
            }

            _currentTarget = best;
            if (best != null)
            {
                AimPosition = best.WorldPosition;

                if (ReferenceEquals(best, _lostTarget))
                    _lostTarget = null;
            }
        }

        public void Clear()
        {
            _currentTarget = null;
            _lostTarget = null;
            AimPosition = null;
        }

        private void RememberLostTarget(ICombatTarget target)
        {
            if (target == null || !target.IsAvailable)
                return;

            if (_lostTarget == null ||
                GetPriority(target.Kind) <= GetPriority(_lostTarget.Kind))
            {
                _lostTarget = target;
            }
        }

        private static int GetPriority(CombatTargetKind kind)
        {
            switch (kind)
            {
                case CombatTargetKind.Player:
                    return 0;
                case CombatTargetKind.Tank:
                    return 1;
                case CombatTargetKind.Monster:
                    return 2;
                default:
                    return int.MaxValue;
            }
        }
    }
}
