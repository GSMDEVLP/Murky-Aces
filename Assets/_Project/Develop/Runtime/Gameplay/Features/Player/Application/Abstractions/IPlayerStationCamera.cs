using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Application.Abstractions
{
    public interface IPlayerStationCamera
    {
        bool CanUseAnchor(Transform anchor);
        bool TryUseStationAnchor(Transform anchor);
        bool TryRestoreWalkingAnchor();
    }
}