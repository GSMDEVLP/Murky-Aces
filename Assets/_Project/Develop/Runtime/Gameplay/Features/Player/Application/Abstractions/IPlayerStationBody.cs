using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Application.Abstractions
{
    public interface IPlayerStationBody
    {
        bool IsAttached { get; }

        bool TryAttach(Transform seatAnchor);
        bool TryDetach(Transform exitAnchor);
    }
}