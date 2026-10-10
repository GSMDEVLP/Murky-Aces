using System;
using System.Collections.Generic;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Radar.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Radar.Application
{
    public sealed class RadarTargetRegistry
    {
        private readonly Dictionary<ulong, IRadarTarget> _targets = new();

        public int Count => _targets.Count;

        public IEnumerable<IRadarTarget> Targets => _targets.Values;

        public void Register(IRadarTarget target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            ulong id = target.TargetId;

            if (_targets.TryGetValue(id, out IRadarTarget existing))
            {
                if (ReferenceEquals(existing, target))
                    return;

                throw new InvalidOperationException(
                    $"Duplicate radar target id: {id}.");
            }

            _targets.Add(id, target);
        }

        public void Unregister(IRadarTarget target)
        {
            if (target == null)
                return;

            ulong id = target.TargetId;

            if (_targets.TryGetValue(id, out IRadarTarget existing) &&
                ReferenceEquals(existing, target))
            {
                _targets.Remove(id);
            }
        }
    }
}