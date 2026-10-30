using System;
using System.Collections.Generic;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Combat.Targeting.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Targeting.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Targeting.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Targeting.Application
{
    public sealed class DroneTargetSelector
    {
        private readonly DroneTargetScanner _scanner;
        private readonly IDroneVisionSensor _sensor;
        private readonly List<DroneTargetCandidate> _candidates = new();

        private ICombatTarget _currentTarget;

        public Vector3? LastKnownPosition { get; private set; }

        public ICombatTarget CurrentTarget =>
            _currentTarget != null && _currentTarget.IsAvailable
                ? _currentTarget
                : null;

        public DroneTargetSelector(
            DroneTargetScanner scanner,
            IDroneVisionSensor sensor)
        {
            _scanner = scanner ??
                throw new ArgumentNullException(nameof(scanner));

            _sensor = sensor ??
                throw new ArgumentNullException(nameof(sensor));
        }

        public void Refresh()
        {
            ulong previousId = CurrentTarget?.TargetId ?? 0;
            var origin = _sensor.WorldPosition;

            _candidates.Clear();

            foreach (ICombatTarget target in _scanner.DetectedTargets)
            {
                if (target == null || !target.IsAvailable)
                    continue;

                float squaredDistance =
                    (target.WorldPosition - origin).sqrMagnitude;

                _candidates.Add(new DroneTargetCandidate(
                    target.TargetId,
                    target.Kind,
                    squaredDistance));
            }

            ulong selectedId = DroneTargetSelectionRules.Select(
                _candidates,
                previousId);

            _currentTarget = null;

            foreach (ICombatTarget target in _scanner.DetectedTargets)
            {
                if (target != null &&
                    target.IsAvailable &&
                    target.TargetId == selectedId)
                {
                    _currentTarget = target;
                    LastKnownPosition = target.WorldPosition;
                    break;
                }
            }
        }

        public void Clear()
        {
            _currentTarget = null;
            LastKnownPosition = null;
            _candidates.Clear();
        }
    }
}
