using UnityEngine;
using _Project.Develop.Runtime.Core.GameLoop.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Application.Abstractions
{
    public interface IPlayerGpsCamera : IPresentationTickable
    {
        bool IsAvailable { get; }
        bool IsActive { get; }
        bool IsFocused { get; }

        bool TryFocus(Transform anchor);
        bool TryReturn();
        void ForceRestore();
    }
}