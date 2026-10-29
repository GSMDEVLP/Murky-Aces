using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Combat.Targeting.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Combat.Targeting.Abstractions
{
    public interface ICombatTarget
    {
        ulong TargetId { get; }
        CombatTargetKind Kind { get; }
        Vector3 WorldPosition { get; }
        bool IsAvailable { get; }
    }
}