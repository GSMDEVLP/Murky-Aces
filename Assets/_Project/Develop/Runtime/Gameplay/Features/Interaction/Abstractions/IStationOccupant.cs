using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions
{
    public interface IStationOccupant
    {
        bool IsInStation { get; }

        bool TryEnterStation(Transform seatAnchor, Transform cameraAnchor);
        bool TryExitStation(Transform exitAnchor);
    }
}