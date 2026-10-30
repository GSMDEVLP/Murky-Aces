using System;
using _Project.Develop.Runtime.Gameplay.Features.Combat.Targeting.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Targeting.Domain
{
    public readonly struct DroneTargetCandidate
    {
        public ulong TargetId { get; }
        public CombatTargetKind Kind { get; }
        public float SqrDistanceToDrone { get; }

        public DroneTargetCandidate(
            ulong targetId,
            CombatTargetKind kind,
            float sqrDistanceToDrone)
        {
            if (targetId == 0)
                throw new ArgumentOutOfRangeException(nameof(targetId));

            if (float.IsNaN(sqrDistanceToDrone) ||
                float.IsInfinity(sqrDistanceToDrone) ||
                sqrDistanceToDrone < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(sqrDistanceToDrone));
            }

            TargetId = targetId;
            Kind = kind;
            SqrDistanceToDrone = sqrDistanceToDrone;
        }
    }
}