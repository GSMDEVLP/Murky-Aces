using System;
using System.Collections.Generic;
using _Project.Develop.Runtime.Gameplay.Features.Combat.Targeting.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Combat.Targeting.Application
{
    public sealed class CombatTargetRegistry
    {
        private readonly Dictionary<ulong, ICombatTarget> _targets = new();

        public int Count => _targets.Count;

        public IEnumerable<ICombatTarget> Targets => _targets.Values;

        public void Register(ICombatTarget target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            ulong id = target.TargetId;

            if (_targets.TryGetValue(id, out ICombatTarget existing))
            {
                if (ReferenceEquals(existing, target))
                    return;

                throw new InvalidOperationException(
                    $"Duplicate combat target id: {id}.");
            }

            _targets.Add(id, target);
        }

        public void Unregister(ICombatTarget target)
        {
            if (target == null)
                return;

            ulong id = target.TargetId;

            if (_targets.TryGetValue(id, out ICombatTarget existing) &&
                ReferenceEquals(existing, target))
            {
                _targets.Remove(id);
            }
        }
    }
}