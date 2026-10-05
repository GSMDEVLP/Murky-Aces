using System;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Application.Abstractions
{
    public interface IPlayerInteriorBody
    {
        bool CanTeleport { get; }

        bool TryPrepareTeleport(
            Transform destination,
            out Func<bool> tryApply,
            out Func<bool> tryRollback);
    }
}