using UnityEngine;
using System;
using _Project.Develop.Runtime.Gameplay.Features.Player.Application.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions
{
    public interface IStationOccupant
    {
        bool IsInStation { get; }
        bool TryEnterStation(Transform seatAnchor,Transform cameraAnchor,IInteractionScope interactionScope, StationCapabilityProfile capabilityProfile);
        bool TryExitStation(Transform exitAnchor);
        bool TryForceExitStation(Transform exitAnchor);
        bool TrySetReleaseHandler(Func<bool> releaseHandler);
        void ClearReleaseHandler(Func<bool> releaseHandler);
    }
}