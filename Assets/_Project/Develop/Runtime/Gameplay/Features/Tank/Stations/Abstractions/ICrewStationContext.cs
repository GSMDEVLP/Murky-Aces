using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Crew.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Player.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Stations.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Stations.Abstractions
{
    public interface ICrewStationContext
    {
        CrewRoleId RoleId { get; }
        CrewLocationKind RequiredEntryLocation { get; }

        Transform SeatAnchor { get; }
        Transform CameraAnchor { get; }
        Transform ExitAnchor { get; }

        IInteractionScope InteractionScope { get; }
        IStationCapabilityProfile CapabilityProfile { get; }

        bool CanEnter { get; }
        bool CanExit { get; }
    }
}