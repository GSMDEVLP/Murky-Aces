using System;
using System.Collections.Generic;
using _Project.Develop.Runtime.Core.GameLoop.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Combat.Targeting.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Combat.Targeting.Application;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Targeting.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Targeting.Application
{
    public sealed class DroneTargetScanner : IGameplayTickable
    {
        private readonly CombatTargetRegistry _targets;
        private readonly IDroneVisionSensor _sensor;
        private readonly float _scanInterval;

        private readonly List<ICombatTarget> _detected = new();
        private float _timeUntilNextScan;

        public event Action OnScanCompleted;

        public IReadOnlyList<ICombatTarget> DetectedTargets { get; }

        public DroneTargetScanner(
            CombatTargetRegistry targets,
            IDroneVisionSensor sensor,
            float scanInterval)
        {
            _targets = targets ??
                throw new ArgumentNullException(nameof(targets));

            _sensor = sensor ??
                throw new ArgumentNullException(nameof(sensor));

            if (float.IsNaN(scanInterval) ||
                float.IsInfinity(scanInterval) ||
                scanInterval <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(scanInterval));
            }

            _scanInterval = scanInterval;
            DetectedTargets = _detected.AsReadOnly();
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f)
                return;

            _timeUntilNextScan -= deltaTime;

            if (_timeUntilNextScan > 0f)
                return;

            Scan();
            _timeUntilNextScan = _scanInterval;
        }

        public void Clear()
        {
            _detected.Clear();
            _timeUntilNextScan = 0f;
        }

        private void Scan()
        {
            _detected.Clear();

            foreach (ICombatTarget target in _targets.Targets)
            {
                if (_sensor.CanSee(target))
                    _detected.Add(target);
            }
            OnScanCompleted?.Invoke();
        }
    }
}