using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions
{
    public interface IHoldableItem
    {
        bool IsHeld { get; }

        bool TryHold(ulong actorId, Transform anchor);
        bool TryDrop(ulong actorId, Vector3 position, Vector3 impulse);
        bool TryEndCarry(ulong actorId);
    }
}