using UnityEngine;
using System;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Stations.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Player.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Stations.Abstractions
{
    public interface IStationOccupant
    {
        bool IsInStation { get; }
        bool TryEnterStation(
            long expectedRevision,
            CrewRoleId roleId,
            Transform seatAnchor,
            Transform cameraAnchor,
            IInteractionScope interactionScope,
            IStationCapabilityProfile capabilityProfile);
        bool TryExitStation(Transform exitAnchor);
        bool TryForceExitStation(Transform exitAnchor);
        bool TrySetReleaseHandler(Func<bool> releaseHandler);
        void ClearReleaseHandler(Func<bool> releaseHandler);
        bool TryRollbackStationEntry();
    }
}