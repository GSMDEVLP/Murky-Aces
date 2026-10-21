using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Player.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Abstractions
{
    public interface IStationCapabilityProfile
    {
        StationControlContext ControlContext { get; }

        bool BlocksLocomotion { get; }
        bool UsesCockpitLook { get; }
        bool AllowsInteraction { get; }

        Vector2 YawLimits { get; }
        Vector2 PitchLimits { get; }
        Vector2 InitialLookAngles { get; }

        bool HasValidLookSettings { get; }
    }
}