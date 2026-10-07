using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions
{
    public interface IHoldableItem
    {
        bool IsHeld { get; }

        bool TryHold(Transform anchor);

        bool TryDrop(Vector3 position, Vector3 impulse);
    }
}